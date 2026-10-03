using Sz3d.Core.Nodes;

namespace Sz3d.Core.Execution;

/// <summary>
/// 图执行器：对节点做拓扑排序（Kahn 算法）后顺序执行，检测循环依赖。
/// </summary>
public static class GraphExecutor
{
    /// <summary>
    /// 执行整张图。
    /// </summary>
    /// <param name="graph">待执行的图。</param>
    /// <param name="services">可选服务提供者（如 IImageLoader）。</param>
    /// <param name="inputOverrides">可选输入覆盖（供 GroupNode 向子图注入输入）。</param>
    public static ExecutionResult Run(
        NodeGraph graph,
        IServiceProvider? services = null,
        IReadOnlyDictionary<InputPort, object?>? inputOverrides = null)
    {
        var result = new ExecutionResult();

        if (graph.Nodes.Count == 0)
        {
            result.Success = true;
            return result;
        }

        // 1. 拓扑排序
        var indegree = graph.Nodes.ToDictionary(n => n, _ => 0);
        var downstream = new Dictionary<NodeBase, List<NodeBase>>();
        foreach (var n in graph.Nodes) downstream[n] = new List<NodeBase>();

        foreach (var c in graph.Connections)
        {
            if (!indegree.ContainsKey(c.Target.Node) || !indegree.ContainsKey(c.Source.Node)) continue;
            if (c.Source.Node == c.Target.Node) continue; // 忽略自环
            indegree[c.Target.Node]++;
            downstream[c.Source.Node].Add(c.Target.Node);
        }

        var queue = new Queue<NodeBase>(graph.Nodes.Where(n => indegree[n] == 0));
        var order = new List<NodeBase>();
        while (queue.Count > 0)
        {
            var n = queue.Dequeue();
            order.Add(n);
            foreach (var t in downstream[n])
            {
                if (--indegree[t] == 0)
                    queue.Enqueue(t);
            }
        }

        if (order.Count != graph.Nodes.Count)
        {
            result.Error = "图中存在循环依赖，无法执行。";
            return result;
        }

        // 2. 顺序执行
        var outputs = new Dictionary<OutputPort, object?>();
        foreach (var node in order)
        {
            if (!node.Enabled)
            {
                foreach (var op in node.Outputs) outputs[op] = null;
                continue;
            }

            var inputs = new Dictionary<InputPort, object?>();
            foreach (var ip in node.Inputs)
            {
                if (inputOverrides != null && inputOverrides.TryGetValue(ip, out var ov))
                {
                    inputs[ip] = ov;
                    continue;
                }

                var src = graph.FindConnectionTo(ip)?.Source;
                inputs[ip] = src != null && outputs.ContainsKey(src) ? outputs[src] : ip.DefaultValue;
            }

            var ctx = new NodeExecutionContext(node, inputs, services);
            try
            {
                node.Execute(ctx);
            }
            catch (Exception ex)
            {
                result.Error = $"节点「{node.Title}」执行失败：{ex.Message}";
                result.Order = order;
                return result;
            }

            foreach (var op in node.Outputs)
                outputs[op] = ctx.GetOutput(op);
        }

        result.Success = true;
        result.Order = order;
        result.Outputs = outputs;
        return result;
    }
}
