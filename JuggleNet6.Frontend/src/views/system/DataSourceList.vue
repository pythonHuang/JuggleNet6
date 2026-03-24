<template>
  <div class="page-container">
    <div class="page-header">
      <h2>数据源管理</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新建数据源</el-button>
    </div>
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="dsName" label="名称" />
        <el-table-column prop="dsType" label="类型" width="80" />
        <el-table-column prop="host" label="主机" />
        <el-table-column prop="port" label="端口" width="80" />
        <el-table-column prop="dbName" label="数据库" />
        <el-table-column label="操作" width="140" fixed="right">
          <template #default="{ row }">
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑数据源' : '新建数据源'" width="560px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="名称" prop="dsName">
          <el-input v-model="form.dsName" />
        </el-form-item>
        <el-form-item label="主机" prop="host">
          <el-input v-model="form.host" placeholder="localhost" />
        </el-form-item>
        <el-form-item label="端口" prop="port">
          <el-input-number v-model="form.port" :min="1" :max="65535" />
        </el-form-item>
        <el-form-item label="数据库" prop="dbName">
          <el-input v-model="form.dbName" />
        </el-form-item>
        <el-form-item label="用户名" prop="username">
          <el-input v-model="form.username" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="form.password" type="password" show-password />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const tableData = ref([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const form = reactive({ id: 0, dsName: '', dsType: 'mysql', host: 'localhost', port: 3306, dbName: '', username: '', password: '' })
const rules = {
  dsName: [{ required: true, message: '请输入名称', trigger: 'blur' }],
  host: [{ required: true, message: '请输入主机', trigger: 'blur' }],
  dbName: [{ required: true, message: '请输入数据库名', trigger: 'blur' }]
}

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/system/datasource/page', { pageNum: 1, pageSize: 100 })
    tableData.value = res.data.records
  } finally { loading.value = false }
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, dsName: '', host: 'localhost', port: 3306, dbName: '', username: '', password: '' })
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

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除数据源「${row.dsName}」？`, '提示', { type: 'warning' })
  await request.delete(`/system/datasource/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
</style>
