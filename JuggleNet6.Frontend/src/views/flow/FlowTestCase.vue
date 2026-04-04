<template>
  <div class="page-container">
    <div class="page-header">
      <h2>流程测试用例</h2>
      <div style="display:flex;gap:8px">
        <el-button icon="VideoPlay" :disabled="!searchForm.flowKey" @click="runAll" :loading="runningAll">
          批量执行
        </el-button>
        <el-button type="primary" icon="Plus" @click="openAdd">新增用例</el-button>
      </div>
    </div>

    <!-- 搜索 -->
    <el-card class="search-card">
      <el-form inline>
        <el-form-item label="流程Key">
          <el-input v-model="searchForm.flowKey" placeholder="输入流程Key" clearable style="width:220px" />
        </el-form-item>
        <el-form-item label="用例名称">
          <el-input v-model="searchForm.keyword" placeholder="用例名称" clearable />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" icon="Search" @click="loadData">查询</el-button>
          <el-button icon="Refresh" @click="reset">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 批量执行结果 -->
    <el-card v-if="runAllResult" class="result-card">
      <template #header>
        <div style="display:flex;justify-content:space-between;align-items:center">
          <span>批量执行结果</span>
          <el-button size="small" @click="runAllResult = null">关闭</el-button>
        </div>
      </template>
      <div class="batch-summary">
        <el-statistic title="总用例" :value="runAllResult.total" />
        <el-statistic title="通过" :value="runAllResult.passCount" value-style="color:#67c23a" />
        <el-statistic title="失败" :value="runAllResult.failCount" value-style="color:#f56c6c" />
      </div>
      <el-table :data="runAllResult.results" size="small" style="margin-top:12px">
        <el-table-column prop="caseName" label="用例名称" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'SUCCESS' ? 'success' : 'danger'" size="small">
              {{ row.status === 'SUCCESS' ? '通过' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="summary" label="摘要" show-overflow-tooltip />
      </el-table>
    </el-card>

    <!-- 表格 -->
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="flowKey" label="流程Key" width="200" show-overflow-tooltip />
        <el-table-column prop="caseName" label="用例名称" />
        <el-table-column prop="lastRunStatus" label="执行状态" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.lastRunStatus === 'SUCCESS'" type="success" size="small">通过</el-tag>
            <el-tag v-else-if="row.lastRunStatus === 'FAILED'" type="danger" size="small">失败</el-tag>
            <el-tag v-else type="info" size="small">未执行</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="lastRunResult" label="执行摘要" show-overflow-tooltip />
        <el-table-column prop="lastRunTime" label="最近执行" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="doRun(row)" :loading="row._running">执行</el-button>
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" link @click="viewResult(row)" v-if="row._result">查看结果</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
        :total="page.total" layout="total,prev,pager,next" style="margin-top:16px;justify-content:flex-end"
        @current-change="loadData" />
    </el-card>

    <!-- 新建/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑用例' : '新增用例'" width="640px">
      <el-form :model="form" label-width="90px">
        <el-form-item label="流程Key" required>
          <el-input v-model="form.flowKey" placeholder="请输入流程Key" />
        </el-form-item>
        <el-form-item label="用例名称" required>
          <el-input v-model="form.caseName" />
        </el-form-item>
        <el-form-item label="入参JSON">
          <el-input v-model="form.inputJson" type="textarea" :rows="5"
            placeholder='{"param1": "value1", "param2": 123}' />
        </el-form-item>
        <el-form-item label="断言JSON">
          <el-input v-model="form.assertJson" type="textarea" :rows="4"
            placeholder='{"outputVar": "expectedValue"}' />
          <div class="hint">格式：{"变量名": "期望值"}，执行后对比输出变量值</div>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">确认</el-button>
      </template>
    </el-dialog>

    <!-- 执行结果弹窗 -->
    <el-dialog v-model="resultVisible" title="执行结果" width="680px">
      <div v-if="currentResult">
        <el-alert :type="currentResult.success ? 'success' : 'error'" :closable="false"
          style="margin-bottom:12px">
          <template #default>
            <b>{{ currentResult.status }}</b>
            {{ currentResult.summary }}
            <span v-if="currentResult.costMs" style="margin-left:16px;color:#666">
              耗时 {{ currentResult.costMs }}ms
            </span>
          </template>
        </el-alert>

        <div v-if="currentResult.assertResults?.length > 0">
          <div class="section-title">断言结果</div>
          <el-table :data="currentResult.assertResults" size="small" border>
            <el-table-column prop="varName" label="变量名" width="150" />
            <el-table-column prop="expected" label="期望值" />
            <el-table-column prop="actual" label="实际值" />
            <el-table-column prop="passed" label="是否通过" width="100">
              <template #default="{ row }">
                <el-tag :type="row.passed ? 'success' : 'danger'" size="small">
                  {{ row.passed ? '✓ 通过' : '✗ 失败' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </div>

        <div v-if="currentResult.outputs && Object.keys(currentResult.outputs).length > 0">
          <div class="section-title">输出变量</div>
          <el-table :data="Object.entries(currentResult.outputs).map(([k,v]) => ({key:k,value:v}))" size="small">
            <el-table-column prop="key" label="变量名" width="200" />
            <el-table-column prop="value" label="值" show-overflow-tooltip />
          </el-table>
        </div>

        <div v-if="!currentResult.success && currentResult.errorMessage" class="error-msg">
          错误信息：{{ currentResult.errorMessage }}
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const route = useRoute()
const loading = ref(false)
const runningAll = ref(false)
const tableData = ref<any[]>([])
const page = reactive({ num: 1, size: 10, total: 0 })
const searchForm = reactive({ flowKey: (route.query.flowKey as string) || '', keyword: '' })
const dialogVisible = ref(false)
const isEdit = ref(false)
const resultVisible = ref(false)
const currentResult = ref<any>(null)
const runAllResult = ref<any>(null)
const form = reactive({ id: 0, flowKey: '', caseName: '', inputJson: '', assertJson: '', remark: '' })

onMounted(() => { if (searchForm.flowKey) loadData() })

async function loadData() {
  if (!searchForm.flowKey) { ElMessage.warning('请先输入流程Key进行查询'); return }
  loading.value = true
  try {
    const res: any = await request.post('/flow/testcase/page', {
      pageNum: page.num, pageSize: page.size,
      flowKey: searchForm.flowKey, keyword: searchForm.keyword || undefined
    })
    tableData.value = res.data.records.map((r: any) => ({ ...r, _running: false, _result: null }))
    page.total = res.data.total
  } finally { loading.value = false }
}

function reset() {
  searchForm.flowKey = ''
  searchForm.keyword = ''
  tableData.value = []
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, flowKey: searchForm.flowKey, caseName: '', inputJson: '', assertJson: '', remark: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, { id: row.id, flowKey: row.flowKey, caseName: row.caseName,
    inputJson: row.inputJson || '', assertJson: row.assertJson || '', remark: row.remark || '' })
  dialogVisible.value = true
}

async function handleSubmit() {
  if (!form.flowKey || !form.caseName) { ElMessage.warning('请填写流程Key和用例名称'); return }
  await request.post('/flow/testcase/save', { ...form, id: isEdit.value ? form.id : undefined })
  ElMessage.success(isEdit.value ? '修改成功' : '新增成功')
  dialogVisible.value = false
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除用例「${row.caseName}」？`, '提示', { type: 'warning' })
  await request.delete(`/flow/testcase/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

async function doRun(row: any) {
  row._running = true
  try {
    const res: any = await request.post(`/flow/testcase/run/${row.id}`, {})
    row._result = res.data
    row.lastRunStatus = res.data?.status
    row.lastRunResult = res.data?.summary
    row.lastRunTime   = new Date().toLocaleString()
    if (res.data?.success) {
      ElMessage.success(`执行通过：${res.data?.summary}`)
    } else {
      ElMessage.error(`执行失败：${res.data?.summary}`)
    }
    currentResult.value = res.data
    resultVisible.value = true
  } finally { row._running = false }
}

async function runAll() {
  if (!searchForm.flowKey) return
  runningAll.value = true
  try {
    const res: any = await request.post(`/flow/testcase/runAll/${searchForm.flowKey}`, {})
    runAllResult.value = res.data
    ElMessage.success(`批量执行完成：${res.data?.passCount}/${res.data?.total} 通过`)
    loadData()
  } finally { runningAll.value = false }
}

function viewResult(row: any) {
  currentResult.value = row._result
  resultVisible.value = true
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.search-card { margin-bottom: 16px; }
.result-card { margin-bottom: 16px; background: #f8fff8; }
.batch-summary { display: flex; gap: 40px; padding: 8px 0; }
.section-title { font-weight: 600; margin: 12px 0 8px; color: #333; }
.hint { font-size: 12px; color: #999; margin-top: 4px; }
.error-msg { margin-top: 12px; padding: 8px 12px; background: #fff2f0; border-radius: 4px; color: #f56c6c; }
</style>
