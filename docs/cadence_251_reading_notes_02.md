# Cadence SPB 25.1 阅读笔记 02

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\cap_ug\cap_ug.tgf`
- `C:\Cadence\SPB_25.1\doc\cisug\cisug.tgf`
- `C:\Cadence\SPB_25.1\doc\algPN\Allegro_X_PCB_Editor_and_Allegro_X_APD__What_s_New_in_25.1.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Layout_Reuse_with_Copy_and_Paste_Layout.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Single-Click_Export_for_Manufacturing_Output.html`
- `C:\Cadence\SPB_25.1\doc\algPN\SKILL_Enhancements.html`

## 1. `cap_ug` 和 `cisug` 的当前状态

### 1.1 本地安装形态

- `cap_ug` 目录中当前只有 `cap_ug.tgf`
- `cisug` 目录中当前只有 `cisug.tgf`

这说明在这套本地安装里，这两块正文没有以散开的 HTML 文档形式落地出来。

### 1.2 `.tgf` 的作用

- `.tgf` 更像帮助系统的标签路由表
- 它把命令、对话框、工具栏、帮助入口映射到正文路径
- 但当前很多映射都回退到了：
  - `$minDoc/FAQ_for_Accessing_Product_Documentation.html`

这意味着：

- 本地帮助系统知道应该有哪些正文主题
- 但当前安装并没有把完整正文内容都放下来
- 对知识库来说，这两块应标记为：
  - 已发现主题结构
  - 本地正文缺失或未展开

### 1.3 已从 `.tgf` 中识别出的关键主题

#### `cap_ug`

可以识别出一些明显的主题名：

- `Searching_in_Capture.html`
- `Working_with_Footprints.html`
- `Part_Manager_Toolbar.html`
- `Signal_Navigation_in_Capture.html`
- `Capture_Toolbar.html`

#### `cisug`

可以识别出一些明显的主题名：

- `menu_command_descriptions.html`
- `dialog_box_descriptions.html`
- `CIS_Dialog_Box_Descriptions.html`
- `DBC_wizard_mapping_ICA_properties`
- `DBC_wizard_selecting_ICA_properties`
- `DBC_wizard_assigning_activepart_ID`

这些名字对我们很有价值，因为它们说明官方正文确实覆盖了：

- `Part Manager`
- `Footprints`
- `CIS 对话框`
- `DBC Wizard`
- `ICA 属性映射`

即使当前本地正文缺失，也可以把这些主题先纳入知识库索引。

## 2. Allegro X Layout Editors 25.1 首轮要点

### 2.1 25.1 总体方向

Allegro X PCB Editor / APD 25.1 的重点方向包括：

- 约束与属性管理增强
- 布局复用
- 制造输出流程收敛
- 自动化与 SKILL 扩展

### 2.2 Layout Reuse with Copy and Paste Layout

这一项对库复用思路很重要。

官方说明：

- 25.1 引入了 `Copy/Paste Layout`
- 直接替代部分基于文件的旧方法：
  - `clppaste`
  - `place replicate create`
  - `place replicate apply`
- 可以在设计之间直接复制：
  - placement
  - routing
  - vias
  - shapes

关键点：

- 不再依赖中间文件
- 粘贴时会有映射对话框协助迁移
- 包括：
  - Layer Mapping
  - Part Mapping
  - Power and GND Mapping

对我们的启发：

- Cadence 25.1 在后端层面明显在增强“复用”能力
- 这和我们做 `PackageFamily / FootprintVariant / 复用封装` 的方向是一致的
- 后面可以把“版图复用”和“封装资产复用”作为同一套复用哲学来看

### 2.3 Single-Click Export for Manufacturing Output

这个功能很适合企业流程标准化。

官方说明：

- 新的 `Exports` 对话框统一管理制造输出
- 能把 fabrication、assembly、testing 等输出按类别集中配置
- 可以保存并复用导出配置
- 与 artwork 配置共享配置文件体系
- 支持从本地配置文件加载
- 也支持从库路径加载
- 通过 `exportpath` 用户偏好来指向库中的配置目录

对我们的启发：

- 以后不仅 `.dbc`、`.dtp` 可以标准化
- 连制造输出配置也可以企业化沉淀
- 这意味着整个“企业知识库”最终不应只存元器件规则，还应覆盖：
  - 设计模板
  - 输出模板
  - 库路径规范

### 2.4 SKILL Enhancements

25.1 新增和修改了不少 SKILL 能力。

这轮我先记对我们当前项目最值得关注的几类：

- 预选集相关：
  - `axlGetPreselectSet`
  - `axlGetPreselectSetCount`
- 约束/容差相关：
  - `axlSetRKSPCTolerance`
  - `axlGetRKSPCTolerance`
  - `axlDumpDRCModelinXML`
- Pulse / 设计 URL：
  - `axlPulseGetDesignUrl`
- 文本样式与文本对象：
  - `axlDBChangeText`
  - `axlDBChangeTextStyle`
  - `axlDBGetTextStyleCharacteristics`
  - `axlDBGetTextStyle`
- 制造参数导入：
  - `axlImportDegassingParams`
  - `axlImportMetalFillParams`

修改过的重要函数中，值得后续再跟进的有：

- `axlCanvasPrintToPDF`
- `axlDBControl`
- `axlTriggerSet`
- `axlDBCreateText`
- `axlDBCreatePin`
- `cmxlExportFile`
- `cmxlImportFile`
- `cmxlFindOrCreateQBR`

对知识库的启发：

- 后续如果我们要做 Allegro 自动化或库审计，SKILL API 章节值得专门开一个知识分支

## 3. 对当前企业库项目的直接影响

### 3.1 正文可得性风险

- Capture/CIS 的“操作正文”在当前本地安装中并不完整
- 所以知识库要区分：
  - 已有正文的知识
  - 只有主题线索、暂无正文的知识

### 3.2 复用理念正在贯穿前后端

- 前端：
  - `PackageFamily`
  - `FootprintVariant`
  - `DBC / CIS / Part Manager`
- 后端：
  - `Copy/Paste Layout`
  - `Exports` 配置复用
  - `SKILL` 批量自动化

这说明我们做知识库时，不能只把它当“元器件库知识库”，而应该逐步扩成：

- 元器件与属性
- 原理图模板
- PCB 复用
- 制造输出
- 自动化接口

## 4. 下一步建议

- 继续阅读 `algPN` 中与：
  - 属性搜索
  - 约束面板
  - 用户偏好搜索
  - 布局复用
  相关的章节
- 同时把 `cap_ug/cisug` 标记为“本地正文缺失，但主题已识别”
