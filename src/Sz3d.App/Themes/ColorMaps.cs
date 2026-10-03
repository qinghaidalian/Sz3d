using System.Windows.Media;
using Sz3d.Core.Data;

namespace Sz3d.App.Themes;

/// <summary>数据类型 / 节点分类 → 颜色映射（RVS 风格深色主题）。</summary>
public static class ColorMaps
{
    public static Brush ForType(DataTypeKind type) => new SolidColorBrush(TypeColor(type));

    public static Color TypeColor(DataTypeKind type) => type switch
    {
        DataTypeKind.String => (Color)ColorConverter.ConvertFromString("#FFE0A458"),
        DataTypeKind.Number => (Color)ColorConverter.ConvertFromString("#FF6FA8DC"),
        DataTypeKind.Bool => (Color)ColorConverter.ConvertFromString("#FFC58FD8"),
        DataTypeKind.Image => (Color)ColorConverter.ConvertFromString("#FF7FC98E"),
        DataTypeKind.PointCloud => (Color)ColorConverter.ConvertFromString("#FFE06C75"),
        DataTypeKind.Pose => (Color)ColorConverter.ConvertFromString("#FFE0C06C"),
        _ => (Color)ColorConverter.ConvertFromString("#FF9A9A9A"),
    };

    public static Brush ForCategory(string category) => new SolidColorBrush(CategoryColor(category));

    public static Color CategoryColor(string category) => category switch
    {
        "输入/输出" => (Color)ColorConverter.ConvertFromString("#FF2F8F83"),
        "2D 处理" => (Color)ColorConverter.ConvertFromString("#FF3A7BD5"),
        "3D 处理" => (Color)ColorConverter.ConvertFromString("#FF7C5CBF"),
        "标定" => (Color)ColorConverter.ConvertFromString("#FF9E6B3A"),
        "机器人" => (Color)ColorConverter.ConvertFromString("#FFC05B3A"),
        "通讯" => (Color)ColorConverter.ConvertFromString("#FF3A9E4F"),
        "深度学习" => (Color)ColorConverter.ConvertFromString("#FFC04FA0"),
        _ => (Color)ColorConverter.ConvertFromString("#FF5F6B7A"),
    };
}
