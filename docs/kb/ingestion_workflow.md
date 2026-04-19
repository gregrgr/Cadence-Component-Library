# 知识库吸收流程

## 原则

- 不追求一次读完全部正文
- 先建立地图，再做分主题吸收
- 每读完一批，就形成结构化笔记
- 笔记优先服务于实施、排错、配置、库治理

## 吸收步骤

1. 识别一个文档主题组
   - 例如 `capPN` 或 `algPN`
2. 先读 TOC、What’s New、Known Problems
3. 再读与当前任务最相关的正文页
4. 把关键规则写成中文笔记
5. 把和工程落地直接相关的内容提取成操作清单
6. 更新覆盖状态

## 每批输出物

- 一份阅读笔记，例如：
  - `cadence_251_reading_notes_01.md`
- 如果涉及配置或实施：
  - 补充到 `.dbc`、CIS、ODBC、库路径等文档
- 如果涉及版本边界或坑点：
  - 加入“已知问题与规避”知识

## 推荐顺序

1. `cdadoc`
2. `capPN`
3. `cisKPNS`
4. `cap_ug`
5. `cisug`
6. `algPN`
7. `allegro`
8. `pcbInstall`
9. `pcbsystemreqs`
10. `PCBmigration`
11. `spbrelPN`
12. `xmlreg`

## 知识提炼模板

每读一页，优先提炼：

- 这页解决什么问题
- 它给了哪些操作入口
- 它有哪些版本边界或限制
- 哪些内容会影响 `CIS / DBC / ODBC / Footprint / Library`
- 哪些内容适合纳入企业流程
