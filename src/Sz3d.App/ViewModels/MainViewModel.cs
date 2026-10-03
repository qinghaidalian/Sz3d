using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Sz3d.App.Infrastructure;
using Sz3d.Core.Data;
using Sz3d.Core.Execution;
using Sz3d.Core.Nodes;
using Sz3d.Core.Services;

namespace Sz3d.App.ViewModels;

/// <summary>主界面 ViewModel：持有图、节点/连线集合、节点库、执行与可视化状态。</summary>
public sealed class MainViewModel : ObservableObject
{
    public NodeGraph Graph { get; } = new();
    public ServiceContainer Services { get; } = new();

    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();
    public ObservableCollection<LibraryCategoryViewModel> Library { get; }

    private readonly Dictionary<OutputPort, object?> _lastOutputs = new();

    public MainViewModel(IImageLoader imageLoader)
    {
        Services.Add(imageLoader);
        Library = BuildLibrary();
        RunCommand = new RelayCommand(Run);
        ClearCommand = new RelayCommand(Clear);
        LoadSampleCommand = new RelayCommand(LoadSample);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection);
    }

    // ============ 节点库 ============
    private static readonly string[] CategoryOrder =
    {
        NodeCategories.IO, NodeCategories.Processing2D, NodeCategories.Processing3D,
        NodeCategories.Utility, NodeCategories.Calibration, NodeCategories.Robot,
        NodeCategories.Communication, NodeCategories.DeepLearning,
    };

    private static ObservableCollection<LibraryCategoryViewModel> BuildLibrary()
    {
        var list = new ObservableCollection<LibraryCategoryViewModel>();
        var byCategory = NodeCatalog.All.GroupBy(d => d.Category).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var cat in CategoryOrder)
            if (byCategory.TryGetValue(cat, out var items))
                list.Add(new LibraryCategoryViewModel(cat, items));
        foreach (var cat in byCategory.Keys.Where(k => !CategoryOrder.Contains(k)).OrderBy(k => k))
            list.Add(new LibraryCategoryViewModel(cat, byCategory[cat]));
        return list;
    }

    // ============ 选择 ============
    private NodeViewModel? _selectedNode;
    public NodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set { if (Set(ref _selectedNode, value)) OnPropertyChanged(nameof(HasSelection)); }
    }

    private ConnectionViewModel? _selectedConnection;
    public ConnectionViewModel? SelectedConnection
    {
        get => _selectedConnection;
        set { if (Set(ref _selectedConnection, value)) OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool HasSelection => SelectedNode != null || SelectedConnection != null;

    // ============ 可视化输出 ============
    private BitmapSource? _imageResult;
    public BitmapSource? ImageResult { get => _imageResult; set => Set(ref _imageResult, value); }

    private string _imageInfo = "（无图像数据）";
    public string ImageInfo { get => _imageInfo; set => Set(ref _imageInfo, value); }

    private IReadOnlyList<CloudCluster> _cloudClusters = Array.Empty<CloudCluster>();
    public IReadOnlyList<CloudCluster> CloudClusters
    {
        get => _cloudClusters;
        private set => Set(ref _cloudClusters, value);
    }

    /// <summary>点云更新事件（由界面订阅以重建 3D 可视化）。</summary>
    public event Action? CloudChanged;

    private string _cloudInfo = "（无点云数据）";
    public string CloudInfo { get => _cloudInfo; set => Set(ref _cloudInfo, value); }

    // ============ 状态与日志 ============
    private string _status = "就绪";
    public string Status { get => _status; set => Set(ref _status, value); }

    public ObservableCollection<string> Log { get; } = new();

    private void AddLog(string msg)
    {
        Log.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
        if (Log.Count > 600) Log.RemoveAt(0);
    }

    // ============ 节点 / 连线操作 ============
    public NodeViewModel AddNode(NodeBase node, double x, double y)
    {
        node.X = x;
        node.Y = y;
        Graph.AddNode(node);
        var vm = new NodeViewModel(node);
        Nodes.Add(vm);
        AddLog($"添加节点：{node.Category} / {node.Title}");
        return vm;
    }

    public bool AddConnection(PortViewModel source, PortViewModel target)
    {
        if (source.IsInput || !target.IsInput)
        {
            Status = "连线方向错误：需从输出端口连到输入端口。";
            AddLog($"✗ {Status}");
            return false;
        }
        if (source.Owner == target.Owner)
        {
            Status = "不能连接同一节点的端口。";
            AddLog($"✗ {Status}");
            return false;
        }
        if (!Connection.IsTypeCompatible(source.DataType, target.DataType))
        {
            Status = $"类型不兼容：{source.DataType} → {target.DataType}。";
            AddLog($"✗ {Status}");
            return false;
        }

        var model = Graph.Connect((OutputPort)source.Port, (InputPort)target.Port);
        var cvm = new ConnectionViewModel(model, source, target);
        Connections.Add(cvm);
        RefreshPortStates();
        AddLog($"连接：{source.Owner.Title}.{source.Name} → {target.Owner.Title}.{target.Name}");
        return true;
    }

    public void RemoveNode(NodeViewModel nvm)
    {
        foreach (var c in Connections.Where(c => c.Source.Owner == nvm || c.Target.Owner == nvm).ToList())
        {
            Graph.RemoveConnection(c.Model);
            Connections.Remove(c);
        }
        Graph.RemoveNode(nvm.Node);
        Nodes.Remove(nvm);
        if (SelectedNode == nvm) SelectedNode = null;
        RefreshPortStates();
        AddLog($"删除节点：{nvm.Title}");
    }

    public void RemoveConnection(ConnectionViewModel cvm)
    {
        Graph.RemoveConnection(cvm.Model);
        Connections.Remove(cvm);
        if (SelectedConnection == cvm) SelectedConnection = null;
        RefreshPortStates();
        AddLog($"删除连线：{cvm.Source.Name} → {cvm.Target.Name}");
    }

    public void SelectNode(NodeViewModel? nvm)
    {
        foreach (var n in Nodes) n.IsSelected = n == nvm;
        SelectedConnection = null;
        SelectedNode = nvm;
        PreviewNode(nvm);
    }

    public void SelectConnection(ConnectionViewModel? cvm)
    {
        foreach (var c in Connections) c.IsSelected = c == cvm;
        SelectedNode = null;
        SelectedConnection = cvm;
    }

    /// <summary>选中节点后，预览其输出到查看器。</summary>
    private void PreviewNode(NodeViewModel? nvm)
    {
        if (nvm == null) return;

        foreach (var outPort in nvm.Outputs)
        {
            var port = outPort.Port as OutputPort;
            if (port == null || !_lastOutputs.TryGetValue(port, out var val)) continue;

            if (val is ImageData img) ShowImage(img);
            else if (val is PointCloudData cloud) ShowCloud(cloud);
        }
    }

    public void RefreshPortStates()
    {
        foreach (var nvm in Nodes)
        {
            foreach (var p in nvm.Inputs)
                p.IsConnected = Graph.FindConnectionTo((InputPort)p.Port) != null;
            foreach (var p in nvm.Inputs) p.RefreshEditable();
        }
    }

    public void RefreshConnections()
    {
        foreach (var c in Connections) c.Refresh();
    }

    // ============ 执行 ============
    public void Run()
    {
        Status = "执行中…";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = GraphExecutor.Run(Graph, Services);
        sw.Stop();

        foreach (var kv in result.Outputs) _lastOutputs[kv.Key] = kv.Value;

        if (!result.Success)
        {
            AddLog($"✗ {result.Error}");
            Status = result.Error ?? "执行失败";
            return;
        }

        // 汇总
        foreach (var n in result.Order)
        {
            var nvm = Nodes.First(v => v.Node == n);
            nvm.StatusText = Summarize(n, result);
            foreach (var p in nvm.Outputs)
            {
                var op = (OutputPort)p.Port;
                p.ValueText = result.Outputs.TryGetValue(op, out var v) && v != null ? DescribeValue(v) : "（无输出）";
            }
        }

        // 展示最后一个图像 / 点云输出
        ImageData? lastImage = null;
        PointCloudData? lastCloud = null;
        foreach (var n in result.Order)
        {
            foreach (var op in n.Outputs)
            {
                if (!result.Outputs.TryGetValue(op, out var v) || v == null) continue;
                if (v is ImageData img) lastImage = img;
                else if (v is PointCloudData cloud) lastCloud = cloud;
            }
        }
        ShowImage(lastImage);
        ShowCloud(lastCloud);

        RefreshPortStates();
        AddLog($"✓ 执行成功，{result.Order.Count} 个节点，耗时 {sw.ElapsedMilliseconds} ms");
        Status = $"执行成功 · {result.Order.Count} 个节点 · {sw.ElapsedMilliseconds} ms";
    }

    private static string Summarize(NodeBase node, ExecutionResult result)
    {
        foreach (var op in node.Outputs)
        {
            if (!result.Outputs.TryGetValue(op, out var v) || v == null) continue;
            switch (v)
            {
                case ImageData img: return $"{img.Width}×{img.Height}";
                case PointCloudData cloud: return $"{cloud.Count} 点";
                case PoseData pose: return $"({pose.X:F1},{pose.Y:F1},{pose.Z:F1})";
                case string s: return s.Length > 12 ? s[..12] : s;
                default: return v.ToString() ?? "";
            }
        }
        return string.Empty;
    }

    private static string DescribeValue(object v) => v switch
    {
        ImageData img => $"{img.Width} × {img.Height} · {img.Channels} 通道",
        PointCloudData c => $"{c.Count} 点",
        PoseData p => p.ToString(),
        string s => s,
        bool b => b ? "true" : "false",
        double d => d.ToString("G6", System.Globalization.CultureInfo.InvariantCulture),
        _ => v.ToString() ?? string.Empty,
    };

    // ============ 命令 ============
    public ICommand RunCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand LoadSampleCommand { get; }
    public ICommand DeleteSelectionCommand { get; }

    public void Clear()
    {
        Graph.Nodes.Clear();
        Graph.Connections.Clear();
        Nodes.Clear();
        Connections.Clear();
        _lastOutputs.Clear();
        SelectedNode = null;
        SelectedConnection = null;
        ShowImage(null);
        ShowCloud(null);
        AddLog("已清空画布");
        Status = "就绪";
    }

    public void DeleteSelection()
    {
        if (SelectedNode != null) RemoveNode(SelectedNode);
        else if (SelectedConnection != null) RemoveConnection(SelectedConnection);
    }

    public void LoadSample()
    {
        Clear();

        var genCloud = AddNode(NodeCatalog.Find("GeneratePointCloud")!.Create(), 60, 60);
        var voxel = AddNode(NodeCatalog.Find("VoxelDownsample")!.Create(), 340, 60);
        AddConnection(genCloud.Outputs[0], voxel.Inputs[0]);

        var genImg = AddNode(NodeCatalog.Find("GenerateImage")!.Create(), 60, 260);
        var gray = AddNode(NodeCatalog.Find("Grayscale")!.Create(), 320, 260);
        var blur = AddNode(NodeCatalog.Find("Blur")!.Create(), 560, 260);
        AddConnection(genImg.Outputs[0], gray.Inputs[0]);
        AddConnection(gray.Outputs[0], blur.Inputs[0]);

        Run();
        SelectNode(voxel);
        RefreshConnections();
        Status = "示例工程已加载并执行完成";
    }

    // ============ 可视化辅助 ============
    private void ShowImage(ImageData? img)
    {
        ImageResult = ImageUtils.ToBitmapSource(img);
        ImageInfo = img == null ? "（无图像数据）" : $"{img.Width} × {img.Height} · {img.Channels} 通道";
    }

    private void ShowCloud(PointCloudData? cloud)
    {
        if (cloud == null)
        {
            CloudClusters = Array.Empty<CloudCluster>();
            CloudInfo = "（无点云数据）";
            CloudChanged?.Invoke();
            return;
        }

        // 按量化颜色分组（每通道 2 bit → 最多 64 组），供经典 HelixToolkit 逐色渲染
        var buckets = new Dictionary<int, List<Point3D>>();
        bool hasColor = cloud.Colors != null;
        for (int i = 0; i < cloud.Count; i++)
        {
            var p = cloud.Points[i];
            int key;
            if (hasColor)
            {
                byte r = (byte)(cloud.Colors![i * 4] & 0xC0);
                byte g = (byte)(cloud.Colors![i * 4 + 1] & 0xC0);
                byte b = (byte)(cloud.Colors![i * 4 + 2] & 0xC0);
                key = (r << 16) | (g << 8) | b;
            }
            else
            {
                key = 0x7FC9F7; // 默认浅蓝
            }

            if (!buckets.TryGetValue(key, out var list))
                buckets[key] = list = new List<Point3D>();
            list.Add(new Point3D(p.X, p.Y, p.Z));
        }

        var clusters = new List<CloudCluster>(buckets.Count);
        foreach (var kv in buckets)
        {
            int key = kv.Key;
            var color = Color.FromRgb((byte)(key >> 16), (byte)((key >> 8) & 0xFF), (byte)(key & 0xFF));
            clusters.Add(new CloudCluster(color, new Point3DCollection(kv.Value)));
        }

        CloudClusters = clusters;
        CloudInfo = $"{cloud.Count} 点 · {clusters.Count} 颜色组";
        CloudChanged?.Invoke();
    }
}
