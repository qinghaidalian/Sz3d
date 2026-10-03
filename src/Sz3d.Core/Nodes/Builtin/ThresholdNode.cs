using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>2D 二值化：按阈值将灰度图像分割为 0/255。</summary>
public sealed class ThresholdNode : NodeBase
{
    public ThresholdNode()
    {
        Title = "二值化";
        Category = NodeCategories.Processing2D;
        Description = "按阈值对图像做二值化（0 / 255）。";
        AddInput("图像", DataTypeKind.Image);
        AddInput("阈值", DataTypeKind.Number, 128.0);
        AddOutput("图像", DataTypeKind.Image);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var img = ctx.GetInput<ImageData>("图像");
        var th = (int)Math.Round(ctx.GetInput<double>("阈值"));
        if (img == null) { ctx.SetOutput("图像", null); return; }
        th = Math.Clamp(th, 0, 255);

        var gray = img.Channels == 1 ? img : ToGray(img);
        var bin = new ImageData(gray.Width, gray.Height, 1);
        for (int i = 0; i < gray.Width * gray.Height; i++)
            bin.Pixels[i] = gray.Pixels[i] >= th ? (byte)255 : (byte)0;

        ctx.SetOutput("图像", bin);
    }

    private static ImageData ToGray(ImageData img)
    {
        var g = new ImageData(img.Width, img.Height, 1);
        for (int i = 0; i < img.Width * img.Height; i++)
        {
            int d = i * 4;
            g.Pixels[i] = (byte)(0.299 * img.Pixels[d] + 0.587 * img.Pixels[d + 1] + 0.114 * img.Pixels[d + 2] + 0.5);
        }
        return g;
    }
}
