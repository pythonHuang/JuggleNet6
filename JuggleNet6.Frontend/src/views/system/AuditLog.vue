<template>
  <div class="page-container">
    <div class="page-header">
      <h2>操作审计日志</h2>
      <div style="display:flex;gap:8px;align-items:center">
        <el-select v-model="filterModule" placeholder="操作模块" clearable size="small" style="width:120px" @change="loadData">
          <el-option label="租户" value="tenant" />
          <el-option label="角色" value="role" />
          <el-option label="用户" value="user" />
        </el-select>
        <el-select v-model="filterAction" placeholder="操作类型" clearable size="small" style="width:110px" @change="loadData">
          <el-option label="新增" value="add" />
          <el-option label="更新" value="update" />
          <el-option label="删除" value="delete" />
        </el-select>
        <el-button icon="Refresh" size="small" @click="loadData">刷新</el-button>
      </div>
    </div>

    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="module" label="模块" width="80" align="center">
          <template #default="{ row }">
            <el-tag size="small" :type="moduleTagType(row.module)">{{ moduleLabel(row.module) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="actionType" label="操作" width="80" align="center">
          <template #default="{ row }">
            <span :style="{ color: ({ add: '#ff4d4f', update: '#1890ff', delete: '#1890ff' } as any)[row.actionType] || '#333' }">
              {{ ({ add: '新增', update: '更新', delete: '删除' } as any)[row.actionType] || row.actionType }}
            </span>
          </template>
        </el-table-column>
        <el-table-column prop="targetId" label="目标ID" width="80" />
        <el-table-column prop="changeContent" label="变更内容" show-overflow-tooltip />
        <el-table-column prop="operatorName" label="操作人" width="100" />
        <el-table-column prop="createdAt" label="操作时间" width="180" show-overflow-tooltip />
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
const filterModule = ref('')
const filterAction = ref('')

const moduleMap: Record<string, string> = { tenant: '租户', role: '角色', user: '用户' }
function moduleLabel(m: string) { return moduleMap[m] || m }
function moduleTagType(m: string) {
  return { tenant: 'info', role: 'warning', user: 'success' }[m] || ''
}

onMounted(() => loadData())

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/audit-log/page', {
      pageNum: page.num,
      pageSize: page.size,
      module: filterModule.value || undefined,
      actionType: filterAction.value || undefined
    })
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
