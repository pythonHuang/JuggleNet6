<template>
  <div class="designer-container">
    <!-- 工具栏 -->
    <div class="toolbar">
      <div class="toolbar-left">
        <el-button icon="ArrowLeft" link @click="router.back()" style="color:#fff">返回</el-button>
        <span class="flow-title">{{ flowInfo?.flowName }} - 流程设计器</span>
      </div>
      <div class="toolbar-center">
        <el-button size="small" @click="addNode('START')" :disabled="hasStart">
          <el-icon><VideoPlay /></el-icon> 开始节点
        </el-button>
        <el-button size="small" @click="addNode('METHOD')">
          <el-icon><Connection /></el-icon> 方法节点
        </el-button>
        <el-button size="small" @click="addNode('CONDITION')">
          <el-icon><Switch /></el-icon> 条件节点
        </el-button>
        <el-button size="small" @click="addNode('END')" :disabled="hasEnd">
          <el-icon><VideoCamera /></el-icon> 结束节点
        </el-button>
      </div>
      <div class="toolbar-right">
        <el-button size="small" type="warning" @click="openDebug">调试</el-button>
        <el-button size="small" type="success" @click="saveFlow">保存</el-button>
        <el-button size="small" type="primary" @click="deployFlow">部署</el-button>
      </div>
    </div>

    <div class="designer-body">
      <!-- 左侧面板 -->
      <div class="left-panel">
        <div class="panel-title">流程节点</div>
        <div v-for="node in nodes" :key="node.key"
          class="node-item" :class="['node-' + node.elementType.toLowerCase()]"
          :class2="selectedNodeKey === node.key ? 'node-selected' : ''"
          @click="selectNode(node)">
          <span class="node-icon">{{ nodeIcon(node.elementType) }}</span>
          <div class="node-info">
            <div class="node-type">{{ nodeTypeName(node.elementType) }}</div>
            <div class="node-key">{{ node.key }}</div>
          </div>
          <el-button size="small" type="danger" circle icon="Delete"
            @click.stop="removeNode(node.key)" style="margin-left:auto" />
        </div>
        <el-empty v-if="nodes.length === 0" description="暂无节点，请添加" :image-size="60" />
      </div>

      <!-- 中间流程图 -->
      <div class="canvas-area">
        <div class="flow-hint" v-if="nodes.length === 0">
          <el-icon size="48" color="#ccc"><Connection /></el-icon>
          <p>从工具栏添加节点，构建流程</p>
        </div>
        <div class="flow-chart" v-else>
          <div v-for="(node, idx) in nodes" :key="node.key" class="flow-node-wrap">
            <div class="flow-node" :class="['fn-' + node.elementType.toLowerCase()]"
              :class2="selectedNodeKey === node.key ? 'fn-selected' : ''"
              @click="selectNode(node)">
              <div class="fn-icon">{{ nodeIcon(node.elementType) }}</div>
              <div class="fn-name">{{ node.method?.methodCode || node.key }}</div>
              <div class="fn-type">{{ nodeTypeName(node.elementType) }}</div>
            </div>
            <div class="fn-arrow" v-if="idx < nodes.length - 1">↓</div>
          </div>
        </div>
      </div>

      <!-- 右侧属性面板 -->
      <div class="right-panel" v-if="selectedNode">
        <div class="panel-title">节点属性 - {{ nodeTypeName(selectedNode.elementType) }}</div>
        <div class="prop-content">
          <div class="prop-item">
            <label>节点Key</label>
            <el-input :value="selectedNode.key" disabled size="small" />
          </div>

          <!-- METHOD 节点属性 -->
          <template v-if="selectedNode.elementType === 'METHOD'">
            <div class="prop-item">
              <label>选择 API</label>
              <el-cascader v-model="methodApiSelection" :options="apiOptions"
                @change="onApiSelect" placeholder="选择套件/接口" size="small" style="width:100%" />
            </div>
            <div class="prop-item" v-if="selectedNode.method?.url">
              <label>URL</label>
              <el-input :value="selectedNode.method.url" disabled size="small" />
            </div>
            <div class="prop-item">
              <label>输入填充规则</label>
              <div v-for="(rule, i) in selectedNode.method?.inputFillRules" :key="i" class="fill-rule">
                <el-select v-model="rule.source" placeholder="来源变量" size="small" style="width:44%">
                  <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
                </el-select>
                <span style="margin:0 4px">→</span>
                <el-input v-model="rule.target" placeholder="API入参名" size="small" style="width:44%" />
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.inputFillRules.splice(i, 1)" />
              </div>
              <el-button size="small" icon="Plus" @click="addFillRule('input')">添加输入规则</el-button>
            </div>
            <div class="prop-item">
              <label>输出填充规则</label>
              <div v-for="(rule, i) in selectedNode.method?.outputFillRules" :key="i" class="fill-rule">
                <el-input v-model="rule.source" placeholder="API出参path" size="small" style="width:44%" />
                <span style="margin:0 4px">→</span>
                <el-select v-model="rule.target" placeholder="目标变量" size="small" style="width:44%">
                  <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
                </el-select>
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.outputFillRules.splice(i, 1)" />
              </div>
              <el-button size="small" icon="Plus" @click="addFillRule('output')">添加输出规则</el-button>
            </div>
          </template>

          <!-- CONDITION 节点属性 -->
          <template v-if="selectedNode.elementType === 'CONDITION'">
            <div class="prop-item">
              <label>条件分支</label>
              <div v-for="(cond, i) in selectedNode.conditions" :key="i" class="condition-item">
                <el-input v-model="cond.conditionName" placeholder="分支名称" size="small" style="width:100%;margin-bottom:4px" />
                <el-select v-model="cond.conditionType" size="small" style="width:100px;margin-right:4px">
                  <el-option value="CUSTOM" label="自定义" />
                  <el-option value="DEFAULT" label="默认(else)" />
                </el-select>
                <el-input v-if="cond.conditionType === 'CUSTOM'"
                  v-model="cond.expression" placeholder="如: env_isLogin == true" size="small" style="width:calc(100% - 110px)" />
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.conditions!.splice(i, 1)" style="margin-top:4px" />
              </div>
              <el-button size="small" icon="Plus" @click="addCondition">添加分支</el-button>
            </div>
          </template>

          <div class="prop-item" style="margin-top:16px">
            <label>后续节点</label>
            <el-select v-model="selectedNode.outgoings[0]" placeholder="选择下一节点" size="small" style="width:100%"
              clearable @change="onOutgoingChange">
              <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                :label="`${nodeTypeName(n.elementType)}: ${n.key}`" />
            </el-select>
          </div>
        </div>
      </div>
      <div class="right-panel" v-else>
        <div class="panel-title">节点属性</div>
        <el-empty description="点击节点查看属性" :image-size="60" />
      </div>
    </div>

    <!-- 调试弹窗 -->
    <el-dialog v-model="debugVisible" title="流程调试" width="600px">
      <el-form label-width="100px">
        <el-form-item label="输入参数(JSON)">
          <el-input v-model="debugParams" type="textarea" :rows="6" placeholder='{"input_xxx": "value"}' />
        </el-form-item>
      </el-form>
      <div v-if="debugResult !== null" style="margin-top:16px">
        <div style="font-weight:bold;margin-bottom:8px">执行结果:</div>
        <el-input v-model="debugResultStr" type="textarea" :rows="6" readonly />
      </div>
      <template #footer>
        <el-button @click="debugVisible = false">关闭</el-button>
        <el-button type="primary" @click="runDebug" :loading="debugLoading">执行</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '../../utils/request'

