# t_system_config - 系统配置表

---

## 表结构

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| id | INTEGER | PK, AUTOINCREMENT | 主键 |
| config_key | TEXT | NOT NULL | 配置键（唯一） |
| config_value | TEXT | | 配置值 |
| config_name | TEXT | | 配置名称 |
| config_group | TEXT | | 配置分组 |
| remark | TEXT | | 备注说明 |
| deleted | INTEGER | DEFAULT 0 | 软删除标记 |
| tenant_id | INTEGER | | 所属租户ID（null=全局配置） |
| created_at | TEXT | | 创建时间 |
| created_by | INTEGER | | 创建人ID |
| updated_at | TEXT | | 更新时间 |
| updated_by | INTEGER | | 更新人ID |

---

## 字段说明

| 字段 | 说明 |
|------|------|
| config_key | 全局唯一配置键 |
| config_value | 配置值，字符串形式存储 |
| config_group | 分组标识，便于分类管理 |

---

## 配置分组示例

| config_group | 说明 |
|--------------|------|
| system | 系统配置 |
| email | 邮件配置 |
| alert | 告警配置 |
| sms | 短信配置 |

---

## 示例数据

```json
[
  {
    "id": 1,
    "config_key": "system.name",
    "config_value": "Juggle 接口编排平台",
    "config_name": "系统名称",
    "config_group": "system"
  },
  {
    "id": 2,
    "config_key": "alert.email.enabled",
    "config_value": "true",
    "config_name": "邮件告警启用",
    "config_group": "alert"
  },
  {
    "id": 3,
    "config_key": "alert.email.smtp_host",
    "config_value": "smtp.example.com",
    "config_name": "SMTP服务器",
    "config_group": "alert"
  }
]
```
