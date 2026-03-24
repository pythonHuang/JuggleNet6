# Juggle 接口编排平台 - 系统详细设计文档

> 版本：v1.0  
> 日期：2026-03-24  
> 技术栈：ASP.NET Core 6 + Vue3 + SQLite + EF Core

---

## 一、数据库详细设计

### 1.1 t_user（用户表）

```sql
CREATE TABLE t_user (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    user_name   TEXT(30),
    password    TEXT(60),       -- MD5 加密
    deleted     INTEGER DEFAULT 0,
    created_at  TEXT,           -- ISO 8601 datetime
    created_by  INTEGER,
    updated_at  TEXT,
    updated_by  INTEGER
);
-- 初始数据：juggle / juggle (MD5: 24cb6bcbc65730e9650745d379613563)
```

### 1.2 t_suite（套件表）

```sql
CREATE TABLE t_suite (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    suite_code           TEXT(60),       -- 套件唯一标识
    suite_name           TEXT(30),       -- 套件名称
    suite_classify_id    INTEGER,        -- 分类ID（可为空）
    suite_image          TEXT,           -- 图片（Base64 或 URL）
    suite_version        TEXT(10),       -- 版本号（如 v1.0.0）
    suite_desc           TEXT(140),      -- 描述
    suite_help_doc_json  TEXT(300),      -- 帮助文档 JSON
    suite_flag           INTEGER,        -- 0:自定义, 1:官方
    deleted              INTEGER DEFAULT 0,
    created_at           TEXT,
    created_by           INTEGER,
    updated_at           TEXT,
    updated_by           INTEGER
);
```

### 1.3 t_api（API 接口表）

```sql
CREATE TABLE t_api (
    id                       INTEGER PRIMARY KEY AUTOINCREMENT,
    suite_id                 INTEGER,        -- 所属套件 ID
    api_code                 TEXT(100),      -- 接口编码（URL Path + Method 的 MD5）
    api_protocol             TEXT(10),       -- 协议（HTTP/HTTPS）
    api_url                  TEXT(150),      -- 接口完整地址
    api_name                 TEXT(50),       -- 接口名称
    api_desc                 TEXT(200),      -- 接口描述
    api_request_type         TEXT(10),       -- 请求类型（GET/POST/PUT/DELETE）
    api_request_content_type TEXT(40),       -- 请求体类型
    deleted                  INTEGER DEFAULT 0,
    created_at               TEXT,
    created_by               INTEGER,
    updated_at               TEXT,
    updated_by               INTEGER
);
```

### 1.4 t_parameter（参数表）

统一管理 API 入参/出参/Header、对象属性、流程入参/出参：

```sql
CREATE TABLE t_parameter (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    param_type   INTEGER(2),     -- 1:API入参, 2:API出参, 3:对象属性, 4:Header, 5:流程入参, 6:流程出参
    param_key    TEXT(40),       -- 参数 key
    param_name   TEXT(40),       -- 参数名称
    param_position TEXT(20),     -- 参数位置（query/body/header/path）
    param_desc   TEXT(200),      -- 参数描述
    data_type    TEXT,           -- 数据类型 JSON（如 {"type":"String"}）
    required     INTEGER(2),     -- 是否必填（1:是, 0:否）
    source_type  TEXT(8),        -- 来源类型（api/object/flow）
    source_id    INTEGER,        -- 来源 ID（ApiId/ObjectId/FlowDefinitionId）
    deleted      INTEGER DEFAULT 0,
    created_at   TEXT,
    created_by   INTEGER,
    updated_at   TEXT,
    updated_by   INTEGER
);
```

**param_type 枚举值：**

| 值 | 说明 |
|----|------|
| 1 | API 接口入参 |
| 2 | API 接口出参 |
| 3 | 对象属性 |
| 4 | API Header |
| 5 | 流程入参（source_type=flow） |
| 6 | 流程出参（source_type=flow） |

**data_type JSON 结构：**

```json
// 基础类型
{"type": "String"}
{"type": "Integer"}
{"type": "Double"}
{"type": "Boolean"}
{"type": "Date"}

// 集合类型
{"type": "List", "itemType": "String"}
{"type": "List", "itemType": "Object", "objectKey": "OrderDTO", "objectStructure": [...]}

// 对象类型
{"type": "Object", "objectKey": "OrderDTO", "objectStructure": [
  {"propKey": "orderNo", "propName": "订单号", "dataType": {"type": "String"}},
  {"propKey": "userId", "propName": "用户ID", "dataType": {"type": "Integer"}}
]}
```

