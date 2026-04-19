# Cadence SPB 25.1 阅读笔记 05

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\pcbInstall\gettingstd.html`
- `C:\Cadence\SPB_25.1\doc\pcbInstall\faq.html`
- `C:\Cadence\SPB_25.1\doc\pcbsystemreqs\faq.html`
- `C:\Cadence\SPB_25.1\doc\PCBmigration\Migrating_Unmanaged_Libraries_Indexed_by_Pulse.html`

## 1. 安装文档主线

`pcbInstall/gettingstd.html` 更像安装总入口，而不是细节手册正文。它的作用是把 25.1 安装工作和下面几类主题绑定起来：

- 安装步骤
- 系统要求
- 许可配置
- 升级与迁移

对知识库的启发是，后续不能只按“产品功能”组织内容，还要补一条“环境与部署”主线。

## 2. 安装与许可 FAQ

`pcbInstall/faq.html` 里最值得记住的是许可和平台边界。

### 许可服务器配置

- 客户端通过 License Client Configuration Utility 配置服务器。
- 服务器地址格式是 `port@host`。
- 多服务器在 Windows 下用 `;` 分隔，在 Linux 下用 `:` 分隔。
- triad 服务器用逗号分隔。

### 25.1 许可文件边界

- 25.1 的 server-based 产品需要新的 25.1 license file。
- 新 license file 可以向下兼容更早版本。
- 不建议把 25.1 feature 人工拼接进旧的 24.1 license file。
- 更新 license 后不要依赖 `lmreread`，推荐重启 license server。

### 平台与部署限制

- OrCAD X 只支持 Windows。
- License manager 可以部署在 Windows 或 Linux。
- Linux license server 可以给 Windows 客户端发放许可。
- VM 上运行 license server 是支持的。
- 桌面虚拟化环境不支持。
- 客户端侧 IPv6 不是完整支持场景，建议采用 IPv4 或 dual-stack。

### 对知识库的启发

- 后续要单独建一节 `Licensing and Deployment Boundaries`。
- 很多“软件打不开”或“功能灰掉”并不是产品 bug，而是 license file、server OS、网络协议或客户端平台边界问题。

## 3. 系统要求 FAQ

`pcbsystemreqs/faq.html` 的价值在于，它不只是给出配置表，而是在解释性能和稳定性的真实影响因素。

### 磁盘与内存

- 安装器会检查最小磁盘空间。
- 没有一个绝对固定的“推荐内存上限”。
- 对多数用户来说，8 GB 内存可工作，但复杂设计通常需要更多。
- 官方给出的粗略经验值是：可按“完成板设计文件大小的约 3 倍”估算内存需求，再加操作系统开销。
- 如果系统没有分页，继续加内存不一定继续提升性能。

### CPU 与并行度

- 两个 processing units 已经能带来明显收益。
- 四个更适合物理设计。
- 更复杂、更高密度的设计会从更多核心中受益。
- 8 个及以上核心更适合大规模 DRC、高阶 SI、GRE 这类任务。

### 图形与显示

- 建议使用独立显卡。
- 图形驱动需要保持更新。
- 多显示器环境下，如果缩放比例不一致，菜单或窗口位置可能出现错位。
- 官方建议把高分辨率显示器设为主显示器，尽量避免在缩放比例不同的显示器之间来回拖动主应用窗口。

### 真实性能瓶颈

文档明确提醒，性能问题不一定来自 EDA 本身，也可能来自：

- 杀毒软件
- IP protection software
- 后台抢占资源的进程
- 文件服务器负载
- WAN/LAN 访问 schematic 或设计数据的延迟

### 对知识库的启发

- 后续要把“硬件要求”和“性能排查”拆开记录。
- 现场卡顿问题需要做成 checklist，而不是只回一句“升级电脑配置”。

## 4. Pulse 索引但未托管库的迁移

`PCBmigration/Migrating_Unmanaged_Libraries_Indexed_by_Pulse.html` 和我们当前做企业库特别相关，因为它讲的是“索引”和“托管”之间的边界。

### DE-HDL 情况

- 如果库只是被 Pulse 索引，但本身不是 managed library，那么迁移到 25.1 不需要额外任务。

### OrCAD X Capture CIS 情况

如果是被 Pulse 索引、但不是 managed 的 OrCAD X Capture CIS 库，官方迁移动作是：

1. 进入 `<CDS_SITE>/cdssetup/OrCAD_Capture`
2. 把 `24.1.0` 目录改名为 `25.1.0`
3. 编辑 `capture.ini`
4. 把 `Version=24.1-2024` 改为 `Version=25.1-2025`
5. 打开 Pulse Service Manager Settings
6. 进入 `Library Management`
7. 启用 library indexing
8. 把 Library Path 指向包含该 `capture.ini` 的完整 `<CDS_SITE>` 路径

### 官方建议

- 最好在 Pulse server 上运行 OrCAD X Capture CIS，先完成库与 ODBC 配置。
- 然后再把生成出来的 `capture.ini` 作为 Pulse 侧的 library indexing 输入。
- OrCAD X library indexing 仅支持 Windows。

### 对我们当前项目的意义

- Pulse indexing 不等于 cloud-managed library。
- 25.1 迁移中，很多配置延续点实际上落在 `capture.ini` 和 `CDS_SITE` 结构上。
- 这说明我们后面除了数据库和 `.dbc`，还要把 `CDS_SITE` 结构、`capture.ini`、ODBC 依赖关系一起纳入知识库。

## 5. 本轮共性结论

这一轮最重要的不是学到某个具体按钮，而是把 25.1 的“运行边界”看清了：

- 功能能不能用，不只看版本，还看 license file 和 server/client 组合。
- 性能好不好，不只看 CPU/RAM，还看显卡、驱动、后台软件、文件服务器和网络。
- 迁移能不能稳，不只看数据库，还看 `CDS_SITE`、`capture.ini`、Pulse indexing 以及 Windows 支持边界。

## 6. 对知识库建设的直接补充

建议在知识库中新增这些主题分支：

- `Licensing and Deployment Boundaries`
- `System Requirements and Performance Triage`
- `Migration Paths and Configuration Artifacts`
- `CDS_SITE / capture.ini / ODBC Dependency Chain`

## 7. 下一步建议

下一批优先继续读：

- `pcbInstall` 中剩余安装与许可相关文档
- `pcbsystemreqs` 中剩余平台与环境边界
- `PCBmigration` 中更多与库、项目、配置延续相关的迁移主题

这样知识库就能从“功能理解”继续扩展到“部署、运维、升级、迁移”的完整闭环。
