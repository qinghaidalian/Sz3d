using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Sz3d.App.ViewModels;
using Sz3d.Core.Nodes;

namespace Sz3d.App.Controls;

public partial class GraphCanvasView : UserControl
{
    private MainViewModel VM => (MainViewModel)DataContext;

    // 视图变换（世界坐标 → 屏幕坐标：screen = world * scale + pan）
    private double _scale = 1.0;
    private Vector _pan = new(40, 40);

    // 拖拽状态
    private NodeViewModel? _dragNode;
    private Point _dragStartMouse;
    private Point _dragStartNode;
    private bool _panning;
    private Point _panStartMouse;
    private Vector _panStart;

    // 临时连线
    private PortViewModel? _tempSource;
    private Path? _tempLine;

    public GraphCanvasView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyTransform();
    }

    private void ApplyTransform()
    {
        var tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(_scale, _scale));
        tg.Children.Add(new TranslateTransform(_pan.X, _pan.Y));
        World.RenderTransform = tg;
    }

    private Point ScreenToWorld(Point screen) => new((screen.X - _pan.X) / _scale, (screen.Y - _pan.Y) / _scale);

    // ============ 缩放 ============
    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        double factor = e.Delta > 0 ? 1.15 : 1 / 1.15;
        double newScale = Math.Clamp(_scale * factor, 0.15, 3.0);
        var c = e.GetPosition(Root);
        double wx = (c.X - _pan.X) / _scale;
        double wy = (c.Y - _pan.Y) / _scale;
        _scale = newScale;
        _pan = new Vector(c.X - wx * newScale, c.Y - wy * newScale);
        ApplyTransform();
        e.Handled = true;
    }

    // ============ 按下 ============
    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var src = e.OriginalSource as DependencyObject;

        var port = FindDataContext<PortViewModel>(src);
        if (port != null)
        {
            // 输入/输出端口均可作为连线起点，方向在松开时判定
            StartTempConnection(port);
            e.Handled = true;
            return;
        }

        var node = FindDataContext<NodeViewModel>(src);
        if (node != null)
        {
            VM.SelectNode(node);
            _dragNode = node;
            _dragStartMouse = e.GetPosition(Root);
            _dragStartNode = new Point(node.X, node.Y);
            Root.CaptureMouse();
            e.Handled = true;
            return;
        }

        var conn = FindDataContext<ConnectionViewModel>(src);
        if (conn != null)
        {
            VM.SelectConnection(conn);
            e.Handled = true;
            return;
        }

        // 空白处：取消选择并开始平移
        VM.SelectNode(null);
        _panning = true;
        _panStartMouse = e.GetPosition(Root);
        _panStart = _pan;
        Root.CaptureMouse();
        e.Handled = true;
    }

    // ============ 抬起 ============
    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_tempSource != null)
        {
            // 用真实命中测试定位落点端口（不依赖鼠标捕获状态下的 OriginalSource）
            var port = HitTestPort(e.GetPosition(Root));
            if (IsValidTarget(_tempSource, port))
            {
                if (!_tempSource.IsInput && port!.IsInput)
                    VM.AddConnection(_tempSource, port);
                else if (_tempSource.IsInput && !port!.IsInput)
                    VM.AddConnection(port, _tempSource);
            }
            ClearTempLine();
        }

        _dragNode = null;
        _panning = false;
        Root.ReleaseMouseCapture();
    }

    // ============ 移动 ============
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(Root);

        if (_tempSource != null)
        {
            UpdateTempLine(ScreenToWorld(pos));
            // 悬停在有效落点端口上时，临时线变绿提示
            var hover = HitTestPort(pos);
            if (_tempLine != null)
                _tempLine.Stroke = IsValidTarget(_tempSource, hover) ? Brushes.LightGreen : Brushes.SkyBlue;
        }
        else if (_dragNode != null)
        {
            double dx = (pos.X - _dragStartMouse.X) / _scale;
            double dy = (pos.Y - _dragStartMouse.Y) / _scale;
            _dragNode.X = _dragStartNode.X + dx;
            _dragNode.Y = _dragStartNode.Y + dy;
            VM.RefreshConnections();
        }
        else if (_panning)
        {
            _pan = _panStart + (pos - _panStartMouse);
            ApplyTransform();
        }
    }

    // ============ 右键菜单 ============
    private void OnRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var src = e.OriginalSource as DependencyObject;
        var node = FindDataContext<NodeViewModel>(src);
        var conn = FindDataContext<ConnectionViewModel>(src);

        var menu = new ContextMenu();
        if (node != null)
        {
            VM.SelectNode(node);
            menu.Items.Add(MenuItem("删除节点", () => VM.RemoveNode(node)));
        }
        else if (conn != null)
        {
            VM.SelectConnection(conn);
            menu.Items.Add(MenuItem("删除连线", () => VM.RemoveConnection(conn)));
        }
        else
        {
            VM.SelectNode(null);
            menu.Items.Add(MenuItem("运行", () => VM.Run()));
            menu.Items.Add(MenuItem("加载示例工程", () => VM.LoadSample()));
            menu.Items.Add(new Separator());
            menu.Items.Add(MenuItem("清空画布", () => VM.Clear()));
        }
        menu.IsOpen = true;
        e.Handled = true;
    }

    private static MenuItem MenuItem(string header, Action action)
    {
        var mi = new MenuItem { Header = header };
        mi.Click += (_, _) => action();
        return mi;
    }

    // ============ 拖放（从节点库） ============
    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(NodeDescriptor)) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(NodeDescriptor)) is NodeDescriptor desc)
        {
            var world = ScreenToWorld(e.GetPosition(Root));
            var nvm = VM.AddNode(desc.Create(), world.X, world.Y);
            VM.SelectNode(nvm);
        }
        e.Handled = true;
    }

    // ============ 临时连线 ============
    private void StartTempConnection(PortViewModel source)
    {
        _tempSource = source;
        _tempLine = new Path
        {
            Stroke = Brushes.SkyBlue,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            IsHitTestVisible = false,   // 关键：临时线不参与命中测试，否则会挡住落点端口
        };
        TempLayer.Children.Add(_tempLine);
        UpdateTempLine(new Point(source.CanvasX, source.CanvasY));
        Root.CaptureMouse();
    }

    private void UpdateTempLine(Point end)
    {
        if (_tempLine == null || _tempSource == null) return;
        _tempLine.Data = BuildBezier(new Point(_tempSource.CanvasX, _tempSource.CanvasY), end);
    }

    private void ClearTempLine()
    {
        if (_tempLine != null) TempLayer.Children.Remove(_tempLine);
        _tempLine = null;
        _tempSource = null;
    }

    private static Geometry BuildBezier(Point s, Point t)
    {
        double dx = Math.Max(40, Math.Abs(t.X - s.X) * 0.5);
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(s, false, false);
            ctx.BezierTo(new Point(s.X + dx, s.Y), new Point(t.X - dx, t.Y), t, true, false);
        }
        g.Freeze();
        return g;
    }

    // ============ 工具 ============
    private PortViewModel? HitTestPort(Point pos)
    {
        if (Root.InputHitTest(pos) is DependencyObject hit)
            return FindDataContext<PortViewModel>(hit);
        return null;
    }

    private static bool IsValidTarget(PortViewModel source, PortViewModel? target)
        => target != null
           && target != source
           && target.Owner != source.Owner
           && ((!source.IsInput && target.IsInput) || (source.IsInput && !target.IsInput))
           && Connection.IsTypeCompatible(source.DataType, target.DataType);

    private static T? FindDataContext<T>(DependencyObject? d) where T : class
    {
        while (d != null)
        {
            if (d is FrameworkElement fe && fe.DataContext is T t) return t;
            d = VisualTreeHelper.GetParent(d);
        }
        return null;
    }
}
