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
        <el-table-column label="操作" width="140" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="openPermissions(row)">权限</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新建 Token 弹窗 -->
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

    <!-- 权限设置弹窗 -->
    <el-dialog v-model="permDialogVisible" title="Token 权限设置" width="700px" :close-on-click-modal="false">
      <div class="perm-tip">不配置任何权限时，该 Token 可访问所有流程。配置后仅可访问已勾选的流程和接口。</div>

      <el-tabs v-model="permTab">
        <el-tab-pane label="流程权限" name="flow">
          <div style="margin-bottom:10px;display:flex;justify-content:space-between;align-items:center">
            <el-input v-model="flowSearch" placeholder="搜索流程" size="small" clearable style="width:200px" />
            <el-button size="small" @click="toggleAllFlows">全选/取消</el-button>
          </div>
          <div style="max-height:350px;overflow-y:auto">
            <el-checkbox-group v-model="selectedFlowKeys">
              <div v-for="f in filteredFlows" :key="f.flowKey" style="margin-bottom:4px">
                <el-checkbox :value="f.flowKey" :label="f.flowName + ' (' + f.flowKey + ')'" />
              </div>
            </el-checkbox-group>
            <div v-if="filteredFlows.length === 0" style="color:#999;text-align:center;padding:20px">暂无已部署的流程</div>
          </div>
        </el-tab-pane>

        <el-tab-pane label="接口权限" name="api">
          <div style="margin-bottom:10px;display:flex;justify-content:space-between;align-items:center">
            <el-input v-model="apiSearch" placeholder="搜索接口" size="small" clearable style="width:200px" />
            <el-button size="small" @click="toggleAllApis">全选/取消</el-button>
          </div>
          <div style="max-height:350px;overflow-y:auto">
            <el-checkbox-group v-model="selectedApiCodes">
              <div v-for="api in filteredApis" :key="api.methodCode" style="margin-bottom:4px">
                <el-checkbox :value="api.methodCode" :label="api.methodName + ' [' + api.requestType + '] ' + (api.url || '')" />
              </div>
            </el-checkbox-group>
            <div v-if="filteredApis.length === 0" style="color:#999;text-align:center;padding:20px">暂无接口</div>
          </div>
        </el-tab-pane>
      </el-tabs>

      <template #footer>
        <el-button @click="permDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="savePermissions" :loading="permSaving">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const tableData = ref([])
const dialogVisible = ref(false)
const formRef = ref()
const form = reactive({ tokenName: '', expiredAt: '' })
const rules = { tokenName: [{ required: true, message: '请输入Token名称', trigger: 'blur' }] }

// 权限相关
const permDialogVisible = ref(false)
const permSaving = ref(false)
const permTab = ref('flow')
const permTokenId = ref(0)
const flowSearch = ref('')
const apiSearch = ref('')
const selectedFlowKeys = ref<string[]>([])
const selectedApiCodes = ref<string[]>([])
const allFlows = ref<any[]>([])
const allApis = ref<any[]>([])

const filteredFlows = computed(() => {
  if (!flowSearch.value) return allFlows.value
  const kw = flowSearch.value.toLowerCase()
  return allFlows.value.filter(f => (f.flowName + f.flowKey).toLowerCase().includes(kw))
})

const filteredApis = computed(() => {
  if (!apiSearch.value) return allApis.value
  const kw = apiSearch.value.toLowerCase()
  return allApis.value.filter(a => (a.methodName + a.methodCode + (a.url || '')).toLowerCase().includes(kw))
})

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

// ===== 权限管理 =====
async function openPermissions(row: any) {
  permTokenId.value = row.id
  permTab.value = 'flow'
  permDialogVisible.value = true

  // 并行加载流程列表、接口列表、已有权限
  const [flowsRes, permsRes] = await Promise.all([
    request.post('/flow/info/page', { pageNum: 1, pageSize: 500 }),
    request.get(`/system/token/permissions/${row.id}`)
  ])

  allFlows.value = flowsRes.data?.records || []
  // 加载所有套件的接口
  const suitesRes: any = await request.get('/suite/list')
  const apiList: any[] = []
  for (const suite of (suitesRes.data || [])) {
    const apisRes: any = await request.post('/suite/api/list', { suiteCode: suite.suiteCode })
    for (const api of (apisRes.data || [])) {
      apiList.push({ ...api, suiteName: suite.suiteName })
    }
  }
  allApis.value = apiList

  // 回显已有权限
  const perms: any[] = permsRes.data || []
  selectedFlowKeys.value = perms.filter(p => p.permissionType === 'FLOW').map(p => p.resourceKey)
  selectedApiCodes.value = perms.filter(p => p.permissionType === 'API').map(p => p.resourceKey)
}

function toggleAllFlows() {
  if (selectedFlowKeys.value.length === filteredFlows.value.length) {
    selectedFlowKeys.value = []
  } else {
    selectedFlowKeys.value = filteredFlows.value.map(f => f.flowKey)
  }
}

function toggleAllApis() {
  if (selectedApiCodes.value.length === filteredApis.value.length) {
    selectedApiCodes.value = []
  } else {
    selectedApiCodes.value = filteredApis.value.map(a => a.methodCode)
  }
}

async function savePermissions() {
  permSaving.value = true
  try {
    const permissions: any[] = []
    for (const key of selectedFlowKeys.value) {
      const flow = allFlows.value.find(f => f.flowKey === key)
      permissions.push({ permissionType: 'FLOW', resourceKey: key, resourceName: flow?.flowName || '' })
    }
    for (const code of selectedApiCodes.value) {
      const api = allApis.value.find(a => a.methodCode === code)
      permissions.push({ permissionType: 'API', resourceKey: code, resourceName: api?.methodName || '' })
    }
    await request.post(`/system/token/permissions/${permTokenId.value}`, permissions)
    ElMessage.success('权限保存成功')
    permDialogVisible.value = false
  } finally {
    permSaving.value = false
  }
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.perm-tip { color: #e6a23c; font-size: 12px; margin-bottom: 12px; background: #fdf6ec; padding: 8px 12px; border-radius: 4px; }
</style>
