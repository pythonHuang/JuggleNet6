<template>
  <div class="flow-log-page">
    <!-- 搜索栏 -->
    <el-card shadow="never" class="search-card">
      <el-form :model="searchForm" inline>
        <el-form-item label="流程Key">
          <el-input v-model="searchForm.flowKey" placeholder="输入流程Key" clearable style="width:200px" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="searchForm.status" placeholder="全部" clearable style="width:120px">
            <el-option label="成功" value="SUCCESS" />
            <el-option label="失败" value="FAILED" />
          </el-select>
        </el-form-item>
        <el-form-item label="日期范围">
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="~"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            style="width:240px"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadList(1)">
            <el-icon><Search /></el-icon> 查询
          </el-button>
          <el-button @click="resetSearch">重置</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 列表 -->
    <el-card shadow="never" class="table-card">
      <el-table :data="logList" stripe border size="small" v-loading="loading" height="100%">
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="flowName" label="流程名称" min-width="130" show-overflow-tooltip />
        <el-table-column prop="flowKey" label="流程Key" min-width="150" show-overflow-tooltip />
        <el-table-column prop="version" label="版本" width="80" />
        <el-table-column prop="triggerType" label="触发方式" width="90">
          <template #default="{ row }">
            <el-tag size="small" :type="row.triggerType === 'open' ? 'warning' : 'info'">
              {{ row.triggerType }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="80">
          <template #default="{ row }">
            <el-tag size="small" :type="row.status === 'SUCCESS' ? 'success' : 'danger'">
              {{ row.status === 'SUCCESS' ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="costMs" label="耗时(ms)" width="90" align="right" />
        <el-table-column prop="startTime" label="执行时间" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">{{ formatTime(row.startTime) }}</template>
        </el-table-column>
        <el-table-column label="错误信息" min-width="150" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="error-msg" v-if="row.errorMessage">{{ row.errorMessage }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="130" fixed="right">
          <template #default="{ row }">
            <el-button size="small" link type="primary" @click="viewDetail(row)">详情</el-button>
            <el-button size="small" link type="danger" @click="deleteLog(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-bar">
        <el-pagination
          v-model:current-page="pageNum"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @current-change="loadList"
          @size-change="() => loadList(1)"
        />
      </div>
    </el-card>

    <!-- 执行详情抽屉 -->
    <el-drawer v-model="detailDrawer" title="流程执行详情" size="65%" direction="rtl" destroy-on-close>
      <template v-if="detailData">
        <!-- 主日志信息 -->
        <el-descriptions :column="2" border size="small" style="margin-bottom:16px">
          <el-descriptions-item label="流程名称">{{ detailData.log?.flowName }}</el-descriptions-item>
          <el-descriptions-item label="流程Key">{{ detailData.log?.flowKey }}</el-descriptions-item>
          <el-descriptions-item label="版本">{{ detailData.log?.version }}</el-descriptions-item>
          <el-descriptions-item label="触发方式">{{ detailData.log?.triggerType }}</el-descriptions-item>
          <el-descriptions-item label="执行状态">
            <el-tag :type="detailData.log?.status === 'SUCCESS' ? 'success' : 'danger'" size="small">
              {{ detailData.log?.status === 'SUCCESS' ? '成功' : '失败' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="耗时">{{ detailData.log?.costMs }} ms</el-descriptions-item>
          <el-descriptions-item label="开始时间">{{ formatTime(detailData.log?.startTime) }}</el-descriptions-item>
          <el-descriptions-item label="结束时间">{{ formatTime(detailData.log?.endTime) }}</el-descriptions-item>
          <el-descriptions-item label="错误信息" :span="2">
            <span class="error-msg">{{ detailData.log?.errorMessage || '-' }}</span>
          </el-descriptions-item>
        </el-descriptions>

        <!-- 输入输出 -->
        <el-tabs>
          <el-tab-pane label="输入参数">
            <pre class="json-pre">{{ formatJson(detailData.log?.inputJson) }}</pre>
          </el-tab-pane>
          <el-tab-pane label="输出结果">
            <pre class="json-pre">{{ formatJson(detailData.log?.outputJson) }}</pre>
          </el-tab-pane>
          <el-tab-pane :label="`节点明细 (${detailData.nodeLogs?.length || 0})`">
            <el-timeline style="margin-top:12px;padding-left:12px">
              <el-timeline-item
                v-for="node in detailData.nodeLogs"
                :key="node.id"
                :type="node.status === 'SUCCESS' ? 'success' : node.status === 'FAILED' ? 'danger' : 'primary'"
                :timestamp="`#${node.seqNo} · ${node.nodeType} · ${node.costMs}ms`"
              >
                <el-card shadow="never" class="node-log-card" :class="node.status === 'FAILED' ? 'node-failed' : ''">
                  <div class="node-log-header">
                    <span class="node-key">{{ node.nodeLabel || node.nodeKey }}</span>
                    <el-tag size="small" :type="node.status === 'SUCCESS' ? 'success' : 'danger'" style="margin-left:8px">
                      {{ node.status }}
                    </el-tag>
                  </div>
                  <div class="node-log-error" v-if="node.errorMessage">{{ node.errorMessage }}</div>
                  <div class="node-log-detail" v-if="node.detail">{{ node.detail }}</div>
                  <el-collapse accordion style="margin-top:8px" v-if="node.inputSnapshot || node.outputSnapshot">
                    <el-collapse-item title="变量快照">
                      <div style="display:flex;gap:12px">
                        <div style="flex:1">
                          <div class="snapshot-label">输入</div>
                          <pre class="json-pre-sm">{{ formatJson(node.inputSnapshot) }}</pre>
                        </div>
                        <div style="flex:1">
                          <div class="snapshot-label">输出</div>
                          <pre class="json-pre-sm">{{ formatJson(node.outputSnapshot) }}</pre>
                        </div>
                      </div>
                    </el-collapse-item>
                  </el-collapse>
                </el-card>
              </el-timeline-item>
            </el-timeline>
          </el-tab-pane>
        </el-tabs>
      </template>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Search } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import request from '../../utils/request'

const loading = ref(false)
const logList = ref<any[]>([])
const total = ref(0)
const pageNum = ref(1)
const pageSize = ref(20)
const dateRange = ref<string[]>([])
const searchForm = ref({ flowKey: '', status: '' })
const detailDrawer = ref(false)
const detailData = ref<any>(null)

const loadList = async (page?: number) => {
  if (page) pageNum.value = page
  loading.value = true
  try {
    const res: any = await request.post('/flow/log/page', {
      flowKey: searchForm.value.flowKey || undefined,
      status: searchForm.value.status || undefined,
      startDate: dateRange.value?.[0] || undefined,
      endDate: dateRange.value?.[1] || undefined,
      pageNum: pageNum.value,
      pageSize: pageSize.value
    })
    logList.value = res.data?.records ?? []
    total.value = res.data?.total ?? 0
  } finally {
    loading.value = false
  }
}

const resetSearch = () => {
  searchForm.value = { flowKey: '', status: '' }
  dateRange.value = []
  loadList(1)
}

const viewDetail = async (row: any) => {
  const res: any = await request.get(`/flow/log/detail/${row.id}`)
  detailData.value = res.data ?? res
  detailDrawer.value = true
}

const deleteLog = async (id: number) => {
  await ElMessageBox.confirm('确定删除该条日志吗?', '提示', { type: 'warning' })
  await request.delete(`/flow/log/${id}`)
  ElMessage.success('删除成功')
  loadList()
}

const formatTime = (t?: string) => {
  if (!t) return '-'
  return new Date(t).toLocaleString('zh-CN')
}

const formatJson = (str?: string) => {
  if (!str) return '-'
  try { return JSON.stringify(JSON.parse(str), null, 2) } catch { return str }
}

onMounted(() => loadList())
</script>

<style scoped>
/* 页面占满父容器高度，flex列布局，不产生页面级滚动条 */
.flow-log-page {
  padding: 16px;
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-sizing: border-box;
}
.search-card {
  flex-shrink: 0;
}
/* 表格卡片占满剩余高度 */
.table-card {
  margin-top: 12px;
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
/* el-card body 也需要 flex 伸缩 */
.table-card :deep(.el-card__body) {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding-bottom: 0;
}
/* 表格本身占满card body */
.table-card :deep(.el-table) {
  flex: 1;
  min-height: 0;
}
/* 分页固定在底部 */
.pagination-bar {
  flex-shrink: 0;
  padding: 10px 0 2px;
  display: flex;
  justify-content: flex-end;
}
.error-msg { color: var(--el-color-danger); font-size: 12px; }
.json-pre {
  background: #1e1e1e;
  color: #d4d4d4;
  padding: 10px;
  border-radius: 4px;
  font-size: 12px;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 300px;
  overflow-y: auto;
  margin: 0;
}
.json-pre-sm {
  background: #1e1e1e;
  color: #d4d4d4;
  padding: 8px;
  border-radius: 4px;
  font-size: 11px;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 200px;
  overflow-y: auto;
  margin: 0;
}
.node-log-card { font-size: 13px; }
.node-log-card.node-failed { border-color: var(--el-color-danger-light-5); }
.node-log-header { display: flex; align-items: center; }
.node-key { font-weight: 600; }
.node-log-error { color: var(--el-color-danger); font-size: 12px; margin-top: 4px; }
.node-log-detail { color: var(--el-color-info); font-size: 12px; margin-top: 4px; }
.snapshot-label { font-size: 11px; color: #999; margin-bottom: 4px; }
</style>
