# t_role_menu - 角色菜单权限表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| role_id | INTEGER | NOT NULL | 关联角色ID |
| menu_key | TEXT | NOT NULL | 菜单标识，如 "flow/define" |
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
| role_id | 关联 t_role 表的角色ID |
| menu_key | 菜单唯一标识，支持通配符如 "suite/*" 表示所有套件菜单 |

---

## 设计说明

- 角色与菜单权限的**多对多中间表**
- 支持细粒度菜单级别权限控制
- menu_key 使用通配符可批量授权，如：
  - `flow/*` - 所有流程相关菜单
  - `suite/*` - 所有套件相关菜单
  - `system/*` - 所有系统管理菜单

---

## 示例数据

```json
[
  {
    "id": 1,
    "role_id": 1,
    "menu_key": "flow/define",
    "tenant_id": 1
  },
  {
    "id": 2,
    "role_id": 1,
    "menu_key": "suite/*",
    "tenant_id": 1
  }
]
```
