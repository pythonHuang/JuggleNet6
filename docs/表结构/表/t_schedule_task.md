# t_schedule_task - 定时任务调度表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_key | TEXT | NOT NULL | 关联流程Key |
| flow_name | TEXT | | 流程名称（冗余） |
| cron_expression | TEXT | | Cron表达式 |
| input_json | TEXT | | 触发时的固定入参 JSON |
| status | INTEGER | DEFAULT 0 | 状态：0=暂停, 1=启用 |
| last_run_time | TEXT | | 上次执行时间 |
| last_run_status | TEXT | | 上次执行状态 |
| next_run_time | TEXT | | 下次执行时间（预计算） |
| run_count | INTEGER | DEFAULT 0 | 累计执行次数 |
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
| flow_key | 关联 t_flow_definition |
| cron_expression | 标准6位Cron表达式 |
| input_json | 固定入参，触发时作为流程入参 |
| next_run_time | 后台预计算，用于前端展示下次执行时间 |

---

## Cron 表达式说明

| 位置 | 含义 | 取值范围 |
|------|------|---------|
| 第1位 | 秒 | 0-59 |
| 第2位 | 分钟 | 0-59 |
| 第3位 | 小时 | 0-23 |
| 第4位 | 日期 | 1-31 |
| 第5位 | 月份 | 1-12 |
| 第6位 | 星期 | 0-6 (0=周日) |

---

## 常用 Cron 表达式示例

| 表达式 | 含义 |
|--------|------|
| `0 */5 * * * *` | 每5分钟执行一次 |
| `0 0 * * * *` | 每小时整点执行 |
| `0 0 0 * * *` | 每天零点执行 |
| `0 0 9 * * 1-5` | 工作日9点执行 |
| `0 0 8 * * *` | 每天8点执行 |

---

## 示例数据

```json
{
  "id": 1,
  "flow_key": "data_sync",
  "flow_name": "数据同步流程",
  "cron_expression": "0 */30 * * * *",
  "input_json": "{\"syncType\": \"full\"}",
  "status": 1,
  "last_run_time": "2026-03-24T10:30:00",
  "last_run_status": "SUCCESS",
  "next_run_time": "2026-03-24T11:00:00",
  "run_count": 128,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
