# Cadence SPB 25.1 阅读笔记 14

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\share\pcb\help\AdjustSpacing.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\AddConnect.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\AutoConnect.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\DeleteTab.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\GenerateTab.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\MoveTab.pdf`

## 1. 这一批命令已经形成两个清晰簇

这批 PDF 能很自然分成两组：

### A. Bundle / Auto-routing 辅助

- `AdjustSpacing`
- `AddConnect`
- `AutoConnect`

### B. Tabbed Routing 全流程

- `GenerateTab`
- `MoveTab`
- `DeleteTab`

这很有价值，因为 `share\pcb\help` 不是杂乱无章的零散命令，而是能按工作流聚类。

## 2. Auto-interactive Adjust Spacing

`AdjustSpacing.pdf` 关注的是 bundle 内已布线路由的 spacing 调整。

### 核心能力

- 把 bundle 里的 routed traces 调到用户指定 spacing 或最小 DRC
- 可用于压缩走线以腾空间
- 也可用于扩开 spacing 以满足电气或制造建议

### 真正重要的概念

- 操作由 `Breakout Bar` 和 `Transition line` 控制范围
- 从中心 trace 开始向外调整
- 遇到障碍物时会退化到 no less than minimum DRC
- dynamic shape 会被当作 obstacle

### 关键限制

- 只支持 bundle，不支持 rats / nets / clines 单独选
- trunk routing 中如果有 arc、off-angle 或 uncoupled segments，可能失败
- 没有 plow through dynamic shapes 的选项

### 知识库意义

这是一个典型的“半自动整理空间”命令，适合归入：

- channel cleanup
- bundle compression / spreading

## 3. Add Connect 现在像一个功能容器

`AddConnect.pdf` 不是单一功能说明，而是一份组合帮助文档。

当前可见它至少包含：

- Return Path Vias (Diff Pair)
- Return Path Vias (Single Net)
- Enhanced Contour
- Route Optimization

### 这说明什么

`Add Connect` 已经不只是传统布线入口，而像一个可挂载多个辅助子功能的交互容器。

### 对知识库的启发

后续处理 `share\pcb\help` 时，需要把一些 PDF 当成：

- command family doc

而不是单个命令卡片。`Add Connect` 就是典型例子。

## 4. Auto Connect

`AutoConnect.pdf` 代表的是“低输入成本自动补线”。

### 核心能力

- 自动布 selected rats / clines / bundles
- 用户只需要少量输入
- 可指定 routing layer
- 可选择 rip up existing
- 可选择是否 route through dynamic shapes

### 文档里最值得记住的点

- 不支持自动添加 vias
- 不考虑 electrical requirements，例如 length match
- 在 channel 内支持 nudge existing traces
- Create Flow 里也可作为自动 routing 操作运行

### 对知识库的意义

它非常适合进入：

- quick completion routing
- create-flow compatible helpers

同时也要打上限制标签：

- `no auto-via`
- `no electrical target enforcement`

## 5. Tabbed Routing 流程已经连起来了

这批文档里，`GenerateTab / MoveTab / DeleteTab` 三篇已经构成一个完整闭环。

### Generate Tab

#### 核心能力

- 在 trace segments 上生成 trapezoidal tabs
- 目标是控制 impedance 和 crosstalk

#### 参数模型

- `SWtab`
- `LWtab`
- `Ltab`
- `Ptab`

#### 模式

- `ID1`
- `ID2`
- `PF1`
- `PF2`

#### 关键点

- 重新生成时，已存在 tabs 会先删后建
- `PF2` 模式下 tab 会放在 arc midpoint，可能直接产生 DRC
- 文档明确建议这时结合 `Move Tab` 处理

### Move Tab

#### 核心能力

- 沿 owner segment 移动 tab
- 保持 centerline connectivity
- 提供动态 DRC 反馈

#### 关键限制

- 一次只能选一个 tab
- 只能在 owner segment 范围内移动
- 不能跨 segment

### Delete Tab

#### 核心能力

- 可按 tab instance、cline、cline segment 删除 tabs

#### 使用价值

- 尤其适合 tab count matching
- 也适合快速清除某些局部 segment 上的 tabs

## 6. 对知识库的直接启发

这三篇结合起来说明：

- `share\pcb\help` 里存在完整的小工作流，而不仅是单点命令
- 知识库后续应把它们按流程组织：
  - `Generate -> Tune/Move -> Delete`

而不是各写一张互不相连的卡片

## 7. 本轮共性结论

这一轮最重要的收获是：

- `share\pcb\help` 已经能证明自己不是“零碎附录”，而是一层真实可用的命令工作流知识库
- Auto-routing 辅助命令强调“快速完成”和“局部优化”
- Tabbed Routing 系列则更像“参数化几何控制工具链”

## 8. 对知识库的直接补充

建议新增或强化这些专题：

- `Bundle Compression and Space Recovery`
- `Auto-routing Helpers and Their Limits`
- `Add Connect Command Family`
- `Tabbed Routing Workflow`
- `Parameter-Driven Geometry Utilities`

## 9. 下一步建议

下一批优先继续读：

- `AiAC.pdf`
- `AiCC.pdf`
- `AiDT.pdf`
- `aibt.pdf`
- `aipt.pdf`
- `Timing_Vision.pdf`

这批文件名看起来属于另一组原型或分析命令，适合继续扩展 `share\pcb\help` 的主题图谱。
