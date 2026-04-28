# t_suite - 套件表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| suite_code | TEXT | | 套件编码（唯一） |
| suite_name | TEXT | | 套件名称 |
| suite_classify_id | INTEGER | | 套件分类ID |
| suite_image | TEXT | | 套件图标/封面 |
| suite_version | TEXT | | 套件版本号 |
| suite_desc | TEXT | | 套件描述 |
| suite_help_doc_json | TEXT | | 帮助文档 JSON |
| suite_flag | INTEGER | DEFAULT 0 | 标识：0=自定义, 1=官方 |
| deleted | INTEGER | DEFAULT 0 | 软删除标记 |
| tenant_id | INTEGER | | 所属租户ID（null=全局/官方） |
| created_at | TEXT | | 创建时间 |
| created_by | INTEGER | | 创建人ID |
| updated_at | TEXT | | 更新时间 |
| updated_by | INTEGER | | 更新人ID |

---

## 字段说明

| 字段 | 说明 |
|------|------|
| suite_code | 套件唯一编码，用于API调用 |
| suite_classify_id | 关联套件分类，便于分类管理 |
| suite_image | 套件展示图标URL |
| suite_version | 遵循语义化版本，如 1.0.0 |
| suite_flag | 区分官方套件和用户自定义套件 |

---

## 索引

| 索引名 | 字段 | 类型 | 说明 |
|--------|------|------|------|
| idx_suite_code | suite_code | UNIQUE | 套件编码唯一索引 |

---

## 示例数据

```json
{
  "id": 1,
  "suite_code": "user_suite",
  "suite_name": "用户管理套件",
  "suite_classify_id": 1,
  "suite_image": "https://example.com/user.png",
  "suite_version": "1.0.0",
  "suite_desc": "提供用户注册、登录、权限管理等功能",
  "suite_flag": 1,
  "tenant_id": null,
  "created_at": "2026-03-24T10:00:00"
}
```
