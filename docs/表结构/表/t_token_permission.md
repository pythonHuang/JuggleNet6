# t_token_permission - Token 权限配置表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| token_id | INTEGER | NOT NULL | 关联Token ID |
| permission_type | TEXT | NOT NULL | 权限类型：FLOW / API |
| resource_key | TEXT | NOT NULL | 资源Key（flowKey 或 methodCode） |
| resource_name | TEXT | | 资源名称（冗余存储） |
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
| token_id | 关联 t_token 表 |
| permission_type | FLOW=可调用流程，API=可调用套件接口 |
| resource_key | 具体资源标识 |

---

## 权限类型说明

| permission_type | 说明 | resource_key 示例 |
|-----------------|------|-------------------|
| FLOW | 流程调用权限 | `order_process` |
| API | 套件接口权限 | `user_suite.getUserInfo` |

---

## 示例数据

```json
[
  {
    "id": 1,
    "token_id": 1,
    "permission_type": "FLOW",
    "resource_key": "order_process",
    "resource_name": "订单处理流程",
    "tenant_id": 1
  },
  {
    "id": 2,
    "token_id": 1,
    "permission_type": "API",
    "resource_key": "user_suite.getUserInfo",
    "resource_name": "获取用户信息",
    "tenant_id": 1
  }
]
```
