# Cadence SPB 25.1 阅读笔记 11

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\license\appe.html`
- `C:\Cadence\SPB_25.1\doc\license\glossary.html`
- `C:\Cadence\SPB_25.1\doc\xmlreg\dtype_release_note.xml`
- `C:\Cadence\SPB_25.1\doc\xmlreg\group_prod_10_release_notes.xml`
- `C:\Cadence\SPB_25.1\doc\spbrelPN\spbrelPNTOC.html`

## 1. license 尾部章节的实际价值

`license/appe.html` 这一章本质上不是方法论文档，而是一个 `Product -> Feature` 对照说明。

它最重要的启发不是里面的旧产品名本身，而是这件事：

- “产品名” 和 “真正 checkout 的 feature 名” 不是天然等价
- 许可排障时，最终还是要落到 feature 层去看

对知识库的意义是：

- 后续应该把“产品视角”和“feature 视角”分开记
- 当用户说“某产品不能用”时，排障仍然要能下钻到 feature mapping

但由于这份映射明显年代久远，它不适合作为 25.1 的现行 feature 对照表，只适合作为“许可语义层”的参考。

## 2. glossary 的价值是统一词汇，不是提供新规则

`license/glossary.html` 没有引入新的流程规则，但它对知识库很有帮助，因为它把一些容易混淆的词拆开了：

- application
- application client
- application file server
- feature
- floating license
- fault-tolerant licensing
- encoded license file
- `cdslmd`

这说明后续知识库应该逐步补一份我们自己的术语表，把：

- Cadence 官方术语
- 我们当前企业库术语
- 实施中常用的口语化表达

对应起来，不然长期维护时会混淆“产品、feature、license、bundle、client、server”这些层。

## 3. xmlreg 的真实角色已经很清楚了

这轮读了 `xmlreg` 里的代表性文件后，可以比较明确地下结论：

- `xmlreg` 不是终端用户帮助正文
- 它是文档注册、分类、分组和外部引用索引层

### 证据

`dtype_release_note.xml` 的作用不是解释功能，而是把多个 release note 文档源挂到一个 `doctype = Release Note` 的集合下，比如：

- `algPN`
- `capPN`
- `pspPN`
- `spbrelPN`
- 其他 release note 文档源

而 `group_prod_10_release_notes.xml` 则更像一个产品组到文档组的映射片段，例如把某个产品组的 release notes 指到 `capPN/cappn.xml`。

### 对知识库的意义

`xmlreg` 很适合作为：

- 文档分类结构参考
- 产品与文档映射参考
- 主题入口自动发现参考

但它不适合作为：

- 功能知识正文
- 实施规则正文
- 用户操作手册正文

也就是说，`xmlreg` 属于知识库的“元数据层”，而不是“内容层”。

## 4. spbrelPN TOC 再次印证版本导航层定位

这轮顺带看 `spbrelPNTOC.html`，进一步确认了：

- `spbrelPN` 的任务就是做版本级导航和全局 framing
- 不负责深入到具体操作细节

这和 `xmlreg` 的结论组合起来很有意义：

- `spbrelPN` 是版本导航层
- `xmlreg` 是索引元数据层
- 真正的正文知识仍然在具体产品目录里

## 5. 本轮共性结论

这一轮最大的意义是把“知识内容”和“知识索引”彻底分开了：

- `license` 尾部章节更适合沉淀概念层和 feature 语义层
- `glossary` 更适合沉淀术语层
- `xmlreg` 应归入知识库元数据层

这会让后面的知识库结构更清晰，不会把所有 XML 都当成“还没读完的正文”。

## 6. 对知识库的直接补充

建议新增或强化这些专题：

- `Product Name vs Feature Name`
- `Cadence Licensing Terminology`
- `Documentation Metadata Layer`
- `Version Navigation Layer vs Content Layer`

## 7. 处理结论

### `license`

- 现阶段可以视为主体处理完成
- 后续只需在需要时回查个别细节
- 其价值主要保留在：
  - 配置 checklist
  - 命令分类
  - 排障顺序
  - feature 语义
  - 术语表

### `xmlreg`

- 可以视为已完成分类处理
- 结论是：它属于“元数据/索引层”，不是正文阅读 backlog
- 后续只在需要扩展自动索引或文档图谱时回查

## 8. 下一步建议

下一批优先处理：

- `allegro`
- `consmgr`
- `libManager`
- `share\pcb\help`

这样 backlog 就会从“安装/迁移/许可/索引层”继续转回“产品正文层”的剩余核心目录。
