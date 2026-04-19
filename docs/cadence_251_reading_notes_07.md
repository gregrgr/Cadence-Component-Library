# Cadence SPB 25.1 阅读笔记 07

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\PCBmigration\Allegro_X_and_OrCAD_X_Front-End_Products.html`
- `C:\Cadence\SPB_25.1\doc\PCBmigration\Migrating_Multi-Library_Release_Server.html`
- `C:\Cadence\SPB_25.1\doc\license\chap1.html`
- `C:\Cadence\SPB_25.1\doc\license\chap3.html`
- `C:\Cadence\SPB_25.1\doc\license\windows.html`

## 1. 前端产品的 24.1 到 25.1 兼容边界

`Allegro_X_and_OrCAD_X_Front-End_Products.html` 给出了一个很直接的结论：

- `Allegro X System Capture`
- `System Connectivity Manager`
- `OrCAD X Capture CIS`
- `PSpice`

这些产品在 24.1 创建的设计，迁移到 25.1 时是完全兼容的，不需要额外升级动作。

这对我们当前项目意义很大，因为它说明：

- 只要你们现在主线是 `OrCAD X Capture CIS 24.1 -> 25.1`
- 且设计数据本身来自受支持前端产品

那么迁移重点通常不在设计文件格式，而在：

- 库配置
- `CDS_SITE`
- `capture.ini`
- ODBC / Pulse / library indexing
- 许可与部署环境

文档也再次强调，`DE-HDL` 支持已终止，这意味着任何仍然依赖 DE-HDL 的流都不应再被当成 25.1 的长期主线。

## 2. MLR 迁移的本质

`Migrating_Multi-Library_Release_Server.html` 很有价值，因为它解释了 `MLR (Multiple Library Revisions)` 的存在意义。

### MLR 的典型场景

MLR 用于这种组织形态：

- Pulse library server 维持在较低版本
- 客户端工具逐步升级到较高版本

例如：

- Pulse Server 22.1 -> Client 23.1
- Pulse Server 22.1 -> Client 24.1
- Pulse Server 24.1 -> Client 25.1

也就是说，MLR 本质上是“服务器版本滞后、客户端版本前进”时的缓冲机制。

### 官方给出的两条路

要继续工作，有两种思路：

1. 把 Pulse library server 提升到 23.1 或 24.1
2. 直接把 MLR server 迁到 25.1

### 迁到最新版本时的关键动作

文档里最重要的不是按钮，而是这些迁移动作模式：

- 复制 `vault/model_*`
- 复制 `exchange/transmit/model_*`
- 修改 `lib_dist.ini`
- 修改 `fetch_dump.ini`
- 调整启动脚本到新版本路径
- 运行 `adw_uprev` 升级数据库 schema
- 在 Pulse Service Manager 中执行迁移确认
- 关闭 `Enable MLR`
- 最后运行 `lib_dist` 重新安装并发布 PCB Editor models

### 对知识库的启发

MLR 迁移不是“装个新版本然后点下一步”，而是典型的：

- 数据目录迁移
- 配置文件迁移
- schema uprev
- 服务端状态切换
- 模型重新分发

这说明知识库里必须把“文件系统工件”和“数据库工件”一起记。

## 3. 许可总览文档的价值

`license/chap1.html` 虽然年代很老，但它有两个仍然成立的基础认知。

### 许可不是附属功能

Cadence 把 licensing 描述成产品运行前必须先建立的基础设施。应用启动时从 license server checkout feature，退出时归还。

这对知识库意味着：

- 许可不是“安装后再说”的尾项
- 许可本身是运行时架构的一部分

### 许可知识天然分层

这篇总览文档本身就在提醒管理员，真正的许可知识要分散到：

- 安装文档
- 产品配置文档
- postinstall README
- 产品在线文档
- 操作系统文档
- 硬件文档

这和我们现在做知识库的方式是一致的：不能把所有许可知识塞进一页 FAQ。

## 4. 许可维护的核心模式

`license/chap3.html` 的重点是维护，不是首次配置。

### 过期跟踪

文档提出两种思路：

- server-side 周期检查
- client-side 启动时预警

其中比较实用的点是：

- 可以用 `lmCheckExpiration.cds` 定期检查 license file
- 可以通过 `CDS_LIC_EXPIRE` 让客户端在启动时提示即将到期的许可

虽然这是老工具链，但“过期预警要自动化”这个思路今天仍然完全成立。

### 维护不只是修故障

文档把许可维护拆成：

- 跟踪过期
- 监控使用情况
- 启停 daemon
- 变更 license file

这说明知识库后续应该把“许可运维”单独做成一块，而不是混在安装说明里。

## 5. Windows 许可配置文档的现实价值

`license/windows.html` 同样很老，但它保存了一些仍有参考价值的底层模式。

### 仍然有价值的部分

- Windows 侧许可服务本质上仍然依赖：
  - 主机名
  - 真实网卡地址
  - 本机网络可用性
  - service 配置
  - license file 中的 `SERVER` / `DAEMON` 信息
- “连本机都 ping 不通，许可就跑不起来”这种底层约束今天仍成立

### 不应直接照搬的部分

- 文档使用的 `lmtools`、`lmgrd`、`cdslmd` 操作界面和路径明显带有年代背景
- 不能把这些截图步骤直接当成 25.1 的现行操作指南

### 可以沉淀为知识库模式

这篇文档最适合被吸收成一套 Windows 许可排障 checklist：

- hostname 是否正确
- NIC / hostid 是否有效
- 本机 TCP/IP 是否正常
- license file 的服务端字段是否正确
- daemon/service 是否正常启动

## 6. 本轮共性结论

这一轮把知识库再推进了一步：

- 前端设计文件迁移到 25.1，大多不是格式问题，而是外围配置问题
- MLR 迁移是“目录 + 配置 + schema + 分发”的组合动作
- 许可管理要拆成：
  - 架构理解
  - 初始配置
  - 日常维护
  - 故障排查

## 7. 对知识库的直接补充

建议新增或强化这些专题：

- `Front-End Compatibility Boundary`
- `MLR / Pulse Library Server Migration`
- `Licensing Architecture`
- `License Maintenance and Expiration Monitoring`
- `Windows License Service Troubleshooting Checklist`

## 8. 下一步建议

下一批优先继续读：

- `PCBmigration` 中 back-end / Pulse / Java 相关章节
- `license` 中更偏复杂安装与高级配置的章节
- `spbrelPN` 剩余全局变化内容

这样知识库就会把“前端兼容、库服务器迁移、许可维护”三条线连起来。
