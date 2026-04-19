# Cadence SPB 25.1 阅读笔记 10

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\spbrelPN\spbrelPNTOC.html`
- `C:\Cadence\SPB_25.1\doc\license\chap2.html`
- `C:\Cadence\SPB_25.1\doc\license\appc_old.html`
- `C:\Cadence\SPB_25.1\doc\license\appd.html`

## 1. spbrelPN 的剩余信息其实在结构上

`spbrelPNTOC.html` 本身没有新增很多正文知识，但它把 `spbrelPN` 的定位进一步坐实了：

- 它不是某个单产品说明书
- 它是整个 `OrCAD X / Allegro X` 25.1 的版本级总入口
- 适合承载：
  - release highlights
  - cross-product changes
  - performance framing
  - documentation access strategy
  - discontinued boundary

对知识库来说，这意味着 `spbrelPN` 更像“版本导航层”，不是实施细节层。后续查功能细节，仍然要回到 `capPN`、`algPN`、`pcbInstall`、`PCBmigration` 等具体文档。

## 2. 老许可配置文档里仍然值得保留的框架

`license/chap2.html` 的内容虽然年代很老，但它提供了一套很稳的配置 checklist 思路。

### 许可配置本来就是一个清单问题

文档反复强调，配置许可不是“填一个路径”这么简单，而是要先确认：

- license file 在哪里
- 是单服务器还是三机容错
- license file 是否有效
- hostid 是否匹配
- 端口是否可用
- 谁负责 license admin
- 工作站和客户端如何发现 license

### 对我们当前知识库的价值

今天即使工具和界面变了，这个 checklist 框架依然完全适用。知识库后续可以把现代 Cadence 25.1 的许可配置也整理成同样的分层清单：

- server-side facts
- file validity
- host identity
- network/port
- client discovery
- admin ownership

这会比堆一堆截图更适合现场使用。

## 3. 许可命令文档最有价值的是“命令分层”

`license/appc_old.html` 的内容不算长，但它把许可命令分得很清楚。

### 哪些工具是观察型

- `lmstat`
- `lmdiag`
- `lic_error`
- `lmhostid`
- `cdsIdent`
- `lmver`

这些更适合放进排障流程前半段，用来观察和确认状态。

### 哪些工具是变更型

- `lmdown`
- `lmreread`
- `lmremove`
- `configure`
- `lic_config`
- `mkclients`

这些工具会影响服务或配置，所以文档也特别提醒只有 license administrator 才该运行某些命令。

### 对知识库的启发

后续我们可以把许可命令分成三类来记录：

- `inspect`
- `maintain`
- `mutate`

这样更利于做 runbook，也能减少误操作。

## 4. 详细排障文档的真正价值

`license/appd.html` 是一份很典型的“虽然老，但排障思路仍然非常正确”的文档。

### 它给出的排障顺序非常好

文档建议的顺序大致是：

1. 先看错误指示本身
2. 用 `lic_error` 扩展解释
3. 用 `lmstat` 看 server / daemon 状态
4. 看 debug log
5. 检查是否混入多 vendor 的 FLEXlm license file

这个顺序非常适合继承到现代知识库中，因为它是一个从“现象 -> server 状态 -> log -> 文件结构”的分层收敛过程。

### 常见问题模式

文档重点聚焦在：

- 找不到 log
- daemon 没起来
- hostid 不匹配
- 地址已占用
- daemon 启动失败

这些问题虽然来自旧环境，但本质上仍然是今天许可故障里最常见的一类。

### 需要明确标记的年代边界

这份文档里很多路径、平台名和链接方式已经过时，例如：

- 老 UNIX 路径假设
- 某些平台专用目录名
- 老式工具链组织结构

所以它最适合作为：

- `troubleshooting logic`
- `error pattern taxonomy`

而不适合作为 25.1 的直接操作步骤。

## 5. 本轮共性结论

这一轮最大的收获不是新的产品功能，而是把知识库的“方法层”又补强了一层：

- `spbrelPN` 是版本导航层
- `chap2` 提供配置 checklist 方法
- `appc_old` 提供命令分类方法
- `appd` 提供排障顺序方法

也就是说，知识库正在逐渐从“读过哪些文档”变成“掌握了哪些稳定的方法论”。

## 6. 对知识库的直接补充

建议新增或强化这些专题：

- `Version Navigation Layer`
- `License Configuration Checklist`
- `License Command Taxonomy`
- `License Troubleshooting Flow`
- `Legacy Guidance vs Current Procedure`

## 7. 下一步建议

下一批优先继续读：

- `appe.html` 这类剩余许可章节
- `xmlreg` 中和产品、文档、分类相关的元数据结构
- 如果还要继续扩知识图，再回头系统梳理 `master_index / source_map / topic taxonomy`

这样知识库会继续从“内容库”走向“知识方法库”。
