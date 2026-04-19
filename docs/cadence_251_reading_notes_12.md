# Cadence SPB 25.1 阅读笔记 12

## 本轮阅读范围

- `C:\Cadence\SPB_25.1\doc\allegro\allegro.tgf`
- `C:\Cadence\SPB_25.1\doc\consmgr\consmgr.tgf`
- `C:\Cadence\SPB_25.1\doc\libManager\libManager.tgf`
- `C:\Cadence\SPB_25.1\share\pcb\help\README.txt`
- `C:\Cadence\SPB_25.1\share\pcb\help\help_config.txt`

## 1. allegro / consmgr / libManager 的共同结论

这三个目录当前呈现出相同形态：

- 本地只看到 `.tgf` tag file
- 没有看到完整 HTML/PDF 正文集
- 大多数帮助标签最终都落回最小文档集 FAQ

这说明它们在当前安装里更像“帮助路由层”，而不是“正文内容层”。

### allegro.tgf

- 明确说明 Allegro PCB Editor 和 APD 共用这份 tag file
- 每个 tag 理论上应跳到对应手册 TOC
- 当前安装里这些 tag 大多回退到最小文档 FAQ

### consmgr.tgf

- 暴露出 Constraint Manager 原本有菜单级、对象级、对话框级帮助结构
- 可见主题包括 `cmref`、`cmug`、`propref`、`PCBmigration`
- 但正文并未在当前安装里完整落地

### libManager.tgf

- 能看出 Library Manager 原本有 user guide / what’s new / KPNS / form-level help
- 帮助标签和表单名仍然保留
- 但正文同样没有完整落地，只剩路由骨架

## 2. 对知识库的结论

这三个目录不应再按“待阅读正文目录”处理，而应改成：

- `BLOCKED`
- 原因：本地只有 tag / route layer，正文未随安装完整提供

同时保留一个有价值的结论：

- 即使没有正文，这些 `.tgf` 仍可帮助识别产品的帮助结构、菜单粒度、对话框粒度和主题名

## 3. share\\pcb\\help 的真实角色

`share\pcb\help` 和上面三类目录完全不同。

### README 给出的定位

- 这里放的是“尚未并入正式 Cadence 文档体系”的附加帮助文件
- 工具内访问由 `help_config.txt` 控制

所以它本质上是：

- 补充帮助层
- 快速增量文档层
- release 外的小型功能说明层

### help_config 的作用

`help_config.txt` 明确说明：

- 可为新命令注册帮助文档
- 可覆盖默认 Cadence 帮助
- 支持 `txt / pdf / html`
- 支持在 `cds_site` 或用户环境层增加自定义条目
- 也支持通过 SKILL 动态注册

当前已注册的一批帮助文件包括：

- `AiCC.pdf`
- `Help_Resize_Respace_Diff_Pairs.pdf`
- `AdjustSpacing.pdf`
- `FiberWeaveAddZigzag.pdf`
- `AddConnect.pdf`
- `AutoSwapPins.pdf`
- `AnalyzeTab.pdf`

### 对知识库的意义

`share\pcb\help` 不是一本总手册，而是：

- 命令级补充帮助仓
- 新功能或非标准发布流程功能的说明仓
- 更适合按主题点查

它的最佳处理方式不是顺序全读，而是：

- 先完成结构归类
- 后续按功能热点逐篇吸收 PDF

## 4. 本轮共性结论

这一轮把剩余目录分成了两种不同层：

### A. 路由层

- `allegro`
- `consmgr`
- `libManager`

特点：

- 有帮助结构
- 有主题名
- 无完整正文

### B. 补充帮助层

- `share\pcb\help`

特点：

- 有实际可读 PDF/TXT
- 不是完整手册
- 更适合按需吸收

## 5. 处理结论

### `allegro`

- 结论：`BLOCKED`
- 原因：仅见 tag file，正文未完整落地

### `consmgr`

- 结论：`BLOCKED`
- 原因：仅见 tag file，正文未完整落地

### `libManager`

- 结论：`BLOCKED`
- 原因：仅见 tag file，正文未完整落地

### `share\pcb\help`

- 结论：`DOING`
- 原因：目录中存在实际 PDF/TXT，可作为补充帮助层逐步吸收

## 6. 下一步建议

后续按这条线继续：

1. 从 `share\pcb\help` 中挑信息密度高的 PDF 做第一批吸收
2. 把 `allegro / consmgr / libManager` 作为“缺正文已判定目录”归档
3. 若后续补装了完整文档，再把这些目录从 `BLOCKED` 转回正文处理流
