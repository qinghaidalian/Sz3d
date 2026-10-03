using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>3D 统计离群点去除：基于 k 近邻平均距离的滤波。</summary>
public sealed class StatisticalOutlierRemovalNode : NodeBase
{
    public StatisticalOutlierRemovalNode()
    {
        Title = "统计离群点去除";
        Category = NodeCategories.Processing3D;
        Description = "计算各点 k 近邻平均距离，剔除超过均值 + 倍数×标准差的离群点。";
        AddInput("点云", DataTypeKind.PointCloud);
        AddInput("近邻数", DataTypeKind.Number, 8.0);
        AddInput("标准差倍数", DataTypeKind.Number, 1.0);
        AddOutput("点云", DataTypeKind.PointCloud);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var cloud = ctx.GetInput<PointCloudData>("点云");
        var k = Math.Max(1, (int)Math.Round(ctx.GetInput<double>("近邻数")));
        var mul = ctx.GetInput<double>("标准差倍数");
        if (cloud == null || cloud.Count < 3) { ctx.SetOutput("点云", cloud); return; }

        int n = cloud.Count;
        var meanDist = new double[n];

        for (int i = 0; i < n; i++)
        {
            // 取该点到其它点的 k 个最近距离
            var dists = new double[n - 1];
            int m = 0;
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;
                dists[m++] = cloud.Points[i].DistanceTo(cloud.Points[j]);
            }
            Array.Sort(dists);
            double sum = 0;
            int taken = Math.Min(k, dists.Length);
            for (int t = 0; t < taken; t++) sum += dists[t];
            meanDist[i] = sum / taken;
        }

        double mean = meanDist.Average();
        double variance = meanDist.Sum(d => (d - mean) * (d - mean)) / n;
        double std = Math.Sqrt(variance);
        double threshold = mean + mul * std;

        var keep = new List<int>();
        for (int i = 0; i < n; i++)
            if (meanDist[i] <= threshold) keep.Add(i);

        ctx.SetOutput("点云", cloud.Select(keep.ToArray()));
    }
}
