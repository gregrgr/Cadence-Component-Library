# CadenceComponentLibraryAdmin

`CadenceComponentLibraryAdmin` 是一个面向 `OrCAD Capture CIS / Allegro` 元器件主数据管理的个人专业级 Web 后台。系统使用服务端渲染的 `ASP.NET Core MVC`，核心目标是管理企业元器件主数据库，并通过 `SQL View` 向 OrCAD Capture CIS 发布只读批准件。

## 当前实现范围

当前仓库已经完成到以下阶段：

1. 项目骨架、Docker 部署、Identity、角色和管理员种子
2. 领域实体、`DbContext`、Fluent API、视图 SQL 脚本
3. 基础 CRUD 页面：
   - Company Parts
   - Manufacturer Parts
   - Symbol Families
   - Package Families
   - Footprint Variants
   - Online Candidates
4. 业务规则：
   - `PackageSignature` 自动生成与重复拦截
   - `Approved CompanyPart` 审批前校验
   - `Approved CompanyPart` 修改 Footprint / Symbol 写入 `PartChangeLog`
   - `AltLevel A` 需要 Source / Target footprint 一致
5. 质量报告：
   - Duplicate MPN
   - Approved Part Missing MPN
   - Approved Part Missing Footprint
   - Approved Part References Non-Released Footprint
   - Missing Datasheet
   - Duplicate Package Signature
   - Orphan Footprint
   - Missing Files
6. 发布管理和审计：
   - `Library Releases`
   - `LIB_yyyy.MM.dd` 自动生成
   - Release 前质量检查
   - Release 历史
   - Change Logs 查询与 CSV 导出
7. UI 收口：
   - 左侧菜单
   - 状态 badge
   - 全局提示
   - 删除确认
   - 列表分页

## 技术栈

- `.NET 10`
- `ASP.NET Core MVC`
- `Entity Framework Core`
- `SQL Server`
- `ASP.NET Core Identity`
- `Bootstrap 5`
- Docker Compose

## 解决方案结构

```text
CadenceComponentLibraryAdmin/
├─ src/
│  ├─ CadenceComponentLibraryAdmin.Web/
│  ├─ CadenceComponentLibraryAdmin.Application/
│  ├─ CadenceComponentLibraryAdmin.Domain/
│  └─ CadenceComponentLibraryAdmin.Infrastructure/
├─ tests/
│  └─ CadenceComponentLibraryAdmin.Tests/
├─ library/
├─ storage/
├─ docker-compose.yml
├─ .env.example
├─ README.md
└─ CadenceComponentLibraryAdmin.sln
```

## 角色和默认管理员

系统启动后会自动初始化以下角色：

- `Admin`
- `Librarian`
- `EEReviewer`
- `Purchasing`
- `Designer`
- `Viewer`

默认管理员账号：

- Email: `admin@local.test`
- Password: `Admin@123456`

## 本地运行前提

### 方式 1：使用 Docker

这是当前最推荐的方式，尤其适合本机没有 `.NET SDK` 的场景。

1. 复制环境变量模板

```powershell
Copy-Item .env.example .env
```

2. 在 `.env` 里设置 `SA_PASSWORD`

3. 启动服务

```powershell
docker compose up --build
```

启动后服务地址：

- Web: [http://localhost:8080](http://localhost:8080)
- SQL Server: `localhost:14333`

### 方式 2：使用本机 .NET SDK

如果机器已安装 `.NET 10 SDK`，可直接使用以下命令：

```powershell
dotnet restore
dotnet build
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
dotnet run --project src/CadenceComponentLibraryAdmin.Web
```

## EF Core Migration 命令

创建迁移：

```powershell
dotnet ef migrations add InitialCreate --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```

应用迁移：

```powershell
dotnet ef database update --project src/CadenceComponentLibraryAdmin.Infrastructure --startup-project src/CadenceComponentLibraryAdmin.Web
```

## Docker 持久化

当前 Docker 环境已经配置好持久化：

- SQL Server 数据卷：
  - Docker volume `sqlserver-data`
- 应用数据：
  - `./storage/app-data`
- 库文件：
  - `./library`
- 应用日志：
  - `./storage/logs`

`./library` 目录用于承载 Cadence 相关文件资源，例如：

- `Symbols_OLB`
- `Footprints`
- `Padstacks`
- `3D`
- `Docs`

## 数据库与发布视图

目标数据库名称：

- `CadenceComponentLibrary`

当前基础模型已包含：

- `CompanyParts`
- `ManufacturerParts`
- `SymbolFamilies`
- `PackageFamilies`
- `FootprintVariants`
- `OnlineCandidates`
- `PartAlternates`
- `PartDocs`
- `PartChangeLogs`
- `LibraryReleases`

基础视图脚本位于：

- `src/CadenceComponentLibraryAdmin.Infrastructure/Data/Views/CisViews.sql`

其中包括：

- `dbo.vw_CIS_Release_Parts`
- `dbo.vw_CIS_Alternates`

## 当前主要页面

已实现页面：

- `/`
- `/CompanyParts`
- `/ManufacturerParts`
- `/SymbolFamilies`
- `/PackageFamilies`
- `/FootprintVariants`
- `/OnlineCandidates`
- `/QualityReports`
- `/LibraryReleases`
- `/ChangeLogs`

## 当前已实现的关键规则

### CompanyPart 审批规则

当 `ApprovalStatus = Approved` 时，系统会检查：

1. 至少一个 `IsApproved = true` 的 `ManufacturerPart`
2. 有效 `SymbolFamily`
3. 有效 `PackageFamily`
4. `DefaultFootprint` 存在且状态为 `Released`
5. `DatasheetUrl` 不为空

### PackageSignature 规则

`PackageFamily` 保存前会自动生成：

```text
MountType|LeadCount|BodyL|BodyW|Pitch|EP_L|EP_W
```

若签名重复，系统会拦截保存并提示复用已有封装家族。

### PartChangeLog 规则

当已批准的 `CompanyPart` 修改以下字段时，会自动写入 `PartChangeLog`：

- `DefaultFootprintName`
- `SymbolFamilyCode`

### Release 规则

正式 `Release` 前会执行 `QualityReports`。若仍存在任何质量问题，则不允许正式发布。

## 质量报告

当前 `Quality Reports` 页面已支持：

- 页面查看
- 问题汇总
- 每项报告导出 CSV

文件缺失检查覆盖：

- `SymbolFamily.OlbPath`
- `FootprintVariant.PsmPath`
- `FootprintVariant.DraPath`
- `FootprintVariant.StepPath`

## 当前已知边界

- 当前机器如果只有 `.NET runtime` 而没有 `.NET SDK`，则无法直接在本机执行 `dotnet build` 或生成迁移。
- 仓库目前尚未实现：
  - 自动下载厂商数据
  - 自动生成 footprint
  - 自动写 `.olb`
  - ERP / PLM 双向同步
  - 多租户
  - 复杂工作流引擎

## 下一步建议

如果继续往下迭代，优先建议做这几项：

1. `Approval Queue` 页面
2. `Alternates` 页面和替代料审批
3. `Users / Roles` 后台管理页
4. 更完整的 Dashboard 指标
5. 更细的列表筛选和批量操作
