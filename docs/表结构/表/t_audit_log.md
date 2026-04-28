# t_audit_log - 审计日志表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| module | TEXT | NOT NULL | 操作模块：tenant / role / user / flow 等 |
| action_type | TEXT | NOT NULL | 操作类型：add / update / delete |
| target_id | INTEGER | NOT NULL | 操作目标ID |
| change_content | TEXT | DEFAULT '{}' | 变更内容摘要（JSON格式） |
| operator_name | TEXT | | 操作人用户名 |
| operator_id | INTEGER | | 操作人用户ID |
| operator_tenant_id | INTEGER | | 操作人租户ID |
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
| module | 操作所属模块，便于分类查询和统计 |
| action_type | 操作类型枚举 |
| target_id | 被操作记录的主键ID |
| change_content | JSON格式记录变更前后对比，如 `{"before":{"name":"A"},"after":{"name":"B"}}` |

---

## 操作类型枚举

| action_type | 说明 |
|-------------|------|
| add | 新增 |
| update | 修改 |
| delete | 删除 |
| enable | 启用 |
| disable | 禁用 |
| deploy | 部署 |
| trigger | 触发 |

---

## 示例数据

```json
{
  "id": 1,
  "module": "user",
  "action_type": "update",
  "target_id": 5,
  "change_content": "{\"before\":{\"role_id\":2},\"after\":{\"role_id\":1}}",
  "operator_name": "admin",
  "operator_id": 1,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
