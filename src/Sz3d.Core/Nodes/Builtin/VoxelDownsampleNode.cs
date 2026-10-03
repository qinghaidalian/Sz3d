using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>3D 体素降采样：按体素网格对点云求均值下采样。</summary>
public sealed class VoxelDownsampleNode : NodeBase
{
    public VoxelDownsampleNode()
    {
        Title = "体素降采样";
        Category = NodeCategories.Processing3D;
        Description = "按体素尺寸对点云做均值下采样，降低点密度。";
        AddInput("点云", DataTypeKind.PointCloud);
        AddInput("体素尺寸", DataTypeKind.Number, 2.0);
        AddOutput("点云", DataTypeKind.PointCloud);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var cloud = ctx.GetInput<PointCloudData>("点云");
        var voxel = ctx.GetInput<double>("体素尺寸");
        if (cloud == null || cloud.Count == 0) { ctx.SetOutput("点云", null); return; }
        if (voxel <= 0) voxel = 1.0;

        var buckets = new Dictionary<(long, long, long), (double sx, double sy, double sz, double sr, double sg, double sb, int n)>();

        for (int i = 0; i < cloud.Count; i++)
        {
            var p = cloud.Points[i];
            long kx = (long)Math.Floor(p.X / voxel);
            long ky = (long)Math.Floor(p.Y / voxel);
            long kz = (long)Math.Floor(p.Z / voxel);
            var key = (kx, ky, kz);

            double r = 0.5, g = 0.5, b = 0.8;
            if (cloud.Colors != null)
            {
                r = cloud.Colors[i * 4] / 255.0;
                g = cloud.Colors[i * 4 + 1] / 255.0;
                b = cloud.Colors[i * 4 + 2] / 255.0;
            }

            if (buckets.TryGetValue(key, out var acc))
            {
                buckets[key] = (acc.sx + p.X, acc.sy + p.Y, acc.sz + p.Z, acc.sr + r, acc.sg + g, acc.sb + b, acc.n + 1);
            }
            else
            {
                buckets[key] = (p.X, p.Y, p.Z, r, g, b, 1);
            }
        }

        var pts = new Point3[buckets.Count];
        var cols = new byte[buckets.Count * 4];
        int idx = 0;
        foreach (var acc in buckets.Values)
        {
            double n = acc.n;
            pts[idx] = new Point3(acc.sx / n, acc.sy / n, acc.sz / n);
            cols[idx * 4] = (byte)Math.Clamp(acc.sr / n * 255, 0, 255);
            cols[idx * 4 + 1] = (byte)Math.Clamp(acc.sg / n * 255, 0, 255);
            cols[idx * 4 + 2] = (byte)Math.Clamp(acc.sb / n * 255, 0, 255);
            cols[idx * 4 + 3] = 255;
            idx++;
        }

        ctx.SetOutput("点云", new PointCloudData(pts, cols));
    }
}
