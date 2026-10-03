namespace Sz3d.App.ViewModels;

/// <summary>节点外观尺寸常量，用于统一布局与端口锚点计算。</summary>
public static class NodeLayout
{
    public const double NodeWidth = 184;
    public const double HeaderHeight = 28;
    public const double PortRowHeight = 24;
    public const double TopPadding = 8;
    public const double BottomPadding = 8;
    public const double PortRadius = 6;

    public static double HeightFor(int portCount)
        => HeaderHeight + TopPadding + BottomPadding + portCount * PortRowHeight;

    /// <summary>端口中心在节点内的纵向偏移。</summary>
    public static double PortCenterY(int index)
        => HeaderHeight + TopPadding + index * PortRowHeight + PortRowHeight / 2.0;
}
