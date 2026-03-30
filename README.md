# Juggle 接口编排平台 (.NET 8 + Vue3)

> 图形化微服务接口编排低代码工具 —— 像积木一样灵活，像魔法一样强大

---

## 项目简介

**Juggle** 是一个可视化的接口编排平台，通过拖拽式流程设计器将多个已有的 HTTP API 接口编排为一个复合接口，直接供前端或业务系统调用，极大提升开发效率，无需重复编码。

本项目为 Java/Spring Boot 原版的 **.NET 8 + SQLite + Vue3 重构版**，实现了完整的 DDD 四层架构。

---

## 核心功能

| 功能模块 | 说明 |
|---------|------|
| 🎨 可视化流程设计器 | 拖拽节点、连线建立数据流、自动布局、MiniMap 缩略图 |
| 🔧 多种节点类型 | START / END / METHOD / CONDITION / ASSIGN / CODE / MYSQL / MERGE |
| ▶️ 流程调试 | 在设计界面直接调试，走过节点高亮显示、查看输出结果 |
| 📦 套件 & 接口管理 | 组织管理基础 HTTP API，支持 Swagger 解析导入 |
| 🗄️ 多数据库支持 | SQLite / MySQL / PostgreSQL / SQL Server |
| 📊 执行日志 | 流程执行主日志 + 节点明细日志（含变量快照时间轴） |
| 🔑 Token 管理 | 开放接口访问令牌管理 |
| 📐 全局静态变量 | 跨流程共享变量，支持读写与重置 |
| 🔐 JWT 认证 | 控制台登录认证 |

---

## 技术栈

| 层次 | 技术 |
|------|------|
| 后端框架 | ASP.NET Core 8 Web API |
| 架构模式 | DDD 四层架构（Api / Application / Domain / Infrastructure） |
| 数据库 | SQLite（EF Core 8） |
| 多数据库 | MySqlConnector / Npgsql / Microsoft.Data.SqlClient |
| 认证 | JWT Bearer Token |
| 文档 | Swagger / OpenAPI |
| 前端框架 | Vue 3 (Composition API + TypeScript) |
| UI 组件库 | Element Plus |
| 流程画布 | @vue-flow/core |
| 状态管理 | Pinia |
| 路由 | Vue Router 4 |
| 构建工具 | Vite 8 |

---

## 目录结构

```
JuggleNet6/
├── Juggle.Api/               # 入口层：14个 Controller + Program.cs
│   ├── Controllers/
│   │   ├── Api/              # 业务 Controller（流程、套件、系统等）
│   │   └── Open/             # 开放接口（外部触发流程）
│   ├── wwwroot/              # 前端构建产物（生产模式集成）
│   ├── juggle.db             # SQLite 数据库文件
│   └── Program.cs
│
├── Juggle.Application/       # 应用层：Service + DTO
│   ├── Services/
│   │   ├── Flow/             # 流程执行服务
│   │   └── Impl/             # DataSourceService、JwtService
│   └── Models/
│       ├── Request/          # 请求 DTO
│       └── Response/         # ApiResult 统一响应
│
├── Juggle.Domain/            # 领域层：实体 + 流程引擎
│   ├── Entities/             # 15个领域实体
│   └── Engine/               # FlowEngine + 8个节点执行器
│
├── Juggle.Infrastructure/    # 基础设施层
│   ├── Persistence/          # JuggleDbContext（EF Core）
│   └── Common/               # JsonHelper、Md5Helper
│
├── JuggleNet6.Frontend/      # Vue3 前端
│   └── src/views/            # 功能页面（flow/suite/system/object）
│
├── JuggleNet6.Backend/       # 原单体后端（保留参考，已废弃）
├── Architecture.md           # 系统架构文档
├── DesignDoc.md              # 系统详细设计文档
└── Juggle.sln                # Visual Studio 解决方案
```

---

## 快速开始

### 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)

### 启动后端

```powershell
cd JuggleNet6\Juggle.Api
dotnet run
```

服务启动后访问：
- 应用首页：http://localhost:9127/
- Swagger 文档：http://localhost:9127/swagger

### 启动前端（开发模式）

```powershell
cd JuggleNet6\JuggleNet6.Frontend
npm install
npm run dev
```

前端开发服务：http://localhost:5173/

### 构建前端并集成到后端（生产模式）

```powershell
cd JuggleNet6\JuggleNet6.Frontend
npm run build
Copy-Item dist\* ..\Juggle.Api\wwwroot\ -Recurse -Force
```

---

## 默认账号

| 用户名 | 密码 |
|--------|------|
| juggle | juggle |

---

## 流程节点类型

| 节点 | 说明 |
|------|------|
| START | 开始节点，流程入口 |
| END | 结束节点，流程出口 |
| METHOD | 方法节点，调用外部 HTTP API |
| CONDITION | 条件节点，多分支判断 |
| ASSIGN | 赋值节点，变量赋值/类型转换 |
| CODE | 代码节点，执行 JavaScript 脚本 |
| MYSQL | 数据库节点，执行 SQL 查询 |
| MERGE | 聚合节点，多分支汇聚 |

---

## API 接口规范

| 前缀 | 用途 | 认证方式 |
|------|------|---------|
| `/api/` | 控制台管理接口 | JWT Token |
| `/open/` | 开放接口（外部触发流程） | API Token |

统一响应格式：
```json
{
  "code": 200,
  "message": "success",
  "data": { ... }
}
```

---

## 相关文档

- [架构文档](./Architecture.md) - 系统总体架构设计
- [设计文档](./DesignDoc.md) - 系统详细设计说明
- [后端 README](./Juggle.Api/README.md) - 后端项目说明
- [前端 README](./JuggleNet6.Frontend/README.md) - 前端项目说明

---

## 致谢

本项目基于 [Juggle](https://github.com/somta/Juggle) 原始 Java 版本的设计思路与架构，使用 .NET 8 + Vue3 + SQLite 进行重构实现。

感谢 [@somta](https://github.com/somta) 及原项目团队提供的思路与支持！

> 原项目：https://github.com/somta/Juggle
