<template>
  <div class="page-container">
    <div class="page-header">
      <h2>租户管理</h2>
      <el-button type="primary" icon="Plus" @click="openDialog()">新建租户</el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="tenantName" label="租户名称" width="150" />
        <el-table-column prop="tenantCode" label="租户编码" width="130" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'danger'" size="small">
              {{ row.status === 1 ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="menuCount" label="菜单权限数" width="100" align="center" />
        <el-table-column prop="userCount" label="用户数" width="80" align="center" />
        <el-table-column prop="expiredAt" label="过期时间" width="170" show-overflow-tooltip>
          <template #default="{ row }">
            <span v-if="row.expiredAt" :class="{ 'text-danger': isExpired(row.expiredAt) }">
              {{ formatTime(row.expiredAt) }}
              <el-tag v-if="isExpired(row.expiredAt)" type="danger" size="small" style="margin-left:4px">已过期</el-tag>
            </span>
            <span v-else style="color:#999">永不过期</span>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" show-overflow-tooltip />
        <el-table-column prop="createdAt" label="创建时间" width="170" show-overflow-tooltip />
        <el-table-column label="操作" width="160" fixed="right">
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
    <el-dialog v-model="dlgVisible" :title="isEdit ? '编辑租户' : '新建租户'" width="680px" destroy-on-close>
      <el-form :model="form" label-width="100px">
        <el-form-item label="租户名称" required>
          <el-input v-model="form.tenantName" />
        </el-form-item>
        <el-form-item label="租户编码">
          <el-input v-model="form.tenantCode" placeholder="如 company-a（可选）" />
        </el-form-item>
        <el-form-item label="过期时间">
          <el-date-picker v-model="form.expiredAt" type="datetime" placeholder="不设置则永不过期"
            format="YYYY-MM-DD HH:mm" value-format="YYYY-MM-DDTHH:mm:ss" style="width:100%" />
        </el-form-item>
        <el-form-item label="状态" v-if="isEdit">
          <el-switch v-model="form.status" :active-value="1" :inactive-value="0" />
        </el-form-item>
        <el-form-item label="关联用户">
          <el-select v-model="form.userIds" multiple filterable remote reserve-keyword
            placeholder="搜索用户" :remote-method="searchUsers" :loading="userSearchLoading" style="width:100%">
            <el-option v-for="u in userOptions" :key="u.id" :label="`${u.userName} (ID:${u.id})`" :value="u.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="菜单权限">
          <div class="menu-tree">
            <el-checkbox v-model="checkAll" :indeterminate="isIndeterminate" @change="onCheckAll">全选</el-checkbox>
            <el-divider style="margin:8px 0" />
            <div v-for="group in menuGroups" :key="group.label" style="margin-bottom:10px">
              <div style="font-weight:600;margin-bottom:4px;color:#333">{{ group.label }}</div>
              <el-checkbox-group v-model="form.menuKeys">
                <el-checkbox v-for="menu in group.items" :key="menu.key" :label="menu.key">
                  {{ menu.title }}
                </el-checkbox>
              </el-checkbox-group>
            </div>
          </div>
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
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const submitting = ref(false)
const tableData = ref<any[]>([])
const page = reactive({ num: 1, size: 10, total: 0 })
const dlgVisible = ref(false)
const isEdit = ref(false)
const editId = ref(0)
const form = reactive({
  tenantName: '', tenantCode: '', status: 1, remark: '',
  expiredAt: '' as any,
  menuKeys: [] as string[],
  userIds: [] as number[]
})
const userSearchLoading = ref(false)
const userOptions = ref<any[]>([])

const menuGroups = [
  { label: '流程管理', items: [
    { key: '/flow/define', title: '流程定义' }, { key: '/flow/dashboard', title: '监控仪表盘' },
    { key: '/flow/list', title: '流程列表' }, { key: '/flow/log', title: '执行日志' },
    { key: '/flow/testcase', title: '测试用例' }, { key: '/flow/async-result', title: '异步结果查询' }
  ]},
  { label: '套件管理', items: [{ key: '/suite/list', title: '套件列表' }]},
  { label: '对象管理', items: [{ key: '/object/list', title: '对象管理' }]},
  { label: '系统设置', items: [
    { key: '/system/token', title: 'Token管理' }, { key: '/system/datasource', title: '数据源管理' },
    { key: '/system/static-var', title: '静态变量' }, { key: '/system/schedule', title: '定时任务' },
    { key: '/system/webhook', title: 'Webhook管理' }, { key: '/system/users', title: '用户管理' },
    { key: '/system/config', title: '系统配置' }, { key: '/system/role', title: '角色管理' },
    { key: '/system/tenant', title: '租户管理' }
  ]}
]

const allMenuKeys = computed(() => menuGroups.flatMap(g => g.items.map(i => i.key)))
const checkAll = computed({
  get: () => form.menuKeys.length === allMenuKeys.value.length && allMenuKeys.value.length > 0,
  set: () => {}
})
const isIndeterminate = computed(() => form.menuKeys.length > 0 && form.menuKeys.length < allMenuKeys.value.length)

function onCheckAll(val: boolean) { form.menuKeys = val ? [...allMenuKeys.value] : [] }

function isExpired(expiredAt: string): boolean {
  return new Date(expiredAt) < new Date()
}

function formatTime(t: string): string {
  if (!t) return ''
  return t.replace('T', ' ').substring(0, 16)
}

async function searchUsers(query: string) {
  if (!query) return
  userSearchLoading.value = true
  try {
    const res: any = await request.post('/user/page', { pageNum: 1, pageSize: 20, keyword: query })
    userOptions.value = res.data.records
  } finally { userSearchLoading.value = false }
}

onMounted(() => loadData())

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/tenant/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

async function openDialog(row?: any) {
  isEdit.value = !!row
  editId.value = row?.id || 0
  form.tenantName = row?.tenantName || ''
  form.tenantCode = row?.tenantCode || ''
  form.status = row?.status ?? 1
  form.remark = row?.remark || ''
  form.expiredAt = row?.expiredAt || ''
  form.userIds = []

  if (row?.id) {
    const res: any = await request.get(`/tenant/detail/${row.id}`)
    form.menuKeys = res.data.menuKeys || []
    form.userIds = res.data.userIds || []
    // 加载已关联用户到选项
    if (res.data.users?.length > 0) {
      userOptions.value = res.data.users.map((u: any) => ({ id: u.id, userName: u.userName }))
    }
  } else {
    form.menuKeys = []
    userOptions.value = []
  }
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
.page-container { padding: 16px; height: 100%; display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; flex-shrink: 0; }
.page-header h2 { font-size: 20px; color: #333; }
.table-card { flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden; }
.table-card :deep(.el-card__body) { flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden; padding-bottom: 0; }
.table-card :deep(.el-table) { flex: 1; min-height: 0; }
.pagination-bar { flex-shrink: 0; padding: 10px 0 2px; display: flex; justify-content: flex-end; }
.menu-tree { border: 1px solid #ebeef5; border-radius: 6px; padding: 12px 16px; max-height: 260px; overflow-y: auto; width: 100%; }
.menu-tree .el-checkbox { margin-right: 12px; margin-bottom: 4px; }
.text-danger { color: #f56c6c; }
</style>
