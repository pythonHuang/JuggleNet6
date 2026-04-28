# t_parameter - 参数表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| owner_id | INTEGER | | 归属者ID（API ID / Object ID 等） |
| owner_code | TEXT | | 归属者编码 |
| param_type | INTEGER | NOT NULL | 参数类型 |
| param_code | TEXT | | 参数编码 |
| param_name | TEXT | | 参数名称 |
| data_type | TEXT | | 数据类型 |
| object_code | TEXT | | 引用对象编码（DataType=object时） |
| required | INTEGER | DEFAULT 0 | 是否必填：0=否, 1=是 |
| default_value | TEXT | | 默认值 |
| description | TEXT | | 参数描述 |
| sort_num | INTEGER | DEFAULT 0 | 排序号 |
| deleted | INTEGER | DEFAULT 0 | 软删除标记 |
| tenant_id | INTEGER | | 所属租户ID |
| created_at | TEXT | | 创建时间 |
| created_by | INTEGER | | 创建人ID |
| updated_at | TEXT | | 更新时间 |
| updated_by | INTEGER | | 更新人ID |

---

## 参数类型枚举

| param_type | 说明 | 归属者 |
|------------|------|--------|
| 1 | API入参 | t_api |
| 2 | API出参 | t_api |
| 3 | 对象属性 | t_object |
| 4 | API Header | t_api |
| 5 | 流程入参 | t_flow_definition |
| 6 | 流程出参 | t_flow_definition |

---

## 数据类型枚举

| data_type | 说明 |
|-----------|------|
| string | 字符串 |
| int | 整数 |
| double | 浮点数 |
| boolean | 布尔值 |
| date | 日期 |
| datetime | 日期时间 |
| array | 数组 |
| object | 对象（引用 t_object） |
| json | JSON字符串 |

---

## 示例数据

API入参示例：
```json
{
  "id": 1,
  "owner_id": 1,
  "owner_code": "user_suite.getUserInfo",
  "param_type": 1,
  "param_code": "userId",
  "param_name": "用户ID",
  "data_type": "int",
  "required": 1,
  "description": "用户唯一标识",
  "sort_num": 1
}
```

流程入参示例：
```json
{
  "id": 10,
  "owner_id": 1,
  "owner_code": "order_process",
  "param_type": 5,
  "param_code": "orderId",
  "param_name": "订单ID",
  "data_type": "int",
  "required": 1
}
```
