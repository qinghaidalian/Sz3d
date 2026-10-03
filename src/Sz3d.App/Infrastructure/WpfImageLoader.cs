using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sz3d.Core.Data;
using Sz3d.Core.Services;

namespace Sz3d.App.Infrastructure;

/// <summary>基于 WPF 的图像解码实现（供 Core 的 IImageLoader 使用）。</summary>
public sealed class WpfImageLoader : IImageLoader
{
    public ImageData? Load(string path)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.EndInit();

            var converted = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            int stride = bmp.PixelWidth * 4;
            var bgra = new byte[stride * bmp.PixelHeight];
            converted.CopyPixels(bgra, stride, 0);

            // BGRA → RGBA
            var rgba = new byte[bgra.Length];
            for (int i = 0; i < bgra.Length; i += 4)
            {
                rgba[i] = bgra[i + 2];
                rgba[i + 1] = bgra[i + 1];
                rgba[i + 2] = bgra[i];
                rgba[i + 3] = bgra[i + 3];
            }
            return new ImageData(bmp.PixelWidth, bmp.PixelHeight, 4, rgba);
        }
        catch
        {
            return null;
        }
    }
}
