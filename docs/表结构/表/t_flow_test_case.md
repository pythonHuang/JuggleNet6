# t_flow_test_case - 流程测试用例表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_key | TEXT | NOT NULL | 关联流程Key |
| case_name | TEXT | NOT NULL | 用例名称 |
| input_json | TEXT | | 入参 JSON |
| assert_json | TEXT | | 断言 JSON |
| last_run_status | TEXT | | 最近执行状态 |
| last_run_time | TEXT | | 最近执行时间 |
| last_run_result | TEXT | | 最近执行结果摘要 |
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
| input_json | 测试入参，格式如 `{"userId": 123}` |
| assert_json | 断言配置，格式如 `{"result.code": 200, "data.token": "notNull"}` |
| last_run_status | SUCCESS / FAILED / PENDING |
| last_run_result | 最近一次执行的实际输出摘要 |

---

## 断言配置示例

```json
{
  "result.code": 200,
  "data.success": true,
  "data.token": "notNull",
  "data.userId": 12345
}
```

断言类型：
- 精确匹配：`"field": "expectedValue"`
- 非空验证：`"field": "notNull"`
- 正则匹配：`"field": "/^\\d+$/"`
- 包含关系：`"field": "contains:value"`

---

## 示例数据

```json
{
  "id": 1,
  "flow_key": "user_login",
  "case_name": "正常登录测试",
  "input_json": "{\"username\": \"test\", \"password\": \"123456\"}",
  "assert_json": "{\"code\": 200, \"data.token\": \"notNull\"}",
  "last_run_status": "SUCCESS",
  "last_run_time": "2026-03-24T10:00:00",
  "last_run_result": "{\"code\": 200, \"data\": {\"token\": \"abc123\"}}",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
