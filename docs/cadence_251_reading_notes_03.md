# Cadence SPB 25.1 阅读笔记 03

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\algPN\Smart_Search_for_Properties_and_User_Preferences.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Constraints_Panel.html`
- `C:\Cadence\SPB_25.1\doc\algPN\Differential_Pair_Automatic_Setup.html`
- `C:\Cadence\SPB_25.1\doc\algPN\OpenType_Fonts_Support.html`

## 1. Smart Search for Properties and User Preferences

### 官方要点

- 25.1 引入新的 `Smart Search`
- 用于在 Layout Editor 里搜索：
  - 属性
  - 用户偏好
- 不要求用户知道精确属性名或 preference 名
- 可以输入：
  - 关键词
  - 或格式化问题
- 系统会给出预测结果

### 适用位置

- `Edit Property`
- `User Preferences Editor`

### 对我们的启发

- Cadence 在 25.1 明显在降低“记住属性名和配置名”的门槛
- 这和我们做企业规则时的一个原则是一致的：
  - 人不应该靠死记硬背字段名来工作
- 对知识库来说，后续可以把：
  - 常用属性名
  - 常用 preference
  - 企业约定的命名
  做成“语义入口”，而不是只存字段字面值

## 2. Constraints Panel

### 2.1 核心价值

- 25.1 新增 Docked Constraint Manager，也就是新的 `Constraints` 面板
- 目标是直接在 layout editor 里更直观地分配和可视化约束
- 强调：
  - 在组级别管理约束
  - 让下层对象继承
  - 避免一上来就对单根 net 做覆盖

### 2.2 分层思路

官方这里的思路和我们元器件库的分层非常像：

- 先在 group level 设约束
- 成员继承
- 单个对象上的 override 会覆盖组规则

这和我们当前做的：

- `PackageFamily`
- `FootprintVariant`
- `CompanyPart`
- `ManufacturerPart`

其实是同一种“先做共性，再做局部例外”的治理思想。

### 2.3 Basic / Advanced 模式

Constraints 面板按 `Basic` 和 `Advanced` 模式组织。

#### Physical

- Basic：
  - Line width
  - Differential pair physical rules
- Advanced：
  - BB Via Stagger
  - Pad-Pad Direct Connect
  - Layer / T-Connection restrictions
  - Diff Pair static/dynamic phase controls

#### Spacing

- Basic：
  - Generic Pad
  - 线间距
  - Shape 间距
- Advanced：
  - 更细的 pin/via/object spacing
  - hole / hole type spacing

#### Electrical

- Basic：
  - Pin use
  - Stub length
  - Max via count
  - Total etch length
- Advanced：
  - Wiring
  - Diff Pair
  - Delay
  - Return Path
  - Impedance

### 2.4 对企业流程的启发

- 约束不应优先散落在单个对象上
- 应优先建立：
  - group
  - class
  - cset
  - differential pair 级别规则
- 这对我们知识库的扩展方向很重要：
  - 后面不仅要存“器件库知识”
  - 还要存“约束库知识”

## 3. Differential Pair Automatic Setup

### 官方要点

- 25.1 新增统一的 `Differential Pair Automatic Setup` 对话框
- 用来生成 differential pairs
- 把之前分散在 Constraint Manager 内外的能力合并到了一个入口

### 启动入口

- `Logic -> Assign Differential Pair`
- 或选 nets 后右键：
  - `Create -> Differential Pair -> Auto Setup`

### 生成流程

1. 选预定义 polarity indicator 或自定义 polarity
2. 指定 differential pair name prefix
3. 看到动态预览
4. 点击 `Create Diff Pairs`

### 对我们的启发

- Cadence 正在把“命名 + 批量结构化生成”做成更统一的流程
- 这对我们企业命名规范很重要
- 后面如果做：
  - Net naming 规范
  - Differential pair naming 规范
  这块可以直接纳入知识库

## 4. OpenType Fonts Support

### 4.1 核心意义

- 25.1 开始支持 OpenType fonts
- 这不只是“字体更好看”
- 它直接关系到：
  - PDF Publisher
  - IPC 2581
  - Artwork
  - DRC/DFM
  - Extracta
  - 3DX Canvas
  - IDX / MCAD
  - SKILL

### 4.2 文本管理方式变化

- 新文本可以直接基于 `Font` 或 `Style`
- `text_controls_options_tab` preference 可控制样式列表显示：
  - Font Styles
  - Text Blocks
  - Both

### 4.3 编辑规则

- `edit text` 现在支持字体 / 字体样式更新
- `change` 命令仍然只支持传统 Text Block 相关修改

这个区别很关键：

- 新样式体系和旧文本块体系并存
- 操作方式不同
- 后面做企业模板时要明确团队用哪一套为主

### 4.4 样式复用

- Style 可在：
  - `Setup -> Design Parameters -> Text`
  中配置
- 可用于：
  - Refdes
  - Assembly layer 标注
  - Board annotations
  等对象
- 定义好后可在多个设计中复用

### 4.5 对我们的启发

- 这进一步证明 Cadence 25.1 在多个层面都在强化“模板”和“可复用配置”
- 我们的企业知识库后面应该增加一类内容：
  - 文本样式规范
  - PDF / Artwork 字体策略
  - 新旧文本体系切换规则

## 5. 对当前知识库建设的直接影响

### 5.1 知识库要分两层

一层是“对象库知识”：

- CIS
- DBC
- 元器件
- Footprint

另一层是“设计系统知识”：

- Constraints
- Preferences
- Text styles
- Output configs
- Layout reuse

### 5.2 企业模板的覆盖范围要扩大

原先我们更偏向：

- 元器件字段模板
- DBC 模板
- ODBC / SQL schema

现在看下来，企业模板还应该逐步覆盖：

- `.dtp` 设计模板
- 导出配置
- 约束分层策略
- 文本风格与输出规范

## 6. 下一步建议

- 继续读 `algPN` 中：
  - `Docked_Informational_Panels`
  - `Performance_Improvements`
  - `Pin_Delay_Import_Enhancements`
  - `Region_Support_for_Metal_Fill_Operations`
- 然后再切回：
  - `pcbInstall`
  - `pcbsystemreqs`
  - `PCBmigration`
