# t_flow_info - 流程信息表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_key | TEXT | NOT NULL | 流程唯一标识Key（关联定义） |
| flow_name | TEXT | | 流程名称 |
| flow_desc | TEXT | | 流程描述 |
| flow_type | TEXT | | 流程类型：sync / async |
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
| flow_key | 与 t_flow_definition.flow_key 一一对应 |
| status | 禁用后该流程无法被触发，但历史执行记录保留 |

---

## 设计说明

- **部署流程**时，从 t_flow_definition 复制基本信息到 t_flow_info
- t_flow_info 作为**运行时引用表**，记录已发布流程
- 多个版本共存时，flow_key 相同但 t_flow_version 不同

---

## 示例数据

```json
{
  "id": 1,
  "flow_key": "order_process",
  "flow_name": "订单处理流程",
  "flow_desc": "处理订单创建、支付、发货完整流程",
  "flow_type": "sync",
  "status": 1,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