### 1.5 t_object（自定义对象表）

```sql
CREATE TABLE t_object (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    object_key  TEXT(30),       -- 对象唯一标识（全局唯一）
    object_name TEXT(50),       -- 对象名称
    object_desc TEXT(200),      -- 描述
    deleted     INTEGER DEFAULT 0,
    created_at  TEXT,
    created_by  INTEGER,
    updated_at  TEXT,
    updated_by  INTEGER
);
-- 对象属性存储在 t_parameter 中（param_type=3, source_type='object', source_id=ObjectId）
```

### 1.6 t_flow_definition（流程定义表）

```sql
CREATE TABLE t_flow_definition (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    flow_key     TEXT(20),       -- 流程唯一标识（全局唯一，自动生成）
    flow_name    TEXT(60),       -- 流程名称
    flow_type    TEXT(8),        -- 流程类型（sync/async）
    flow_content TEXT,           -- 流程内容（节点列表 JSON）
    remark       TEXT(200),      -- 备注
    deleted      INTEGER DEFAULT 0,
    created_at   TEXT,
    created_by   INTEGER,
    updated_at   TEXT,
    updated_by   INTEGER
);
-- 流程入参/出参存储在 t_parameter 中（source_type='flow', source_id=FlowDefinitionId）
```

### 1.7 t_variable_info（变量信息表）

```sql
CREATE TABLE t_variable_info (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    flow_definition_id   INTEGER,        -- 所属流程定义 ID
    variable_key         TEXT(30),       -- 变量唯一 key
    variable_name        TEXT(30),       -- 变量名称
    variable_type        INTEGER(1),     -- 1:输入变量, 2:输出变量, 3:环境变量
    data_type            TEXT,           -- 数据类型 JSON
    created_at           TEXT,
    created_by           INTEGER,
    updated_at           TEXT,
    updated_by           INTEGER
);
```

### 1.8 t_flow_info（流程信息表）

流程定义部署后生成：

```sql
CREATE TABLE t_flow_info (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    flow_key    TEXT(20),       -- 流程唯一标识（与 flow_definition 一致）
    flow_name   TEXT(60),
    flow_type   TEXT(8),
    remark      TEXT(200),
    deleted     INTEGER DEFAULT 0,
    created_at  TEXT,
    created_by  INTEGER,
    updated_at  TEXT,
    updated_by  INTEGER
);
```

### 1.9 t_flow_version（流程版本表）

```sql
CREATE TABLE t_flow_version (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    flow_id              INTEGER,        -- 所属 FlowInfo ID
    flow_version         TEXT(8),        -- 版本号（v1, v2, ...）
    flow_version_status  INTEGER DEFAULT 0,  -- 0:禁用, 1:启用
    flow_version_remark  TEXT(200),
    flow_content         TEXT,           -- 部署时的流程内容快照
    inputs               TEXT,           -- 入参 JSON 快照
    outputs              TEXT,           -- 出参 JSON 快照
    variables            TEXT,           -- 变量 JSON 快照
    deleted              INTEGER DEFAULT 0,
    created_at           TEXT,
    created_by           INTEGER,
    updated_at           TEXT,
    updated_by           INTEGER
);
```

### 1.10 t_token（访问令牌表）

```sql
CREATE TABLE t_token (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    token_value TEXT(150),      -- 令牌值（UUID 生成）
    token_desc  TEXT(200),
    deleted     INTEGER DEFAULT 0,
    created_at  TEXT,
    created_by  INTEGER,
    updated_at  TEXT,
    updated_by  INTEGER
);
```

### 1.11 t_data_source（数据源表）

```sql
CREATE TABLE t_data_source (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    data_source_name     TEXT(150),
    data_source_type     TEXT(20),       -- 数据库类型（MySQL/PostgreSQL）
    data_source_desc     TEXT(200),
    address              TEXT(200),      -- 主机地址
    port                 TEXT(10),
    user_name            TEXT(40),
    password             TEXT(40),       -- 建议加密存储
    database_name        TEXT(40),
    connect_ext_info     TEXT(200),      -- 额外连接参数
    min_pool_size        INTEGER,
    max_pool_size        INTEGER,
    query_timeout        INTEGER,
    data_source_ext_info TEXT(200),
    deleted              INTEGER DEFAULT 0,
    created_at           TEXT,
    created_by           INTEGER,
    updated_at           TEXT,
    updated_by           INTEGER
);
```

---

## 二、后端 API 详细设计

### 2.1 统一响应格式

