# t_user - 用户表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| user_name | TEXT | NOT NULL | 用户名（登录账号） |
| password | TEXT | | 密码（加密存储） |
| role_id | INTEGER | | 关联角色ID |
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
| user_name | 用户登录账号，通常为唯一标识 |
| password | 密码经过加密存储（如 MD5/SHA256） |
| role_id | 关联 t_role 表，支持 RBAC 权限控制 |

---

## 索引

| 索引名 | 字段 | 类型 | 说明 |
|--------|------|------|------|
| idx_user_name | user_name | UNIQUE | 用户名唯一索引 |

---

## 示例数据

```json
{
  "id": 1,
  "user_name": "admin",
  "password": "e10adc3949ba59abbe56e057f20f883e",
  "role_id": 1,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00",
  "deleted": 0
}
```
