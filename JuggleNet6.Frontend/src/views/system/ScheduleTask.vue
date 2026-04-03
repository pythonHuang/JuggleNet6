<template>
  <div class="page-container">
    <div class="page-header">
      <h2>定时任务</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新建任务</el-button>
    </div>

    <el-card class="search-card">
      <el-form inline>
        <el-form-item label="流程Key">
          <el-input v-model="searchForm.flowKey" placeholder="输入流程Key" clearable style="width:200px" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="searchForm.status" placeholder="全部" clearable style="width:120px">
            <el-option label="启用" :value="1" />
            <el-option label="暂停" :value="0" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" icon="Search" @click="loadData(1)">查询</el-button>
          <el-button icon="Refresh" @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card>
      <el-table :data="tableData" stripe v-loading="loading" style="width:100%">
        <el-table-column prop="flowKey" label="流程Key" width="200" show-overflow-tooltip />
        <el-table-column prop="flowName" label="流程名称" min-width="130" show-overflow-tooltip />
        <el-table-column prop="cronExpression" label="Cron表达式" width="160" />
        <el-table-column prop="status" label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'info'" size="small">
              {{ row.status === 1 ? '启用' : '暂停' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="lastRunTime" label="上次执行" width="170">
          <template #default="{ row }">
            <span v-if="row.lastRunTime">{{ formatTime(row.lastRunTime) }}</span>
            <span v-else style="color:#ccc">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="lastRunStatus" label="执行结果" width="90">
          <template #default="{ row }">
            <el-tag v-if="row.lastRunStatus" :type="row.lastRunStatus === 'SUCCESS' ? 'success' : 'danger'" size="small">
              {{ row.lastRunStatus === 'SUCCESS' ? '成功' : '失败' }}
            </el-tag>
            <span v-else style="color:#ccc">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="nextRunTime" label="下次执行" width="170">
          <template #default="{ row }">
            <span v-if="row.nextRunTime">{{ formatTime(row.nextRunTime) }}</span>
            <span v-else style="color:#ccc">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="runCount" label="累计执行" width="90" align="right" />
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button size="small" :type="row.status === 1 ? 'warning' : 'success'" link
              @click="toggleStatus(row)">{{ row.status === 1 ? '暂停' : '启用' }}</el-button>
            <el-button size="small" type="primary" link @click="runNow(row)">立即执行</el-button>
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
        :total="page.total" layout="total,prev,pager,next" style="margin-top:16px;justify-content:flex-end"
        @current-change="loadData" />
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑任务' : '新建任务'" width="560px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="流程Key" prop="flowKey">
          <el-select v-model="form.flowKey" placeholder="选择已发布的流程" filterable size="default" style="width:100%">
            <el-option v-for="f in flowOptions" :key="f.flowKey" :value="f.flowKey"
              :label="f.flowName + ' (' + f.flowKey + ')'" />
          </el-select>
        </el-form-item>
        <el-form-item label="Cron表达式" prop="cronExpression">
          <el-input v-model="form.cronExpression" placeholder="0 */5 * * * *（每5分钟）" />
          <div class="cron-hint">
            格式：秒 分 时 日 月 周（6位）<br>
            示例：<code>0 */5 * * * *</code> 每5分钟 | <code>0 30 9 * * *</code> 每天9:30 | <code>0 0 * * * *</code> 每小时
          </div>
        </el-form-item>
        <el-form-item label="入参JSON">
          <el-input v-model="form.inputJson" type="textarea" :rows="3" placeholder='可选，如 {"key":"value"}' />
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
const tableData = ref<any[]>([])
const flowOptions = ref<any[]>([])
const searchForm = reactive({ flowKey: '', status: null as number | null })
const page = reactive({ num: 1, size: 10, total: 0 })
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const form = reactive({ id: 0, flowKey: '', cronExpression: '0 */5 * * * *', inputJson: '' })
const rules = {
  flowKey: [{ required: true, message: '请选择流程', trigger: 'change' }],
  cronExpression: [{ required: true, message: '请输入Cron表达式', trigger: 'blur' }]
}

onMounted(() => { loadData(); loadFlows() })

async function loadFlows() {
  try {
    const res: any = await request.post('/flow/info/page', { pageNum: 1, pageSize: 200 })
    flowOptions.value = res.data?.records || []
  } catch {}
}

async function loadData(p?: number) {
  if (p) page.num = p
  loading.value = true
  try {
    const res: any = await request.post('/schedule/page', {
      pageNum: page.num, pageSize: page.size,
      flowKey: searchForm.flowKey || undefined,
      status: searchForm.status ?? undefined
    })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function resetSearch() {
  searchForm.flowKey = ''
  searchForm.status = null
  loadData(1)
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, flowKey: '', cronExpression: '0 */5 * * * *', inputJson: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, {
    id: row.id, flowKey: row.flowKey,
    cronExpression: row.cronExpression, inputJson: row.inputJson || ''
  })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value?.validate()
  if (isEdit.value) {
    await request.put('/schedule/update', { ...form, status: 1 })
    ElMessage.success('修改成功')
  } else {
    await request.post('/schedule/add', form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function toggleStatus(row: any) {
  await request.post(`/schedule/toggle/${row.id}`)
  ElMessage.success(row.status === 1 ? '已暂停' : '已启用')
  loadData()
}

async function runNow(row: any) {
  await ElMessageBox.confirm('确认立即执行该定时任务？', '提示', { type: 'info' })
  await request.post(`/schedule/runNow/${row.id}`)
  ElMessage.success('任务将在1分钟内执行')
}

async function doDelete(row: any) {
  await ElMessageBox.confirm('确认删除该定时任务？', '提示', { type: 'warning' })
  await request.delete(`/schedule/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

const formatTime = (t?: string) => {
  if (!t) return '-'
  return new Date(t).toLocaleString('zh-CN')
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.search-card { margin-bottom: 16px; }
.cron-hint { font-size: 11px; color: #999; margin-top: 4px; line-height: 1.5; }
.cron-hint code { background: #f5f5f5; padding: 1px 4px; border-radius: 3px; font-size: 11px; }
</style>