**成功响应：**
```json
{
  "code": 200,
  "message": "success",
  "data": { }
}
```

**分页响应：**
```json
{
  "code": 200,
  "message": "success",
  "total": 100,
  "data": [ ]
}
```

**错误响应：**
```json
{
  "code": 40001,
  "message": "流程定义不存在",
  "data": null
}
```

### 2.2 错误码规范

| 错误码区间 | 说明 |
|-----------|------|
| 200 | 成功 |
| 40001 ~ 40099 | 流程相关错误 |
| 40101 ~ 40199 | API/套件相关错误 |
| 40201 ~ 40299 | 对象相关错误 |
| 40301 ~ 40399 | 系统相关错误 |
| 50000 | 服务器内部错误 |

### 2.3 认证 JWT 设计

**登录接口：**
```
POST /api/user/login
Body: { "userName": "juggle", "password": "juggle" }
Response: { "code": 200, "data": { "token": "eyJ..." } }
```

**后续请求携带 Token：**
```
Authorization: Bearer eyJ...
```

**JWT Payload：**
```json
{
  "userId": 1,
  "userName": "juggle",
  "exp": 1704067200
}
```

---

## 三、核心业务流程详细设计

### 3.1 流程设计到部署完整流程

```
1. 创建流程定义
   POST /api/flow/definition/add
   Body: { flowName, flowType, remark, inputs[], outputs[] }
   → 生成 flow_key（格式：flowType_随机10位）
   → 初始化默认流程内容（开始节点 + 结束节点）
   → 保存入参/出参到 t_parameter（source_type='flow'）

2. 流程设计（拖拽编辑）
   PUT /api/flow/definition/save
   Body: { flowDefinitionId, flowContent, variableList[] }
   → 更新 flow_content（JSON 节点列表）
   → 更新/删除 t_variable_info

3. 调试流程
   POST /api/flow/definition/debug/{flowKey}
   Body: { flowData: { input_userName: "juggle", ... } }
   → 直接基于 flow_definition 中的 flow_content 执行流程引擎
   → 返回执行结果

4. 部署流程
   POST /api/flow/definition/deploy
   Body: { flowDefinitionId, versionRemark }
   → 读取 flow_definition 的当前内容
   → 在 t_flow_info 中创建/找到对应记录
   → 计算新版本号（v1, v2, ...）
   → 在 t_flow_version 中创建新版本（快照 inputs/outputs/variables/flow_content）
   → 新版本默认禁用（flow_version_status=0）
```

### 3.2 流程执行（触发）详细设计

```
触发接口：POST /api/flow/version/trigger/{version}/{key}
开放接口：POST /open/flow/trigger/{version}/{key}

执行步骤：
1. 根据 flow_key + flow_version 查询 t_flow_version
2. 检查版本状态（必须为启用状态）
3. 反序列化 flow_content → List<FlowElement>
4. 反序列化 variables → 初始化变量上下文
5. 反序列化 inputs → 验证并填充输入变量到上下文
6. 从 START 节点开始执行（findNextNode）
7. 循环执行节点直到 END：
   - METHOD: 发起 HTTP 请求，结果填充变量
   - CONDITION: 计算条件表达式，找到下一个节点
8. 从变量上下文中读取 outputs 对应的变量值
9. 构建并返回 FlowResult
```

### 3.3 流程引擎 C# 核心类设计

