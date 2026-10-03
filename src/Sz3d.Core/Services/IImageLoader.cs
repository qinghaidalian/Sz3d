using Sz3d.Core.Data;

namespace Sz3d.Core.Services;

/// <summary>
/// 2D 图像加载服务抽象。Core 不依赖 WPF，由宿主（WPF App）实现解码逻辑。
/// </summary>
public interface IImageLoader
{
    /// <summary>从文件加载图像；失败返回 null。</summary>
    ImageData? Load(string path);
}
