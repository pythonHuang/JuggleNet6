# t_role - 角色表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| role_name | TEXT | NOT NULL | 角色名称 |
| role_code | TEXT | | 角色编码（唯一） |
| remark | TEXT | | 备注说明 |
| deleted | INTEGER | DEFAULT 0 | 软删除标记 |
| tenant_id | INTEGER | | 所属租户ID（null=全局角色，跨租户共享） |
| created_at | TEXT | | 创建时间 |
| created_by | INTEGER | | 创建人ID |
| updated_at | TEXT | | 更新时间 |
| updated_by | INTEGER | | 更新人ID |

---

## 字段说明

| 字段 | 说明 |
|------|------|
| role_name | 角色显示名称，如"管理员"、"普通用户" |
| role_code | 角色唯一标识，如"admin"、"user" |
| tenant_id | null = 全局角色（所有租户共享）；有值 = 租户私有角色 |

---

## 设计说明

- **全局角色**：tenant_id 为 null，如系统内置的"超级管理员"角色
- **租户角色**：tenant_id 有值，仅该租户可见和使用
- 角色通过 t_role_menu 表关联菜单权限

---

## 示例数据

```json
{
  "id": 1,
  "role_name": "超级管理员",
  "role_code": "super_admin",
  "tenant_id": null,
  "created_at": "2026-03-24T10:00:00",
  "deleted": 0
}
```
