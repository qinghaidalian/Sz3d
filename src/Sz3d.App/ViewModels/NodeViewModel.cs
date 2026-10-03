using System.Collections.ObjectModel;
using Sz3d.App.Infrastructure;
using Sz3d.Core.Nodes;

namespace Sz3d.App.ViewModels;

/// <summary>节点 ViewModel：包装 Core 节点，负责位置与端口锚点。</summary>
public sealed class NodeViewModel : ObservableObject
{
    public NodeBase Node { get; }

    public ObservableCollection<PortViewModel> Inputs { get; } = new();
    public ObservableCollection<PortViewModel> Outputs { get; } = new();

    public NodeViewModel(NodeBase node)
    {
        Node = node;
        foreach (var p in node.Inputs) Inputs.Add(new PortViewModel(p, this));
        foreach (var p in node.Outputs) Outputs.Add(new PortViewModel(p, this));
        UpdatePortPositions();
    }

    public string Title => Node.Title;
    public string Category => Node.Category;
    public string Description => Node.Description;

    public double X
    {
        get => Node.X;
        set { if (Node.X != value) { Node.X = value; OnPropertyChanged(); UpdatePortPositions(); } }
    }

    public double Y
    {
        get => Node.Y;
        set { if (Node.Y != value) { Node.Y = value; OnPropertyChanged(); UpdatePortPositions(); } }
    }

    public double Width => NodeLayout.NodeWidth;
    public double Height => NodeLayout.HeightFor(Math.Max(Inputs.Count, Outputs.Count));

    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }

    private string _statusText = string.Empty;
    /// <summary>执行后显示在节点上的摘要（如 "1280×960" / "1234 点"）。</summary>
    public string StatusText { get => _statusText; set => Set(ref _statusText, value); }

    public void UpdatePortPositions()
    {
        foreach (var p in Inputs)
        {
            p.CanvasX = X;
            p.CanvasY = Y + NodeLayout.PortCenterY(p.Index);
        }
        foreach (var p in Outputs)
        {
            p.CanvasX = X + Width;
            p.CanvasY = Y + NodeLayout.PortCenterY(p.Index);
        }
    }
}
