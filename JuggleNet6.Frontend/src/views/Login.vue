<template>
  <div class="login-container">
    <div class="login-box">
      <div class="login-logo">
        <span class="logo-icon">⚡</span>
        <h1>Juggle</h1>
        <p>接口编排平台</p>
      </div>
      <el-form ref="formRef" :model="form" :rules="rules" size="large">
        <el-form-item prop="userName">
          <el-input v-model="form.userName" placeholder="用户名" prefix-icon="User" />
        </el-form-item>
        <el-form-item prop="password">
          <el-input v-model="form.password" type="password" placeholder="密码"
            prefix-icon="Lock" show-password @keyup.enter="handleLogin" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" style="width:100%" :loading="loading" @click="handleLogin">
            登录
          </el-button>
        </el-form-item>
      </el-form>
      <p class="hint">默认账号: juggle / juggle</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '../utils/request'

const router = useRouter()
const formRef = ref()
const loading = ref(false)

const form = ref({ userName: 'juggle', password: 'juggle' })
const rules = {
  userName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

async function handleLogin() {
  await formRef.value?.validate()
  loading.value = true
  try {
    const res: any = await request.post('/user/login', form.value)
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('userName', res.data.userName)
    localStorage.setItem('roleCode', res.data.roleCode || '')
    localStorage.setItem('menuKeys', JSON.stringify(res.data.menuKeys || []))
    localStorage.setItem('tenantId', res.data.tenantId || '')
    localStorage.setItem('userId', res.data.userId || '')
    ElMessage.success('登录成功')
    router.push('/')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
}

.login-box {
  width: 380px;
  background: white;
  border-radius: 12px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.login-logo {
  text-align: center;
  margin-bottom: 32px;
}

.logo-icon {
  font-size: 48px;
}

.login-logo h1 {
  font-size: 28px;
  color: #1a1a2e;
  margin: 8px 0 4px;
}

.login-logo p {
  color: #999;
  font-size: 14px;
}

.hint {
  text-align: center;
  color: #aaa;
  font-size: 12px;
  margin-top: 16px;
}
</style>
