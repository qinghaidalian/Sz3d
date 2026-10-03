using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>生成演示图像：渐变 + 圆与条纹，便于无文件时验证流程。</summary>
public sealed class GenerateImageNode : NodeBase
{
    public GenerateImageNode()
    {
        Title = "生成图像";
        Category = NodeCategories.Utility;
        Description = "生成演示用 2D 图像（渐变 + 几何图形）。";
        AddOutput("图像", DataTypeKind.Image);
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        const int w = 256, h = 256;
        var img = new ImageData(w, h, 4);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                img.SetPixel(x, y, 0, (byte)(x));
                img.SetPixel(x, y, 1, (byte)(y));
                img.SetPixel(x, y, 2, (byte)(255 - (x + y) / 2));
                img.SetPixel(x, y, 3, 255);
            }
        }
        // 绘制一个白色圆
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                double dx = x - 128, dy = y - 128;
                if (dx * dx + dy * dy < 60 * 60)
                {
                    img.SetPixel(x, y, 0, 255);
                    img.SetPixel(x, y, 1, 255);
                    img.SetPixel(x, y, 2, 255);
                }
            }
        }
        ctx.SetOutput("图像", img);
    }
}