const route = useRoute()
const router = useRouter()
const flowKey = route.params.flowKey as string

const flowInfo = ref<any>(null)
const nodes = ref<any[]>([])
const selectedNodeKey = ref<string | null>(null)
const allVariables = ref<any[]>([])
const apiOptions = ref<any[]>([])
const methodApiSelection = ref<any[]>([])

// 调试
const debugVisible = ref(false)
const debugLoading = ref(false)
const debugParams = ref('{}')
const debugResult = ref<any>(null)
const debugResultStr = computed(() => debugResult.value ? JSON.stringify(debugResult.value, null, 2) : '')

const selectedNode = computed(() => nodes.value.find(n => n.key === selectedNodeKey.value) || null)
const hasStart = computed(() => nodes.value.some(n => n.elementType === 'START'))
const hasEnd = computed(() => nodes.value.some(n => n.elementType === 'END'))
const otherNodes = computed(() => nodes.value.filter(n => n.key !== selectedNodeKey.value))

onMounted(async () => {
  await loadFlowInfo()
  await loadSuiteApis()
})

async function loadFlowInfo() {
  try {
    const res: any = await request.get(`/flow/definition/infoByKey/${flowKey}`)
    const def = res.data?.definition || res.data
    flowInfo.value = def
    if (def?.flowContent && def.flowContent !== '[]') {
      try { nodes.value = JSON.parse(def.flowContent) } catch { nodes.value = [] }
    }
    allVariables.value = res.data?.variables || []
  } catch {}
}

