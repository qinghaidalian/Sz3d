using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sz3d.Core.Data;

namespace Sz3d.App.Infrastructure;

public static class ImageUtils
{
    /// <summary>将 Core 的 ImageData 转换为 WPF BitmapSource。</summary>
    public static BitmapSource? ToBitmapSource(ImageData? img)
    {
        if (img == null) return null;

        if (img.Channels == 1)
        {
            return BitmapSource.Create(img.Width, img.Height, 96, 96, PixelFormats.Gray8, null, img.Pixels, img.Width);
        }

        // RGBA → BGRA（WPF Bgra32 字节序）
        var bgra = new byte[img.Pixels.Length];
        int n = img.Width * img.Height;
        for (int i = 0; i < n; i++)
        {
            bgra[i * 4] = img.Pixels[i * 4 + 2];
            bgra[i * 4 + 1] = img.Pixels[i * 4 + 1];
            bgra[i * 4 + 2] = img.Pixels[i * 4];
            bgra[i * 4 + 3] = img.Pixels[i * 4 + 3];
        }
        return BitmapSource.Create(img.Width, img.Height, 96, 96, PixelFormats.Bgra32, null, bgra, img.Width * 4);
    }
}