```csharp
// 流程执行上下文
public class FlowExecutionContext
{
    public string FlowKey { get; set; }
    public string FlowVersion { get; set; }
    public Dictionary<string, object> Variables { get; set; }  // 运行时变量
    public List<FlowElement> Elements { get; set; }            // 节点列表
}

// 基础节点
public abstract class FlowElement
{
    public string Key { get; set; }
    public string Name { get; set; }
    public ElementType ElementType { get; set; }
    public List<string> Incomings { get; set; }
    public List<string> Outgoings { get; set; }
}

// 方法节点
public class MethodNode : FlowElement
{
    public MethodConfig Method { get; set; }
}

public class MethodConfig
{
    public string SuiteCode { get; set; }
    public string MethodCode { get; set; }
    public string Url { get; set; }
    public string RequestType { get; set; }
    public string RequestContentType { get; set; }
    public List<InputFillRule> InputFillRules { get; set; }
    public List<OutputFillRule> OutputFillRules { get; set; }
    public List<HeaderFillRule> HeaderFillRules { get; set; }
}

// 条件节点
public class ConditionNode : FlowElement
{
    public List<ConditionItem> Conditions { get; set; }
}

public class ConditionItem
{
    public string ConditionName { get; set; }
    public string ConditionType { get; set; }  // CUSTOM / DEFAULT
    public string Outgoing { get; set; }
    public string Expression { get; set; }
    public List<List<ConditionExpression>> ConditionExpressions { get; set; }
}

// 填充规则
public class FillRule
{
    public string Source { get; set; }
    public string SourceType { get; set; }   // VARIABLE / CONSTANT / OUTPUT_PARAM / INPUT_PARAM
    public DataType SourceDataType { get; set; }
    public string Target { get; set; }
    public string TargetType { get; set; }
    public DataType TargetDataType { get; set; }
}

// 流程引擎
public class FlowEngine
{
    public async Task<FlowResult> ExecuteAsync(FlowExecutionContext context);
    private FlowElement FindStartNode(List<FlowElement> elements);
    private FlowElement FindNextNode(FlowElement current, FlowExecutionContext context);
    private async Task ExecuteMethodNodeAsync(MethodNode node, FlowExecutionContext context);
    private FlowElement ExecuteConditionNode(ConditionNode node, FlowExecutionContext context);
    private bool EvaluateExpression(string expression, Dictionary<string, object> variables);
}
```

### 3.4 HTTP API 调用器设计

```csharp
public class HttpApiCaller
{
    // 根据 API 配置和入参发起 HTTP 请求
    public async Task<Dictionary<string, object>> CallAsync(
        string url,
        string requestType,        // GET/POST/PUT/DELETE
        string contentType,        // application/json / application/x-www-form-urlencoded
        Dictionary<string, string> headers,
        Dictionary<string, object> queryParams,
        Dictionary<string, object> bodyParams
    );
}
```

调用逻辑：
1. 根据 inputFillRules 从变量上下文提取参数
2. 根据参数 position（query/body/header）分类
3. 发起 HTTP 请求
4. 解析响应 JSON
5. 根据 outputFillRules 将响应字段写入变量上下文

### 3.5 条件表达式求值

支持的表达式类型：

| 操作符 | 数据类型 | 示例 |
|--------|---------|------|
| equal | 所有 | env_name=="张三" |
| notEqual | 所有 | env_name!="张三" |
| greaterThan | 数字/日期 | env_age>18 |
| greaterThanOrEqual | 数字/日期 | env_age>=18 |
| lessThan | 数字/日期 | env_age<18 |
| lessThanOrEqual | 数字/日期 | env_age<=18 |
| isEmpty | 字符串/集合 | string.empty(env_name) |
| isNotEmpty | 字符串/集合 | !string.empty(env_name) |
| contains | 字符串 | string.contains(s1,s2) |
| notContains | 字符串 | !string.contains(s1,s2) |

C# 实现方案：使用 `System.Linq.Dynamic.Core` 库或手动解析 `conditionExpressions` 数组。

---

## 四、前端详细设计

### 4.1 流程设计器核心交互

**节点列表（左侧面板）：**
- 开始节点（只能有一个）
- 结束节点
- 方法节点（调用 API）
- 条件节点（多分支判断）

**画布区：**
- 节点拖拽放置
- 节点连线（incomings/outgoings）
- 节点点击展开配置面板

**配置面板（右侧抽屉）：**
- 方法节点：选择套件 → 选择 API → 配置输入规则 → 配置输出规则 → 配置 Header
- 条件节点：配置多个条件分支 + 每个分支的表达式

**左侧菜单：**
- 流程入参/出参配置
- 变量管理（添加/删除变量）

### 4.2 数据类型选择器

```typescript
// 数据类型树形结构
interface DataType {
  type: 'String' | 'Integer' | 'Double' | 'Boolean' | 'Date' | 'List' | 'Object';
  itemType?: string;        // List 的元素类型
  objectKey?: string;       // Object 的对象 key
  objectStructure?: Property[];  // Object 的属性列表
}
```

### 4.3 参数配置组件（ParamSetting）

通用参数配置表格，支持：
- 新增/删除参数行
- 参数 key、名称、数据类型、是否必填
- 数据类型支持级联选择（基础类型/集合类型/对象类型）

### 4.4 输入规则配置（InputRuleSetting）

用于配置「变量 → API 入参」或「常量 → API 入参」的映射：

```typescript
interface InputFillRule {
  source: string;           // 来源（变量 key 或常量值）
  sourceType: 'VARIABLE' | 'CONSTANT';
  sourceDataType?: DataType;
  target: string;           // 目标 API 入参 key
  targetType: 'INPUT_PARAM';
  targetDataType: DataType;
}
```

