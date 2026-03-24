<template>
  <div class="page-container">
    <div class="page-header">
      <h2>流程定义</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新建流程</el-button>
    </div>

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
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="goDesign(row)">设计</el-button>
            <el-button size="small" type="success" link @click="doDeploy(row)">部署</el-button>
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
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
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.search-card { margin-bottom: 16px; }
</style>
