using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes.Builtin;

/// <summary>
/// 组节点：内嵌一个子图，通过暴露的边界端口与外部连线，实现节点嵌套成组。
/// 执行组节点 = 将组输入注入子图、执行子图、收集组输出。
/// </summary>
public sealed class GroupNode : NodeBase
{
    public NodeGraph InnerGraph { get; } = new NodeGraph();

    private readonly Dictionary<InputPort, InputPort> _inputMap = new();
    private readonly Dictionary<OutputPort, OutputPort> _outputMap = new();

    public GroupNode(string title = "组")
    {
        Title = title;
        Category = NodeCategories.Utility;
        Description = "由若干子节点组成的组，可嵌套复用。";
    }

    /// <summary>将子图中的某个输入端口暴露为组输入。</summary>
    public InputPort ExposeInput(string name, DataTypeKind type, InputPort inner)
    {
        var p = AddInput(name, type, inner.DefaultValue);
        _inputMap[p] = inner;
        return p;
    }

    /// <summary>将子图中的某个输出端口暴露为组输出。</summary>
    public OutputPort ExposeOutput(string name, DataTypeKind type, OutputPort inner)
    {
        var p = AddOutput(name, type);
        _outputMap[p] = inner;
        return p;
    }

    public override void Execute(NodeExecutionContext ctx)
    {
        var overrides = new Dictionary<InputPort, object?>();
        foreach (var kv in _inputMap)
            overrides[kv.Value] = ctx.GetInput(kv.Key.Name);

        var result = GraphExecutor.Run(InnerGraph, ctx.Services, overrides);
        if (!result.Success)
            throw new InvalidOperationException($"子图执行失败：{result.Error}");

        foreach (var kv in _outputMap)
            ctx.SetOutput(kv.Key.Name, result.Outputs.TryGetValue(kv.Value, out var v) ? v : null);
    }
}
