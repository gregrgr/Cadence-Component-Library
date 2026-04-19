# Cadence SPB 25.1 阅读笔记 09

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\PCBmigration\Allegro_X_Platform_High-Speed_Products.html`
- `C:\Cadence\SPB_25.1\doc\PCBmigration\Modifying_PingFederate_Setup.html`
- `C:\Cadence\SPB_25.1\doc\PCBmigration\Setting_Custom_Java_Version_for_Flow_Manager.html`
- `C:\Cadence\SPB_25.1\doc\license\appb.html`

## 1. High-Speed 产品迁移负担其实很轻

`Allegro_X_Platform_High-Speed_Products.html` 这篇很短，但信息很明确：

- 面向所有 Allegro-based High-Speed products
- `23.1`、`22.1`、`17.4-2019` 的设计对 `24.1` 是完全兼容的
- 真正需要关注的变化仍然要回看 `Core Back-End Products`

对知识库来说，这个结论很有价值，因为它说明 high-speed 这条线在 25.1 里没有额外叠加出一套单独的复杂迁移规则，很多关键边界仍然继承自 back-end 主线。

换句话说：

- high-speed 迁移问题，优先先查 back-end 数据库和库兼容边界
- 不要一上来就假设是 SI 专属问题

## 2. PingFederate 修改是典型的“升级后身份链路断点”

`Modifying_PingFederate_Setup.html` 讲的是一个很典型的企业 IT 级问题。

### 25.1 的身份侧变化

- Pulse 现在使用 `Keycloak 21.1.2`
- 在这个版本下，PingFederate 发给 Pulse 的 SAML response 里，`Destination` 属性变成必需项

文档给出的示例是把 `Destination` 指向：

- `http://<Pulse access URL>:<port>/auth/realms/Cadence/broker/saml/endpoint`

### 对知识库的启发

这类问题特别值得单独记，因为它具备几个特征：

- 软件升级本身可能成功
- 服务也可能正常启动
- 但 SSO / IAM 链路会因为协议字段要求变化而失败

也就是说，迁移验收不能只做：

- 服务启动验证
- 数据库迁移验证

还要做：

- 登录链路验证
- SAML / IdP 回调验证

## 3. Flow Manager 的 Java 不是通用 Java

`Setting_Custom_Java_Version_for_Flow_Manager.html` 把 Flow Manager 的 Java 边界说得更细了。

### 关键点

- Allegro EDM Flow Manager 默认使用 Cadence 安装目录自带的 Java
- 管理员和 Librarian 可以给它指定自定义 `Java 8 / JRE 1.8`
- 通过环境变量 `CDS_FLOWMANAGER_JAVA_HOME` 指向该路径

### 很关键的现场约束

文档特别强调：

- 如果安装目录在共享盘或网络位置，是否给普通设计师写权限会影响部署方式
- 如果不想给所有用户写权限，可以单独建一个 Java 8 文件夹
- 把 `EDMFlowManagerJRE.exe` 解包到该目录
- 路径里不能有空格
- 而且要求是支持 Java applets 的 Oracle Java 8

### 对知识库的启发

这类信息非常像“安装文档里容易被忽略、但现场最容易踩坑”的点：

- 共享目录权限
- 路径不能有空格
- 特定 Java 发行版要求

这些都应该进入单独的运维 checklist，而不只是留在阅读笔记里。

## 4. 旧许可文档里最有价值的底层结构

`license/appb.html` 虽然很老，但它把许可系统拆得非常结构化，这对知识库依然有帮助。

### 许可配置的基本组件

文档把许可系统拆成：

- license server
- `lmgrd`
- `cdslmd`
- licensing utilities
- license file
- 指定 license file 的方法
- 实际消费许可的应用程序

这让我们可以把现代 Cadence 的许可知识也按同一思路整理，而不是混成一堆工具名和环境变量。

### 组件交互关系

它明确说明了基本链路：

1. 应用先找到 license file
2. 再定位到 license server 上的 `lmgrd`
3. `lmgrd` 转给 `cdslmd`
4. `cdslmd` 决定是否发放 feature

这条链路虽然年代久远，但排障思路今天仍然成立：

- 是找不到 license file
- 还是找不到 server
- 还是 `lmgrd` 正常但 vendor daemon 异常
- 还是 feature 不可用

### 对知识库的启发

后续可以把许可排障路径固定成分层模型：

- `discovery`
- `server reachability`
- `daemon health`
- `feature availability`

这样比笼统地说“许可有问题”更适合现场定位。

## 5. 本轮共性结论

这一轮最重要的收获是：

- 并不是每条产品线都有独立复杂迁移逻辑，很多时候是共用主干边界
- 迁移验收不能只看软件能不能打开，还要看 SSO、Java、共享路径和权限
- 老许可文档虽然不能直接拿来照做，但它提供的“分层理解方式”仍然很有价值

## 6. 对知识库的直接补充

建议新增或强化这些专题：

- `SSO / SAML Migration Validation`
- `Flow Manager Runtime and Shared-Path Constraints`
- `License Resolution Chain`
- `High-Speed Products and Shared Back-End Boundaries`

## 7. 下一步建议

下一批优先继续读：

- `spbrelPN` 剩余全局变化内容
- `license` 中剩余复杂配置与高级排障章节
- `xmlreg` 或其他元数据类目录中与产品分类、索引机制相关的内容

这样知识库会逐步从“文档笔记集”变成“系统知识图”。 
