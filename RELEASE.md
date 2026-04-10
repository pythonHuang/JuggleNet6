# Juggle 接口编排平台 — Release v1.0

> **首个正式发布版本**  
> 将 Java/Spring Boot Juggle 接口编排系统完整移植到 .NET 8 + SQLite + Vue3

---

## 技术栈

| 层 | 技术 |
|---|---|
| 后端 | ASP.NET Core 8 / EF Core 8 / SQLite |
| 前端 | Vue3 / Vite / Element Plus / Pinia |
| 容器 | Docker (multi-stage build) |
| 认证 | JWT + RBAC 角色权限 |

---

## 已实现功能（30 项）

### 核心流程
- 流程设计器（可视化节点画布）
- 13 种节点：START / END / METHOD / CONDITION / MERGE / ASSIGN / CODE / DB(MySQL+多数据源) / SUB_FLOW / LOOP / DELAY / PARALLEL / NOTIFY
- 节点超时 & 重试策略
- 流程版本管理 & 版本对比
- 流程克隆 / 导入 / 导出（含 Word 文档）
- 流程分组管理

### 触发方式
- 同步触发 `GET/POST /open/flow/trigger/{key}`
- 异步触发 + 结果查询
- Webhook 触发（含签名验证）
- 定时任务调度

### 套件 & 接口管理
- 套件 / 接口 / 对象 / 参数管理
- 接口 Mock 功能

### 监控 & 测试
- 监控仪表盘
- 执行日志（含节点级日志）
- 流程测试用例（断言 + 批量执行）
- Monaco Editor 代码编辑（JS / SQL 高亮 + 自动补全）

### 系统管理
- 用户管理
- 角色管理 + 菜单权限（RBAC）
- 多租户数据隔离（JWT Claims 驱动，全局查询过滤器）
- 审计日志
- Token 权限管理
- 系统配置中心
- 全局异常告警（Webhook POST + 邮件 SMTP）

### 数据库支持
- **系统数据库**：SQLite / MySQL / PostgreSQL / SQLServer（通过 `DB_TYPE` 环境变量切换）
- **业务数据源**：SQLite / MySQL / PostgreSQL / SQLServer / Oracle / 达梦（6 种）

---

## Docker 快速启动

```bash
# 拉取镜像
docker pull pythonhuang/juggle-net8:v1.0

# 启动（数据持久化到 juggle_data volume）
docker run -d \
  --name juggle \
  -p 9127:9127 \
  -v juggle_data:/data \
  pythonhuang/juggle-net8:v1.0
```

或使用 docker-compose：

```bash
docker-compose up -d
```

默认账号：`juggle` / `juggle`  
访问地址：http://localhost:9127

---

## 环境变量

| 变量 | 说明 | 默认值 |
|---|---|---|
| `ASPNETCORE_URLS` | 监听地址 | `http://+:9127` |
| `DB_PATH` | SQLite 文件路径 | `/data/juggle.db` |
| `DB_TYPE` | 系统数据库类型 (sqlite/mysql/postgresql/sqlserver) | `sqlite` |
| `DB_CONNECTION_STRING` | 完整连接串（非 SQLite 时使用） | — |
| `Jwt__Key` | JWT 签名密钥（≥32字符） | 内置默认值 |

---

## 源码

https://github.com/pythonHuang/JuggleNet6
