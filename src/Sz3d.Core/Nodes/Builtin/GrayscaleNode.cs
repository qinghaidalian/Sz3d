using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>2D 灰度化：RGBA → 单通道灰度。</summary>
public sealed class GrayscaleNode : NodeBase
{
    public GrayscaleNode()
    {
        Title = "灰度化";
        Category = NodeCategories.Processing2D;
        Description = "将 RGBA 图像转换为单通道灰度图像。";
        AddInput("图像", DataTypeKind.Image);
        AddOutput("图像", DataTypeKind.Image);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var img = ctx.GetInput<ImageData>("图像");
        if (img == null) { ctx.SetOutput("图像", null); return; }

        var gray = new ImageData(img.Width, img.Height, 1);
        int n = img.Width * img.Height;
        if (img.Channels == 1)
        {
            Array.Copy(img.Pixels, gray.Pixels, n);
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                int d = i * 4;
                // 加权亮度（BT.601）
                gray.Pixels[i] = (byte)(0.299 * img.Pixels[d] + 0.587 * img.Pixels[d + 1] + 0.114 * img.Pixels[d + 2] + 0.5);
            }
        }
        ctx.SetOutput("图像", gray);
    }
}
