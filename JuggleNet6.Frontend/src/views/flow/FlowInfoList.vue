<template>
  <div class="page-container">
    <div class="page-header">
      <h2>流程列表（已部署）</h2>
    </div>
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
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
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="goVersions(row)">版本管理</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
        :total="page.total" layout="total,prev,pager,next" style="margin-top:16px;justify-content:flex-end"
        @current-change="loadData" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
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
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
</style>
