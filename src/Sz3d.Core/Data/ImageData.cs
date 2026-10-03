namespace Sz3d.Core.Data;

/// <summary>
/// 2D 图像数据。以原始像素缓冲保存，与 UI 框架解耦（RGBA 或单通道灰度）。
/// </summary>
public sealed class ImageData
{
    /// <summary>宽度（像素）</summary>
    public int Width { get; }

    /// <summary>高度（像素）</summary>
    public int Height { get; }

    /// <summary>通道数：1 = 灰度，4 = RGBA</summary>
    public int Channels { get; }

    /// <summary>原始像素缓冲，长度 = Width * Height * Channels，行优先。</summary>
    public byte[] Pixels { get; }

    public ImageData(int width, int height, int channels)
    {
        if (channels != 1 && channels != 4)
            throw new ArgumentException("仅支持 1（灰度）或 4（RGBA）通道", nameof(channels));
        Width = width;
        Height = height;
        Channels = channels;
        Pixels = new byte[width * height * channels];
    }

    public ImageData(int width, int height, int channels, byte[] pixels)
    {
        Width = width;
        Height = height;
        Channels = channels;
        Pixels = pixels;
    }

    public int IndexOf(int x, int y, int c) => (y * Width + x) * Channels + c;

    public byte GetPixel(int x, int y, int c) => Pixels[IndexOf(x, y, c)];

    public void SetPixel(int x, int y, int c, byte v) => Pixels[IndexOf(x, y, c)] = v;

    /// <summary>复制一份像素数据。</summary>
    public ImageData Clone()
    {
        var copy = new byte[Pixels.Length];
        Array.Copy(Pixels, copy, Pixels.Length);
        return new ImageData(Width, Height, Channels, copy);
    }

    /// <summary>将任意通道布局转换为 4 通道 RGBA。</summary>
    public ImageData ToRgba()
    {
        if (Channels == 4) return this;
        var rgba = new ImageData(Width, Height, 4);
        for (int i = 0; i < Width * Height; i++)
        {
            byte g = Pixels[i];
            int d = i * 4;
            rgba.Pixels[d] = g;
            rgba.Pixels[d + 1] = g;
            rgba.Pixels[d + 2] = g;
            rgba.Pixels[d + 3] = 255;
        }
        return rgba;
    }
}
