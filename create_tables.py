import sqlite3

db = r'D:\WorkBuddyMyWorkSpace\Juggle接口编排Net6\JuggleNet6\Juggle.Api\juggle.db'
conn = sqlite3.connect(db)
cur = conn.cursor()

# 建 t_system_config
cur.execute("""CREATE TABLE IF NOT EXISTS t_system_config (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    config_key TEXT NOT NULL,
    config_value TEXT,
    config_name TEXT,
    config_group TEXT,
    remark TEXT,
    deleted INTEGER NOT NULL DEFAULT 0,
    created_at TEXT,
    created_by TEXT,
    updated_at TEXT,
    updated_by TEXT
)""")

# 建 t_flow_test_case
cur.execute("""CREATE TABLE IF NOT EXISTS t_flow_test_case (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    flow_key TEXT NOT NULL,
    case_name TEXT NOT NULL,
    input_json TEXT,
    assert_json TEXT,
    last_run_status TEXT,
    last_run_time TEXT,
    last_run_result TEXT,
    remark TEXT,
    deleted INTEGER NOT NULL DEFAULT 0,
    created_at TEXT,
    created_by TEXT,
    updated_at TEXT,
    updated_by TEXT
)""")

# 建 t_token_permission（如果不存在）
cur.execute("""CREATE TABLE IF NOT EXISTS t_token_permission (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    token_id INTEGER NOT NULL,
    permission_type TEXT NOT NULL,
    resource_key TEXT NOT NULL,
    resource_name TEXT,
    deleted INTEGER NOT NULL DEFAULT 0,
    created_at TEXT,
    created_by TEXT,
    updated_at TEXT,
    updated_by TEXT
)""")

# 建 t_webhook（如果不存在）
cur.execute("""CREATE TABLE IF NOT EXISTS t_webhook (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    webhook_key TEXT NOT NULL,
    webhook_name TEXT NOT NULL,
    flow_key TEXT NOT NULL,
    flow_name TEXT,
    secret TEXT,
    allowed_method TEXT,
    async_mode INTEGER NOT NULL DEFAULT 0,
    status INTEGER NOT NULL DEFAULT 1,
    trigger_count INTEGER NOT NULL DEFAULT 0,
    last_trigger_time TEXT,
    remark TEXT,
    deleted INTEGER NOT NULL DEFAULT 0,
    created_at TEXT,
    created_by TEXT,
    updated_at TEXT,
    updated_by TEXT
)""")

conn.commit()

# 验证所有表
cur.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
all_tables = [r[0] for r in cur.fetchall()]
print("数据库所有表:", all_tables)
print("表总数:", len(all_tables))

conn.close()
print("完成！")
