# Sz3D · 3D 视觉节点式编程平台（原型）

图漾 RVS（Robot Vision Suite）风格的 3D 视觉算子节点式编程框架原型。将视觉算子封装为**节点（Node）**，
节点间通过**端口（Port）+ 连线（Connection）**连接，支持**组节点（Group）嵌套**，形成完整项目程序流程。

## 快速开始

```bash
dotnet run --project src/Sz3d.App
```

启动后点击工具栏 **「加载示例」**，即可看到：

- 3D 管线：生成点云 → 体素降采样 → 统计离群点去除（3D 视图按高度着色）
- 2D 管线：生成图像 → 灰度化 → 均值滤波 → 二值化（2D 视图）
- 节点画布、连线、属性面板、日志

## 结构

```
src/
  Sz3d.Core/    # 节点框架与算子（无 WPF 依赖，可复用）
    Data/       # ImageData / PointCloudData / PoseData / Point3 / PointCloudIO
    Nodes/      # NodeBase / Port / Connection / NodeGraph / NodeCatalog / 内置算子
    Execution/  # NodeExecutionContext / GraphExecutor（拓扑排序执行、循环检测）
  Sz3d.App/     # WPF 界面（深色 RVS 风格）
    ViewModels/ Controls/ Themes/ Converters/
docs/
  requirements.md  # 需求与技术方案
```

## 已内置算子

| 分类 | 算子 |
|---|---|
| 输入/输出 | 加载图像（PNG/JPG/BMP）、加载点云（PLY/XYZ/TXT） |
| 2D 处理 | 灰度化、均值滤波、二值化 |
| 3D 处理 | 体素降采样、统计离群点去除 |
| 工具 | 生成图像、生成点云、组（嵌套子图） |

## 扩展一个算子

1. 继承 `Sz3d.Core.Nodes.NodeBase`，在构造函数中用 `AddInput` / `AddOutput` 声明端口；
2. 实现 `Execute(NodeExecutionContext ctx)`，用 `ctx.GetInput<T>("名称")` / `ctx.SetOutput("名称", value)`；
3. 在 `NodeCatalog` 注册，即可出现在左侧节点库中。

## 技术栈

.NET 8 · WPF · HelixToolkit.Wpf 3.1 · 深色主题
