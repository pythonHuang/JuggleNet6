<template>
  <div class="static-var-page">
    <el-card shadow="never">
      <template #header>
        <div style="display:flex;align-items:center;justify-content:space-between">
          <span>全局静态变量管理</span>
          <el-button type="primary" size="small" @click="openAdd">
            <el-icon><Plus /></el-icon> 新增变量
          </el-button>
        </div>
      </template>

      <!-- 说明 -->
      <el-alert
        type="info"
        :closable="false"
        style="margin-bottom:14px"
        show-icon
      >
        <template #default>
          静态变量是跨流程的全局变量，在流程中用 <code>$static.getVariableValue('变量编码')</code> 读取，
          用 <code>$static.setVariableValue('变量编码', 新值)</code> 修改，流程执行完成后自动持久化。
          ASSIGN节点中设置 sourceType/targetType 为 <code>STATIC</code> 也可读写。
        </template>
      </el-alert>

      <el-table :data="varList" stripe border size="small" v-loading="loading">
        <el-table-column prop="varCode" label="变量编码" min-width="140" show-overflow-tooltip />
        <el-table-column prop="varName" label="变量名称" min-width="120" show-overflow-tooltip />
        <el-table-column prop="dataType" label="类型" width="90">
          <template #default="{ row }">
            <el-tag size="small" effect="plain">{{ row.dataType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="groupName" label="分组" width="100" />
        <el-table-column label="当前值" min-width="180">
          <template #default="{ row }">
            <div style="display:flex;align-items:center;gap:6px">
              <el-input
                v-model="row.value"
                size="small"
                style="flex:1"
                @keyup.enter="quickSetValue(row)"
                placeholder="当前值"
              />
              <el-button size="small" type="primary" plain @click="quickSetValue(row)" title="保存当前值">
                <el-icon><Check /></el-icon>
              </el-button>
              <el-button size="small" plain @click="resetValue(row)" title="重置为默认值">
                <el-icon><RefreshLeft /></el-icon>
              </el-button>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="defaultValue" label="默认值" width="120" show-overflow-tooltip />
        <el-table-column prop="description" label="描述" min-width="140" show-overflow-tooltip />
        <el-table-column label="操作" width="110" fixed="right">
          <template #default="{ row }">
            <el-button size="small" link type="primary" @click="openEdit(row)">编辑</el-button>
            <el-button size="small" link type="danger" @click="deleteVar(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="editForm.id ? '编辑静态变量' : '新增静态变量'"
      width="520px"
      destroy-on-close
    >
      <el-form :model="editForm" :rules="rules" ref="formRef" label-width="90px">
        <el-form-item label="变量编码" prop="varCode">
          <el-input v-model="editForm.varCode" placeholder="如: counter_total（唯一标识）" :disabled="!!editForm.id" />
        </el-form-item>
        <el-form-item label="变量名称" prop="varName">
          <el-input v-model="editForm.varName" placeholder="中文描述名称" />
        </el-form-item>
        <el-form-item label="数据类型" prop="dataType">
          <el-select v-model="editForm.dataType" style="width:100%">
            <el-option label="string（字符串）" value="string" />
            <el-option label="integer（整数）" value="integer" />
            <el-option label="double（浮点数）" value="double" />
            <el-option label="boolean（布尔）" value="boolean" />
            <el-option label="date（日期）" value="date" />
            <el-option label="json（JSON对象）" value="json" />
          </el-select>
        </el-form-item>
        <el-form-item label="默认值">
          <el-input v-model="editForm.defaultValue" placeholder="变量的初始默认值" />
        </el-form-item>
        <el-form-item label="当前值">
          <el-input v-model="editForm.value" placeholder="留空则使用默认值" />
        </el-form-item>
        <el-form-item label="分组">
          <el-input v-model="editForm.groupName" placeholder="用于分类管理，如：业务配置、计数器" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="editForm.description" type="textarea" :rows="2" placeholder="说明该变量的用途" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitForm">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Plus, Check, RefreshLeft } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const varList = ref<any[]>([])
const dialogVisible = ref(false)
const formRef = ref()

const defaultForm = () => ({
  id: null as number | null,
  varCode: '',
  varName: '',
  dataType: 'string',
  defaultValue: '',
  value: '',
  description: '',
  groupName: ''
})
const editForm = ref(defaultForm())

const rules = {
  varCode: [{ required: true, message: '请填写变量编码', trigger: 'blur' }],
  varName: [{ required: true, message: '请填写变量名称', trigger: 'blur' }],
  dataType: [{ required: true, message: '请选择数据类型', trigger: 'change' }]
}

const loadList = async () => {
  loading.value = true
  try {
    const res: any = await request.get('/system/static-var/list')
    varList.value = res.data ?? res
  } finally {
    loading.value = false
  }
}

const openAdd = () => {
  editForm.value = defaultForm()
  dialogVisible.value = true
}

const openEdit = (row: any) => {
  editForm.value = { ...row }
  dialogVisible.value = true
}

const submitForm = async () => {
  await formRef.value.validate()
  const api = editForm.value.id ? '/system/static-var/update' : '/system/static-var/add'
  const method = editForm.value.id ? 'put' : 'post'
  await (request as any)[method](api, editForm.value)
  ElMessage.success('保存成功')
  dialogVisible.value = false
  loadList()
}

const quickSetValue = async (row: any) => {
  await request.put(`/system/static-var/setValue/${row.id}`, { value: row.value })
  ElMessage.success('值已更新')
}

const resetValue = async (row: any) => {
  await ElMessageBox.confirm('将重置为默认值，确认吗?', '提示', { type: 'warning' })
  await request.put(`/system/static-var/reset/${row.id}`, {})
  ElMessage.success('已重置')
  loadList()
}

const deleteVar = async (id: number) => {
  await ElMessageBox.confirm('确定删除该变量吗?', '提示', { type: 'warning' })
  await request.delete(`/system/static-var/${id}`)
  ElMessage.success('删除成功')
  loadList()
}

onMounted(() => loadList())
</script>

<style scoped>
.static-var-page { padding: 16px; }
code {
  background: #f0f0f0;
  padding: 1px 4px;
  border-radius: 3px;
  font-size: 12px;
  color: #e83e8c;
}
</style>
