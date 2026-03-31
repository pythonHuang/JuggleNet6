<template>
  <div class="page-container">
    <div class="page-header">
      <h2>流程定义</h2>
      <div style="display:flex;gap:8px">
        <el-button icon="Upload" @click="triggerImport">导入</el-button>
        <el-button type="primary" icon="Plus" @click="openAdd">新建流程</el-button>
      </div>
    </div>
    <!-- 隐藏的文件输入框（用于导入） -->
    <input ref="importFileRef" type="file" accept=".json" style="display:none" @change="doImport" />

    <!-- 搜索 -->
    <el-card class="search-card">
      <el-form inline>
        <el-form-item label="流程名称">
          <el-input v-model="searchForm.flowName" placeholder="请输入流程名称" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" icon="Search" @click="loadData">查询</el-button>
          <el-button icon="Refresh" @click="reset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 表格 -->
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading" style="width:100%">
        <el-table-column prop="flowKey" label="流程Key" width="200" show-overflow-tooltip />
        <el-table-column prop="flowName" label="流程名称" />
        <el-table-column prop="flowType" label="类型" width="80">
          <template #default="{ row }">
            <el-tag :type="row.flowType === 'sync' ? 'success' : 'warning'" size="small">
              {{ row.flowType === 'sync' ? '同步' : '异步' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'info'" size="small">
              {{ row.status === 1 ? '已部署' : '草稿' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="flowDesc" label="描述" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="260" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="goDesign(row)">设计</el-button>
            <el-button size="small" type="success" link @click="doDeploy(row)">部署</el-button>
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" type="warning" link @click="doExport(row)">导出</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
        :total="page.total" layout="total,prev,pager,next" style="margin-top:16px;justify-content:flex-end"
        @current-change="loadData" />
    </el-card>

    <!-- 新建/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑流程' : '新建流程'" width="500px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="流程名称" prop="flowName">
          <el-input v-model="form.flowName" placeholder="请输入流程名称" />
        </el-form-item>
        <el-form-item label="流程类型" prop="flowType">
          <el-radio-group v-model="form.flowType">
            <el-radio value="sync">同步</el-radio>
            <el-radio value="async">异步</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.flowDesc" type="textarea" :rows="2" />
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
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const router = useRouter()
const loading = ref(false)
const tableData = ref([])
const searchForm = reactive({ flowName: '' })
const page = reactive({ num: 1, size: 10, total: 0 })
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const importFileRef = ref<HTMLInputElement>()
const form = reactive({ id: 0, flowName: '', flowType: 'sync', flowDesc: '' })
const rules = { flowName: [{ required: true, message: '请输入流程名称', trigger: 'blur' }] }

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/flow/definition/page', {
      pageNum: page.num, pageSize: page.size, flowName: searchForm.flowName
    })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function reset() {
  searchForm.flowName = ''
  page.num = 1
  loadData()
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, flowName: '', flowType: 'sync', flowDesc: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, { id: row.id, flowName: row.flowName, flowType: row.flowType, flowDesc: row.flowDesc })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value?.validate()
  if (isEdit.value) {
    await request.put('/flow/definition/update', form)
    ElMessage.success('修改成功')
  } else {
    await request.post('/flow/definition/add', form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除流程「${row.flowName}」？`, '提示', { type: 'warning' })
  await request.delete(`/flow/definition/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

async function doDeploy(row: any) {
  await ElMessageBox.confirm(`确认部署流程「${row.flowName}」？`, '提示', { type: 'warning' })
  await request.post('/flow/definition/deploy', { flowDefinitionId: row.id })
  ElMessage.success('部署成功')
  loadData()
}

function goDesign(row: any) {
  router.push(`/design/${row.flowKey}`)
}

// ===== 导出 =====
async function doExport(row: any) {
  try {
    // 使用 fetch 直接下载文件（绕过 axios 的 JSON 解析）
    const token = localStorage.getItem('token') || ''
    const resp = await fetch(`/api/flow/definition/export/${row.id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    if (!resp.ok) { ElMessage.error('导出失败'); return }
    const blob = await resp.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `flow_${row.flowKey}_${Date.now()}.json`
    a.click()
    URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch (e) {
    ElMessage.error('导出异常')
  }
}

// ===== 导入 =====
function triggerImport() {
  importFileRef.value!.value = ''
  importFileRef.value!.click()
}

async function doImport(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  try {
    const text = await file.text()
    const json = JSON.parse(text)
    if (json.exportType !== 'flow') { ElMessage.error('文件格式不正确，请选择流程定义的导出文件'); return }
    const res: any = await request.post('/flow/definition/import', json)
    ElMessage.success(`导入成功：${res.data?.flowName || ''}`)
    loadData()
  } catch (ex: any) {
    ElMessage.error('导入失败：' + (ex?.message || ex))
  }
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.search-card { margin-bottom: 16px; }
</style>

