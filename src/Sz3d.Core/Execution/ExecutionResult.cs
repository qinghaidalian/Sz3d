using Sz3d.Core.Nodes;

namespace Sz3d.Core.Execution;

/// <summary>图执行结果。</summary>
public sealed class ExecutionResult
{
    public bool Success { get; internal set; }
    public string? Error { get; internal set; }

    /// <summary>拓扑执行顺序。</summary>
    public List<NodeBase> Order { get; internal set; } = new();

    /// <summary>执行后每个输出端口的取值。</summary>
    public Dictionary<OutputPort, object?> Outputs { get; internal set; } = new();
}
