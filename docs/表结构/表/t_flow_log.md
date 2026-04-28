# t_flow_log - 流程执行日志表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_key | TEXT | | 流程Key |
| flow_name | TEXT | | 流程名称（冗余） |
| version | TEXT | | 版本号 |
| trigger_type | TEXT | | 触发方式：debug / open / schedule |
| status | TEXT | | 执行状态：SUCCESS / FAILED |
| start_time | TEXT | | 开始时间 |
| end_time | TEXT | | 结束时间 |
| cost_ms | INTEGER | | 耗时（毫秒） |
| error_message | TEXT | | 错误信息 |
| input_json | TEXT | | 输入参数 JSON |
| output_json | TEXT | | 输出结果 JSON |
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
| trigger_type | 区分触发来源，便于统计和分析 |
| status | 执行结果状态 |
| cost_ms | 性能监控指标 |
| input_json/output_json | 完整的输入输出数据，便于问题排查 |

---

## 触发类型枚举

| trigger_type | 说明 |
|--------------|------|
| debug | 调试模式触发 |
| open | 开放接口触发（/open/flow/trigger） |
| schedule | 定时任务触发 |
| webhook | Webhook触发 |
| test_case | 测试用例执行 |

---

## 示例数据

```json
{
  "id": 1,
  "flow_key": "order_process",
  "flow_name": "订单处理流程",
  "version": "v1",
  "trigger_type": "open",
  "status": "SUCCESS",
  "start_time": "2026-03-24T10:00:00",
  "end_time": "2026-03-24T10:00:05",
  "cost_ms": 5000,
  "input_json": "{\"orderId\": 12345}",
  "output_json": "{\"code\": 200, \"message\": \"success\"}",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
