using Sz3d.Core.Nodes;

namespace Sz3d.Core.Execution;

/// <summary>
/// 节点执行上下文：向节点提供已解析的输入值、服务，并收集输出值。
/// 由 <see cref="GraphExecutor"/> 创建。
/// </summary>
public sealed class NodeExecutionContext
{
    private readonly Dictionary<string, InputPort> _inputByName;
    private readonly Dictionary<string, OutputPort> _outputByName;
    private readonly Dictionary<InputPort, object?> _inputValues;
    private readonly Dictionary<OutputPort, object?> _outputValues = new();

    /// <summary>可选的服务提供者（如图像解码、相机 SDK 等）。</summary>
    public IServiceProvider? Services { get; }

    internal NodeExecutionContext(NodeBase node, IReadOnlyDictionary<InputPort, object?> inputValues, IServiceProvider? services)
    {
        _inputByName = node.Inputs.ToDictionary(p => p.Name);
        _outputByName = node.Outputs.ToDictionary(p => p.Name);
        _inputValues = inputValues.ToDictionary(kv => kv.Key, kv => kv.Value);
        Services = services;
    }

    public bool HasInput(string name) => _inputByName.ContainsKey(name);

    /// <summary>读取指定名称输入端的值。</summary>
    public object? GetInput(string name)
        => _inputValues.TryGetValue(_inputByName[name], out var v) ? v : null;

    /// <summary>读取强类型输入，支持常见数值/字符串转换。</summary>
    public T? GetInput<T>(string name)
    {
        var v = GetInput(name);
        return Convert<T>(v);
    }

    /// <summary>写入指定名称输出端的值。</summary>
    public void SetOutput(string name, object? value)
        => _outputValues[_outputByName[name]] = value;

    internal object? GetOutput(OutputPort port)
        => _outputValues.TryGetValue(port, out var v) ? v : null;

    /// <summary>获取服务（例如 IImageLoader）。</summary>
    public T? GetService<T>() where T : class
        => Services?.GetService(typeof(T)) as T;

    private static T? Convert<T>(object? v)
    {
        if (v is null) return default;
        if (v is T t) return t;

        try
        {
            if (typeof(T) == typeof(double)) return (T)(object)System.Convert.ToDouble(v);
            if (typeof(T) == typeof(int)) return (T)(object)System.Convert.ToInt32(v);
            if (typeof(T) == typeof(bool)) return (T)(object)System.Convert.ToBoolean(v);
            if (typeof(T) == typeof(string)) return (T)(object)System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)!;
        }
        catch
        {
            // 转换失败时返回默认值
        }
        return default;
    }
}
