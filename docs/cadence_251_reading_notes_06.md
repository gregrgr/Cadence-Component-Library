# Cadence SPB 25.1 阅读笔记 06

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\spbrelPN\Release-Level_Changes.html`
- `C:\Cadence\SPB_25.1\doc\spbrelPN\Accessing_Documentation_and_Other_Knowledge_Resources.html`
- `C:\Cadence\SPB_25.1\doc\license\faq.html`

## 1. 版本级变化总览

`spbrelPN/Release-Level_Changes.html` 的价值不在单个功能点，而在于它把 25.1 这一代产品的“总方向”讲得很清楚。

### 新增与整合方向

- 新引入 `Allegro X AI Advanced Substrate Router`
- `Allegro X Managed Library Solution` 已进入 `System Capture`
- PSpice 新增两个产品层级

### 跨产品变化

- 一个 OnCloud entitlement 可以安装到多台机器，但同一时间只在一台设备上工作
- 版本说明在强调安装、授权、部署和跨设备使用体验，而不只是功能列表

### 性能仍是 25.1 的主线

文档把 `Performance Improvements` 单独提升为 release-level highlights，说明性能优化不是附属项，而是 25.1 的核心方向之一。

### 最重要的边界变化

从 25.1 开始：

- `3D Design Viewer` 不再提供或支持
- `DE-HDL` 及相关工具不再提供或支持

官方同时说明：

- 25.1 之前的旧版本仍可继续使用既有 DE-HDL 设计和库
- 但 25.1 之后不再继续提供这条产品线
- 文档中仍可能残留 DE-HDL 引用，需要后续版本逐步清理

### 对知识库的启发

- 后续知识库必须增加一节 `Discontinued / Legacy Boundary`
- 不能把 24.1 之前的经验默认等价迁移到 25.1
- “功能已废弃但历史项目仍存在”会成为长期运维场景

## 2. 文档访问与知识资源

`spbrelPN/Accessing_Documentation_and_Other_Knowledge_Resources.html` 补强了我们之前对 `Doc Assistant` 的认识。

### 最小文档集策略

- 从 24.1 开始，默认安装只包含少量核心文档
- 这样可以减小安装体积、减少文件数量、加快部署
- 若处于在线环境，帮助使用方式基本不变
- 若处于离线或受限网络环境，需要额外下载产品文档
- 若 Doc Assistant 无法在线下载，可通过 `downloads.cadence.com` 的文档安装器补齐

### Cadence 官方知识资源分层

除了本地产品文档，Cadence 还把知识来源分成：

- Video Library
- Training Courses
- Blog Series
- Customer Support
- ASK

### 对我们当前项目的意义

- 本地安装目录不是完整知识全集，只是最小可运行知识面
- 后续知识库要区分：
  - 本地已安装文档
  - 在线可下载补全文档
  - ASK / blog / training 这类补充知识资源

## 3. 许可 FAQ 的时代差异与可继承规律

`license/faq.html` 明显是较老版本的手册内容，不是 25.1 新文档。它不能直接当成当前版本操作手册，但里面有一些底层许可排障规律仍然值得保留。

### 仍然有价值的通用规律

- 拥塞或高延迟网络会造成 licensing heartbeat 失败
- 大站点更适合 dedicated license server
- 大而混杂的 license file 会增加调试难度
- `UNSUPPORTED` 往往意味着客户端和服务端看到的 FEATURE 集不一致
- 空闲许可可通过 timeout 策略回收
- 单机也需要正常的 TCP/IP 基础设施

### 对我们今天仍有参考价值的点

- 许可问题经常不是“软件坏了”，而是网络、主机名映射、客户端/服务端特征不一致
- 许可排障要分别看：
  - server 端 license file
  - client 端实际请求的 feature
  - 网络连通性
  - hostid / machine identity

### 需要明确保留的警告

- 这份 `license/faq.html` 的版本很老，不能直接作为 25.1 的现行配置依据
- 它更适合作为 `Licensing Troubleshooting Patterns` 的历史经验来源
- 具体到 25.1 的许可部署，应优先以 `pcbInstall`、新 release note 和当前 license client/server 工具行为为准

## 4. 本轮共性结论

这轮文档把知识库再往上拉了一层：

- `spbrelPN` 负责告诉我们“这一代产品总体往哪里走”
- `pcbInstall / pcbsystemreqs / migration` 负责告诉我们“怎么装、怎么跑、怎么迁”
- `license` 则更像“底层排障经验库”

三者结合起来，知识库就不再只是功能摘抄，而更像一套完整的运行知识体系。

## 5. 对知识库的直接补充

建议新增或强化这些专题：

- `Release-Level Strategy and Product Direction`
- `Discontinued / Legacy Boundary`
- `Minimum Local Docs vs Downloadable Docs`
- `Licensing Troubleshooting Patterns`

## 6. 下一步建议

下一批优先继续读：

- `spbrelPN` 剩余总览内容
- `license` 中更接近工具和平台的章节
- `pcbInstall` 与 `PCBmigration` 的剩余主题

这样知识库会同时具备：

- 功能视角
- 环境视角
- 版本视角
- 许可排障视角
