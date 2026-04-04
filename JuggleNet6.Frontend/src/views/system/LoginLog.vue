<template>
  <div class="page-container">
    <div class="page-header">
      <h2>登录日志</h2>
      <el-button icon="Refresh" @click="loadData">刷新</el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="userName" label="用户名" width="120" />
        <el-table-column prop="result" label="登录结果" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.result === 'success' ? 'success' : 'danger'" size="small">
              {{ row.result === 'success' ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="ipAddress" label="登录IP" width="140" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="登录时间" width="180" show-overflow-tooltip />
        <el-table-column prop="userAgent" label="User-Agent" show-overflow-tooltip />
      </el-table>
      <div class="pagination-bar">
        <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
          :total="page.total" layout="total,prev,pager,next" @current-change="loadData" />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import request from '../../utils/request'

const loading = ref(false)
const tableData = ref<any[]>([])
const page = reactive({ num: 1, size: 20, total: 0 })

onMounted(() => loadData())

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/user/login-log/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}
</script>

<style scoped>
.page-container { padding: 16px; height: 100%; display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; flex-shrink: 0; }
.page-header h2 { font-size: 20px; color: #333; }
.table-card { flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden; }
.table-card :deep(.el-card__body) { flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden; padding-bottom: 0; }
.table-card :deep(.el-table) { flex: 1; min-height: 0; }
.pagination-bar { flex-shrink: 0; padding: 10px 0 2px; display: flex; justify-content: flex-end; }
</style>
