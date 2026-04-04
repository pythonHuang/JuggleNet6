<template>
  <div class="webhook-container">
    <!-- 头部 -->
    <div class="page-header">
      <h2>Webhook 管理</h2>
      <el-button type="primary" @click="openDialog()" icon="Plus">新建 Webhook</el-button>
    </div>

    <!-- 列表 -->
    <div class="table-card">
    <el-table :data="webhooks" v-loading="loading" stripe border height="100%">
      <el-table-column prop="webhookKey" label="Webhook Key" min-width="180">
        <template #default="{ row }">
          <code style="background:#f5f5f5;padding:2px 6px;border-radius:4px;font-size:12px">{{ row.webhookKey }}</code>
          <el-button type="primary" link size="small" @click="copyUrl(row)" style="margin-left:8px" icon="CopyDocument" />
        </template>
      </el-table-column>
      <el-table-column prop="webhookName" label="名称" min-width="120" />
      <el-table-column prop="flowKey" label="关联流程" min-width="150">
        <template #default="{ row }">
          {{ row.flowName || row.flowKey }}
        </template>
      </el-table-column>
      <el-table-column prop="allowedMethod" label="请求方法" width="100" align="center">
        <template #default="{ row }">
          <el-tag size="small" :type="row.allowedMethod === 'POST' ? 'primary' : 'success'">{{ row.allowedMethod }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="asyncMode" label="执行模式" width="100" align="center">
        <template #default="{ row }">
          <el-tag size="small" :type="row.asyncMode == 1 ? 'warning' : 'info'">{{ row.asyncMode == 1 ? '异步' : '同步' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="80" align="center">
        <template #default="{ row }">
          <el-switch v-model="row.status" :active-value="1" :inactive-value="0" @change="toggleStatus(row)" size="small" />
        </template>
      </el-table-column>
      <el-table-column prop="triggerCount" label="触发次数" width="90" align="center" />
      <el-table-column prop="lastTriggerTime" label="最后触发" width="160">
        <template #default="{ row }">
          <span v-if="row.lastTriggerTime" style="font-size:12px;color:#666">{{ formatTime(row.lastTriggerTime) }}</span>
          <span v-else style="color:#ccc">-</span>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="150" fixed="right">
        <template #default="{ row }">
          <el-button type="primary" link size="small" @click="openDialog(row)">编辑</el-button>
          <el-button type="danger" link size="small" @click="deleteWebhook(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
    <div class="pagination-bar">
      <el-pagination v-model:current-page="pageNum" v-model:page-size="pageSize"
        :total="total" :page-sizes="[10,20,50]" layout="total, sizes, prev, pager, next"
        @size-change="loadList" @current-change="loadList" />
    </div>
    </div>

    <!-- 新建/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="editId ? '编辑 Webhook' : '新建 Webhook'" width="560px" destroy-on-close>
      <el-form :model="form" label-width="100px" label-position="right">
        <el-form-item label="Webhook Key" required>
          <el-input v-model="form.webhookKey" placeholder="唯一标识，如 order_created" :disabled="!!editId" />
          <div style="font-size:12px;color:#999;margin-top:4px">触发 URL: POST http://host/open/webhook/{webhookKey}</div>
        </el-form-item>
        <el-form-item label="名称">
          <el-input v-model="form.webhookName" placeholder="便于识别的名称" />
        </el-form-item>
        <el-form-item label="关联流程" required>
          <el-select v-model="form.flowKey" placeholder="选择已发布的流程" filterable style="width:100%">
            <el-option v-for="f in publishedFlows" :key="f.flowKey" :value="f.flowKey"
              :label="f.flowName + ' (' + f.flowKey + ')'" />
          </el-select>
        </el-form-item>
        <el-form-item label="请求方法">
          <el-radio-group v-model="form.allowedMethod">
            <el-radio value="POST">POST</el-radio>
            <el-radio value="GET">GET</el-radio>
            <el-radio value="ANY">ANY</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="执行模式">
          <el-radio-group v-model="form.asyncMode">
            <el-radio :value="0">同步（等待结果）</el-radio>
            <el-radio :value="1">异步（立即返回）</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="签名密钥">
          <el-input v-model="form.secret" placeholder="留空则不验签" show-password />
          <div style="font-size:12px;color:#999;margin-top:4px">HMAC-SHA256 签名，调用方需在 Header 中携带 X-Webhook-Signature</div>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" placeholder="备注说明" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveWebhook" :loading="saving">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const saving = ref(false)
const webhooks = ref<any[]>([])
const total = ref(0)
const pageNum = ref(1)
const pageSize = ref(20)
const dialogVisible = ref(false)
const editId = ref<number>(0)
const publishedFlows = ref<any[]>([])

const form = ref<any>({
  webhookKey: '', webhookName: '', flowKey: '', flowName: '',
  secret: '', allowedMethod: 'POST', asyncMode: 0, remark: ''
})

onMounted(() => {
  loadList()
  loadFlows()
})

async function loadList() {
  loading.value = true
  try {
    const res: any = await request.post('/system/webhook/page', {
      pageNum: pageNum.value, pageSize: pageSize.value
    })
    webhooks.value = res.data?.records || []
    total.value = res.data?.total || 0
  } catch {}
  loading.value = false
}

async function loadFlows() {
  try {
    const res: any = await request.get('/system/webhook/published-flows')
    publishedFlows.value = res.data || []
  } catch {}
}

function openDialog(row?: any) {
  if (row) {
    editId.value = row.id
    form.value = {
      webhookKey: row.webhookKey || '',
      webhookName: row.webhookName || '',
      flowKey: row.flowKey || '',
      flowName: row.flowName || '',
      secret: row.secret || '',
      allowedMethod: row.allowedMethod || 'POST',
      asyncMode: row.asyncMode ?? 0,
      remark: row.remark || ''
    }
  } else {
    editId.value = 0
    form.value = {
      webhookKey: '', webhookName: '', flowKey: '', flowName: '',
      secret: '', allowedMethod: 'POST', asyncMode: 0, remark: ''
    }
  }
  dialogVisible.value = true
}

async function saveWebhook() {
  if (!form.value.webhookKey) return ElMessage.warning('请填写 Webhook Key')
  if (!form.value.flowKey) return ElMessage.warning('请选择关联流程')

  saving.value = true
  try {
    if (editId.value > 0) {
      await request.put('/system/webhook/update', { id: editId.value, ...form.value })
      ElMessage.success('更新成功')
    } else {
      await request.post('/system/webhook/save', form.value)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadList()
  } catch {}
  saving.value = false
}

async function deleteWebhook(row: any) {
  await ElMessageBox.confirm(`确定删除 Webhook [${row.webhookKey}]？`, '确认')
  try {
    await request.delete(`/system/webhook/delete/${row.id}`)
    ElMessage.success('删除成功')
    loadList()
  } catch {}
}

async function toggleStatus(row: any) {
  try {
    await request.put(`/system/webhook/toggle/${row.id}`)
    ElMessage.success(row.status === 1 ? '已启用' : '已禁用')
  } catch {
    row.status = row.status === 1 ? 0 : 1
  }
}

function copyUrl(row: any) {
  const baseUrl = window.location.origin
  const url = `${baseUrl}/open/webhook/${row.webhookKey}`
  navigator.clipboard.writeText(url).then(() => {
    ElMessage.success('已复制触发 URL')
  }).catch(() => {
    ElMessage.warning('复制失败，请手动复制')
  })
}

function formatTime(t: string) {
  if (!t) return ''
  try { return new Date(t).toLocaleString('zh-CN') } catch { return t }
}
</script>

<style scoped>
.webhook-container {
  padding: 16px;
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-sizing: border-box;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  flex-shrink: 0;
}
.page-header h2 { margin: 0; font-size: 18px; }
.table-card {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.table-card :deep(.el-table) {
  flex: 1;
  min-height: 0;
}
.pagination-bar {
  flex-shrink: 0;
  padding: 10px 0 2px;
  display: flex;
  justify-content: flex-end;
}
</style>