### 4.5 输出规则配置（OutputRuleSetting）

用于配置「API 出参 → 变量」的映射：

```typescript
interface OutputFillRule {
  source: string;           // 来源 API 出参 key
  sourceType: 'OUTPUT_PARAM';
  sourceDataType: DataType;
  target: string;           // 目标变量 key
  targetType: 'VARIABLE';
  targetDataType: DataType;
}
```

---

## 五、接口详细设计

### 5.1 用户登录

**接口：** `POST /api/user/login`

**请求：**
```json
{
  "userName": "juggle",
  "password": "juggle"
}
```

**响应：**
```json
{
  "code": 200,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": 1,
    "userName": "juggle"
  }
}
```

### 5.2 创建流程定义

**接口：** `POST /api/flow/definition/add`

**请求：**
```json
{
  "flowName": "用户登录送礼品",
  "flowType": "sync",
  "remark": "示例流程",
  "inputs": [
    { "paramKey": "userName", "paramName": "用户名", "dataType": {"type": "String"}, "required": true }
  ],
  "outputs": [
    { "paramKey": "orderName", "paramName": "订单名称", "dataType": {"type": "String"} }
  ]
}
```

**响应：**
```json
{
  "code": 200,
  "data": true
}
```

### 5.3 保存流程内容

**接口：** `PUT /api/flow/definition/save`

**请求：**
```json
{
  "flowDefinitionId": 1,
  "flowContent": "[{...节点列表JSON...}]",
  "variableList": [
    {
      "variableKey": "env_isLogin",
      "variableName": "是否登录成功",
      "variableType": 3,
      "dataType": {"type": "Boolean"}
    }
  ]
}
```

### 5.4 调试流程

**接口：** `POST /api/flow/definition/debug/{flowKey}`

**请求：**
```json
{
  "flowData": {
    "userName": "juggle",
    "password": "123456",
    "deposit": 50000.0
  }
}
```

**响应：**
```json
{
  "code": 200,
  "data": {
    "success": true,
    "result": {
      "userName": "juggle",
      "age": 18,
      "orderName": "送10元话费"
    },
    "errorMessage": null
  }
}
```

### 5.5 部署流程

**接口：** `POST /api/flow/definition/deploy`

**请求：**
```json
{
  "flowDefinitionId": 1,
  "versionRemark": "第一版"
}
```

**响应：**
```json
{
  "code": 200,
  "data": true
}
```

### 5.6 触发流程（开放接口）

**接口：** `POST /open/flow/trigger/{version}/{key}`

**示例：** `POST /open/flow/trigger/v1/sync_abcdefghij`

**请求头：**
```
X-Juggle-Token: your-api-token
Content-Type: application/json
```

**请求：**
```json
{
  "flowData": {
    "userName": "juggle",
    "password": "123456",
    "deposit": 200000.0
  }
}
```

**同步响应：**
```json
{
  "code": 200,
  "data": {
    "success": true,
    "result": {
      "userName": "juggle",
      "age": 18,
      "orderName": "送一双耐克的鞋"
    }
  }
}
```

**异步响应：**
```json
{
  "code": 200,
  "data": {
    "instanceId": "flow-instance-uuid"
  }
}
```

### 5.7 新增套件

**接口：** `POST /api/suite/add`

**请求：**
```json
{
  "suiteName": "微信服务",
  "suiteVersion": "v1.0.0",
  "suiteDesc": "微信相关接口套件",
  "suiteImage": "base64..."
}
```

### 5.8 新增 API 接口

**接口：** `POST /api/api/add`

**请求：**
```json
{
  "suiteId": 1,
  "apiName": "发送模板消息",
  "apiUrl": "https://api.weixin.qq.com/cgi-bin/message/template/send",
  "apiRequestType": "POST",
  "apiRequestContentType": "application/json",
  "apiDesc": "微信模板消息推送",
  "inputs": [
    { "paramKey": "touser", "paramName": "目标用户", "dataType": {"type": "String"}, "position": "body", "required": true }
  ],
  "outputs": [
    { "paramKey": "msgid", "paramName": "消息ID", "dataType": {"type": "Integer"} }
  ],
  "headers": [
    { "headerKey": "access_token", "headerName": "AccessToken", "defaultValue": "" }
  ]
}
```

### 5.9 调试 API

**接口：** `POST /api/api/debug/{apiId}`

**请求：**
```json
{
  "inputParams": {
    "userId": 1
  },
  "headers": {
    "Authorization": "Bearer xxx"
  }
}
```