async function loadSuiteApis() {
  try {
    const suitesRes: any = await request.get('/suite/list')
    const suites = suitesRes.data || []
    const options: any[] = []
    for (const suite of suites) {
      const apisRes: any = await request.post('/suite/api/list', { suiteCode: suite.suiteCode })
      const apis = apisRes.data || []
      if (apis.length > 0) {
        options.push({
          value: suite.suiteCode,
          label: suite.suiteName,
          children: apis.map((a: any) => ({
            value: a.methodCode,
            label: a.methodName,
            api: a
          }))
        })
      }
    }
    apiOptions.value = options
  } catch {}
}

function nodeIcon(type: string) {
  const map: Record<string, string> = { START: '▶', END: '⏹', METHOD: '⚙', CONDITION: '◆' }
  return map[type] || '?'
}

function nodeTypeName(type: string) {
  const map: Record<string, string> = { START: '开始', END: '结束', METHOD: '方法', CONDITION: '条件' }
  return map[type] || type
}

function addNode(type: string) {
  const key = `${type.toLowerCase()}_${Date.now()}`
  const node: any = { key, elementType: type, incomings: [], outgoings: [] }
  if (type === 'METHOD') {
    node.method = { suiteCode: '', methodCode: '', url: '', requestType: 'GET', contentType: 'JSON', inputFillRules: [], outputFillRules: [], headerFillRules: [] }
  }
  if (type === 'CONDITION') {
    node.conditions = [{ conditionName: '默认分支', conditionType: 'DEFAULT', outgoing: '' }]
  }
  nodes.value.push(node)
}

function removeNode(key: string) {
  nodes.value = nodes.value.filter(n => n.key !== key)
  if (selectedNodeKey.value === key) selectedNodeKey.value = null
  // 清理其他节点对此节点的引用
  nodes.value.forEach(n => {
    n.outgoings = n.outgoings.filter((k: string) => k !== key)
  })
}

function selectNode(node: any) {
  selectedNodeKey.value = node.key
}

function onOutgoingChange(val: string) {
  if (selectedNode.value) {
    selectedNode.value.outgoings = val ? [val] : []
  }
}

function onApiSelect(val: any[]) {
  if (!selectedNode.value || selectedNode.value.elementType !== 'METHOD') return
  // val = [suiteCode, methodCode]
  const suiteCode = val[0], methodCode = val[1]
  const suiteOption = apiOptions.value.find(s => s.value === suiteCode)
  const apiOption = suiteOption?.children?.find((a: any) => a.value === methodCode)
  if (apiOption?.api) {
    const api = apiOption.api
    selectedNode.value.method.suiteCode = suiteCode
    selectedNode.value.method.methodCode = api.methodCode
    selectedNode.value.method.url = api.url
    selectedNode.value.method.requestType = api.requestType
    selectedNode.value.method.contentType = api.contentType
  }
}

function addFillRule(type: 'input' | 'output') {
  if (!selectedNode.value?.method) return
  const rule = { source: '', sourceType: type === 'input' ? 'VARIABLE' : 'OUTPUT_PARAM', target: '', targetType: type === 'input' ? 'INPUT_PARAM' : 'VARIABLE' }
  if (type === 'input') selectedNode.value.method.inputFillRules.push(rule)
  else selectedNode.value.method.outputFillRules.push(rule)
}

