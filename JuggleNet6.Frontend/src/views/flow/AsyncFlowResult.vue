<template>
  <div class="page-container">
    <div class="page-header">
      <h2>异步流程结果查询</h2>
      <div class="header-desc">通过流程日志 ID（logId）查询异步触发流程的执行结果</div>
    </div>

    <!-- 查询框 -->
    <el-card class="query-card">
      <el-form inline @submit.prevent="doQuery">
        <el-form-item label="日志 ID（logId）">
          <el-input v-model="logId" placeholder="请输入 logId" clearable style="width:220px" @keyup.enter="doQuery" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" icon="Search" :loading="loading" @click="doQuery">查询</el-button>
          <el-button v-if="result && result.status === 'RUNNING'" type="warning" icon="Refresh"
            :loading="polling" @click="startPolling">自动轮询（5s）</el-button>
          <el-button v-if="polling" type="danger" icon="Close" @click="stopPolling">停止轮询</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 结果展示 -->
    <el-card v-if="result" class="result-card">
      <template #header>
        <div class="card-title">
          <span>执行结果</span>
          <el-tag :type="statusType(result.status)" size="small">
            <el-icon v-if="result.status === 'RUNNING'" class="is-loading"><Loading /></el-icon>
            {{ statusText(result.status) }}
          </el-tag>
        </div>
      </template>

      <el-descriptions :column="2" border size="small">
        <el-descriptions-item label="日志 ID">{{ result.logId }}</el-descriptions-item>
        <el-descriptions-item label="执行状态">
          <el-tag :type="statusType(result.status)" size="small">{{ statusText(result.status) }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="流程名称">{{ result.flowName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="流程 Key">{{ result.flowKey || '-' }}</el-descriptions-item>
        <el-descriptions-item label="版本">{{ result.version || '-' }}</el-descriptions-item>
        <el-descriptions-item label="耗时">{{ result.costMs != null ? result.costMs + ' ms' : '-' }}</el-descriptions-item>
        <el-descriptions-item label="开始时间">{{ formatTime(result.startTime) }}</el-descriptions-item>
        <el-descriptions-item label="结束时间">{{ result.endTime ? formatTime(result.endTime) : '执行中...' }}</el-descriptions-item>
        <el-descriptions-item v-if="result.errorMessage" label="错误信息" :span="2">
          <span style="color:#f56c6c">{{ result.errorMessage }}</span>
        </el-descriptions-item>
      </el-descriptions>

      <!-- 输出结果 -->
      <div v-if="result.status !== 'RUNNING' && result.output != null" class="output-section">
        <div class="output-title">输出结果</div>
        <pre class="output-json">{{ JSON.stringify(result.output, null, 2) }}</pre>
      </div>

      <div v-if="result.status === 'RUNNING'" class="running-tip">
        <el-icon class="is-loading"><Loading /></el-icon>
        流程正在异步执行中，可点击「自动轮询」每 5 秒自动刷新状态
      </div>
    </el-card>

    <!-- 使用说明 -->
    <el-card class="help-card">
      <template #header><span>使用说明</span></template>
      <div class="help-content">
        <p><strong>异步触发流程接口：</strong></p>
        <el-text code>POST /open/flow/triggerAsync/{flowKey}</el-text>
        <p style="margin-top:8px">请求 Header: <el-text code>X-Access-Token: {your_token}</el-text></p>
        <p>请求 Body: <el-text code>{"param1": "value1", ...}</el-text></p>
        <p>响应示例：<el-text code>{"code":200, "data":{"logId":123, "message":"流程已提交异步执行..."}}</el-text></p>
        <el-divider />
        <p><strong>查询执行结果接口：</strong></p>
        <el-text code>GET /open/flow/result/{logId}</el-text>
        <p style="margin-top:8px">请求 Header: <el-text code>X-Access-Token: {your_token}</el-text></p>
        <p>返回的 <el-text code>status</el-text> 字段：</p>
        <ul>
          <li><el-tag type="primary" size="small">RUNNING</el-tag> — 流程仍在执行中，需继续轮询</li>
          <li><el-tag type="success" size="small">SUCCESS</el-tag> — 执行成功，<el-text code>output</el-text> 中包含输出结果</li>
          <li><el-tag type="danger" size="small">FAILED</el-tag> — 执行失败，<el-text code>errorMessage</el-text> 中包含错误原因</li>
        </ul>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Loading } from '@element-plus/icons-vue'
import request from '../../utils/request'

const logId = ref('')
const loading = ref(false)
const polling = ref(false)
const result = ref<any>(null)
let pollTimer: number | null = null

async function doQuery() {
  if (!logId.value.trim()) {
    ElMessage.warning('请输入日志 ID')
    return
  }
  loading.value = true
  try {
    const res: any = await request.get(`/flow/log/asyncResult/${logId.value.trim()}`)
    result.value = res.data
    if (result.value?.status !== 'RUNNING') {
      stopPolling()
    }
  } catch (e: any) {
    ElMessage.error(e?.message || '查询失败')
  } finally {
    loading.value = false
  }
}

function startPolling() {
  if (polling.value) return
  polling.value = true
  pollTimer = window.setInterval(async () => {
    await doQuery()
    if (result.value?.status !== 'RUNNING') {
      stopPolling()
      ElMessage.success('流程执行完毕，轮询已停止')
    }
  }, 5000)
}

function stopPolling() {
  polling.value = false
  if (pollTimer !== null) {
    clearInterval(pollTimer)
    pollTimer = null
  }
}

onUnmounted(stopPolling)

function statusType(status: string) {
  if (status === 'SUCCESS') return 'success'
  if (status === 'RUNNING') return 'primary'
  return 'danger'
}

function statusText(status: string) {
  if (status === 'SUCCESS') return '成功'
  if (status === 'RUNNING') return '执行中'
  return '失败'
}

function formatTime(t: string | null) {
  if (!t) return '-'
  try { return new Date(t).toLocaleString('zh-CN') } catch { return t }
}
</script>

<style scoped>
.page-container { padding: 20px; max-width: 960px; }
.page-header { margin-bottom: 16px; }
.page-header h2 { margin: 0 0 4px; }
.header-desc { color: #888; font-size: 13px; }
.query-card { margin-bottom: 16px; }
.result-card { margin-bottom: 16px; }
.card-title { display: flex; align-items: center; gap: 10px; font-weight: 600; }
.output-section { margin-top: 16px; }
.output-title { font-weight: 600; margin-bottom: 8px; color: #333; }
.output-json {
  background: #f5f7fa;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  padding: 12px;
  font-size: 13px;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 400px;
  overflow-y: auto;
}
.running-tip {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 16px;
  color: #409eff;
  font-size: 14px;
}
.help-card { }
.help-content { font-size: 13px; line-height: 1.8; }
.help-content ul { padding-left: 20px; }
.help-content li { margin: 4px 0; display: flex; align-items: center; gap: 8px; }
</style>
