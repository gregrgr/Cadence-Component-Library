# Cadence SPB 25.1 阅读笔记 16

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\share\pcb\help\Detune.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\Help_Snake.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\route_offset.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\edit_vertex.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\splitviews.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\CreateFlow.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\move.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\New_Slide.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\Allegro_Add_Arc_Prototype.pdf`
- `C:\Cadence\SPB_25.1\share\pcb\help\Allegro_Drafting_Prototypes.pdf`

## 1. 这批文档把补充帮助层基本补齐了

这一轮的 PDF 继续证明，`share\pcb\help` 不是零散附件，而是一层覆盖：

- route cleanup
- route generation
- geometry editing
- view environment
- drafting prototypes

的补充命令知识库。

## 2. Detune

`Detune.pdf` 的定位很清楚：它是把已有 tuning / phase bumps 快速移除掉。

### 核心能力

- 自动识别并删除标准 tuning bumps
- 也能删除部分标准 phase bumps
- 保持 cline 主路径尽量不变

### 真正重要的点

- 它是为了让后续 etch edit、push/shove、更大范围改线更容易进行
- TimingVision 当前模式会决定 Detune 删除 timing bumps 还是 phase bumps
- 它对非标准 bump 的支持有限

## 3. Snake Router

`Help_Snake.pdf` 代表的是一种很具体的 breakout 几何工具。

### 核心能力

- 面向 hexagon-style pitch pattern
- 适合 2 lines between pads 的 breakout
- 用 arcs 生成 clines，因为常规 45 度线段在典型 pitch 下装不进去

### 关键边界

- 优化目标是 circular pad shapes
- line width / line-to-line space 不从约束推导，而要显式输入
- `# Lines Between Pads` 当前只支持 `2`

## 4. Route Connect with Offset

`route_offset.pdf` 的核心是非标准角度布线。

### 核心能力

- 在 Add Connect 中使用 offset angle 路由
- 目标是减少 fiberglass substrate 上的阻抗不连续
- 也适用于 staggered connector breakout 和 tester card 路由

### 关键交互

- `TAB` 在 soft bend / hard turn 间切换
- 可用 function key 在 conventional / offset routing 间切换
- offset angle 范围是 `0` 到 `22.5` 度

## 5. Edit Vertex Snap to 45

`edit_vertex.pdf` 很短，但价值很直接。

### 核心能力

- 把 off-angle 路由在 Edit Vertex 中吸附回 45 度

### 典型场景

- component move 时启用了 stretch etch
- 结果产生了不理想角度
- 需要快速恢复 octolinear 意图

## 6. Split Views

`splitviews.pdf` 代表的是视图工作方式变化。

### 核心能力

- 打开独立于主画布的第二视图
- 支持独立 zoom / pan
- 当前编辑能力有限
- 可与主视图做 swap

## 7. Create Flow

`CreateFlow.pdf` 是本批里层级较高的一篇。

### 核心能力

- 交互式创建 bundle
- 画 guided flow path
- 再自动调用 `AiBT` 或 `Auto Connect`
- 一步完成 flow intent + auto etch creation

### 关键概念

- 持久 bundle
- bundled rats 可脱离旧 bundle 重建 flow
- flow path 可用任意角、45、90
- 创建后可继续配合 bundle-based commands 做 reroute / edit

## 8. Move / New Slide

### Move Component with Slide Etch Option

- `Slide Etch` 比 `Stretch Etch` 更能保持 octolinear 意图
- 适合 attached routes 的移动场景

### New Slide

- 基于 move-intersect
- 编辑更平滑、更局部、更可预测
- 默认就是 enhanced arc mode
- 新增 `Min Corner Size`、`Min Arc Radius`、`Auto Join`

## 9. Add Arc / Drafting Prototypes

### Allegro Add Arc Prototype

- Add Arc 支持多种三点定义法
- 包括 `Start/Center/End`、`Start/End/Radius`、`Center/Start/Length` 等
- 可锁定 angle / length

### Allegro Drafting Prototypes

补出了一批 drafting/edit 原型：

- Extend Segments
- Trim Segments
- Add Parallel Line
- Add Perpendicular Line
- Add Tangent Line

## 10. 本轮共性结论

这一轮之后，`share\pcb\help` 这条帮助层已经形成完整轮廓：

- 查询与预选
- diff pair / return path / weave / tuning
- tabbed routing
- auto-routing / flow intent
- geometry cleanup / slide / vertex
- drafting primitives
- timing visualization
- workspace/view 管理

它已经足够被视为一个独立的“补充命令知识层”。

## 11. 处理结论

### `share\pcb\help`

- 现阶段可以视为主体处理完成
- 后续只需按需回查某篇 PDF 细节
- 它应在知识库中长期保留为：
  - 补充命令层
  - 原型命令层
  - 快速操作卡片层
