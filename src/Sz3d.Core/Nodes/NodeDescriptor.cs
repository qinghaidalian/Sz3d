namespace Sz3d.Core.Nodes;

/// <summary>节点描述符：节点库中可拖入画布的算子模板。</summary>
public sealed class NodeDescriptor
{
    public string TypeName { get; }
    public string Title { get; }
    public string Category { get; }
    public string Description { get; }
    public Func<NodeBase> Factory { get; }

    public NodeDescriptor(string typeName, string title, string category, string description, Func<NodeBase> factory)
    {
        TypeName = typeName;
        Title = title;
        Category = category;
        Description = description;
        Factory = factory;
    }

    public NodeBase Create() => Factory();

    public override string ToString() => Title;
}