**响应：**
```json
{
  "code": 200,
  "data": {
    "id": 1,
    "name": "张三",
    "age": 18,
    "birthday": "2000-01-01"
  }
}
```

---

## 六、C# 项目结构详细设计

### 6.1 后端项目结构

```
JuggleNet6.Backend/
├── Program.cs                          # 应用入口，最小托管模型
├── appsettings.json                    # 配置文件
├── JuggleNet6.Backend.csproj
│
├── Controllers/                        # 接口层
│   ├── Api/
│   │   ├── FlowDefinitionController.cs
│   │   ├── FlowInfoController.cs
│   │   ├── FlowVersionController.cs
│   │   ├── ApiController.cs
│   │   ├── SuiteController.cs
│   │   ├── ObjectController.cs
│   │   ├── TokenController.cs
│   │   ├── DataSourceController.cs
│   │   ├── UserController.cs
│   │   └── DataTypeInfoController.cs
│   ├── Open/
│   │   └── FlowOpenController.cs
│   └── Example/
│       ├── UserExampleController.cs
│       ├── GoodsExampleController.cs
│       └── OrderExampleController.cs
│
├── Services/                           # 应用服务层（接口 + 实现）
│   ├── Flow/
│   │   ├── IFlowDefinitionService.cs
│   │   ├── FlowDefinitionService.cs
│   │   ├── IFlowVersionService.cs
│   │   ├── FlowVersionService.cs
│   │   └── IFlowInfoService.cs
│   ├── Suite/
│   │   ├── IApiService.cs
│   │   ├── ApiService.cs
│   │   ├── ISuiteService.cs
│   │   └── SuiteService.cs
│   ├── IObjectService.cs
│   ├── ObjectService.cs
│   ├── ITokenService.cs
│   ├── TokenService.cs
│   ├── IDataSourceService.cs
│   ├── DataSourceService.cs
│   └── IUserService.cs
│
├── Domain/                             # 领域层
│   ├── Flow/
│   │   ├── FlowDefinition.cs           # 流程定义领域模型
│   │   ├── FlowVersion.cs
│   │   ├── FlowInfo.cs
│   │   └── FlowTypeEnum.cs
│   ├── Suite/
│   │   ├── Api.cs
│   │   ├── Suite.cs
│   │   └── RequestTypeEnum.cs
│   ├── Object.cs
│   ├── Parameter.cs
│   ├── VariableInfo.cs
│   ├── Token.cs
│   ├── DataSource.cs
│   └── Engine/                         # 流程引擎核心
│       ├── FlowEngine.cs
│       ├── FlowExecutionContext.cs
│       ├── Models/
│       │   ├── FlowElement.cs
│       │   ├── StartNode.cs
│       │   ├── EndNode.cs
│       │   ├── MethodNode.cs
│       │   ├── ConditionNode.cs
│       │   ├── MethodConfig.cs
│       │   ├── FillRule.cs
│       │   ├── DataType.cs
│       │   ├── Variable.cs
│       │   └── FlowResult.cs
│       ├── Executors/
│       │   ├── INodeExecutor.cs
│       │   ├── StartNodeExecutor.cs
│       │   ├── EndNodeExecutor.cs
│       │   ├── MethodNodeExecutor.cs
│       │   └── ConditionNodeExecutor.cs
│       └── Expression/
│           └── ExpressionEvaluator.cs
│
├── Infrastructure/                     # 基础设施层
│   ├── Persistence/
│   │   ├── JuggleDbContext.cs           # EF Core DbContext
│   │   ├── Entities/                    # 数据库实体（与 Domain 分离）
│   │   │   ├── UserEntity.cs
│   │   │   ├── SuiteEntity.cs
│   │   │   ├── ApiEntity.cs
│   │   │   ├── ParameterEntity.cs
│   │   │   ├── ObjectEntity.cs
│   │   │   ├── FlowDefinitionEntity.cs
│   │   │   ├── VariableInfoEntity.cs
│   │   │   ├── FlowInfoEntity.cs
│   │   │   ├── FlowVersionEntity.cs
│   │   │   ├── TokenEntity.cs
│   │   │   └── DataSourceEntity.cs
│   │   └── Repositories/               # 仓储实现
│   │       ├── FlowDefinitionRepository.cs
│   │       ├── FlowVersionRepository.cs
│   │       └── ...
│   ├── Http/
│   │   └── HttpApiCaller.cs
│   └── Common/
│       ├── JsonHelper.cs
│       └── Md5Helper.cs
│
├── DTOs/                               # 数据传输对象
│   ├── Request/                        # 请求 DTO
│   │   ├── LoginRequest.cs
│   │   ├── FlowDefinitionAddRequest.cs
│   │   └── ...
│   └── Response/                       # 响应 DTO
│       ├── ApiResult.cs                # 统一响应包装
│       ├── PageResult.cs               # 分页响应
│       ├── FlowDefinitionDto.cs
│       └── ...
│
├── Middleware/                         # 中间件
│   ├── AuthMiddleware.cs               # JWT 认证
│   └── ExceptionMiddleware.cs          # 全局异常处理
│
└── Migrations/                         # EF Core 迁移文件
```

