<template>
  <div class="page-container">
    <div class="page-header">
      <h2>套件管理</h2>
      <div style="display:flex;gap:8px">
        <el-button icon="Upload" @click="triggerImport">导入</el-button>
        <el-button type="primary" icon="Plus" @click="openAdd">新建套件</el-button>
      </div>
    </div>
    <!-- 隐藏文件输入框 -->
    <input ref="importFileRef" type="file" accept=".json" style="display:none" @change="doImport" />

    <el-card class="search-card">
      <el-form inline>
        <el-form-item label="套件名称">
          <el-input v-model="searchForm.suiteName" placeholder="请输入套件名称" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" icon="Search" @click="loadData">查询</el-button>
          <el-button icon="Refresh" @click="reset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="suiteCode" label="套件Code" width="220" show-overflow-tooltip />
        <el-table-column prop="suiteName" label="套件名称" />
        <el-table-column prop="suiteVersion" label="版本" width="80" />
        <el-table-column prop="suiteDesc" label="描述" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="goApiList(row)">接口管理</el-button>
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

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑套件' : '新建套件'" width="480px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="套件名称" prop="suiteName">
          <el-input v-model="form.suiteName" />
        </el-form-item>
        <el-form-item label="版本">
          <el-input v-model="form.suiteVersion" placeholder="v1.0.0" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.suiteDesc" type="textarea" :rows="2" />
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
const searchForm = reactive({ suiteName: '' })
const page = reactive({ num: 1, size: 10, total: 0 })
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const importFileRef = ref<HTMLInputElement>()
const form = reactive({ id: 0, suiteName: '', suiteVersion: 'v1.0.0', suiteDesc: '' })
const rules = { suiteName: [{ required: true, message: '请输入套件名称', trigger: 'blur' }] }

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/suite/page', {
      pageNum: page.num, pageSize: page.size, suiteName: searchForm.suiteName
    })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function reset() { searchForm.suiteName = ''; page.num = 1; loadData() }

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, suiteName: '', suiteVersion: 'v1.0.0', suiteDesc: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, { id: row.id, suiteName: row.suiteName, suiteVersion: row.suiteVersion, suiteDesc: row.suiteDesc })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value?.validate()
  if (isEdit.value) {
    await request.put('/suite/update', form)
    ElMessage.success('修改成功')
  } else {
    await request.post('/suite/add', form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除套件「${row.suiteName}」？`, '提示', { type: 'warning' })
  await request.delete(`/suite/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

function goApiList(row: any) {
  router.push(`/suite/api/${row.suiteCode}/${row.id}`)
}

// ===== 导出 =====
async function doExport(row: any) {
  try {
    const token = localStorage.getItem('token') || ''
    const resp = await fetch(`/api/suite/export/${row.id}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    if (!resp.ok) { ElMessage.error('导出失败'); return }
    const blob = await resp.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `suite_${row.suiteCode}_${Date.now()}.json`
    a.click()
    URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch {
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
    if (json.exportType !== 'suite') { ElMessage.error('文件格式不正确，请选择套件的导出文件'); return }
    const res: any = await request.post('/suite/import', json)
    ElMessage.success(`导入成功：${res.data?.suiteName || ''}`)
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

