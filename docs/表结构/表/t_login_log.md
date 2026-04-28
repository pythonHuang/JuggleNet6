# t_login_log - 登录日志表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| user_id | INTEGER | | 登录用户ID |
| user_name | TEXT | | 登录用户名 |
| login_type | TEXT | DEFAULT 'login' | 登录类型：login / logout |
| result | TEXT | DEFAULT 'success' | 登录结果：success / fail |
| ip_address | TEXT | | 登录IP地址 |
| user_agent | TEXT | | 浏览器User-Agent |
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
| login_type | 区分登录和登出行为 |
| result | 记录登录成功/失败，便于安全审计 |
| ip_address | 记录登录来源IP，支持异常登录告警 |
| user_agent | 记录浏览器信息，支持设备识别 |

---

## 使用场景

- 用户登录审计
- 异常登录检测（如异地登录）
- 用户活跃度统计
- 安全合规要求

---

## 示例数据

```json
{
  "id": 1,
  "user_id": 1,
  "user_name": "admin",
  "login_type": "login",
  "result": "success",
  "ip_address": "192.168.1.100",
  "user_agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)...",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
