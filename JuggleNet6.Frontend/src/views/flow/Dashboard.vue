<template>
  <div class="dashboard-page">
    <h2 style="margin-bottom:16px">📊 监控仪表盘</h2>

    <!-- 今日概览卡片 -->
    <el-row :gutter="16" style="margin-bottom:20px">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-number" style="color:#409eff">{{ stats.today?.total ?? 0 }}</div>
          <div class="stat-label">今日执行</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-number" style="color:#67c23a">{{ stats.today?.success ?? 0 }}</div>
          <div class="stat-label">今日成功</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-number" style="color:#f56c6c">{{ stats.today?.failed ?? 0 }}</div>
          <div class="stat-label">今日失败</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-number" style="color:#e6a23c">{{ stats.today?.successRate ?? 0 }}%</div>
          <div class="stat-label">成功率</div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-bottom:20px">
      <el-col :span="8">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-number">{{ stats.avgCostMs ?? 0 }}</div>
          <div class="stat-label">平均耗时(ms)</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-number">{{ stats.totalLogs ?? 0 }}</div>
          <div class="stat-label">总执行次数</div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover" class="stat-card" style="cursor:pointer" @click="$router.push('/flow/log')">
          <div class="stat-number" style="color:#f56c6c">{{ stats.recentFailed?.length ?? 0 }}</div>
          <div class="stat-label">最近失败（点击查看）</div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16">
      <!-- 近7天趋势 -->
      <el-col :span="14">
        <el-card shadow="hover">
          <template #header><span style="font-weight:600">📈 近7天执行趋势</span></template>
          <div class="chart-area" v-if="stats.weekTrend?.length">
            <div v-for="item in stats.weekTrend" :key="item.date" class="bar-group">
              <el-tooltip :content="`${item.date}: 总${item.total} 成功${item.success} 失败${item.failed}`" placement="top">
                <div class="bar-wrapper">
                  <div class="bar bar-success" :style="{ height: barHeight(item.success, maxTotal) + 'px' }">
                    <span class="bar-label" v-if="item.success">{{ item.success }}</span>
                  </div>
                  <div class="bar bar-failed" :style="{ height: barHeight(item.failed, maxTotal) + 'px' }">
                    <span class="bar-label" v-if="item.failed">{{ item.failed }}</span>
                  </div>
                </div>
              </el-tooltip>
              <div class="bar-date">{{ item.date.slice(5) }}</div>
            </div>
            <div class="chart-legend">
              <span><span class="legend-dot" style="background:#67c23a"></span>成功</span>
              <span><span class="legend-dot" style="background:#f56c6c"></span>失败</span>
            </div>
          </div>
          <el-empty v-else description="暂无数据" :image-size="60" />
        </el-card>
      </el-col>

      <!-- 右侧面板 -->
      <el-col :span="10">
        <!-- Top 流程 -->
        <el-card shadow="hover" style="margin-bottom:16px">
          <template #header><span style="font-weight:600">🔥 流程执行排行 Top5</span></template>
          <div v-for="(item, idx) in stats.topFlows" :key="item.flowKey" class="rank-item">
            <span class="rank-num" :class="Number(idx) < 3 ? 'rank-top' : ''">{{ Number(idx) + 1 }}</span>
            <span class="rank-name" :title="item.flowName">{{ item.flowName || item.flowKey }}</span>
            <span class="rank-count">{{ item.count }}次</span>
          </div>
          <el-empty v-if="!stats.topFlows?.length" description="暂无数据" :image-size="40" />
        </el-card>

        <!-- 最近失败 -->
        <el-card shadow="hover">
          <template #header><span style="font-weight:600">⚠️ 最近失败</span></template>
          <div v-for="item in stats.recentFailed" :key="item.id" class="fail-item">
            <div class="fail-header">
              <span class="fail-name">{{ item.flowName || item.flowKey }}</span>
              <span class="fail-time">{{ formatTime(item.startTime) }}</span>
            </div>
            <div class="fail-error">{{ item.errorMessage || '未知错误' }}</div>
          </div>
          <el-empty v-if="!stats.recentFailed?.length" description="暂无失败记录 🎉" :image-size="40" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import request from '../../utils/request'

const stats = ref<any>({})

const maxTotal = computed(() => {
  if (!stats.value.weekTrend?.length) return 1
  return Math.max(...stats.value.weekTrend.map((i: any) => i.total), 1)
})

function barHeight(value: number, max: number) {
  return max > 0 ? Math.max(Number(value) / Number(max) * 120, value > 0 ? 16 : 0) : 0
}

function formatTime(t?: string) {
  if (!t) return '-'
  return t.substring(5, 16).replace('T', ' ')
}

onMounted(async () => {
  try {
    const res: any = await request.get('/flow/log/dashboard')
    stats.value = res.data ?? res ?? {}
  } catch {}
})
</script>

<style scoped>
.dashboard-page { padding: 20px; }
.stat-card { text-align: center; }
.stat-number { font-size: 32px; font-weight: 700; line-height: 1.2; }
.stat-label { font-size: 13px; color: #999; margin-top: 4px; }
.chart-area { display: flex; align-items: flex-end; gap: 12px; height: 160px; padding: 8px 0; }
.bar-group { flex: 1; display: flex; flex-direction: column; align-items: center; }
.bar-wrapper { display: flex; gap: 2px; align-items: flex-end; height: 130px; }
.bar { border-radius: 3px 3px 0 0; min-width: 14px; transition: height 0.3s; position: relative; display: flex; flex-direction: column; justify-content: flex-start; }
.bar-success { background: #67c23a; }
.bar-failed { background: #f56c6c; }
.bar-label { font-size: 10px; color: #fff; text-align: center; padding-top: 2px; }
.bar-date { font-size: 11px; color: #999; margin-top: 4px; }
.chart-legend { display: flex; gap: 16px; justify-content: center; margin-top: 4px; font-size: 12px; color: #666; }
.legend-dot { display: inline-block; width: 10px; height: 10px; border-radius: 2px; margin-right: 4px; vertical-align: middle; }
.rank-item { display: flex; align-items: center; padding: 6px 0; border-bottom: 1px solid #f0f0f0; }
.rank-item:last-child { border-bottom: none; }
.rank-num { width: 24px; height: 24px; border-radius: 4px; background: #f0f0f0; color: #999; font-size: 12px; font-weight: 600; display: flex; align-items: center; justify-content: center; margin-right: 10px; flex-shrink: 0; }
.rank-top { background: #409eff; color: #fff; }
.rank-name { flex: 1; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.rank-count { font-size: 13px; color: #409eff; font-weight: 600; margin-left: 8px; }
.fail-item { padding: 8px 0; border-bottom: 1px solid #f5f5f5; }
.fail-item:last-child { border-bottom: none; }
.fail-header { display: flex; justify-content: space-between; }
.fail-name { font-size: 13px; font-weight: 500; }
.fail-time { font-size: 11px; color: #999; }
.fail-error { font-size: 12px; color: #f56c6c; margin-top: 2px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
</style>
