# t_data_source - 数据源配置表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| ds_name | TEXT | | 数据源名称 |
| ds_type | TEXT | | 数据源类型：mysql / postgres / sqlserver / oracle 等 |
| host | TEXT | | 主机地址 |
| port | INTEGER | | 端口号 |
| db_name | TEXT | | 数据库名称 |
| username | TEXT | | 用户名 |
| password | TEXT | | 密码（加密存储） |
| deleted | INTEGER | DEFAULT 0 | 软删除标记 |
| tenant_id | INTEGER | | 所属租户ID |
| created_at | TEXT | | 创建时间 |
| created_by | INTEGER | | 创建人ID |
| updated_at | TEXT | | 更新时间 |
| updated_by | INTEGER | | 更新人ID |

---

## 字段说明

| 字段 | 说明 |
|------|------|
| ds_name | 数据源显示名称 |
| ds_type | 支持多种数据库类型 |
| password | 敏感信息，应加密存储 |

---

## 支持的数据库类型

| ds_type | 说明 |
|---------|------|
| mysql | MySQL 数据库 |
| postgres | PostgreSQL 数据库 |
| sqlserver | SQL Server 数据库 |
| oracle | Oracle 数据库 |
| dm | 达梦数据库 |

---

## 示例数据

```json
{
  "id": 1,
  "ds_name": "业务数据库",
  "ds_type": "mysql",
  "host": "192.168.1.100",
  "port": 3306,
  "db_name": "business_db",
  "username": "juggle",
  "password": "encrypted_password",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
