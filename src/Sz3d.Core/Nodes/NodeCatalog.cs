using Sz3d.Core.Nodes.Builtin;

namespace Sz3d.Core.Nodes;

/// <summary>节点目录：注册全部可用算子，供节点库面板与后续序列化读取。</summary>
public static class NodeCatalog
{
    public static readonly IReadOnlyList<NodeDescriptor> All = Build();

    private static List<NodeDescriptor> Build() => new()
    {
        new("LoadImage", "加载图像", NodeCategories.IO, "从文件加载 2D 图像", () => new LoadImageNode()),
        new("LoadPointCloud", "加载点云", NodeCategories.IO, "从文件加载 3D 点云", () => new LoadPointCloudNode()),
        new("GenerateImage", "生成图像", NodeCategories.Utility, "生成演示用 2D 图像", () => new GenerateImageNode()),
        new("GeneratePointCloud", "生成点云", NodeCategories.Utility, "生成演示用 3D 点云", () => new GeneratePointCloudNode()),
        new("Grayscale", "灰度化", NodeCategories.Processing2D, "RGBA → 灰度", () => new GrayscaleNode()),
        new("Blur", "均值滤波", NodeCategories.Processing2D, "盒式均值模糊", () => new BlurNode()),
        new("Threshold", "二值化", NodeCategories.Processing2D, "阈值分割", () => new ThresholdNode()),
        new("VoxelDownsample", "体素降采样", NodeCategories.Processing3D, "体素网格均值下采样", () => new VoxelDownsampleNode()),
        new("StatisticalOutlierRemoval", "统计离群点去除", NodeCategories.Processing3D, "基于 k 近邻的离群点滤波", () => new StatisticalOutlierRemovalNode()),
        new("Group", "组", NodeCategories.Utility, "空组节点，可将子图拖入嵌套", () => new GroupNode()),
    };

    public static NodeDescriptor? Find(string typeName) => All.FirstOrDefault(d => d.TypeName == typeName);
}
