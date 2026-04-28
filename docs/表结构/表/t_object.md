# t_object - 自定义对象表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| object_code | TEXT | | 对象编码（唯一） |
| object_name | TEXT | | 对象名称 |
| object_desc | TEXT | | 对象描述 |
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
| object_code | 对象唯一编码，用于参数引用 |
| object_name | 中文名称 |
| object_desc | 详细描述说明 |

---

## 设计说明

- 自定义对象用于定义复杂的数据结构
- 对象属性通过 t_parameter 表的 `param_type=3` 记录
- 支持嵌套对象（param_type=object）

---

## 索引

| 索引名 | 字段 | 类型 | 说明 |
|--------|------|------|------|
| idx_object_code | object_code | UNIQUE | 对象编码唯一索引 |

---

## 示例数据

```json
{
  "id": 1,
  "object_code": "user_info",
  "object_name": "用户信息",
  "object_desc": "包含用户基本信息的数据结构",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```

关联的参数（t_parameter）：
```json
[
  {"param_code": "userId", "param_name": "用户ID", "data_type": "int"},
  {"param_code": "userName", "param_name": "用户名", "data_type": "string"},
  {"param_code": "email", "param_name": "邮箱", "data_type": "string"}
]
```
