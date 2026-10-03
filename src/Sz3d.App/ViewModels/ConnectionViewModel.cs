using System.Windows;
using System.Windows.Media;
using Sz3d.App.Infrastructure;
using Sz3d.App.Themes;
using Sz3d.Core.Nodes;

namespace Sz3d.App.ViewModels;

/// <summary>连线 ViewModel：包装 Core 连线，动态计算贝塞尔几何。</summary>
public sealed class ConnectionViewModel : ObservableObject
{
    public Connection Model { get; }
    public PortViewModel Source { get; }
    public PortViewModel Target { get; }

    public ConnectionViewModel(Connection model, PortViewModel source, PortViewModel target)
    {
        Model = model;
        Source = source;
        Target = target;
    }

    public Brush Brush => ColorMaps.ForType(Source.DataType);

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { if (Set(ref _isSelected, value)) OnPropertyChanged(nameof(StrokeThickness)); }
    }

    public double StrokeThickness => IsSelected ? 4.0 : 2.2;

    public Geometry Geometry => BuildGeometry();

    private Geometry BuildGeometry()
    {
        var s = new Point(Source.CanvasX, Source.CanvasY);
        var t = new Point(Target.CanvasX, Target.CanvasY);
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

    public void Refresh() => OnPropertyChanged(nameof(Geometry));
}
