# Cadence SPB 25.1 本地文档源地图

## 总览

已扫描到约 `534` 个 HTML/XML/PDF 文档资源。

说明：

- `doc\cdadoc` 是 Doc Assistant 自身文档
- `doc\capPN` 是 Capture / Capture CIS 25.1 release notes
- `doc\cisKPNS` 是 Capture CIS 25.1 已知问题
- `doc\xmlreg` 更像文档注册与产品映射元数据，不是给最终用户直接阅读的正文

## 主要目录

- `cdadoc`
  - Doc Assistant 用户指南
  - 约 50 个文档资源
- `capPN`
  - OrCAD X Capture CIS 25.1 新功能
  - 约 9 个文档资源
- `cisKPNS`
  - Capture CIS 已知问题
  - 约 4 个文档资源
- `algPN`
  - Allegro / APD / PCB 25.1 新功能
  - 约 30 个文档资源
- `pcbInstall`
  - 安装与部署
  - 约 21 个文档资源
- `license`
  - 许可文档
  - 约 16 个文档资源
- `PCBmigration`
  - 迁移说明
  - 约 12 个文档资源
- `pcbsystemreqs`
  - 系统要求
  - 约 5 个文档资源
- `spbrelPN`
  - SPB release-level 变化
  - 约 5 个文档资源
- `xmlreg`
  - 文档注册与产品映射元数据
  - 约 320 个 XML 资源

## 重要但待确认的正文区

这些目录扫描时没有直接统计出 HTML/XML/PDF 数量，但从命名上判断仍然重要，后续要单独验证内容形态：

- `cap_ug`
- `cisug`
- `allegro`
- `consmgr`
- `libManager`
- `orcadx`
- `topxp`
- `specctra`

补充确认：

- `cap_ug` 当前本地仅发现 `cap_ug.tgf`
- `cisug` 当前本地仅发现 `cisug.tgf`
- 这两个 `.tgf` 文件更像帮助标签路由表，而不是正文全文
- 已能从其中识别出部分官方主题名，但当前安装并未找到对应 HTML 正文

## 对当前项目最重要的知识源

优先级 A：

- `cdadoc`
- `capPN`
- `cisKPNS`
- `cap_ug`
- `cisug`

优先级 B：

- `algPN`
- `allegro`
- `consmgr`
- `libManager`

优先级 C：

- `pcbInstall`
- `pcbsystemreqs`
- `PCBmigration`
- `spbrelPN`
- `xmlreg`
