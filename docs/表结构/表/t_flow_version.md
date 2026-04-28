# t_flow_version - 流程版本表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_info_id | INTEGER | NOT NULL | 关联流程信息ID |
| flow_key | TEXT | NOT NULL | 流程唯一标识Key |
| version | TEXT | NOT NULL | 版本号，如 v1, v2 |
| flow_content | TEXT | | 部署时的流程JSON快照 |
| status | INTEGER | DEFAULT 1 | 状态：0=禁用, 1=启用 |
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
| flow_info_id | 关联 t_flow_info 表，记录属于哪个流程 |
| version | 版本号，每次部署自增，支持回滚 |
| flow_content | 部署时的快照，修改流程定义不影响已部署版本 |
| status | 同一 flow_key 下只能有一个版本为启用状态 |

---

## 设计说明

- **版本管理**：每次部署生成新版本，支持版本回滚
- **快照机制**：flow_content 存储部署时的完整配置，与定义解耦
- **灰度发布**：可同时部署多个版本，通过切换状态实现灰度

---

## 示例数据

```json
{
  "id": 1,
  "flow_info_id": 1,
  "flow_key": "order_process",
  "version": "v1",
  "flow_content": "[{\"key\":\"start_1\",\"elementType\":\"START\",...}]",
  "status": 1,
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
