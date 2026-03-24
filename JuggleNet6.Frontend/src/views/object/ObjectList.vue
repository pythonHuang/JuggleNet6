<template>
  <div class="page-container">
    <div class="page-header">
      <h2>对象管理</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新建对象</el-button>
    </div>
    <el-card>
      <el-table :data="tableData" stripe v-loading="loading">
        <el-table-column prop="objectCode" label="对象Code" width="220" show-overflow-tooltip />
        <el-table-column prop="objectName" label="对象名称" />
        <el-table-column prop="objectDesc" label="描述" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="140" fixed="right">
          <template #default="{ row }">
            <el-button size="small" link @click="openEdit(row)">编辑</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
        :total="page.total" layout="total,prev,pager,next" style="margin-top:16px;justify-content:flex-end"
        @current-change="loadData" />
    </el-card>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑对象' : '新建对象'" width="480px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="对象名称" prop="objectName">
          <el-input v-model="form.objectName" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.objectDesc" type="textarea" :rows="2" />
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
const page = reactive({ num: 1, size: 10, total: 0 })
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref()
const form = reactive({ id: 0, objectName: '', objectDesc: '' })
const rules = { objectName: [{ required: true, message: '请输入对象名称', trigger: 'blur' }] }

onMounted(loadData)

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/object/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function openAdd() {
  isEdit.value = false
  Object.assign(form, { id: 0, objectName: '', objectDesc: '' })
  dialogVisible.value = true
}

function openEdit(row: any) {
  isEdit.value = true
  Object.assign(form, { id: row.id, objectName: row.objectName, objectDesc: row.objectDesc })
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value?.validate()
  if (isEdit.value) {
    await request.put('/object/update', form)
    ElMessage.success('修改成功')
  } else {
    await request.post('/object/add', form)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除对象「${row.objectName}」？`, '提示', { type: 'warning' })
  await request.delete(`/object/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
</style>
