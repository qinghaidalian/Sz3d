using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>2D 均值滤波（盒式模糊），支持 3×3 / 5×5。</summary>
public sealed class BlurNode : NodeBase
{
    public BlurNode()
    {
        Title = "均值滤波";
        Category = NodeCategories.Processing2D;
        Description = "对图像做盒式均值模糊（3×3 或 5×5）。";
        AddInput("图像", DataTypeKind.Image);
        AddInput("核大小", DataTypeKind.Number, 3.0);
        AddOutput("图像", DataTypeKind.Image);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var img = ctx.GetInput<ImageData>("图像");
        var kernel = (int)Math.Round(ctx.GetInput<double>("核大小"));
        if (img == null) { ctx.SetOutput("图像", null); return; }
        if (kernel < 3) kernel = 3;
        if (kernel % 2 == 0) kernel++;
        if (kernel > 5) kernel = 5;

        var src = img;
        var dst = new ImageData(img.Width, img.Height, img.Channels);
        int r = kernel / 2;
        int c = src.Channels;

        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                for (int ch = 0; ch < c; ch++)
                {
                    int sum = 0, cnt = 0;
                    for (int dy = -r; dy <= r; dy++)
                    {
                        for (int dx = -r; dx <= r; dx++)
                        {
                            int sx = x + dx, sy = y + dy;
                            if (sx < 0 || sy < 0 || sx >= src.Width || sy >= src.Height) continue;
                            sum += src.GetPixel(sx, sy, ch);
                            cnt++;
                        }
                    }
                    dst.SetPixel(x, y, ch, (byte)(sum / Math.Max(1, cnt)));
                }
            }
        }
        ctx.SetOutput("图像", dst);
    }
}
