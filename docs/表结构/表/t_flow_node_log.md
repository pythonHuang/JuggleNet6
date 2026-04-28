# t_flow_node_log - 流程节点执行明细日志表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_log_id | INTEGER | NOT NULL | 关联流程日志ID |
| node_key | TEXT | | 节点Key |
| node_label | TEXT | | 节点标签/名称 |
| node_type | TEXT | | 节点类型 |
| seq_no | INTEGER | | 执行顺序（从1开始） |
| status | TEXT | | 执行状态：SUCCESS / FAILED / SKIPPED |
| start_time | TEXT | | 开始时间 |
| end_time | TEXT | | 结束时间 |
| cost_ms | INTEGER | | 耗时（毫秒） |
| input_snapshot | TEXT | | 节点输入（变量快照 JSON） |
| output_snapshot | TEXT | | 节点输出（变量快照 JSON） |
| detail | TEXT | | 日志详情（SQL/HTTP请求等） |
| error_message | TEXT | | 错误信息 |
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
| flow_log_id | 关联 t_flow_log 表，标识属于哪次流程执行 |
| node_type | 节点类型：START/END/METHOD/CONDITION/ASSIGN/CODE/MYSQL等 |
| seq_no | 执行顺序，用于还原执行路径 |
| input_snapshot | 进入节点时的变量状态快照 |
| output_snapshot | 离开节点时的变量状态快照 |
| detail | 详细信息，如HTTP请求体、SQL语句等 |

---

## 节点类型枚举

| node_type | 说明 |
|-----------|------|
| START | 开始节点 |
| END | 结束节点 |
| METHOD | HTTP API调用节点 |
| CONDITION | 条件分支节点 |
| ASSIGN | 变量赋值节点 |
| CODE | 代码执行节点 |
| MYSQL | 数据库操作节点 |
| MERGE | 聚合节点 |
| DELAY | 延时节点 |
| PARALLEL | 并行执行节点 |
| LOOP | 循环节点 |
| SUB_FLOW | 子流程调用节点 |
| NOTIFY | 通知节点 |

---

## 示例数据

```json
{
  "id": 1,
  "flow_log_id": 1,
  "node_key": "method_order",
  "node_label": "查询订单",
  "node_type": "METHOD",
  "seq_no": 2,
  "status": "SUCCESS",
  "start_time": "2026-03-24T10:00:01",
  "end_time": "2026-03-24T10:00:03",
  "cost_ms": 2000,
  "input_snapshot": "{\"orderId\": 12345}",
  "output_snapshot": "{\"orderId\": 12345, \"orderStatus\": \"paid\"}",
  "detail": "HTTP POST https://api.example.com/order/get",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:01"
}
```
