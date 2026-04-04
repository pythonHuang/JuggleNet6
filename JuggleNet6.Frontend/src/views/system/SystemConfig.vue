<template>
  <div class="page-container">
    <div class="page-header">
      <h2>系统配置中心</h2>
      <el-button type="primary" icon="Check" @click="saveAll" :loading="saving">保存配置</el-button>
    </div>

    <div v-loading="loading">
      <!-- 邮件配置 -->
      <el-card class="config-card">
        <template #header>
          <div class="card-title"><el-icon><Message /></el-icon> 邮件 SMTP 配置</div>
        </template>
        <el-form :model="form" label-width="130px">
          <el-row :gutter="24">
            <el-col :span="12">
              <el-form-item label="SMTP服务器">
                <el-input v-model="form['email.smtp.host']" placeholder="smtp.example.com" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="端口">
                <el-input v-model="form['email.smtp.port']" placeholder="465" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="启用SSL">
                <el-switch v-model="form['email.smtp.ssl']" active-value="true" inactive-value="false" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="SMTP用户名">
                <el-input v-model="form['email.smtp.username']" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="SMTP密码">
                <el-input v-model="form['email.smtp.password']" type="password" show-password />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="发件人地址">
                <el-input v-model="form['email.from.address']" placeholder="noreply@example.com" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="发件人名称">
                <el-input v-model="form['email.from.name']" placeholder="Juggle告警" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </el-card>

      <!-- 告警配置 -->
      <el-card class="config-card">
        <template #header>
          <div class="card-title"><el-icon><Bell /></el-icon> 告警配置</div>
        </template>
        <el-form :model="form" label-width="150px">
          <el-row :gutter="24">
            <el-col :span="8">
              <el-form-item label="启用全局告警">
                <el-switch v-model="form['alert.enabled']" active-value="true" inactive-value="false" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="流程失败时告警">
                <el-switch v-model="form['alert.on.fail.enabled']" active-value="true" inactive-value="false" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="慢执行阈值(ms)">
                <el-input v-model="form['alert.min.cost.ms']" placeholder="0=不限" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="告警Webhook地址">
                <el-input v-model="form['alert.webhook.url']" placeholder="https://..." />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="告警Webhook密钥">
                <el-input v-model="form['alert.webhook.secret']" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="告警邮件收件人">
                <el-input v-model="form['alert.email.to']" placeholder="admin@example.com" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </el-card>

      <!-- 系统配置 -->
      <el-card class="config-card">
        <template #header>
          <div class="card-title"><el-icon><Setting /></el-icon> 系统配置</div>
        </template>
        <el-form :model="form" label-width="130px">
          <el-row :gutter="24">
            <el-col :span="8">
              <el-form-item label="默认分页大小">
                <el-input-number v-model.number="form['system.page.size']" :min="5" :max="100" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="日志保留天数">
                <el-input-number v-model.number="form['system.log.keep.days']" :min="1" :max="365" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Message, Bell, Setting } from '@element-plus/icons-vue'
import request from '../../utils/request'

const loading = ref(false)
const saving = ref(false)
const form = reactive<Record<string, any>>({})

onMounted(() => loadConfig())

async function loadConfig() {
  loading.value = true
  try {
    const res: any = await request.get('/system/config/all')
    const grouped = res.data || {}
    // 展平所有分组的配置项到 form
    for (const group of Object.values(grouped) as any[]) {
      for (const item of group) {
        form[item.configKey] = item.configValue ?? ''
      }
    }
  } finally { loading.value = false }
}

async function saveAll() {
  saving.value = true
  try {
    const items = Object.entries(form).map(([configKey, configValue]) => ({ configKey, configValue: String(configValue) }))
    await request.post('/system/config/save', items)
    ElMessage.success('配置保存成功')
  } finally { saving.value = false }
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.page-header h2 { font-size: 20px; color: #333; }
.config-card { margin-bottom: 16px; }
.card-title { display: flex; align-items: center; gap: 8px; font-weight: 600; }
</style>
