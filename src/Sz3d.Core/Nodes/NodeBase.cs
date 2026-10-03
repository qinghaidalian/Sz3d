using Sz3d.Core.Data;
using Sz3d.Core.Execution;

namespace Sz3d.Core.Nodes;

/// <summary>
/// 节点基类：所有视觉算子的统一抽象。
/// 子类在构造函数中通过 <see cref="AddInput"/> / <see cref="AddOutput"/> 声明端口，
/// 并在 <see cref="Execute"/> 中实现算子逻辑。
/// </summary>
public abstract class NodeBase
{
    private readonly List<InputPort> _inputs = new();
    private readonly List<OutputPort> _outputs = new();

    /// <summary>节点唯一标识。</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>节点标题（显示在标题栏）。</summary>
    public string Title { get; set; } = "节点";

    /// <summary>节点分类（用于节点库分组与着色）。</summary>
    public string Category { get; set; } = "通用";

    /// <summary>节点功能描述。</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>画布位置（世界坐标，随项目保存）。</summary>
    public double X { get; set; }
    public double Y { get; set; }

    /// <summary>节点是否被启用。</summary>
    public bool Enabled { get; set; } = true;

    public IReadOnlyList<InputPort> Inputs => _inputs;
    public IReadOnlyList<OutputPort> Outputs => _outputs;

    /// <summary>声明一个输入端口。</summary>
    protected InputPort AddInput(string name, DataTypeKind type, object? defaultValue = null)
    {
        var p = new InputPort(name, type, defaultValue) { Node = this, Index = _inputs.Count };
        _inputs.Add(p);
        return p;
    }

    /// <summary>声明一个输出端口。</summary>
    protected OutputPort AddOutput(string name, DataTypeKind type)
    {
        var p = new OutputPort(name, type) { Node = this, Index = _outputs.Count };
        _outputs.Add(p);
        return p;
    }

    /// <summary>执行节点逻辑：从 <see cref="NodeExecutionContext.GetInput"/> 读取输入，写入 <see cref="NodeExecutionContext.SetOutput"/>。</summary>
    public abstract void Execute(NodeExecutionContext ctx);

    /// <summary>克隆节点（新 Id、不复制连线）。供节点库实例化使用。</summary>
    public virtual NodeBase Clone()
    {
        var node = (NodeBase)MemberwiseClone();
        // 端口与 Id 需要全新实例，由子类通过工厂重建更安全；此处仅作浅拷贝兜底。
        return node;
    }

    public override string ToString() => $"{Category} / {Title}";
}
