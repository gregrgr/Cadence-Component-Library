# Cadence SPB 25.1 阅读笔记 13

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\share\pcb\help\Allegro_Find_by_Query.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\Help_Resize_Respace_Diff_Pairs.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\AutoSwapPins.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\ReturnPathVias.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\AnalyzeTab.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\FiberWeaveAddZigzag.pdf`

## 1. 这批补充 PDF 的共同风格

这批 `share\pcb\help` 文档已经能看出非常稳定的写法：

- 篇幅不长
- 强调 `Summary / Command / Procedure / Key Concepts`
- 更像命令说明卡，而不是系统手册
- 主要服务于某个具体命令或原型功能

所以这条帮助层最适合的吸收方法是：

- 按命令主题做专题汇总
- 提炼“适用场景、入口、关键参数、风险提示”
- 不需要逐页复述

## 2. Find by Query

`Allegro_Find_by_Query.pdf` 说明了一个典型的对象查询与预选流程。

### 核心能力

- 允许按对象类型选范围
- 按多个条件建立过滤规则
- 把匹配对象加入 preselect buffer
- 之后可以在画布上继续用 RMB 调出相关命令

### 真正有价值的点

- 它不只是“搜索”，而是“搜索 -> 候选集 -> 预选 -> 操作”
- 支持 `AND / OR / NOR`
- 支持保存和加载 query
- 能限制到当前视图内对象

### 对知识库的启发

这类功能非常适合纳入：

- `query-driven editing`
- `selection workflows`

因为它本质上是在解决“大设计里如何把对象先筛出来再操作”的问题。

## 3. Resize / Respace Diff Pairs

`Help_Resize_Respace_Diff_Pairs.pdf` 非常像一份工程风险提示文档。

### 核心能力

- 修改已布好的 diff pair 的线宽和间距
- 适用于约束变化已经发生，但布线已存在的场景

### 关键警告

文档明确提醒：

- 邻近元素在处理时会被忽略
- 运行后很可能出现 DRC
- 约束本身不会自动被修改
- 必须先执行命令，再更新约束
- 如果约束还没同步，不要立刻再次对同一对象运行命令

### 对知识库的意义

这类文档很适合进入“操作前警告库”，因为它不是单纯告诉你按钮在哪，而是在告诉你：

- 什么时候能用
- 用完会留下什么后果
- 正确顺序是什么

## 4. Auto-interactive Swap Pins Closest End

`AutoSwapPins.pdf` 讲的是自动引脚交换来减少 bundle breakout 交叉。

### 核心能力

- 基于现有 swap codes
- 自动交换 eligible bundle member pins
- 目标是减少 sequenced rat rakes crossing
- 只支持 sequenced bundle

### 关键前提

- 必须先生成 sequence
- closest end 由光标位置决定
- 多层 bundle 不会通过改层来消交叉，只在同层逻辑内处理

### 对知识库的启发

这是一个很典型的“命令入口看起来简单，但前提条件很多”的功能，后续知识库可以把这类命令统一标注：

- prerequisites
- scope limits
- what it will not do

## 5. Return Path Vias

`ReturnPathVias.pdf` 的信息量很高，而且和高速设计直接相关。

### 核心能力

- 在 diff pair `Add Connect` 过程中，交互式添加 return path vias
- 提供一组常用 pattern
- 并支持在 slide 时让 diff pair vias 与 return path vias 保持联动

### 关键参数

- Return Path Net
- Return Path Via / padstack / via structure
- spacing
- angle

### 模式类型

文档中可见的 pattern 包括：

- `1 Via`
- `In line`
- `Equidistant`
- `Offset`
- `Diamond`
- `Rectangular`

### 对知识库的启发

这类文档特别适合被提炼成：

- 模式库
- 参数字典
- 高速布线辅助命令清单

因为它不仅有“怎么用”，还有“有哪些常见几何模式”。

## 6. Analyze Tab

`AnalyzeTab.pdf` 代表的是一种“分析表单型命令”。

### 核心能力

- 辅助 tab count matching
- 辅助 pitch validation
- 能生成自定义 tab count / pitch report

### 文档结构特点

它的目录明显偏“表单工作流”：

- add rule
- edit rule
- add nets
- analyze within diff pair / net group
- exclude nets
- filter / sort
- output report

### 对知识库的意义

这说明 `share\pcb\help` 里不仅有单个动作命令，也有“轻量分析工作台型功能”。后续应把它们单独归类，不要和简单 RMB 命令混在一起。

## 7. Fiber Weave Off-Angle Routing

`FiberWeaveAddZigzag.pdf` 讲的是通过 zigzag / off-angle route 降低 fiberglass weave 对高速信号的影响。

### 核心能力

- 把正交平行走线改成 zigzag
- 支持 differential pair 和 single-ended
- 可指定 angle offset 和 max length
- 可定义 full segment 或用户指定起止点

### 关键点

- 这是典型的 SI-aware routing adjustment
- 不只是几何变形，而是为了降低介质织构带来的系统性风险

### 对知识库的启发

这类命令应归入：

- signal-integrity aware editing
- post-route optimization helpers

## 8. 本轮共性结论

这一轮已经能说明 `share\pcb\help` 的核心价值：

- 它不是主手册
- 它是“命令说明与快速操作专题库”
- 特别适合沉淀那些：
  - 尚未进正式手册的功能
  - Unsupported Prototypes
  - 高速设计辅助命令
  - 交互式分析表单

## 9. 对知识库的直接补充

建议新增或强化这些专题：

- `Query and Preselection Workflows`
- `High-Speed Routing Helper Commands`
- `Risky Commands with Required Order of Operations`
- `Pattern-Based Via / Routing Utilities`
- `Supplemental Command Cards`

## 10. 下一步建议

下一批继续优先看：

- `AdjustSpacing.pdf`
- `AddConnect.pdf`
- `AutoConnect.pdf`
- `DeleteTab.pdf`
- `GenerateTab.pdf`
- `MoveTab.pdf`

因为这几篇和当前这批在主题上能形成较完整的命令簇。
