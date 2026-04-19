# Cadence SPB 25.1 阅读笔记 04

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\algPN\Docked_Informational_Panels.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Performance_Improvements.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Pin_Delay_Import_Enhancements.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Region_Support_for_Metal_Fill_Operations.html`

## 1. Docked Informational Panels

### 官方要点

25.1 在 Layout Editor 中增加了两个新的停靠式信息面板：

- `Search Panel`
- `Reports Panel`

### Search Panel

用途：

- 查询数据库对象
- 从 canvas 和 panel 双向 cross-probe
- 快速找对象和信息

特点：

- 按需加载内容，降低内存占用
- 实时更新
- 自定义列过滤
- 支持拖放调整列

### Reports Panel

用途：

- 自动加载 selected Quick Reports
- 不再依赖单独漂浮报表窗口

支持的报告包括：

- Shape Islands Report
- Unassigned Shapes Report
- Missing Teardrops Report
- Missing Tapers Report
- Dangling Traces, Vias, and Antenna Report

### 对知识库的启发

- Cadence 正在把“查看信息”和“修改对象”结合得更紧
- 后续知识库除了记录命令和规则，也要记录：
  - 哪些信息可以直接在 panel 中看
  - 哪些检查可以直接在 UI 内完成

## 2. Performance Improvements

### 官方要点

25.1 的性能增强覆盖很多区域，包括：

- DRCs
- 导入导出
- Placement / Interactive Placement
- Routing / Interactive Routing
- SKILL
- Shapes
- Padstack
- DBDoctor
- Report
- Graphical Display
- 3D View
- In-Design Analysis

### 对我们的启发

- 25.1 不只是功能增加，底层交互性能也明显在改善
- 这对我们后面做企业流程推广有帮助：
  - 更适合把更多检查、搜索、导入、导出留在官方流程里
  - 不必急着把所有事情都外包给外部脚本

## 3. Pin Delay Import Enhancements

### 官方要点

Pin delay 导入功能增强了：

- 自动识别
- 列校验
- 单元格校验
- 对话框内直接调整列类别和值
- 无需回源修改 CSV
- 能查询数据库并标记设计中缺失的 `RefDes + Pin Number` 组合
- 可以通过停用行/列做选择性导入

### 这件事为什么重要

这和我们做企业数据库的导入流程特别像。

Cadence 在这里强调的不是“把 CSV 导进去”本身，而是：

- 导入前校验
- 列语义映射
- 数据质量反馈
- 选择性接受

### 对我们的启发

后续我们自己的 CSV / SQL 导入链路，也应该遵循同样的规则：

- 先识别字段
- 再校验值
- 明确错误位置
- 允许部分选择性导入

这说明我们未来的企业库导入工具，最好不是简单的 `BULK INSERT`，而是：

- staging
- validation
- selective promotion

## 4. Region Support for Metal Fill Operations

### 官方要点

- 25.1 允许为不同 layer 指定自定义 metal fill pattern
- 也允许在同一层的不同 region 上指定不同 pattern
- 设置入口在：
  - `Metal Fill Parameters`
  - `Global Dynamic Shape Parameters`

许可说明：

- 这个功能带有 license 限制
- 需要：
  - `Silicon Layout Option`
  - 或 `Allegro X APD Expert`

### 对我们的启发

这里再次验证了一件事：

- Cadence 25.1 很强调“按区域、按层、按对象上下文”做局部规则
- 这和我们前面看到的 group-level constraint、package family、footprint variant 是同样的治理模式

也就是说，企业知识库未来不应该只是“全局唯一规则库”，而应该允许表达：

- 全局规则
- family 规则
- variant 规则
- region / layer / domain 局部规则

## 5. 这轮的共性结论

这一轮读完后，Cadence 25.1 的一个很清晰的总体方向已经浮出来了：

- 更强调实时可见的信息面板
- 更强调结构化导入与校验
- 更强调按层、按组、按区域管理规则
- 更强调复用配置，而不是每次手工从头做

这和我们企业知识库的目标非常一致。

## 6. 对知识库建设的直接建议

知识库后续建议补充以下主题分支：

- `UI Panels and Review Surfaces`
- `Import and Validation Workflows`
- `Constraint Inheritance and Overrides`
- `Region / Layer / Variant Scoped Rules`

## 7. 下一步建议

- 切到 `pcbInstall`
- 再读 `pcbsystemreqs`
- 然后读 `PCBmigration`

这样我们就能把“功能知识”往“部署与环境边界”再补完整一层。
