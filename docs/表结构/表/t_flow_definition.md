# t_flow_definition - 流程定义表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_key | TEXT | NOT NULL | 流程唯一标识Key |
| flow_name | TEXT | | 流程名称 |
| flow_desc | TEXT | | 流程描述 |
| flow_content | TEXT | | 流程节点JSON配置 |
| flow_type | TEXT | | 流程类型：sync（同步）/ async（异步） |
| group_name | TEXT | | 分组名称（便于分类管理） |
| status | INTEGER | DEFAULT 0 | 状态：0=草稿, 1=已部署 |
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
| flow_key | 全局唯一，用于API调用，如 `/open/flow/trigger/demo_flow` |
| flow_name | 流程显示名称 |
| flow_content | JSON格式的节点配置，部署时会复制到 t_flow_version |
| flow_type | sync=同步执行等待结果，async=异步执行立即返回实例ID |
| status | 草稿状态可编辑，部署后变为只读 |
| group_name | 支持流程分组，如"订单流程"、"用户流程" |

---

## 索引

| 索引名 | 字段 | 类型 | 说明 |
|--------|------|------|------|
| idx_flow_key | flow_key | UNIQUE | 流程Key唯一索引 |

---

## 示例数据

```json
{
  "id": 1,
  "flow_key": "order_process",
  "flow_name": "订单处理流程",
  "flow_desc": "处理订单创建、支付、发货完整流程",
  "flow_content": "[{\"key\":\"start_1\",\"elementType\":\"START\",...}]",
  "flow_type": "sync",
  "group_name": "订单流程",
  "status": 0,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
