<template>
  <div class="page-container">
    <!-- 面包屑 -->
    <el-breadcrumb separator="/" style="margin-bottom:16px">
      <el-breadcrumb-item :to="{ path: '/object/list' }">对象管理</el-breadcrumb-item>
      <el-breadcrumb-item>{{ objectCode }} 属性</el-breadcrumb-item>
    </el-breadcrumb>

    <el-card>
      <template #header>
        <div style="display:flex;align-items:center;justify-content:space-between">
          <span>对象属性 &nbsp;<el-tag size="small" type="info">{{ objectCode }}</el-tag></span>
          <el-button type="primary" size="small" @click="openAdd">
            <el-icon><Plus /></el-icon> 新增属性
          </el-button>
        </div>
      </template>

      <el-table :data="attrList" stripe border size="small" v-loading="loading">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="paramCode" label="属性Code" min-width="140" show-overflow-tooltip />
        <el-table-column prop="paramName" label="属性名称" min-width="120" show-overflow-tooltip />
        <el-table-column prop="dataType"  label="数据类型" width="100">
          <template #default="{ row }">
            <el-tag size="small" effect="plain">{{ row.dataType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="objectCode" label="关联对象" width="160" show-overflow-tooltip>
          <template #default="{ row }">
            <span v-if="row.objectCode" style="color:#409eff">{{ row.objectCode }}</span>
            <span v-else style="color:#ccc">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="required" label="必填" width="70" align="center">
          <template #default="{ row }">
            <el-tag size="small" :type="row.required === 1 ? 'danger' : 'info'">
              {{ row.required === 1 ? '是' : '否' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="defaultValue" label="默认值" width="110" show-overflow-tooltip />
        <el-table-column prop="description"  label="描述"   min-width="140" show-overflow-tooltip />
        <el-table-column label="操作" width="110" fixed="right">
          <template #default="{ row, $index }">
            <el-button size="small" link type="primary" @click="openEdit(row, $index)">编辑</el-button>
            <el-button size="small" link type="danger" @click="doDelete($index)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div style="margin-top:16px;text-align:right">
        <el-button type="primary" @click="saveAll" :loading="saving">保存全部</el-button>
      </div>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="editIndex >= 0 ? '编辑属性' : '新增属性'"
      width="500px"
      destroy-on-close
    >
      <el-form :model="editForm" :rules="rules" ref="formRef" label-width="90px">
        <el-form-item label="属性Code" prop="paramCode">
          <el-input v-model="editForm.paramCode" placeholder="如: user_name（英文标识）" />
        </el-form-item>
        <el-form-item label="属性名称" prop="paramName">
          <el-input v-model="editForm.paramName" placeholder="中文描述名称" />
        </el-form-item>
        <el-form-item label="数据类型" prop="dataType">
          <el-select v-model="editForm.dataType" style="width:100%" @change="onTypeChange">
            <el-option label="string（字符串）"   value="string" />
            <el-option label="integer（整数）"    value="integer" />
            <el-option label="double（浮点数）"   value="double" />
            <el-option label="boolean（布尔）"    value="boolean" />
            <el-option label="json（JSON对象）"   value="json" />
            <el-option label="object（对象类型）" value="object" />
            <el-option label="array（对象数组）"  value="array" />
          </el-select>
        </el-form-item>
        <el-form-item label="关联对象" v-if="editForm.dataType === 'object' || editForm.dataType === 'array'">
          <el-select v-model="editForm.objectCode" placeholder="选择关联对象（可选）" clearable style="width:100%">
            <el-option
              v-for="obj in objectOptions"
              :key="obj.objectCode"
              :label="`${obj.objectName}（${obj.objectCode}）`"
              :value="obj.objectCode"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="是否必填">
          <el-switch v-model="editForm.required" :active-value="1" :inactive-value="0" />
        </el-form-item>
        <el-form-item label="默认值">
          <el-input v-model="editForm.defaultValue" placeholder="可选" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="editForm.description" type="textarea" :rows="2" placeholder="属性用途说明" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitForm">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Plus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import request from '../../utils/request'

const route = useRoute()
const objectId   = Number(route.params.objectId)
const objectCode = route.params.objectCode as string

const loading = ref(false)
const saving  = ref(false)
const attrList = ref<any[]>([])
const dialogVisible = ref(false)
const editIndex = ref(-1)
const formRef = ref()
const objectOptions = ref<any[]>([])

const defaultForm = () => ({
  paramCode: '', paramName: '', dataType: 'string',
  objectCode: '', required: 0, defaultValue: '', description: ''
})
const editForm = ref(defaultForm())

const rules = {
  paramCode: [{ required: true, message: '请填写属性Code', trigger: 'blur' }],
  paramName: [{ required: true, message: '请填写属性名称', trigger: 'blur' }],
  dataType:  [{ required: true, message: '请选择数据类型', trigger: 'change' }]
}

const loadList = async () => {
  loading.value = true
  try {
    // ParamType=3 对象属性
    const res: any = await request.get('/parameter/list', { params: { ownerId: objectId, paramType: 3 } })
    attrList.value = res.data ?? []
  } finally { loading.value = false }
}

const loadObjects = async () => {
  const res: any = await request.get('/object/list')
  objectOptions.value = res.data ?? []
}

const openAdd = () => {
  editIndex.value = -1
  editForm.value = defaultForm()
  dialogVisible.value = true
}

const openEdit = (row: any, idx: number) => {
  editIndex.value = idx
  editForm.value = { ...row }
  dialogVisible.value = true
}

const onTypeChange = (val: string) => {
  if (val !== 'object' && val !== 'array') editForm.value.objectCode = ''
}

const submitForm = async () => {
  await formRef.value.validate()
  if (editIndex.value >= 0) {
    attrList.value[editIndex.value] = { ...editForm.value }
  } else {
    attrList.value.push({ ...editForm.value })
  }
  dialogVisible.value = false
}

const doDelete = (idx: number) => {
  attrList.value.splice(idx, 1)
}

const saveAll = async () => {
  saving.value = true
  try {
    await request.post('/parameter/save', {
      ownerId: objectId,
      ownerCode: objectCode,
      paramType: 3,
      parameters: attrList.value
    })
    ElMessage.success('保存成功')
    loadList()
  } finally { saving.value = false }
}

onMounted(() => {
  loadList()
  loadObjects()
})
</script>

<style scoped>
.page-container { padding: 20px; }
</style>
