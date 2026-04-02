<template>
  <div class="page-container">
    <div class="page-header">
      <div>
        <el-button icon="ArrowLeft" link @click="router.back()">返回</el-button>
        <h2 style="display:inline;margin-left:8px">接口管理 - {{ suiteCode }}</h2>
      </div>
      <el-button type="primary" icon="Plus" @click="openAdd">新建接口</el-button>
    </div>

    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="methodCode" label="接口Code" width="220" show-overflow-tooltip />
        <el-table-column prop="methodName" label="接口名称" />
        <el-table-column label="类型" width="110">
          <template #default="{ row }">
            <el-tag :type="row.methodType === 'WEBSERVICE' ? 'warning' : 'info'" size="small">
              {{ row.methodType === 'WEBSERVICE' ? 'WebService' : 'HTTP' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="requestType" label="请求方式" width="90">
          <template #default="{ row }">
            <el-tag v-if="row.methodType !== 'WEBSERVICE'" :type="methodColor(row.requestType)" size="small">{{ row.requestType }}</el-tag>
            <el-tag v-else type="warning" size="small">SOAP</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="url" label="URL" show-overflow-tooltip />
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="openDetail(row)">详情/参数</el-button>
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑接口' : '新建接口'" width="600px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="接口名称" prop="methodName">
          <el-input v-model="form.methodName" />
        </el-form-item>
        <el-form-item label="接口类型">
          <el-radio-group v-model="form.methodType">
            <el-radio value="HTTP">API 接口（HTTP）</el-radio>
            <el-radio value="WEBSERVICE">WebService（SOAP）</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="form.methodType === 'HTTP'" label="请求方式" prop="requestType">
          <el-radio-group v-model="form.requestType">
            <el-radio value="GET">GET</el-radio>
            <el-radio value="POST">POST</el-radio>
            <el-radio value="PUT">PUT</el-radio>
            <el-radio value="DELETE">DELETE</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="URL" prop="url">
          <el-input v-model="form.url" :placeholder="form.methodType === 'WEBSERVICE' ? 'http://...?wsdl 或 http://...?op=MethodName' : 'http://...'" />
        </el-form-item>
        <el-form-item v-if="form.methodType === 'HTTP'" label="内容类型">
          <el-select v-model="form.contentType">
            <el-option value="JSON" label="JSON" />
            <el-option value="FORM" label="FORM" />
          </el-select>
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.methodDesc" type="textarea" :rows="2" />
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
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const route = useRoute()
const router = useRouter()
const suiteCode = route.params.suiteCode as string
const loading = ref(false)
const tableData = ref([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const form = reactive({
  id: 0, suiteCode, methodName: '', methodType: 'HTTP', requestType: 'GET', url: '', contentType: 'JSON', methodDesc: ''
})
const rules = {
  methodName: [{ required: true, message: '请输入接口名称', trigger: 'blur' }],
  url: [{ required: true, message: '请输入URL', trigger: 'blur' }]
}

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/suite/api/list', { suiteCode })
    tableData.value = res.data
  } finally { loading.value = false }
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, methodName: '', methodType: 'HTTP', requestType: 'GET', url: '', contentType: 'JSON', methodDesc: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, {
    id: row.id,
    methodName: row.methodName,
    methodType: row.methodType || 'HTTP',
    requestType: row.requestType,
    url: row.url,
    contentType: row.contentType,
    methodDesc: row.methodDesc
  })
  dialogVisible.value = true
}

function openDetail(row: any) {
  router.push(`/suite/api/${suiteCode}/${row.id}/detail`)
}

async function handleSubmit() {
  await formRef.value?.validate()
  if (isEdit.value) {
    await request.put('/suite/api/update', form)
    ElMessage.success('修改成功')
  } else {
    await request.post('/suite/api/add', form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除接口「${row.methodName}」？`, '提示', { type: 'warning' })
  await request.delete(`/suite/api/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

function methodColor(type: string) {
  const map: Record<string, string> = { GET: 'success', POST: 'primary', PUT: 'warning', DELETE: 'danger' }
  return map[type] || 'info'
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
</style>
