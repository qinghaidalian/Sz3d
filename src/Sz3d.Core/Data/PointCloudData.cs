namespace Sz3d.Core.Data;

/// <summary>
/// 3D 点云数据。点坐标 + 可选逐点颜色（RGBA）。
/// </summary>
public sealed class PointCloudData
{
    public Point3[] Points { get; private set; }

    /// <summary>逐点颜色，长度 = Points.Length * 4（RGBA）；为 null 时使用单一颜色。</summary>
    public byte[]? Colors { get; private set; }

    public int Count => Points.Length;

    public PointCloudData(Point3[] points, byte[]? colors = null)
    {
        Points = points;
        Colors = colors;
    }

    public PointCloudData(int capacity)
    {
        Points = new Point3[capacity];
    }

    /// <summary>按索引筛选点，返回新点云（保留颜色）。</summary>
    public PointCloudData Select(int[] indices)
    {
        var pts = new Point3[indices.Length];
        byte[]? cols = null;
        if (Colors != null)
        {
            cols = new byte[indices.Length * 4];
            for (int i = 0; i < indices.Length; i++)
            {
                pts[i] = Points[indices[i]];
                cols[i * 4] = Colors[indices[i] * 4];
                cols[i * 4 + 1] = Colors[indices[i] * 4 + 1];
                cols[i * 4 + 2] = Colors[indices[i] * 4 + 2];
                cols[i * 4 + 3] = Colors[indices[i] * 4 + 3];
            }
        }
        else
        {
            for (int i = 0; i < indices.Length; i++)
                pts[i] = Points[indices[i]];
        }
        return new PointCloudData(pts, cols);
    }
}
