<template>
  <div class="page-container">
    <div class="page-header">
      <h2>用户管理</h2>
      <el-button type="primary" icon="Plus" @click="openAdd">新增用户</el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="tableData" stripe v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="userName" label="用户名" />
        <el-table-column prop="roleName" label="角色" width="130">
          <template #default="{ row }">
            <el-tag v-if="row.roleName" size="small">{{ row.roleName }}</el-tag>
            <span v-else style="color:#ccc">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="tenantName" label="租户" width="130">
          <template #default="{ row }">
            <el-tag v-if="row.tenantName" type="info" size="small">{{ row.tenantName }}</el-tag>
            <span v-else style="color:#ccc">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" link @click="openReset(row)">重置密码</el-button>
            <el-button size="small" type="danger" link @click="doDelete(row)"
              :disabled="row.id === 1">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div class="pagination-bar">
        <el-pagination v-model:current-page="page.num" v-model:page-size="page.size"
          :total="page.total" layout="total,prev,pager,next"
          @current-change="loadData" />
      </div>
    </el-card>

    <!-- 修改我的密码 -->
    <el-card class="pwd-card">
      <template #header><span>修改我的密码</span></template>
      <el-form :model="changePwdForm" label-width="100px" style="max-width:400px">
        <el-form-item label="原密码">
          <el-input v-model="changePwdForm.oldPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="新密码">
          <el-input v-model="changePwdForm.newPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="确认新密码">
          <el-input v-model="changePwdForm.confirmPassword" type="password" show-password />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="doChangePwd">确认修改</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 新增用户弹窗 -->
    <el-dialog v-model="addVisible" title="新增用户" width="450px">
      <el-form :model="addForm" label-width="80px">
        <el-form-item label="用户名" required>
          <el-input v-model="addForm.userName" />
        </el-form-item>
        <el-form-item label="密码" required>
          <el-input v-model="addForm.password" type="password" show-password />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="addForm.roleId" placeholder="请选择角色" clearable style="width:100%">
            <el-option v-for="r in roleOptions" :key="r.id" :label="r.roleName" :value="r.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="租户">
          <el-select v-model="addForm.tenantId" placeholder="请选择租户" clearable style="width:100%">
            <el-option v-for="t in tenantOptions" :key="t.id" :label="t.tenantName" :value="t.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="addVisible = false">取消</el-button>
        <el-button type="primary" @click="doAdd">确认</el-button>
      </template>
    </el-dialog>

    <!-- 重置密码弹窗 -->
    <el-dialog v-model="resetVisible" :title="`重置密码：${resetForm.userName}`" width="400px">
      <el-form :model="resetForm" label-width="80px">
        <el-form-item label="新密码">
          <el-input v-model="resetForm.newPassword" type="password" show-password />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="resetVisible = false">取消</el-button>
        <el-button type="primary" @click="doReset">确认重置</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const tableData = ref<any[]>([])
const page = reactive({ num: 1, size: 10, total: 0 })
const addVisible = ref(false)
const resetVisible = ref(false)
const roleOptions = ref<any[]>([])
const tenantOptions = ref<any[]>([])
const addForm = reactive({ userName: '', password: '', roleId: null as number | null, tenantId: null as number | null })
const resetForm = reactive({ id: 0, userName: '', newPassword: '' })
const changePwdForm = reactive({ oldPassword: '', newPassword: '', confirmPassword: '' })

onMounted(() => { loadData(); loadOptions() })

async function loadOptions() {
  try {
    const [roles, tenants] = await Promise.all([
      request.get('/role/all') as any,
      request.get('/tenant/all') as any
    ])
    roleOptions.value = roles.data || []
    tenantOptions.value = tenants.data || []
  } catch { /* ignore */ }
}

async function loadData() {
  loading.value = true
  try {
    const res: any = await request.post('/user/page', { pageNum: page.num, pageSize: page.size })
    tableData.value = res.data.records
    page.total = res.data.total
  } finally { loading.value = false }
}

function openAdd() {
  addForm.userName = ''; addForm.password = ''
  addForm.roleId = null; addForm.tenantId = null
  addVisible.value = true
}

async function doAdd() {
  if (!addForm.userName || !addForm.password) { ElMessage.warning('请填写完整'); return }
  await request.post('/user/add', addForm)
  ElMessage.success('新增成功')
  addVisible.value = false
  loadData()
}

function openReset(row: any) {
  resetForm.id = row.id; resetForm.userName = row.userName; resetForm.newPassword = ''
  resetVisible.value = true
}

async function doReset() {
  if (!resetForm.newPassword) { ElMessage.warning('请输入新密码'); return }
  await request.put('/user/resetPwd', { id: resetForm.id, newPassword: resetForm.newPassword })
  ElMessage.success('密码重置成功')
  resetVisible.value = false
}

async function doDelete(row: any) {
  await ElMessageBox.confirm(`确认删除用户「${row.userName}」？`, '提示', { type: 'warning' })
  await request.delete(`/user/delete/${row.id}`)
  ElMessage.success('删除成功')
  loadData()
}

async function doChangePwd() {
  if (!changePwdForm.oldPassword || !changePwdForm.newPassword) {
    ElMessage.warning('请填写完整'); return
  }
  if (changePwdForm.newPassword !== changePwdForm.confirmPassword) {
    ElMessage.error('两次密码不一致'); return
  }
  await request.put('/user/changePwd', {
    oldPassword: changePwdForm.oldPassword,
    newPassword: changePwdForm.newPassword
  })
  ElMessage.success('密码修改成功，请重新登录')
  changePwdForm.oldPassword = ''; changePwdForm.newPassword = ''; changePwdForm.confirmPassword = ''
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
.pwd-card { flex-shrink: 0; margin-top: 12px; }
</style>
