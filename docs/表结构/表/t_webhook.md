# t_webhook - Webhook 触发配置表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| webhook_key | TEXT | NOT NULL | Webhook唯一标识Key |
| webhook_name | TEXT | NOT NULL | Webhook名称 |
| flow_key | TEXT | NOT NULL | 关联流程Key |
| flow_name | TEXT | | 流程名称（冗余） |
| secret | TEXT | | 签名密钥（HMAC-SHA256） |
| allowed_method | TEXT | DEFAULT 'POST' | 允许的HTTP方法 |
| async_mode | INTEGER | DEFAULT 0 | 异步模式：0=同步, 1=异步 |
| status | INTEGER | DEFAULT 1 | 状态：1=启用, 0=禁用 |
| trigger_count | INTEGER | DEFAULT 0 | 触发次数统计 |
| last_trigger_time | TEXT | | 最后触发时间 |
| remark | TEXT | | 备注说明 |
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
| webhook_key | 访问URL：`/open/webhook/{webhook_key}` |
| secret | 设置后请求需携带签名头 X-Webhook-Signature |
| async_mode | 同步模式等待执行完成，异步模式立即返回logId |

---

## 触发方式

**同步触发**：`POST /open/webhook/{webhookKey}`
```json
{
  "data": { ... },
  "timestamp": 1711251200000,
  "signature": "sha256=xxx"
}
```

**异步触发**：同上，设置 `async_mode=1`

---

## 签名验证

如果配置了 secret，请求头需包含签名：
```
X-Webhook-Signature: sha256={HMAC-SHA256(requestBody, secret)}
```

---

## 示例数据

```json
{
  "id": 1,
  "webhook_key": "order_notify",
  "webhook_name": "订单通知Webhook",
  "flow_key": "order_process",
  "flow_name": "订单处理流程",
  "secret": "my-secret-key",
  "allowed_method": "POST",
  "async_mode": 0,
  "status": 1,
  "trigger_count": 156,
  "last_trigger_time": "2026-03-24T10:00:00",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
