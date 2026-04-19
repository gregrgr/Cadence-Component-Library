# Cadence SPB 25.1 处理 Backlog

## 使用规则

- `TODO`：尚未处理
- `DOING`：当前正在处理
- `DONE`：已处理并有产出
- `BLOCKED`：本地缺正文或需要外部条件
- `N/A`：确认对当前知识库无正文价值

## 目录级台账

| 目录 | 状态 | 说明 |
| --- | --- | --- |
| `cdadoc` | `DONE` | 已形成 Doc Assistant 基础笔记 |
| `capPN` | `DONE` | 已吸收 25.1 front-end 关键变化 |
| `cisKPNS` | `DONE` | 已吸收已知问题与限制 |
| `cap_ug` | `BLOCKED` | 本地仅见 `.tgf` 路由，正文未完整落地 |
| `cisug` | `BLOCKED` | 本地仅见 `.tgf` 路由，正文未完整落地 |
| `algPN` | `DONE` | 已形成多批后端功能笔记 |
| `pcbInstall` | `DONE` | 已吸收安装与许可边界 |
| `pcbsystemreqs` | `DONE` | 已吸收系统要求与性能边界 |
| `PCBmigration` | `DONE` | 已吸收 front-end、back-end、Pulse、Java、SSO 等主题 |
| `spbrelPN` | `DONE` | 已作为版本导航层吸收 |
| `license` | `DONE` | 主体已吸收完成，保留为方法论与术语回查层 |
| `xmlreg` | `DONE` | 已判定为文档注册与索引元数据层，不作为正文 backlog |
| `allegro` | `BLOCKED` | 仅见 tag file，正文未随当前安装完整落地 |
| `consmgr` | `BLOCKED` | 仅见 tag file，正文未随当前安装完整落地 |
| `libManager` | `BLOCKED` | 仅见 tag file，正文未随当前安装完整落地 |
| `share\\pcb\\help` | `DONE` | 主体已吸收完成，保留为补充命令知识层按需回查 |

## 下一轮处理顺序

1. 无

## 完成门槛

目录只有在满足以下条件后才能从 `TODO/DOING` 变成 `DONE`：

- 已确认该目录是否有可读正文
- 已形成至少一份笔记或结构化说明
- 已把核心结论同步到 `coverage_status.md`
- 已把目录的处理结论同步到本台账
