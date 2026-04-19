# Cadence SPB 25.1 阅读笔记 15

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\share\pcb\help\AiAC.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\AiCC.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\AiDT.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\aibt.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\aipt.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\Timing_Vision.pdf`

## 1. 这批文档把帮助层从“命令卡片”推进到了“交互环境”

上一批主要是具体命令，这一批已经出现三种不同层级：

- 路径生成类命令
- 调优类命令
- 图形环境类工具

这说明 `share\pcb\help` 不只是补充按钮说明，它还在承载一部分高速布线与调优工作流。

## 2. Add Connect Scribble Prototype

`AiAC.pdf` 讲的是 `Add Connect` 里的 scribble 路由模式。

### 核心能力

- 通过两次 pick 或简单 scribble path 生成复杂 route path
- 支持在复杂 package / connector breakout 中快速给出解
- 使用局部 push / shove，而不是大范围扰动

### 真正的价值

- 能处理 pinch point 需要的 off-angle 走线
- 解决方案尽量留在 scribble 原有 channel 内
- 只在必要时移动已有 clines

### 对知识库的启发

这类命令适合归入：

- `path-guided routing`
- `localized shove routing`

因为它不是全自动 router，而是“用户给路径意图，系统帮你落地”。

## 3. Auto Interactive Convert Corner

`AiCC.pdf` 非常像一个微调命令。

### 核心能力

- 把已有 route corner 转成：
  - arc
  - 45 degree
  - 90 degree

### 关键参数

- `Convert Type`
- `Allow in cns areas`
- `Preferred Radius Size`
- `Min Radius`
- `Preferred Corner Size`
- `Min Corner Size`
- `Allow DRCs`

### 关键点

- 支持 nets / clines / segments
- 失败 corner 会写到 viewlog

### 对知识库的启发

这是典型的：

- geometry cleanup / corner normalization

后续可以和 `route_offset`、`edit_vertex` 这类命令一起归类。

## 4. Auto Interactive Delay Tune

`AiDT.pdf` 已经进入正式高速约束调优范畴。

### 核心能力

- 对已有 clines 或 segments 生成 tuning pattern
- 自动计算所需长度以满足 timing constraints
- 用受控 push / shove 方式加 pattern

### 模式

- `Accordion`
- `Trombone`

### 关键参数

- `Gap`
- `Min/Max Amplitude`
- `Miter Size`
- `Max Levels`

### 文档里最关键的点

- AiDT 在开始生成前可能经历 load / analysis step
- 这个准备阶段会受 session/design/edit history 影响
- 它不是简单图形命令，而是先分析再生成

### 对知识库的意义

AiDT 应归入：

- `constraint-driven delay tuning`
- `analysis-before-edit commands`

## 5. Route Optimization in AiBT

`aibt.pdf` 当前看到的核心主题是 `Route Optimization`。

### 核心能力

- 在 `Add Connect` 和 `AiBT` 中，把 routes 自动居中到 channel 内
- 目标是提高制造良率和电气性能
- 尽量增大 pad-to-trace spacing，并减少不必要 jog

### 控制方式

- 通过 `optimize_in_channel` 环境偏好变量
- 值大于 0 时启用
- 值越小，channel 定义越明确，居中结果越明显
- 值为 0 时关闭

### 对知识库的启发

这类能力更像“routing behavior preference”，不是一次性命令。

它适合进入：

- `environment-driven routing behavior`
- `manufacturing/electrical optimization preferences`

## 6. Auto-Interactive Phase Tuning

`aipt.pdf` 和 `AiDT` 形成天然互补。

### 核心能力

- 只针对 differential pair 的正负两半
- 解决 `Static Phase` 和 `Dynamic Phase`

### 关键概念

- `Static Phase`：正负半边总长差
- `Dynamic Phase`：转角时内外侧路径不等长导致的相位失衡

### 命令定位

它不是一般 delay tuning，而是：

- `differential-pair-specific phase tuning`

### 对知识库的意义

这和 `AiDT` 需要明确区分：

- `AiDT` 处理一般 delay constraints
- `AiPT` 处理 differential pair phase problem

这个区分非常适合放进“高速调优工具对照表”。

## 7. Timing Vision

`Timing_Vision.pdf` 是这批里层级最高的一篇，因为它不是单个动作命令，而是环境。

### 核心能力

- 在 routing canvas 上实时显示 Delay 和 Phase 信息
- 通过颜色、stipple pattern、定制 data tip 帮助用户理解 timing problem
- 不改变物理 routing
- 不永久覆盖用户已有的颜色设置

### 文档里的关键定位

它是支持这些工具的图形环境：

- `AiDT`
- `AiPT`

### 许可要求

- 需要 `High Speed Option License`

### 对知识库的启发

Timing Vision 应归入：

- `visual analysis environment`
- `timing-debug canvas layer`

它不是“调优命令”，而是“帮助调优命令理解问题的可视化环境”。

## 8. 本轮共性结论

这一轮最大的收获是，`share\pcb\help` 已经开始展现出较完整的高速设计工具链结构：

- 路径生成：`AiAC`
- 几何微调：`AiCC`
- 一般延时调优：`AiDT`
- 差分相位调优：`AiPT`
- 通道内优化：`AiBT/Route Optimization`
- 实时可视化环境：`Timing Vision`

这已经不是零散命令帮助，而是一套可以支持高速 routing / tuning 的辅助知识层。

## 9. 对知识库的直接补充

建议新增或强化这些专题：

- `Path-Guided Routing and Scribble Modes`
- `Geometry Cleanup and Corner Conversion`
- `Delay Tuning vs Phase Tuning`
- `Routing Behavior Preferences`
- `Timing Visualization Environment`
- `High-Speed Helper Toolchain`

## 10. 下一步建议

下一批优先继续看：

- `Detune.pdf`
- `Help_Snake.pdf`
- `route_offset.pdf`
- `edit_vertex.pdf`
- `splitviews.pdf`
- `CreateFlow.pdf`

这样可以继续把“几何编辑 / 调优 / 可视化 / 流程编排”这几层补齐。
