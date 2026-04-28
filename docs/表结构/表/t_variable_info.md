# t_variable_info - 变量信息表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| flow_definition_id | INTEGER | | 关联流程定义ID |
| flow_key | TEXT | | 流程Key |
| variable_code | TEXT | | 变量编码 |
| variable_name | TEXT | | 变量名称 |
| data_type | TEXT | | 数据类型：string / int / boolean / object / array |
| variable_type | TEXT | | 变量类型：input / output / env |
| default_value | TEXT | | 默认值 |
| description | TEXT | | 变量描述 |
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
| variable_code | 变量唯一标识，流程中通过 `input_xxx` / `output_xxx` / `env_xxx` 引用 |
| variable_name | 中文名称，用于展示 |
| variable_type | input=入参, output=出参, env=内部环境变量 |
| data_type | 决定数据类型和校验规则 |

---

## 变量类型说明

| variable_type | 说明 | 引用方式 |
|--------------|------|---------|
| input | 流程输入参数 | input_xxx |
| output | 流程输出结果 | output_xxx |
| env | 内部环境变量 | env_xxx |

---

## 示例数据

```json
{
  "id": 1,
  "flow_definition_id": 1,
  "flow_key": "user_login",
  "variable_code": "input_userName",
  "variable_name": "用户名",
  "data_type": "string",
  "variable_type": "input",
  "default_value": "",
  "description": "登录用户名",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
