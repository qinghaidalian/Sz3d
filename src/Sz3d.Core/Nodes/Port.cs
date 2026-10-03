using Sz3d.Core.Data;

namespace Sz3d.Core.Nodes;

/// <summary>端口基类：节点的输入或输出端点，带数据类型。</summary>
public abstract class Port
{
    /// <summary>端口名称（节点内唯一）。</summary>
    public string Name { get; }

    /// <summary>端口数据类型。</summary>
    public DataTypeKind DataType { get; }

    /// <summary>所属节点。</summary>
    public NodeBase Node { get; internal set; } = null!;

    /// <summary>是否为输入端口。</summary>
    public bool IsInput { get; }

    /// <summary>UI 使用：输入/输出端口在各自集合中的序号。</summary>
    public int Index { get; internal set; }

    protected Port(string name, DataTypeKind dataType, bool isInput)
    {
        Name = name;
        DataType = dataType;
        IsInput = isInput;
    }
}

/// <summary>输入端口：可被连线驱动，未连线时使用默认值作为参数。</summary>
public sealed class InputPort : Port
{
    public object? DefaultValue { get; set; }

    public InputPort(string name, DataTypeKind dataType, object? defaultValue = null)
        : base(name, dataType, isInput: true)
    {
        DefaultValue = defaultValue;
    }
}

/// <summary>输出端口：由节点执行产生数据。</summary>
public sealed class OutputPort : Port
{
    public OutputPort(string name, DataTypeKind dataType)
        : base(name, dataType, isInput: false)
    {
    }
}
