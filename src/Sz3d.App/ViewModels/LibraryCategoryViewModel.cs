using Sz3d.Core.Nodes;

namespace Sz3d.App.ViewModels;

/// <summary>节点库分类（含该分类下的算子模板）。</summary>
public sealed class LibraryCategoryViewModel
{
    public string Name { get; }
    public IReadOnlyList<NodeDescriptor> Items { get; }

    public LibraryCategoryViewModel(string name, IReadOnlyList<NodeDescriptor> items)
    {
        Name = name;
        Items = items;
    }
}
