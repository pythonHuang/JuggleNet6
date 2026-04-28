# t_base - 基础实体基类

> 所有业务实体表均包含以下公共字段

---

## 公共字段说明

| 字段名 | 类型 | 说明 |
|--------|------|------|
| id | INTEGER | 主键，自增 |
| deleted | INTEGER | 软删除标记：0=正常, 1=已删除 |
| tenant_id | INTEGER | 所属租户ID，null=全局数据（不隔离） |
| created_at | TEXT | 创建时间（ISO 8601 格式） |
| created_by | INTEGER | 创建人用户ID |
| updated_at | TEXT | 更新时间（ISO 8601 格式） |
| updated_by | INTEGER | 更新人用户ID |

---

## 说明

`t_base` 不是独立的数据表，而是**所有业务表的公共基类字段**。

### 设计理念

1. **软删除**：使用 `deleted` 字段实现逻辑删除，数据不物理删除，方便数据恢复和审计
2. **租户隔离**：`tenant_id` 字段支持多租户数据隔离，null 表示全局数据
3. **审计字段**：`created_at/updated_at` 记录数据生命周期，`created_by/updated_by` 记录操作人

### 继承关系

```
BaseEntity (抽象基类)
    ├── UserEntity (用户)
    ├── TenantEntity (租户)
    ├── RoleEntity (角色)
    ├── RoleMenuEntity (角色菜单)
    ├── SuiteEntity (套件)
    ├── ApiEntity (API接口)
    ├── ObjectEntity (自定义对象)
    ├── ParameterEntity (参数)
    ├── FlowDefinitionEntity (流程定义)
    ├── FlowInfoEntity (流程信息)
    ├── FlowVersionEntity (流程版本)
    ├── FlowLogEntity (流程日志)
    ├── FlowNodeLogEntity (节点日志)
    ├── FlowTestCaseEntity (测试用例)
    ├── VariableInfoEntity (变量)
    ├── StaticVariableEntity (静态变量)
    ├── TokenEntity (Token)
    ├── TokenPermissionEntity (Token权限)
    ├── DataSourceEntity (数据源)
    ├── SystemConfigEntity (系统配置)
    ├── WebhookEntity (Webhook)
    ├── ScheduleTaskEntity (定时任务)
    ├── LoginLogEntity (登录日志)
    └── AuditLogEntity (审计日志)
```
