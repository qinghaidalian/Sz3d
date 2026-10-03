using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>加载 3D 点云（PLY/XYZ/TXT）。</summary>
public sealed class LoadPointCloudNode : NodeBase
{
    public LoadPointCloudNode()
    {
        Title = "加载点云";
        Category = NodeCategories.IO;
        Description = "从文件加载 3D 点云，支持 PLY(ASCII)、XYZ、TXT。";
        AddInput("文件路径", DataTypeKind.String, string.Empty);
        AddOutput("点云", DataTypeKind.PointCloud);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var path = ctx.GetInput<string>("文件路径") ?? string.Empty;
        var cloud = string.IsNullOrWhiteSpace(path) ? null : PointCloudIO.Load(path);
        ctx.SetOutput("点云", cloud);
    }
}
