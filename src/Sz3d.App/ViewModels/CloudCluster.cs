using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Sz3d.App.ViewModels;

/// <summary>同色点云分组：一个颜色 + 一组点，用于经典 HelixToolkit 的逐色渲染。</summary>
public sealed class CloudCluster
{
    public Color Color { get; }
    public Point3DCollection Points { get; }

    public CloudCluster(Color color, Point3DCollection points)
    {
        Color = color;
        Points = points;
    }
}
