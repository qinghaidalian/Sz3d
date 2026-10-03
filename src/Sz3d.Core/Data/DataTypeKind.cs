namespace Sz3d.Core.Data;

/// <summary>
/// 端口数据类型。用于端口类型校验与可视化路由。
/// </summary>
public enum DataTypeKind
{
    /// <summary>任意类型（弱类型端口）</summary>
    Any,

    /// <summary>字符串</summary>
    String,

    /// <summary>数值（double）</summary>
    Number,

    /// <summary>布尔</summary>
    Bool,

    /// <summary>2D 图像</summary>
    Image,

    /// <summary>3D 点云</summary>
    PointCloud,

    /// <summary>位姿（位置 + 姿态）</summary>
    Pose,
}
