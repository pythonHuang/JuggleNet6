<template>
  <div class="page-container">
    <div class="page-header">
      <h2>租户管理</h2>
      <el-button type="primary" icon="Plus" @click="openDialog()">新建租户</el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="tenantName" label="租户名称" width="160" />
        <el-table-column prop="tenantCode" label="租户编码" width="150" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'" size="small">
              {{ row.status === 1 ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="userCount" label="用户数" width="90" align="center" />
        <el-table-column prop="remark" label="备注" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="openDialog(row)">编辑</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)"
              :disabled="row.id === 1">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div class="pagination-bar">
        <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
          :total="page.total" layout="total,prev,pager,next" @current-change="loadData" />
      </div>
    </el-card>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dlgVisible" :title="isEdit ? '编辑租户' : '新建租户'" width="480px">
      <el-form :model="form" label-width="90px">
        <el-form-item label="租户名称" required>
          <el-input v-model="form.tenantName" />
        </el-form-item>
        <el-form-item label="租户编码">
          <el-input v-model="form.tenantCode" placeholder="如 company-a（可选）" />
        </el-form-item>
        <el-form-item label="状态" v-if="isEdit">
          <el-switch v-model="form.status" :active-value="1" :inactive-value="0" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dlgVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="doSubmit">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const submitting = ref(false)
const tableData = ref<any[]>([])
const page = reactive({ num: 1, size: 10, total: 0 })
const dlgVisible = ref(false)
const isEdit = ref(false)
const editId = ref(0)
const form = reactive({ tenantName: '', tenantCode: '', status: 1, remark: '' })

onMounted(() => loadData())

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/tenant/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function openDialog(row?: any) {
  isEdit.value = !!row
  editId.value = row?.id || 0
  form.tenantName = row?.tenantName || ''
  form.tenantCode = row?.tenantCode || ''
  form.status = row?.status ?? 1
  form.remark = row?.remark || ''
  dlgVisible.value = true
}

async function doSubmit() {
  if (!form.tenantName) { ElMessage.warning('请输入租户名称'); return }
  submitting.value = true
  try {
    if (isEdit.value) {
      await request.put('/tenant/update', { id: editId.value, ...form })
    } else {
      await request.post('/tenant/add', form)
    }
    ElMessage.success(isEdit.value ? '更新成功' : '新增成功')
    dlgVisible.value = false
    loadData()
  } finally { submitting.value = false }
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除租户「${row.tenantName}」？`, '提示', { type: 'warning' })
  await request.delete(`/tenant/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}
</script>

<style scoped>
.page-container {
  padding: 16px; height: 100%; display: flex; flex-direction: column;
  overflow: hidden; box-sizing: border-box;
}
.page-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 12px; flex-shrink: 0;
}
.page-header h2 { font-size: 20px; color: #333; }
.table-card {
  flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden;
}
.table-card :deep(.el-card__body) {
  flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden; padding-bottom: 0;
}
.table-card :deep(.el-table) { flex: 1; min-height: 0; }
.pagination-bar {
  flex-shrink: 0; padding: 10px 0 2px; display: flex; justify-content: flex-end;
}
</style>
