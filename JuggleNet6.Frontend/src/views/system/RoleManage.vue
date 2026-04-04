<template>
  <div class="page-container">
    <div class="page-header">
      <h2>角色管理</h2>
      <el-button type="primary" icon="Plus" @click="openDialog()">新建角色</el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="roleName" label="角色名称" width="150" />
        <el-table-column prop="roleCode" label="角色编码" width="150" show-overflow-tooltip />
        <el-table-column prop="tenantName" label="所属租户" width="130">
          <template #default="{ row }">
            <el-tag v-if="row.tenantName" type="info" size="small">{{ row.tenantName }}</el-tag>
            <el-tag v-else type="warning" size="small">全局</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" show-overflow-tooltip />
        <el-table-column prop="menuCount" label="菜单权限数" width="110" align="center" />
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click="openDialog(row)" :disabled="row.id === 1">编辑</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)" :disabled="row.id === 1">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div class="pagination-bar">
        <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
          :total="page.total" layout="total,prev,pager,next" @current-change="loadData" />
      </div>
    </el-card>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dlgVisible" :title="isEdit ? '编辑角色' : '新建角色'" width="560px" destroy-on-close>
      <el-form :model="form" label-width="90px">
        <el-form-item label="角色名称" required>
          <el-input v-model="form.roleName" placeholder="请输入角色名称" />
        </el-form-item>
        <el-form-item label="角色编码">
          <el-input v-model="form.roleCode" placeholder="如 admin, viewer（可选）" />
        </el-form-item>
        <el-form-item label="所属租户">
          <el-select v-model="form.tenantId" placeholder="留空为全局角色" clearable style="width:100%" @change="onTenantChange">
            <el-option v-for="t in tenantOptions" :key="t.id" :label="t.tenantName" :value="t.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="菜单权限">
          <div class="menu-tree">
            <template v-if="tenantMenuKeys.length > 0">
              <el-alert type="info" :closable="false" style="margin-bottom:8px">
                权限范围受租户限制，仅可选择租户已授权的菜单
              </el-alert>
            </template>
            <el-checkbox v-model="checkAll" :indeterminate="isIndeterminate" @change="onCheckAll">全选</el-checkbox>
            <el-divider style="margin:8px 0" />
            <div v-for="group in filteredMenuGroups" :key="group.label" style="margin-bottom:10px">
              <div style="font-weight:600;margin-bottom:4px;color:#333">{{ group.label }}</div>
              <el-checkbox-group v-model="form.menuKeys">
                <el-checkbox v-for="menu in group.items" :key="menu.key" :label="menu.key"
                  :disabled="menu.disabled">
                  {{ menu.title }}
                </el-checkbox>
              </el-checkbox-group>
            </div>
            <div v-if="filteredMenuGroups.every(g => g.items.length === 0)" style="color:#999;text-align:center;padding:12px">
              该租户未配置菜单权限
            </div>
          </div>
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
const isSuperAdmin = ref(localStorage.getItem('roleCode') === 'admin')
const tenantOptions = ref<any[]>([])
const tenantMenuKeys = ref<string[]>([])
const form = reactive({ roleName: '', roleCode: '', remark: '', tenantId: null as number | null, menuKeys: [] as string[] })

// 菜单树结构
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

// 根据租户权限过滤菜单组（租户权限限制时禁用不可选项）
const filteredMenuGroups = computed(() => {
  if (tenantMenuKeys.value.length === 0) {
    // 无限制：全部可选
    return menuGroups.map(g => ({
      label: g.label,
      items: g.items.map(i => ({ ...i, disabled: false }))
    }))
  }
  return menuGroups.map(g => ({
    label: g.label,
    items: g.items.map(i => ({
      ...i,
      disabled: !tenantMenuKeys.value.includes(i.key)
    }))
  }))
})

// 可选的菜单 key（受租户权限限制）
const availableMenuKeys = computed(() => {
  if (tenantMenuKeys.value.length === 0) return menuGroups.flatMap(g => g.items.map(i => i.key))
  return menuGroups.flatMap(g => g.items.map(i => i.key)).filter(k => tenantMenuKeys.value.includes(k))
})

const checkAll = computed({
  get: () => availableMenuKeys.value.length > 0 && form.menuKeys.length === availableMenuKeys.value.length,
  set: () => {}
})
const isIndeterminate = computed(() => form.menuKeys.length > 0 && form.menuKeys.length < availableMenuKeys.value.length)

function onCheckAll(val: boolean) {
  form.menuKeys = val ? [...availableMenuKeys.value] : []
}

function onTenantChange() {
  // 切换租户时，重置菜单选择为空（等待详情加载后再恢复）
  form.menuKeys = []
  // 如果有选定租户，加载该租户的菜单权限边界
  if (form.tenantId) {
    loadTenantMenuKeys(form.tenantId)
  } else {
    tenantMenuKeys.value = []
  }
}

async function loadTenantMenuKeys(tenantId: number) {
  try {
    const res: any = await request.get(`/tenant/detail/${tenantId}`)
    tenantMenuKeys.value = res.data.menuKeys || []
  } catch {
    tenantMenuKeys.value = []
  }
}

async function loadTenantOptions() {
  try {
    const res: any = await request.get('/tenant/active')
    tenantOptions.value = res.data || []
  } catch { /* ignore */ }
}

onMounted(() => { loadData(); loadTenantOptions() })

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/role/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

async function openDialog(row?: any) {
  isEdit.value = !!row
  editId.value = row?.id || 0
  form.roleName = row?.roleName || ''
  form.roleCode = row?.roleCode || ''
  form.remark = row?.remark || ''
  form.tenantId = row?.tenantId || null
  form.menuKeys = []
  tenantMenuKeys.value = []

  // 编辑时加载详情（含菜单权限 + 租户权限边界）
  if (row?.id) {
    try {
      const res: any = await request.get(`/role/detail/${row.id}`)
      form.menuKeys = res.data.menuKeys || []
      tenantMenuKeys.value = res.data.tenantMenuKeys || []
    } catch { /* ignore */ }
  }

  dlgVisible.value = true
}

async function doSubmit() {
  if (!form.roleName) { ElMessage.warning('请输入角色名称'); return }

  // 超管可设租户，非超管不能设置（后端也会校验）
  submitting.value = true
  try {
    const payload = {
      roleName: form.roleName,
      roleCode: form.roleCode,
      remark: form.remark,
      tenantId: isSuperAdmin.value ? form.tenantId : null,
      menuKeys: form.menuKeys
    }
    if (isEdit.value) {
      await request.put('/role/update', { id: editId.value, ...payload })
    } else {
      await request.post('/role/add', payload)
    }
    ElMessage.success(isEdit.value ? '更新成功' : '新增成功')
    dlgVisible.value = false
    loadData()
  } finally { submitting.value = false }
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除角色「${row.roleName}」？`, '提示', { type: 'warning' })
  await request.delete(`/role/delete/${row.id}`)
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
.menu-tree {
  border: 1px solid #ebeef5; border-radius: 6px; padding: 12px 16px;
  max-height: 300px; overflow-y: auto; width: 100%;
}
.menu-tree .el-checkbox { margin-right: 12px; margin-bottom: 4px; }
.menu-tree .el-checkbox.is-disabled { opacity: 0.45; }
</style>