### 6.2 前端项目结构

```
JuggleNet6.Frontend/
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
├── env.d.ts
│
└── src/
    ├── App.vue
    ├── main.ts
    │
    ├── views/                          # 页面视图
    │   ├── LoginView.vue
    │   ├── LayoutView.vue
    │   ├── flow/
    │   │   ├── FlowDefineList.vue      # 流程定义列表
    │   │   ├── FlowList.vue            # 流程列表
    │   │   ├── FlowVersionList.vue     # 版本列表
    │   │   ├── FlowDebug.vue           # 流程调试
    │   │   ├── FlowDesign.vue          # 流程设计器入口
    │   │   ├── define/                 # 流程定义子组件
    │   │   └── design/                 # 流程设计器核心
    │   │       ├── components/
    │   │       │   ├── AddNodeModal.vue
    │   │       │   ├── ConditionFilterModal.vue
    │   │       │   ├── EditNodeDrawer.vue
    │   │       │   ├── left-menu/
    │   │       │   │   ├── LeftMenu.vue
    │   │       │   │   ├── ParamSettingModal.vue
    │   │       │   │   └── VariableSetting.vue
    │   │       │   └── node-form/
    │   │       │       ├── MethodForm.vue
    │   │       │       └── ConditionForm.vue
    │   │       ├── renderer/           # 流程图渲染器
    │   │       └── data/               # 数据模型
    │   ├── suite/
    │   │   ├── SuiteList.vue
    │   │   ├── ApiList.vue
    │   │   └── ApiDebug.vue
    │   ├── object/
    │   │   └── ObjectList.vue
    │   ├── market/
    │   │   ├── SuiteMarket.vue
    │   │   └── TemplateMarket.vue
    │   └── system/
    │       ├── TokenList.vue
    │       └── DataSourceList.vue
    │
    ├── components/                     # 可复用组件
    │   ├── common/
    │   │   ├── CodeEditor.vue          # Monaco 代码编辑器
    │   │   ├── DataTypeDisplay.vue     # 数据类型展示
    │   │   ├── VariableSelect.vue      # 变量选择器
    │   │   └── ResizableDrawer.vue     # 可拖拽抽屉
    │   ├── form/
    │   │   ├── DataTypeSelect.vue      # 数据类型选择器
    │   │   ├── ParamSetting.vue        # 参数配置
    │   │   ├── InputRuleSetting.vue    # 输入规则配置
    │   │   ├── OutputRuleSetting.vue   # 输出规则配置
    │   │   ├── ApiSelect.vue           # API 选择器
    │   │   └── SuiteSelect.vue         # 套件选择器
    │   └── filter/
    │       ├── FilterGroup.vue         # 条件表达式组
    │       ├── FilterItem.vue          # 单条件表达式
    │       └── FilterValue.vue         # 条件值输入
    │
    ├── service/                        # API 服务层
    │   ├── base/
    │   │   └── index.ts                # Axios 封装（统一请求/响应处理）
    │   ├── api/
    │   │   ├── flow.ts
    │   │   ├── flowDefine.ts
    │   │   ├── flowVersion.ts
    │   │   ├── suite.ts
    │   │   ├── api.ts
    │   │   ├── object.ts
    │   │   ├── token.ts
    │   │   ├── dataSource.ts
    │   │   └── user.ts
    │   └── module/                     # 业务封装层
    │
    ├── router/
    │   └── index.ts
    │
    ├── typings/                        # 类型定义
    │   ├── api.ts
    │   ├── dataType.ts
    │   ├── flowDefine.ts
    │   ├── flowDesign.ts
    │   ├── suite.ts
    │   ├── object.ts
    │   └── parameter.ts
    │
    ├── const/                          # 常量
    │   ├── dataType.ts                 # 数据类型枚举
    │   └── application.ts             # 应用常量
    │
    └── utils/                          # 工具函数
        ├── CommonUtil.ts
        └── dataType.ts
```