function addCondition() {
  if (!selectedNode.value?.conditions) return
  selectedNode.value.conditions.push({ conditionName: '新分支', conditionType: 'CUSTOM', expression: '', outgoing: '' })
}

async function saveFlow() {
  if (!flowInfo.value?.id) return ElMessage.error('流程信息未加载')
  await request.put('/flow/definition/save', {
    id: flowInfo.value.id,
    flowContent: JSON.stringify(nodes.value)
  })
  ElMessage.success('保存成功')
}

async function deployFlow() {
  await saveFlow()
  await request.post('/flow/definition/deploy', { flowDefinitionId: flowInfo.value?.id })
  ElMessage.success('部署成功')
}

function openDebug() { debugVisible.value = true; debugResult.value = null }

async function runDebug() {
  debugLoading.value = true
  try {
    let params = {}
    try { params = JSON.parse(debugParams.value) } catch { ElMessage.error('参数JSON格式错误'); return }
    const res: any = await request.post(`/flow/definition/debug/${flowKey}`, { params })
    debugResult.value = res.data
    ElMessage.success('执行完成')
  } finally { debugLoading.value = false }
}
</script>

<style scoped>
.designer-container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #1a1a2e;
}

.toolbar {
  height: 52px;
  background: #001529;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  border-bottom: 1px solid #0a2540;
  flex-shrink: 0;
}

.toolbar-left { display: flex; align-items: center; gap: 12px; }
.toolbar-center { display: flex; gap: 8px; }
.toolbar-right { display: flex; gap: 8px; }
.flow-title { color: #fff; font-size: 14px; font-weight: 500; }

.designer-body {
  flex: 1;
  display: flex;
  overflow: hidden;
}

.left-panel, .right-panel {
  width: 260px;
  background: #fff;
  border-right: 1px solid #eee;
  overflow-y: auto;
  flex-shrink: 0;
}

.right-panel { border-right: none; border-left: 1px solid #eee; }

.panel-title {
  font-size: 13px;
  font-weight: 600;
  color: #333;
  padding: 12px 16px;
  border-bottom: 1px solid #eee;
  background: #f8f9fa;
}

.node-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  cursor: pointer;
  border-bottom: 1px solid #f0f0f0;
  transition: background 0.15s;
}

.node-item:hover { background: #f5f7ff; }

.node-icon {
  font-size: 18px;
  width: 28px;
  text-align: center;
}

.node-info { flex: 1; min-width: 0; }
.node-type { font-size: 12px; font-weight: 600; color: #333; }
.node-key { font-size: 11px; color: #999; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.canvas-area {
  flex: 1;
  overflow-y: auto;
  background: #f5f7fa;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 32px;
}

.flow-hint {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  margin-top: 80px;
  color: #999;
}

.flow-chart {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0;
}

.flow-node-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.flow-node {
  width: 160px;
  padding: 12px 16px;
  border-radius: 8px;
  text-align: center;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  transition: box-shadow 0.2s;
}

.flow-node:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.15); }

.fn-start  { background: #52c41a; color: #fff; }
.fn-end    { background: #ff4d4f; color: #fff; }
.fn-method { background: #1890ff; color: #fff; }
.fn-condition { background: #fa8c16; color: #fff; }

.fn-icon   { font-size: 20px; margin-bottom: 4px; }
.fn-name   { font-size: 12px; font-weight: 500; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.fn-type   { font-size: 11px; opacity: 0.8; }

.fn-arrow  { font-size: 20px; color: #aaa; margin: 4px 0; }

.prop-content { padding: 12px; }
.prop-item { margin-bottom: 16px; }
.prop-item label { display: block; font-size: 12px; color: #666; margin-bottom: 4px; font-weight: 500; }

.fill-rule {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-bottom: 6px;
}

.condition-item {
  border: 1px solid #eee;
  border-radius: 4px;
  padding: 8px;
  margin-bottom: 8px;
}
</style>
