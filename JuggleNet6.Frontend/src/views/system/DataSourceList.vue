<template>
  <div class="page-container">
    <div class="page-header">
      <h2>数据源管理</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新建数据源</el-button>
    </div>
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="dsName" label="名称" width="140" />
        <el-table-column prop="dsType" label="类型" width="110">
          <template #default="{ row }">
            <el-tag :type="dbTypeColor(row.dsType)" size="small">{{ dbTypeLabel(row.dsType) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="连接信息">
          <template #default="{ row }">
            <span v-if="row.dsType === 'sqlite'" class="conn-hint">{{ row.dbName || 'juggle.db' }}</span>
            <span v-else class="conn-hint">{{ row.username }}@{{ row.host }}:{{ row.port }}/{{ row.dbName }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" type="success" link @click="testConn(row)" :loading="row._testing">测试连接</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑数据源' : '新建数据源'" width="560px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="名称" prop="dsName">
          <el-input v-model="form.dsName" placeholder="数据源唯一标识" />
        </el-form-item>
        <el-form-item label="数据库类型" prop="dsType">
          <el-radio-group v-model="form.dsType" @change="onDsTypeChange">
            <el-radio-button value="mysql">MySQL</el-radio-button>
            <el-radio-button value="sqlite">SQLite</el-radio-button>
            <el-radio-button value="postgresql">PostgreSQL</el-radio-button>
            <el-radio-button value="sqlserver">SQL Server</el-radio-button>
            <el-radio-button value="oracle">Oracle</el-radio-button>
            <el-radio-button value="dm">达梦</el-radio-button>
          </el-radio-group>
        </el-form-item>

        <!-- SQLite 专属：只需文件路径 -->
        <template v-if="form.dsType === 'sqlite'">
          <el-form-item label="数据库文件" prop="dbName">
            <el-input v-model="form.dbName" placeholder="如: juggle.db 或绝对路径 D:/data/app.db" />
          </el-form-item>
          <div class="form-tip">SQLite 无需主机/端口/用户名/密码，仅需文件路径。</div>
        </template>

        <!-- 网络数据库 -->
        <template v-else>
          <el-form-item label="主机" prop="host">
            <el-input v-model="form.host" placeholder="localhost 或 IP" />
          </el-form-item>
          <el-form-item label="端口" prop="port">
            <el-input-number v-model="form.port" :min="1" :max="65535" style="width:160px" />
            <span class="port-hint">默认：MySQL=3306，PostgreSQL=5432，SQL Server=1433，Oracle=1521，达梦=5236</span>
          </el-form-item>
          <el-form-item label="数据库名" prop="dbName">
            <el-input v-model="form.dbName" placeholder="数据库名称" />
          </el-form-item>
          <el-form-item label="用户名" prop="username">
            <el-input v-model="form.username" />
          </el-form-item>
          <el-form-item label="密码">
            <el-input v-model="form.password" type="password" show-password />
          </el-form-item>
        </template>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="success" @click="testConnForm" :loading="testingForm">测试连接</el-button>
        <el-button type="primary" @click="handleSubmit">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const tableData = ref<any[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const testingForm = ref(false)
const form = reactive({
  id: 0, dsName: '', dsType: 'mysql',
  host: 'localhost', port: 3306, dbName: '', username: '', password: ''
})
const rules = {
  dsName: [{ required: true, message: '请输入名称', trigger: 'blur' }],
  dsType: [{ required: true }],
  dbName: [{ required: true, message: '请输入数据库名/文件路径', trigger: 'blur' }],
  host: [{
    validator: (_: any, __: any, cb: Function) => {
      if (form.dsType !== 'sqlite' && !form.host) cb(new Error('请输入主机地址'))
      else cb()
    }, trigger: 'blur'
  }]
}

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/system/datasource/page', { pageNum: 1, pageSize: 100 })
    tableData.value = res.data.records
  } finally { loading.value = false }
}

function onDsTypeChange(val: string) {
  // 自动更新默认端口
  const portMap: Record<string, number> = {
    mysql: 3306, postgresql: 5432, sqlserver: 1433, sqlite: 0,
    oracle: 1521, dm: 5236
  }
  if (portMap[val] !== undefined) form.port = portMap[val]
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, dsName: '', dsType: 'mysql', host: 'localhost', port: 3306, dbName: '', username: '', password: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, { ...row })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value?.validate()
  if (isEdit.value) {
    await request.put('/system/datasource/update', form)
    ElMessage.success('修改成功')
  } else {
    await request.post('/system/datasource/add', form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function testConn(row: any) {
  row._testing = true
  try {
    const res: any = await request.post(`/system/datasource/test/${row.id}`)
    if (res.code === 200) ElMessage.success(`✓ 连接成功：${res.data}`)
    else ElMessage.error(`✗ 连接失败：${res.message}`)
  } catch (e: any) {
    ElMessage.error(`✗ ${e.message}`)
  } finally { row._testing = false }
}

async function testConnForm() {
  // 先保存再测试，或者直接用 form 构建一个临时 id 测试（需先保存）
  testingForm.value = true
  try {
    if (!form.id) {
      // 先 add 再 test
      const addRes: any = await request.post('/system/datasource/add', form)
      const newId = addRes.data
      const res: any = await request.post(`/system/datasource/test/${newId}`)
      if (res.code === 200) {
        ElMessage.success(`✓ 连接成功，已保存为 ID=${newId}`)
        form.id = newId
        isEdit.value = true
        loadData()
      } else {
        // 删除刚创建的记录
        await request.delete(`/system/datasource/delete/${newId}`)
        ElMessage.error(`✗ 连接失败：${res.message}`)
      }
    } else {
      await request.put('/system/datasource/update', form)
      const res: any = await request.post(`/system/datasource/test/${form.id}`)
      if (res.code === 200) ElMessage.success(`✓ 连接成功：${res.data}`)
      else ElMessage.error(`✗ 连接失败：${res.message}`)
    }
  } catch (e: any) {
    ElMessage.error(`✗ ${e.message}`)
  } finally { testingForm.value = false }
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除数据源「${row.dsName}」？`, '提示', { type: 'warning' })
  await request.delete(`/system/datasource/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

function dbTypeLabel(type: string) {
  const m: Record<string, string> = {
    mysql: 'MySQL', sqlite: 'SQLite', postgresql: 'PostgreSQL', sqlserver: 'SQL Server',
    mssql: 'SQL Server', postgres: 'PostgreSQL', oracle: 'Oracle', dm: '达梦'
  }
  return m[type?.toLowerCase()] || type
}

function dbTypeColor(type: string) {
  const m: Record<string, string> = {
    mysql: 'warning', sqlite: 'info', postgresql: 'success', sqlserver: 'danger',
    mssql: 'danger', postgres: 'success', oracle: 'warning', dm: 'success'
  }
  return m[type?.toLowerCase()] || ''
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.form-tip { color: #888; font-size: 12px; background: #f5f5f5; padding: 8px 12px; border-radius: 6px; margin: -8px 0 12px; }
.port-hint { font-size: 11px; color: #aaa; margin-left: 8px; }
.conn-hint { font-size: 12px; color: #555; font-family: monospace; }
</style>
