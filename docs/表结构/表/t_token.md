# t_token - 访问令牌表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| token_value | TEXT | | Token值（唯一） |
| token_name | TEXT | | Token名称 |
| expired_at | TEXT | | 过期时间，null=永不过期 |
| status | INTEGER | DEFAULT 1 | 状态：1=启用, 0=禁用 |
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
| token_value | API访问凭证，用于 /open/* 接口认证 |
| token_name | Token显示名称，便于识别用途 |
| expired_at | 过期时间，到期自动失效 |

---

## 使用场景

- 开放接口（/open/*）的身份认证
- 第三方系统接入凭证
- Webhook触发验证

---

## 示例数据

```json
{
  "id": 1,
  "token_value": "sk-a1b2c3d4e5f6...",
  "token_name": "第三方系统接入Token",
  "expired_at": "2027-12-31T23:59:59",
  "status": 1,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
