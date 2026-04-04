<template>
  <div class="page-container">
    <div class="page-header">
      <h2>流程列表（已部署）</h2>
    </div>
    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="flowKey" label="流程Key" width="200" show-overflow-tooltip />
        <el-table-column prop="flowName" label="流程名称" />
        <el-table-column prop="flowType" label="类型" width="80">
          <template #default="{ row }">
            <el-tag :type="row.flowType === 'sync' ? 'success' : 'warning'" size="small">
              {{ row.flowType === 'sync' ? '同步' : '异步' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="flowDesc" label="描述" show-overflow-tooltip />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="goVersions(row)">版本管理</el-button>
            <el-tooltip content="复制最新版本调用地址">
              <el-button size="small" link icon="CopyDocument" @click="copyFlowUrl(row)" />
            </el-tooltip>
          </template>
        </el-table-column>
      </el-table>
      <div class="pagination-bar">
        <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
          :total="page.total" layout="total,prev,pager,next"
          @current-change="loadData" />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '../../utils/request'

const router = useRouter()
const loading = ref(false)
const tableData = ref([])
const page = reactive({ num: 1, size: 10, total: 0 })

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/flow/info/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function goVersions(row: any) {
  router.push(`/flow/version/${row.flowKey}`)
}

function copyFlowUrl(row: any) {
  const baseUrl = window.location.origin
  const url = `${baseUrl}/open/flow/trigger/${row.flowKey}\nMethod: POST\nHeader: X-Access-Token: <your-token>\nBody: {"flowData": {}}`
  navigator.clipboard.writeText(url).then(() => {
    ElMessage.success('调用地址已复制到剪贴板')
  }).catch(() => {
    ElMessage.error('复制失败')
  })
}
</script>

<style scoped>
.page-container {
  padding: 16px;
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-sizing: border-box;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  flex-shrink: 0;
}
.page-header h2 { font-size: 20px; color: #333; }
.table-card {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.table-card :deep(.el-card__body) {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding-bottom: 0;
}
.table-card :deep(.el-table) {
  flex: 1;
  min-height: 0;
}
.pagination-bar {
  flex-shrink: 0;
  padding: 10px 0 2px;
  display: flex;
  justify-content: flex-end;
}
</style>
