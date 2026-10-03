using Sz3d.Core.Data;
using Sz3d.Core.Execution;
using Sz3d.Core.Services;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>加载 2D 图像（PNG/JPG/BMP 等）。</summary>
public sealed class LoadImageNode : NodeBase
{
    public LoadImageNode()
    {
        Title = "加载图像";
        Category = NodeCategories.IO;
        Description = "从文件加载 2D 图像，输出 RGBA 图像数据。";
        AddInput("文件路径", DataTypeKind.String, string.Empty);
        AddOutput("图像", DataTypeKind.Image);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var path = ctx.GetInput<string>("文件路径") ?? string.Empty;
        ImageData? img = null;
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            img = ctx.GetService<IImageLoader>()?.Load(path);
        ctx.SetOutput("图像", img);
    }
}
