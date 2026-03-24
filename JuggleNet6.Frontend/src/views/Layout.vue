<template>
  <el-container style="height:100vh">
    <!-- 侧边栏 -->
    <el-aside width="220px" style="background:#001529;overflow:hidden">
      <div class="sidebar-logo">
        <span>⚡</span> Juggle
      </div>
      <el-menu :default-active="activeMenu" router background-color="#001529"
        text-color="#aaa" active-text-color="#fff" style="border:none">
        <el-sub-menu index="flow">
          <template #title>
            <el-icon><Connection /></el-icon>
            <span>流程管理</span>
          </template>
          <el-menu-item index="/flow/define">流程定义</el-menu-item>
          <el-menu-item index="/flow/list">流程列表</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="suite">
          <template #title>
            <el-icon><Grid /></el-icon>
            <span>套件管理</span>
          </template>
          <el-menu-item index="/suite/list">套件列表</el-menu-item>
        </el-sub-menu>
        <el-menu-item index="/object/list">
          <el-icon><DataBoard /></el-icon>
          <span>对象管理</span>
        </el-menu-item>
        <el-sub-menu index="system">
          <template #title>
            <el-icon><Setting /></el-icon>
            <span>系统设置</span>
          </template>
          <el-menu-item index="/system/token">Token管理</el-menu-item>
          <el-menu-item index="/system/datasource">数据源管理</el-menu-item>
        </el-sub-menu>
      </el-menu>
    </el-aside>

    <el-container>
      <!-- 顶部导航 -->
      <el-header style="background:#fff;border-bottom:1px solid #eee;display:flex;align-items:center;justify-content:space-between;padding:0 24px">
        <el-breadcrumb separator="/">
          <el-breadcrumb-item>Juggle 接口编排平台</el-breadcrumb-item>
        </el-breadcrumb>
        <el-dropdown @command="handleCommand">
          <span style="cursor:pointer;display:flex;align-items:center;gap:8px">
            <el-avatar :size="32" style="background:#0f3460">{{ userName?.charAt(0)?.toUpperCase() }}</el-avatar>
            {{ userName }}
            <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="logout">退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-header>

      <!-- 主内容 -->
      <el-main style="background:#f5f7fa;overflow-y:auto">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'

const route = useRoute()
const router = useRouter()
const userName = computed(() => localStorage.getItem('userName') || 'User')
const activeMenu = computed(() => route.path)

function handleCommand(cmd: string) {
  if (cmd === 'logout') {
    ElMessageBox.confirm('确认退出登录？', '提示', { type: 'warning' })
      .then(() => {
        localStorage.removeItem('token')
        localStorage.removeItem('userName')
        router.push('/login')
      })
  }
}
</script>

<style scoped>
.sidebar-logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: bold;
  color: #fff;
  gap: 8px;
  border-bottom: 1px solid #0a2540;
}
</style>
