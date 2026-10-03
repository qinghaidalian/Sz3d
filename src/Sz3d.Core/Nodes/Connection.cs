using Sz3d.Core.Data;

namespace Sz3d.Core.Nodes;

/// <summary>节点之间的连线：输出端口 → 输入端口。</summary>
public sealed class Connection
{
    public OutputPort Source { get; }
    public InputPort Target { get; }

    public Connection(OutputPort source, InputPort target)
    {
        Source = source;
        Target = target;
    }

    /// <summary>校验连线类型是否兼容（Any 与任意类型兼容）。</summary>
    public static bool IsTypeCompatible(DataTypeKind source, DataTypeKind target)
        => source == DataTypeKind.Any || target == DataTypeKind.Any || source == target;

    public override string ToString() => $"{Source.Node.Title}.{Source.Name} → {Target.Node.Title}.{Target.Name}";
}
