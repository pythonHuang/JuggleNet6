# t_static_variable - 静态（全局）变量表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| var_code | TEXT | | 变量编码（唯一键） |
| var_name | TEXT | | 变量名称（中文描述） |
| data_type | TEXT | | 数据类型：string / integer / double / boolean / date / json |
| value | TEXT | | 当前值（字符串形式存储） |
| default_value | TEXT | | 默认值 |
| description | TEXT | | 描述说明 |
| group_name | TEXT | | 分组名称 |
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
| var_code | 变量唯一编码，流程中用 `$static.xxx` 引用 |
| value | 当前值，存储为字符串，使用时转换类型 |
| group_name | 可按业务分组，便于管理 |

---

## 使用方式

在流程中通过表达式引用：
```
$static.redis_host     // 获取静态变量值
$static.max_retry_count
```

---

## 示例数据

```json
{
  "id": 1,
  "var_code": "redis_host",
  "var_name": "Redis服务器地址",
  "data_type": "string",
  "value": "192.168.1.100:6379",
  "default_value": "localhost:6379",
  "description": "缓存服务器地址",
  "group_name": "缓存配置",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
