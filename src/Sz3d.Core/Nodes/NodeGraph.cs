namespace Sz3d.Core.Nodes;

/// <summary>
/// 图：一个项目/子图，包含节点集合与连线集合。
/// </summary>
public sealed class NodeGraph
{
    public string Name { get; set; } = "项目";

    public List<NodeBase> Nodes { get; } = new();
    public List<Connection> Connections { get; } = new();

    public NodeBase AddNode(NodeBase node)
    {
        Nodes.Add(node);
        return node;
    }

    /// <summary>连接输出端口到输入端口。若输入已被连接则先断开旧连线。</summary>
    public Connection Connect(OutputPort source, InputPort target)
    {
        DisconnectTarget(target);
        var c = new Connection(source, target);
        Connections.Add(c);
        return c;
    }

    /// <summary>断开以该输入端口为目标的连线。</summary>
    public void DisconnectTarget(InputPort target)
    {
        Connections.RemoveAll(c => c.Target == target);
    }

    /// <summary>查找驱动某输入端口的连线。</summary>
    public Connection? FindConnectionTo(InputPort target)
        => Connections.FirstOrDefault(c => c.Target == target);

    /// <summary>移除节点及其所有关联连线。</summary>
    public void RemoveNode(NodeBase node)
    {
        Connections.RemoveAll(c => c.Source.Node == node || c.Target.Node == node);
        Nodes.Remove(node);
    }

    /// <summary>移除连线。</summary>
    public void RemoveConnection(Connection c) => Connections.Remove(c);
}
