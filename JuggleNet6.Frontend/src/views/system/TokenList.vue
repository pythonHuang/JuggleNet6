<template>
  <div class="page-container">
    <div class="page-header">
      <h2>Token 管理</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新建 Token</el-button>
    </div>
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="tokenName" label="Token名称" />
        <el-table-column prop="tokenValue" label="Token值" show-overflow-tooltip />
        <el-table-column prop="expiredAt" label="过期时间" width="180" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="80" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" title="新建 Token" width="480px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="Token名称" prop="tokenName">
          <el-input v-model="form.tokenName" />
        </el-form-item>
        <el-form-item label="过期时间">
          <el-date-picker v-model="form.expiredAt" type="datetime" placeholder="不填表示永不过期"
            value-format="YYYY-MM-DDTHH:mm:ss" style="width:100%" />
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
const tableData = ref([])
const dialogVisible = ref(false)
const formRef = ref()
const form = reactive({ tokenName: '', expiredAt: '' })
const rules = { tokenName: [{ required: true, message: '请输入Token名称', trigger: 'blur' }] }

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/system/token/page', { pageNum: 1, pageSize: 100 })
    tableData.value = res.data.records
  } finally { loading.value = false }
}

function openAdd() {
  Object.assign(form, { tokenName: '', expiredAt: '' })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value?.validate()
  await request.post('/system/token/add', form)
  ElMessage.success('创建成功')
  dialogVisible.value = false
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除 Token「${row.tokenName}」？`, '提示', { type: 'warning' })
  await request.delete(`/system/token/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
</style>
