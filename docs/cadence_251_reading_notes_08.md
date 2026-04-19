# Cadence SPB 25.1 阅读笔记 08

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\PCBmigration\Allegro_X_Platform_Core_Back-End_Products.html`
- `C:\Cadence\SPB_25.1\doc\PCBmigration\Allegro_X_Pulse_and_Allegro_EDM.html`
- `C:\Cadence\SPB_25.1\doc\PCBmigration\Defining_Custom_Java_Version.html`
- `C:\Cadence\SPB_25.1\doc\license\appa.html`

## 1. Back-End 设计数据库的真正边界

`Allegro_X_Platform_Core_Back-End_Products.html` 把 25.1 在 back-end 侧最重要的边界说得很清楚。

### 数据库升级是单向主线

- 17.2、17.4、22.1、23.1、24.1 的设计都可以在 25.1 打开
- 但设计一旦在 25.1 保存，就不能再直接回到更早版本打开

这意味着：

- “能打开旧设计”不等于“可以来回双向编辑”
- 25.1 对 back-end 数据库来说，本质上是一次 uprev

### downrev 不是无限制回退

文档说明可以通过：

- `downrev` 命令
- `File -> Export -> Downrev Design`

把数据库导出到较早版本，但只支持导出到 `23.1` 或 `24.1`。如果要回更老版本，还需要借助 23.1 作为中间跳板。

### 16.6 是特别边界

- `16.6` 设计不能直接在 `25.1` 打开
- 需要先用 `DB Doctor` 或 `Physical Design Database Health Monitor` 升级

这说明知识库后续应该把版本迁移分成：

- 24.1 -> 25.1 常规迁移
- 23.1 -> 25.1 可控迁移
- 16.6 -> 25.1 特殊升级迁移

## 2. Back-End 库兼容边界

这篇文档里还有一个对企业库很关键的结论：

- `17.2` 及之后创建的 Allegro libraries 可以直接在 `25.1` 用
- `25.1` 保存的 library symbols 和 padstacks 会尽可能保存为 `24.1` 可兼容的最低数据库版本

这件事非常重要，因为它说明 Cadence 在库资产上比设计数据库更保守，尽量维持前后兼容。

但同时文档也给出硬边界：

- `16.6` 的 Allegro libraries 不能直接给 `25.1` 用
- 仍然要先经过 `DB Doctor` uprev

### 对我们当前项目的意义

这和我们做 `PackageFamily / FootprintVariant` 的企业库模型是相吻合的：

- 共享库资产要尽量稳定
- 库版本升级要比设计数据库升级更可控
- 老古董库必须单独走升级流程，不能混入正式发布库

## 3. Router、Viewer、3D 的平台现实

文档还透露了几个很“现场”的事实。

### Allegro PCB Router

- 仍然有 32-bit 依赖
- 在 `RHEL 8` 上需要额外安装一批 32-bit libraries
- 如果这些库没装，Router 和相关命令就跑不起来

这提醒我们：

- 不是所有 25.1 组件都完全“现代化无历史包袱”
- Linux 环境部署一定要保留组件级依赖清单

### Viewer 与 3D

- `Allegro Classic Free Viewer` 在 25.1 里仍可从标准安装中访问
- 但 standalone 安装方式不再支持
- APD 侧旧 `3D Viewer` 已被 `3DX Canvas` 取代

这说明 25.1 的总体策略是：

- 保留部分过渡能力
- 但明确推动用户迁到新的查看和 3D 平台

## 4. Pulse / EDM 集群迁移的主线

`Allegro_X_Pulse_and_Allegro_EDM.html` 是目前读到的迁移文档里信息量最大的一篇之一。

### 个体模式 vs 协作模式

- 如果 `System Capture` 只在 individual mode 下使用，本地从 `24.1 -> 25.1` 不需要额外手工迁移
- 但必须先关闭旧版本 local Pulse instance

这给了我们一个很重要的分流：

- 单人模式迁移简单
- 团队协作模式迁移本质上是服务器迁移

### Pulse cluster 迁移不是单节点升级

协作模式下，管理员需要迁移 `Pulse server cluster`。文档特别强调：

- 最好在计划停机窗口中执行
- 从 `23.1 -> 25.1` 时，Pulse 会抽取 System Capture 设计数据用于索引
- 这一步可能耗时较长
- 抽取期间，新提交设计的 PDF 在 `Version Control` 对话框里可能暂时不可用

这意味着迁移不仅影响管理员，也会短时影响设计师的可见性体验。

### 迁移动作的本质

文档中的关键动作模式包括：

- 停旧 primary node
- 安装 25.1
- 若是 service 安装，按同目录或异目录两种方式处理 service
- 启动 25.1 primary node
- 在 Pulse Service Manager 中执行版本迁移确认
- 复制旧版 `Conf Root`
- 修订自定义 flows 和 scripts 中的版本路径
- 编辑 `<startworkbench>.bat`
- 运行 `adw_uprev`
- 必要时处理新的 `library distribution access key`

### 对知识库的启发

Pulse / EDM 迁移至少要拆成四个子专题：

- service lifecycle
- config root / custom flows carry-over
- schema uprev
- access key / distribution security

## 5. Custom Java 的现实意义

`Defining_Custom_Java_Version.html` 虽然篇幅不长，但很关键。

### 25.1 的 Java 现实

- Cadence 安装默认自带 `Java 11 JRE`
- 但部分 Pulse services 仍依赖旧版本
- 为了兼容，Cadence允许你同时定义：
  - `CDS_JAVA_HOME` -> Java 11
  - `CDS_JAVA8_HOME` -> Java 8

### 这说明什么

这说明 25.1 的服务栈并不是完全统一到单一 Java 版本，而是存在过渡层。

对运维和知识库来说，这意味着：

- Java 版本本身就是迁移边界
- 环境变量是服务启动链的一部分
- “能启动安装包”不等于“所有 Pulse 服务都能正常跑”

文档还提到可以通过 `tools/bin` 下的清理脚本删除安装目录中默认自带的 Java，这也说明 Cadence 在支持“企业自管 Java 运行时”。

## 6. 旧许可附录对今天仍有价值的点

`license/appa.html` 仍然很老，但它至少保留了一个对知识库有用的结构视角。

### 许可不是孤立文件，而是一套层级

文档把许可相关工件拆到了：

- `install_dir/share/license`
- `install_dir/tools/bin`

并分别列出：

- `clients`
- `license.<HOSTID>`
- `options`
- `cdslmd`
- `lm*` utilities

虽然今天具体路径和工具形态已经演进，但这种“许可体系由配置文件、daemon、工具集共同组成”的认知仍然成立。

### 对我们当前知识库的帮助

后续可以把现代 Cadence 25.1 的许可知识也按类似结构归档：

- server-side artifacts
- client-side artifacts
- environment variables
- monitoring / maintenance utilities

## 7. 本轮共性结论

这一轮最重要的收获是把几个容易混淆的边界分开了：

- 设计数据库兼容，不等于可双向回写
- 库兼容比设计数据库更稳定，但超老版本仍要 uprev
- Pulse 迁移不是“客户端升级”，而是 cluster、schema、flows、access key 一起升级
- Java 运行时本身是 Pulse/EDM 迁移的一部分

## 8. 对知识库的直接补充

建议新增或强化这些专题：

- `Back-End Database Uprev / Downrev Boundary`
- `Library Compatibility and Legacy Upgrade`
- `Pulse Cluster Migration Runbook`
- `Java Runtime Dependency Matrix`
- `License Artifact Topology`

## 9. 下一步建议

下一批优先继续读：

- `PCBmigration` 里的 high-speed、PingFederate、Flow Manager Java 主题
- `license` 里更复杂的 configuration 章节
- `spbrelPN` 里剩余全局变化内容

这样知识库就能继续从“可用”推进到“可运维、可迁移、可排障”。