---

## 七、系统配置设计

### 7.1 后端配置文件（appsettings.json）

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=juggle.db"
  },
  "Jwt": {
    "Secret": "your-jwt-secret-key-at-least-32-chars",
    "ExpireHours": 24
  },
  "Juggle": {
    "ApiTimeout": 30,
    "MaxFlowExecutionSeconds": 60
  },
  "AllowedHosts": "*",
  "Urls": "http://0.0.0.0:9127"
}
```

### 7.2 前端配置文件（.env.development）

```
VITE_API_BASE_URL=http://localhost:9127
```

### 7.3 前端配置文件（.env.production）

```
VITE_API_BASE_URL=/
```

---

## 八、初始化数据设计

系统初始化时需要插入以下数据：

```sql
-- 默认用户：账号 juggle，密码 juggle（MD5 加密）
INSERT INTO t_user (user_name, password, deleted) 
VALUES ('juggle', '24cb6bcbc65730e9650745d379613563', 0);

-- 示例套件
INSERT INTO t_suite (suite_code, suite_name, suite_flag, suite_version, deleted)
VALUES ('example_suite', '系统示例接口套件', 0, 'v1.0.0', 0);

-- 示例接口（7个，对应示例用户/商品/订单接口）
-- （参考原 data.sql）

-- 示例对象
INSERT INTO t_object (object_key, object_name, object_desc, deleted)
VALUES ('OrderDTO', '订单传输对象', '用于示例接口的对象', 0);

-- 示例流程定义
INSERT INTO t_flow_definition (flow_key, flow_name, flow_type, flow_content, remark, deleted)
VALUES ('sync_example', '示例流程', 'sync', '...JSON...', '', 0);
```

---

## 九、编码规范

### 9.1 后端 C# 规范

- 使用 `record` 类型定义 DTO（不可变）
- 服务层使用接口 + 实现分离
- 使用 `ILogger<T>` 做日志记录
- 异步方法统一使用 `async/await` + `Async` 后缀
- 异常通过全局中间件统一处理，业务异常使用自定义 `BizException`
- EF Core 使用 Code First + Migration

### 9.2 前端 Vue3 规范

- 全部使用 `<script setup>` 语法（Composition API）
- 使用 TypeScript 强类型
- 组件命名：大驼峰（PascalCase）
- 函数命名：小驼峰（camelCase）
- 常量命名：大写下划线（UPPER_SNAKE_CASE）
- 响应式数据优先使用 `ref`，对象使用 `reactive`
- API 调用统一通过 service 层，不在组件中直接使用 axios

---

## 十、测试策略

### 10.1 示例流程测试

系统内置示例流程（sync_example）用于验证核心功能：

**测试场景 1：登录失败**
```json
输入：{ "userName": "wrong", "password": "wrong", "deposit": 1000 }
预期：流程在条件节点走"默认else分支"直接结束，无订单返回
```

**测试场景 2：存款 < 10万**
```json
输入：{ "userName": "juggle", "password": "123456", "deposit": 50000 }
预期：返回 orderName = "送10元话费"
```

**测试场景 3：存款 > 10万**
```json
输入：{ "userName": "juggle", "password": "123456", "deposit": 300000 }
预期：返回 orderName = "送一双耐克的鞋"
```

---

## 十一、版本迭代计划

### v1.0（基础版本）
- [ ] 数据库 SQLite + EF Core 初始化
- [ ] 用户登录（JWT）
- [ ] 套件 CRUD
- [ ] API 接口 CRUD
- [ ] 自定义对象 CRUD
- [ ] 流程定义 CRUD
- [ ] 变量管理

### v1.1（流程引擎）
- [ ] 流程引擎核心（方法节点 + 条件节点）
- [ ] 流程调试接口
- [ ] 流程部署接口
- [ ] 流程版本管理（启用/禁用）
- [ ] 流程触发（同步）

### v1.2（完善功能）
- [ ] 开放接口（/open）
- [ ] Token 管理
- [ ] 数据源管理
- [ ] API 调试
- [ ] Swagger 接口文档

### v1.3（前端）
- [ ] 登录页
- [ ] 流程定义列表 + 编辑
- [ ] 流程设计器（拖拽式）
- [ ] 流程调试页
- [ ] 套件/API 管理
- [ ] 对象管理
- [ ] 系统设置

### v1.4（优化）
- [ ] 前后端打包部署
- [ ] 单元测试
- [ ] 错误日志
