# Cadence SPB 25.1 阅读笔记 01

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\cdadoc\Introduction_to_Doc_Assistant.html`
- `C:\Cadence\SPB_25.1\doc\cdadoc\Accessing_Doc_Assistant.html`
- `C:\Cadence\SPB_25.1\doc\cdadoc\Doc_Assistant_User_Interface.html`
- `C:\Cadence\SPB_25.1\doc\cdadoc\Loading_Document_Libraries.html`
- `C:\Cadence\SPB_25.1\doc\capPN\OrCAD_X_Capture_CIS__What_s_New_in_25.1.html`
- `C:\Cadence\SPB_25.1\doc\capPN\Design_Review_and_Markup_Support_for_Schematic.html`
- `C:\Cadence\SPB_25.1\doc\capPN\Advanced_Find_and_Replace_Properties.html`
- `C:\Cadence\SPB_25.1\doc\capPN\Save_and_Load_Design_Templates_and_Properties.html`
- `C:\Cadence\SPB_25.1\doc\cisKPNS\Known_Problems_and_Solutions_in_OrCAD_X_Capture_CIS_Release_25.1.html`

## 1. 对 `cda.exe` 的重新认识

- `cda.exe` 是 `Doc Assistant 25.10` 启动器，不是文档正文。
- Doc Assistant 是 Cadence 的统一文档查看器，既能看本地安装文档，也能看在线最新文档。
- 它默认以在线模式启动，但可以切换到离线模式。
- 对我们这种要系统阅读本地 25.1 文档的人来说，真正重要的是 `C:\Cadence\SPB_25.1\doc\...` 目录，而不是只盯着 `cda.exe` 本身。

## 2. Doc Assistant 的关键机制

### 2.1 工作模式

- 在线模式：显示与本机已安装产品相关的在线文档。
- 离线模式：显示安装层级下已有的本地文档。

### 2.2 启动方式

- Windows 开始菜单
- 产品 Help 菜单
- 产品 Help 按钮
- Linux 下可用 `CDA`

### 2.3 环境变量

- `CDA_ENABLE=true`
  - 可把 Doc Assistant 设为默认帮助工具。
- `QTWEBENGINE_DISABLE_SANDBOX=1`
  - Windows 下如果从网络盘启动 Doc Assistant，需要这个变量。
- `CDA_WINDOW_SIZE=宽x高`
  - 控制窗口尺寸。
- `CDA_DOC`
  - 可加载多个文档根目录。
- `CDS_SITE`
  - 适合在网络安装下给多用户共用 `help.ini`。

### 2.4 文档库加载

- 可在 `<inst_dir>/tools.<port>/cda/config/help.ini` 配置额外文档库。
- 可通过 `library.lbr` 预定义多个文档库。
- 也可通过 `PATH` 或 `CDA_DOC` 加载多个安装目录的文档树。

### 2.5 UI 结构

- Search Bar：统一搜索在线和离线文档。
- My Products：只显示选中产品的文档。
- My Dashboard：最近访问、书签、下载、设置。
- Hamburger Menu：切换在线/离线、主题、缩放、版本、历史、设置等。

## 3. OrCAD X Capture CIS 25.1 本轮重点

### 3.1 25.1 新功能

本轮读到的 Capture CIS 25.1 新功能主要有：

- `Design Review and Markup Support for Schematic`
- `Advanced Find and Replace Properties`
- `Save and Load Design Templates and Properties`

### 3.2 与我们建库工作最相关的点

#### A. Advanced Find and Replace Properties

- 现在可以在 `Replace` 对话框中按对象和属性做更细粒度替换。
- 可按 `parts / pins / nets / text / class` 等对象查找。
- 支持附加搜索条件，包括对象属性和显示属性。
- 这和我们后面做：
  - 批量属性规范化
  - 数据库件替换
  - 设计中属性清理
  - 统一 `MANUFACTURER / MPN / PCB_FOOTPRINT`
  有直接关系。

#### B. Save and Load Design Templates and Properties

- 设计模板设置现在可以保存为 `.dtp` 文件。
- 后续可以加载复用。
- 这意味着我们可以把企业属性规范、页面设置、默认设计属性做成可复用模板，而不是每个项目手配。

#### C. Design Review and Markup Support for Schematic

- 可以在原理图上做评论和标注。
- 支持矩形、箭头和文字评论。
- 审阅者可以回看并回复。
- 这对 Librarian 和 Designer 的协作很有价值，尤其适合：
  - 新器件审核
  - 符号/封装问题回批
  - 原理图替代料评审

## 4. Capture CIS 25.1 已知问题

本轮读到的已知问题，最值得记住的是：

- `Find and Replace` 在同一会话中可能残留 CM 设计过滤器。
- 某些场景下切换设计视图后，`Copy to Replace` 需要刷新后才恢复。
- 非 root schematic 页面上 `Find Similar` 有限制。
- 某些属性即使被选中替换，也可能由于排除配置而不生效。
- 官方多次提到可通过 `CTRL+H` 刷新 `Find and Replace` 对话框。

这意味着：

- 后面我们如果用 25.1 做批量属性修正，要把“刷新对话框”和“会话状态污染”纳入操作规范。
- 不适合把 `Find and Replace` 当作完全无副作用的批量治理工具。

## 5. 对当前 CIS/库项目的直接启发

### 5.1 Doc Assistant 部分

- 我们可以继续完全基于本地文档推进，不需要只依赖外部网页。
- 后面如果要跨版本对比，可以通过 Doc Assistant 的多文档库机制读取其它 release。

### 5.2 Capture CIS 部分

- `.dbc`、实例属性、替代料流程仍然是主线。
- 25.1 的高级属性替换适合做设计内治理，但要注意已知问题。
- `.dtp` 模板值得纳入企业落地方案，后续可作为“原理图企业模板”。

### 5.3 审核协作部分

- 25.1 自带的评论/标注能力，可以纳入器件审核流程，不必全部依赖外部表单或邮件。

## 6. 下一批建议阅读

- `C:\Cadence\SPB_25.1\doc\cdadoc\Doc_Assistant_Settings.html`
- `C:\Cadence\SPB_25.1\doc\cdadoc\Adding_Documents_from_Other_Releases.html`
- `C:\Cadence\SPB_25.1\doc\cdadoc\Doc_Assistant_Network_Modes.html`
- `C:\Cadence\SPB_25.1\doc\cap_ug\Finding_and_Replacing_Properties.html`
- `C:\Cadence\SPB_25.1\doc\cap_ug\Reviewing_Designs_using_Comments_and_Markups_in_OrCAD_X_Capture.html`
