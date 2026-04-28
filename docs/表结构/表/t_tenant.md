# t_tenant - 租户表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| tenant_name | TEXT | NOT NULL | 租户名称 |
| tenant_code | TEXT | | 租户编码（唯一） |
| remark | TEXT | | 备注说明 |
| status | INTEGER | DEFAULT 1 | 状态：1=启用, 0=禁用 |
| expired_at | TEXT | | 过期时间，null=永不过期 |
| menu_keys | TEXT | DEFAULT '[]' | 租户菜单权限（JSON数组） |
| deleted | INTEGER | DEFAULT 0 | 软删除标记 |
| tenant_id | INTEGER | | 所属租户ID（null表示全局） |
| created_at | TEXT | | 创建时间 |
| created_by | INTEGER | | 创建人ID |
| updated_at | TEXT | | 更新时间 |
| updated_by | INTEGER | | 更新人ID |

---

## 字段说明

| 字段 | 说明 |
|------|------|
| tenant_code | 租户唯一编码，用于系统标识 |
| status | 启用后可正常使用，禁用后该租户用户无法登录 |
| expired_at | 到期后自动禁用，支持订阅制商业模式 |
| menu_keys | JSON数组格式，如 `["flow/define","flow/log"]`，控制菜单可见性 |

---

## 索引

| 索引名 | 字段 | 类型 | 说明 |
|--------|------|------|------|
| idx_tenant_code | tenant_code | UNIQUE | 租户编码唯一索引 |

---

## 示例数据

```json
{
  "id": 1,
  "tenant_name": "演示租户",
  "tenant_code": "demo",
  "status": 1,
  "expired_at": null,
  "menu_keys": "[\"flow/define\",\"flow/list\",\"flow/version\",\"suite/*\",\"object/*\",\"system/*\"]",
  "created_at": "2026-03-24T10:00:00",
  "deleted": 0
}
```
