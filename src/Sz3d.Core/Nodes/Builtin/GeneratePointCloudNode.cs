using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>生成演示点云：波动曲面 + 随机离群点，按高度着色。</summary>
public sealed class GeneratePointCloudNode : NodeBase
{
    public GeneratePointCloudNode()
    {
        Title = "生成点云";
        Category = NodeCategories.Utility;
        Description = "生成演示用 3D 点云（波动曲面，含少量离群点）。";
        AddInput("点数", DataTypeKind.Number, 5000.0);
        AddInput("离群点", DataTypeKind.Number, 100.0);
        AddOutput("点云", DataTypeKind.PointCloud);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        int count = Math.Clamp((int)Math.Round(ctx.GetInput<double>("点数")), 100, 200000);
        int outliers = Math.Max(0, (int)Math.Round(ctx.GetInput<double>("离群点")));
        var rng = new Random(1234);

        var pts = new Point3[count];
        var cols = new byte[count * 4];

        double minZ = double.MaxValue, maxZ = double.MinValue;
        int nSurface = count - outliers;
        for (int i = 0; i < nSurface; i++)
        {
            double x = (rng.NextDouble() - 0.5) * 200;
            double y = (rng.NextDouble() - 0.5) * 200;
            double z = 30 * Math.Sin(x * 0.05) * Math.Cos(y * 0.05);
            pts[i] = new Point3(x, y, z);
            minZ = Math.Min(minZ, z); maxZ = Math.Max(maxZ, z);
        }
        for (int i = nSurface; i < count; i++)
        {
            double x = (rng.NextDouble() - 0.5) * 300;
            double y = (rng.NextDouble() - 0.5) * 300;
            double z = (rng.NextDouble() - 0.5) * 200;
            pts[i] = new Point3(x, y, z);
            minZ = Math.Min(minZ, z); maxZ = Math.Max(maxZ, z);
        }

        double span = Math.Max(1, maxZ - minZ);
        for (int i = 0; i < count; i++)
        {
            double t = (pts[i].Z - minZ) / span;
            // 蓝 → 青 → 黄 → 红 的伪彩色
            (byte r, byte g, byte b) = HeightColor(t);
            cols[i * 4] = r; cols[i * 4 + 1] = g; cols[i * 4 + 2] = b; cols[i * 4 + 3] = 255;
        }

        ctx.SetOutput("点云", new PointCloudData(pts, cols));
    }

    private static (byte, byte, byte) HeightColor(double t)
    {
        t = Math.Clamp(t, 0, 1);
        if (t < 0.25) return ((byte)0, (byte)(t / 0.25 * 255), (byte)255);
        if (t < 0.5) return ((byte)0, (byte)255, (byte)(255 - (t - 0.25) / 0.25 * 255));
        if (t < 0.75) return ((byte)((t - 0.5) / 0.25 * 255), (byte)255, (byte)0);
        return ((byte)255, (byte)(255 - (t - 0.75) / 0.25 * 255), (byte)0);
    }
}
