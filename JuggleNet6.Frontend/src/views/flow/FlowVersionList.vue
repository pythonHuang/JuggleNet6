<template>
  <div class="page-container">
    <div class="page-header">
      <div>
        <el-button icon="ArrowLeft" link @click="router.back()">返回</el-button>
        <h2 style="display:inline;margin-left:8px">版本管理 - {{ flowKey }}</h2>
      </div>
    </div>
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="version" label="版本号" width="100" />
        <el-table-column prop="status" label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'" size="small">
              {{ row.status === 1 ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="triggerTest(row)">触发测试</el-button>
            <el-button size="small" :type="row.status === 1 ? 'warning' : 'success'" link
              @click="toggleStatus(row)">
              {{ row.status === 1 ? '禁用' : '启用' }}
            </el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox, ElInputNumber } from 'element-plus'
import request from '../../utils/request'

const route = useRoute()
const router = useRouter()
const flowKey = route.params.flowKey as string
const loading = ref(false)
const tableData = ref([])

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/flow/version/page', { pageNum: 1, pageSize: 100, flowKey })
    tableData.value = res.data.records
  } finally { loading.value = false }
}

async function toggleStatus(row: any) {
  const newStatus = row.status === 1 ? 0 : 1
  await request.put('/flow/version/status', { id: row.id, status: newStatus })
  ElMessage.success(newStatus === 1 ? '已启用' : '已禁用')
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除版本 ${row.version}？`, '提示', { type: 'warning' })
  await request.delete(`/flow/version/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

async function triggerTest(row: any) {
  await ElMessageBox.confirm(`触发测试版本 ${row.version}（空参数）？`, '提示', { type: 'info' })
  const res: any = await request.post(`/flow/version/trigger/${row.version}/${flowKey}`, { params: {} })
  ElMessage.success('触发成功，请查看控制台输出')
  console.log('流程执行结果:', res.data)
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
</style>
