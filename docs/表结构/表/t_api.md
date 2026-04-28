# t_api - API 接口表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| suite_code | TEXT | | 所属套件编码 |
| method_code | TEXT | | 接口方法编码 |
| method_name | TEXT | | 接口方法名称 |
| method_desc | TEXT | | 接口方法描述 |
| url | TEXT | | 请求地址 |
| request_type | TEXT | | 请求类型：GET / POST / PUT / DELETE |
| content_type | TEXT | | 内容类型：JSON / FORM |
| mock_json | TEXT | | Mock 数据 JSON |
| method_type | TEXT | DEFAULT 'HTTP' | 接口类型：HTTP / WEBSERVICE |
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
| suite_code | 关联所属套件 |
| method_code | 接口唯一编码 |
| url | 完整请求地址或相对路径 |
| request_type | HTTP方法类型 |
| mock_json | 启用Mock时的返回数据 |

---

## 索引

| 索引名 | 字段 | 类型 | 说明 |
|--------|------|------|------|
| idx_method_code | method_code | UNIQUE | 方法编码唯一索引 |

---

## 示例数据

```json
{
  "id": 1,
  "suite_code": "user_suite",
  "method_code": "getUserInfo",
  "method_name": "获取用户信息",
  "method_desc": "根据用户ID获取用户详细信息",
  "url": "https://api.example.com/user/info",
  "request_type": "GET",
  "content_type": "JSON",
  "method_type": "HTTP",
  "tenant_id": 1,
  "created_at": "2026-03-24T10:00:00"
}
```
