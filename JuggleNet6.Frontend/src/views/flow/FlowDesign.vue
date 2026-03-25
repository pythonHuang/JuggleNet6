<template>
  <div class="designer-container">
    <!-- 工具栏 -->
    <div class="toolbar">
      <div class="toolbar-left">
        <el-button icon="ArrowLeft" link @click="router.back()" style="color:#fff">返回</el-button>
        <span class="flow-title">{{ flowInfo?.flowName }} - 流程设计器</span>
      </div>
      <div class="toolbar-center">
        <el-button size="small" @click="addNode('START')" :disabled="hasStart" class="tb-btn-start">
          ▶ 开始
        </el-button>
        <el-button size="small" @click="addNode('METHOD')" class="tb-btn-method">
          ⚙ 方法
        </el-button>
        <el-button size="small" @click="addNode('ASSIGN')" class="tb-btn-assign">
          ← 赋值
        </el-button>
        <el-button size="small" @click="addNode('CODE')" class="tb-btn-code">
          { } 代码
        </el-button>
        <el-button size="small" @click="addNode('MYSQL')" class="tb-btn-mysql">
          ⊕ 数据库
        </el-button>
        <el-button size="small" @click="addNode('CONDITION')" class="tb-btn-condition">
          ◆ 条件
        </el-button>
        <el-button size="small" @click="addNode('END')" :disabled="hasEnd" class="tb-btn-end">
          ⏹ 结束
        </el-button>
      </div>
      <div class="toolbar-right">
        <el-button size="small" @click="variableDrawer = true" icon="Setting">变量</el-button>
        <el-button size="small" type="warning" @click="openDebug">调试</el-button>
        <el-button size="small" type="success" @click="saveFlow">保存</el-button>
        <el-button size="small" type="primary" @click="deployFlow">部署</el-button>
      </div>
    </div>

    <div class="designer-body">
      <!-- 左侧节点列表 -->
      <div class="left-panel">
        <div class="panel-title">节点列表（{{ nodes.length }}）</div>
        <div v-for="node in nodes" :key="node.key"
          class="node-item" :class="selectedNodeKey === node.key ? 'node-selected' : ''"
          @click="selectNode(node)">
          <span class="node-icon-badge" :class="'badge-' + node.elementType.toLowerCase()">
            {{ nodeIcon(node.elementType) }}
          </span>
          <div class="node-info">
            <div class="node-type-label">{{ nodeTypeName(node.elementType) }}</div>
            <div class="node-key-label">{{ node.key }}</div>
          </div>
          <el-button size="small" type="danger" circle icon="Delete"
            @click.stop="removeNode(node.key)" style="margin-left:auto;flex-shrink:0" />
        </div>
        <el-empty v-if="nodes.length === 0" description="请从工具栏添加节点" :image-size="60" />
      </div>

      <!-- 中间流程图 -->
      <div class="canvas-area">
        <div class="flow-hint" v-if="nodes.length === 0">
          <div style="font-size:48px;color:#ddd">⬡</div>
          <p>从工具栏添加节点，构建您的流程</p>
        </div>
        <div class="flow-chart" v-else>
          <div v-for="(node, idx) in nodes" :key="node.key" class="flow-node-wrap">
            <div class="flow-node" :class="['fn-' + node.elementType.toLowerCase(), selectedNodeKey === node.key ? 'fn-selected' : '']"
              @click="selectNode(node)">
              <div class="fn-icon">{{ nodeIcon(node.elementType) }}</div>
              <div class="fn-name">{{ getNodeLabel(node) }}</div>
              <div class="fn-type">{{ nodeTypeName(node.elementType) }}</div>
            </div>
            <div class="fn-arrow" v-if="idx < nodes.length - 1">↓</div>
          </div>
        </div>
      </div>

      <!-- 右侧属性面板 -->
      <div class="right-panel">
        <div class="panel-title" v-if="selectedNode">
          <span :class="'type-dot-' + selectedNode.elementType.toLowerCase()">●</span>
          {{ nodeTypeName(selectedNode.elementType) }} 属性
        </div>
        <div class="panel-title" v-else>节点属性</div>

        <div class="prop-content" v-if="selectedNode">
          <div class="prop-item">
            <label>节点Key</label>
            <el-input :value="selectedNode.key" disabled size="small" />
          </div>

          <!-- START 节点 -->
          <template v-if="selectedNode.elementType === 'START'">
            <div class="prop-tip">开始节点是流程入口，无需额外配置。</div>
          </template>

          <!-- END 节点 -->
          <template v-if="selectedNode.elementType === 'END'">
            <div class="prop-tip">结束节点是流程出口，无需额外配置。</div>
          </template>

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
            <div class="prop-section-title">输入填充规则</div>
            <div v-for="(rule, i) in selectedNode.method?.inputFillRules" :key="i" class="fill-rule">
              <el-select v-model="rule.source" placeholder="来源变量" size="small" style="width:44%">
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
              <span class="arrow-icon">→</span>
              <el-input v-model="rule.target" placeholder="API入参名" size="small" style="width:44%" />
              <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.inputFillRules.splice(i, 1)" />
            </div>
            <el-button size="small" icon="Plus" @click="addFillRule('input')" style="margin-bottom:8px">添加输入规则</el-button>
            <div class="prop-section-title">输出填充规则</div>
            <div v-for="(rule, i) in selectedNode.method?.outputFillRules" :key="i" class="fill-rule">
              <el-input v-model="rule.source" placeholder="API出参path" size="small" style="width:44%" />
              <span class="arrow-icon">→</span>
              <el-select v-model="rule.target" placeholder="目标变量" size="small" style="width:44%">
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
              <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.outputFillRules.splice(i, 1)" />
            </div>
            <el-button size="small" icon="Plus" @click="addFillRule('output')">添加输出规则</el-button>
          </template>

          <!-- ASSIGN 节点属性 -->
          <template v-if="selectedNode.elementType === 'ASSIGN'">
            <div class="prop-tip">赋值节点：将常量或变量赋值给目标变量。</div>
            <div class="prop-section-title">赋值规则</div>
            <div v-for="(rule, i) in selectedNode.assignRules" :key="i" class="assign-rule">
              <div class="assign-row">
                <el-select v-model="rule.sourceType" size="small" style="width:90px;flex-shrink:0">
                  <el-option value="CONSTANT" label="常量" />
                  <el-option value="VARIABLE" label="变量" />
                </el-select>
                <el-input v-if="rule.sourceType === 'CONSTANT'" v-model="rule.source"
                  placeholder="常量值" size="small" style="flex:1" />
                <el-select v-else v-model="rule.source" placeholder="来源变量" size="small" style="flex:1">
                  <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
                </el-select>
              </div>
              <div class="assign-row" style="margin-top:4px">
                <span style="font-size:12px;color:#666;width:90px;flex-shrink:0">→ 赋值给</span>
                <el-select v-model="rule.target" placeholder="目标变量" size="small" style="flex:1">
                  <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
                </el-select>
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.assignRules!.splice(i, 1)" />
              </div>
            </div>
            <el-button size="small" icon="Plus" @click="addAssignRule">添加赋值规则</el-button>
          </template>

          <!-- CODE 节点属性 -->
          <template v-if="selectedNode.elementType === 'CODE'">
            <div class="prop-tip">
              代码节点：编写 JavaScript 脚本操作变量。<br>
              读取变量：<code>$var.getVariableValue('key')</code><br>
              写入变量：<code>$var.setVariableValue('key', val)</code>
            </div>
            <div class="prop-item">
              <label>脚本语言</label>
              <el-select v-model="selectedNode.codeConfig.scriptType" size="small" style="width:120px">
                <el-option value="javascript" label="JavaScript" />
              </el-select>
            </div>
            <div class="prop-item">
              <label>脚本内容</label>
              <el-input v-model="selectedNode.codeConfig.script" type="textarea" :rows="10"
                placeholder="// 示例：&#10;var name = $var.getVariableValue('input_name')&#10;$var.setVariableValue('output_result', 'Hello, ' + name)"
                class="code-editor" />
            </div>
          </template>

          <!-- MYSQL 节点属性 -->
          <template v-if="selectedNode.elementType === 'MYSQL'">
            <div class="prop-tip">数据库节点：执行 SQL，支持 <code>${varName}</code> 模板变量。</div>
            <div class="prop-item">
              <label>数据源</label>
              <el-select v-model="selectedNode.mysqlConfig.dataSourceName" placeholder="选择数据源" size="small" style="width:100%">
                <el-option v-for="ds in dataSources" :key="ds.dataSourceName" :value="ds.dataSourceName" :label="ds.dataSourceName" />
              </el-select>
            </div>
            <div class="prop-item">
              <label>操作类型</label>
              <el-radio-group v-model="selectedNode.mysqlConfig.operationType" size="small">
                <el-radio-button value="QUERY">查询</el-radio-button>
                <el-radio-button value="UPDATE">更改</el-radio-button>
              </el-radio-group>
            </div>
            <div class="prop-item">
              <label>SQL 语句</label>
              <el-input v-model="selectedNode.mysqlConfig.sql" type="textarea" :rows="5"
                placeholder="SELECT * FROM table WHERE id = ${input_id}" class="code-editor" />
            </div>
            <div class="prop-item" v-if="selectedNode.mysqlConfig.operationType === 'QUERY'">
              <label>查询结果写入变量</label>
              <el-select v-model="selectedNode.mysqlConfig.outputVariable" placeholder="选择目标变量" size="small" style="width:100%" clearable>
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
            </div>
          </template>

          <!-- CONDITION 节点属性 -->
          <template v-if="selectedNode.elementType === 'CONDITION'">
            <div class="prop-tip">条件节点：根据表达式选择分支。每个分支需指定下一节点。</div>
            <div class="prop-section-title">条件分支</div>
            <div v-for="(cond, i) in selectedNode.conditions" :key="i" class="condition-item">
              <div style="display:flex;gap:4px;align-items:center;margin-bottom:4px">
                <el-input v-model="cond.conditionName" placeholder="分支名称" size="small" style="flex:1" />
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.conditions!.splice(i, 1)" />
              </div>
              <el-select v-model="cond.conditionType" size="small" style="width:100%;margin-bottom:4px">
                <el-option value="CUSTOM" label="自定义条件" />
                <el-option value="DEFAULT" label="默认(else)" />
              </el-select>
              <el-input v-if="cond.conditionType === 'CUSTOM'"
                v-model="cond.expression" placeholder="如: env_isLogin == true" size="small" style="margin-bottom:4px" />
              <div style="display:flex;align-items:center;gap:4px">
                <span style="font-size:12px;color:#666;white-space:nowrap">跳转到:</span>
                <el-select v-model="cond.outgoing" placeholder="下一节点" size="small" style="flex:1">
                  <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                    :label="`${nodeTypeName(n.elementType)}: ${n.key}`" />
                </el-select>
              </div>
            </div>
            <el-button size="small" icon="Plus" @click="addCondition">添加分支</el-button>
          </template>

          <!-- 后续节点（非CONDITION、非END节点） -->
          <div class="prop-item" style="margin-top:16px"
            v-if="!['CONDITION','END'].includes(selectedNode.elementType)">
            <label>后续节点</label>
            <el-select v-model="selectedNode.outgoings[0]" placeholder="选择下一节点" size="small"
              style="width:100%" clearable @change="onOutgoingChange">
              <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                :label="`${nodeTypeName(n.elementType)}: ${n.key}`" />
            </el-select>
          </div>
        </div>
        <el-empty v-else description="点击节点查看/编辑属性" :image-size="60" style="padding-top:40px" />
      </div>
    </div>

    <!-- 变量管理抽屉 -->
    <el-drawer v-model="variableDrawer" title="流程变量管理" size="520px" direction="rtl">
      <div style="padding:0 4px">
        <div style="margin-bottom:12px;display:flex;justify-content:space-between;align-items:center">
          <span style="color:#666;font-size:13px">定义流程的输入/输出/中间变量，在节点填充规则中引用。</span>
          <el-button size="small" type="primary" icon="Plus" @click="addVariable">添加变量</el-button>
        </div>
        <el-table :data="allVariables" border size="small">
          <el-table-column prop="variableCode" label="变量Code" width="140" />
          <el-table-column prop="variableName" label="变量名" width="110" />
          <el-table-column prop="variableType" label="类型" width="80">
            <template #default="{ row }">
              <el-tag size="small" :type="varTypeColor(row.variableType)">{{ varTypeName(row.variableType) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="dataType" label="数据类型" width="80" />
          <el-table-column label="操作" width="60">
            <template #default="{ row, $index }">
              <el-button size="small" type="danger" link @click="allVariables.splice($index, 1)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div style="margin-top:16px;text-align:right">
          <el-button type="primary" @click="saveVariables">保存变量</el-button>
        </div>
      </div>

      <!-- 添加变量对话框 -->
      <el-dialog v-model="varDialogVisible" title="添加变量" width="400px" append-to-body>
        <el-form :model="varForm" label-width="80px" size="small">
          <el-form-item label="变量Code">
            <el-input v-model="varForm.variableCode" placeholder="如: input_city" />
          </el-form-item>
          <el-form-item label="变量名">
            <el-input v-model="varForm.variableName" placeholder="如: 城市名称" />
          </el-form-item>
          <el-form-item label="类型">
            <el-select v-model="varForm.variableType" style="width:100%">
              <el-option value="INPUT" label="输入参数" />
              <el-option value="OUTPUT" label="输出参数" />
              <el-option value="VARIABLE" label="中间变量" />
            </el-select>
          </el-form-item>
          <el-form-item label="数据类型">
            <el-select v-model="varForm.dataType" style="width:100%">
              <el-option value="string" label="string" />
              <el-option value="integer" label="integer" />
              <el-option value="double" label="double" />
              <el-option value="boolean" label="boolean" />
              <el-option value="object" label="object" />
              <el-option value="array" label="array" />
            </el-select>
          </el-form-item>
          <el-form-item label="默认值">
            <el-input v-model="varForm.defaultValue" placeholder="可选" />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="varDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="confirmAddVariable">确认</el-button>
        </template>
      </el-dialog>
    </el-drawer>

    <!-- 调试弹窗 -->
    <el-dialog v-model="debugVisible" title="🐛 流程调试" width="640px">
      <el-form label-width="100px">
        <el-form-item label="输入参数">
          <el-input v-model="debugParams" type="textarea" :rows="6"
            placeholder='{"input_city": "北京", "input_name": "张三"}' class="code-editor" />
        </el-form-item>
      </el-form>
      <div v-if="debugResult !== null" style="margin-top:16px">
        <el-divider />
        <div style="font-weight:bold;margin-bottom:8px;color:#333">
          <el-icon :color="debugResult.success ? '#52c41a' : '#ff4d4f'">
            <component :is="debugResult.success ? 'CircleCheck' : 'CircleClose'" />
          </el-icon>
          {{ debugResult.success ? '执行成功' : '执行失败' }}
        </div>
        <el-input v-model="debugResultStr" type="textarea" :rows="8" readonly class="code-editor" />
      </div>
      <template #footer>
        <el-button @click="debugVisible = false">关闭</el-button>
        <el-button type="primary" @click="runDebug" :loading="debugLoading">执行</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
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
const dataSources = ref<any[]>([])
const methodApiSelection = ref<any[]>([])

// 变量管理
const variableDrawer = ref(false)
const varDialogVisible = ref(false)
const varForm = ref({ variableCode: '', variableName: '', variableType: 'VARIABLE', dataType: 'string', defaultValue: '' })

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

// 监听选中节点变化，初始化 methodApiSelection
watch(selectedNode, (node) => {
  if (node?.elementType === 'METHOD' && node.method?.suiteCode && node.method?.methodCode) {
    methodApiSelection.value = [node.method.suiteCode, node.method.methodCode]
  } else {
    methodApiSelection.value = []
  }
})

onMounted(async () => {
  await Promise.all([loadFlowInfo(), loadSuiteApis(), loadDataSources()])
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
  } catch (e) {
    console.error('loadFlowInfo', e)
  }
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

async function loadDataSources() {
  try {
    const res: any = await request.get('/system/datasource/list')
    dataSources.value = res.data || []
  } catch {}
}

function nodeIcon(type: string) {
  const map: Record<string, string> = {
    START: '▶', END: '⏹', METHOD: '⚙', CONDITION: '◆',
    ASSIGN: '←', CODE: '{ }', MYSQL: '⊕'
  }
  return map[type] || '?'
}

function nodeTypeName(type: string) {
  const map: Record<string, string> = {
    START: '开始', END: '结束', METHOD: '方法', CONDITION: '条件',
    ASSIGN: '赋值', CODE: '代码', MYSQL: '数据库'
  }
  return map[type] || type
}

function getNodeLabel(node: any) {
  if (node.elementType === 'METHOD') return node.method?.methodCode || node.key
  if (node.elementType === 'MYSQL') return node.mysqlConfig?.dataSourceName || node.key
  return node.key
}

function addNode(type: string) {
  const key = `${type.toLowerCase()}_${Date.now()}`
  const node: any = { key, elementType: type, incomings: [], outgoings: [] }
  if (type === 'METHOD') {
    node.method = {
      suiteCode: '', methodCode: '', url: '', requestType: 'GET', contentType: 'JSON',
      inputFillRules: [], outputFillRules: [], headerFillRules: []
    }
  }
  if (type === 'CONDITION') {
    node.conditions = [{ conditionName: '默认分支', conditionType: 'DEFAULT', expression: '', outgoing: '' }]
  }
  if (type === 'ASSIGN') {
    node.assignRules = []
  }
  if (type === 'CODE') {
    node.codeConfig = { scriptType: 'javascript', script: '' }
  }
  if (type === 'MYSQL') {
    node.mysqlConfig = { dataSourceName: '', dataSourceType: 'sqlite', sql: '', operationType: 'QUERY', outputVariable: '' }
  }
  nodes.value.push(node)
  selectNode(node)
}

function removeNode(key: string) {
  nodes.value = nodes.value.filter(n => n.key !== key)
  if (selectedNodeKey.value === key) selectedNodeKey.value = null
  nodes.value.forEach(n => {
    n.outgoings = (n.outgoings || []).filter((k: string) => k !== key)
    if (n.conditions) {
      n.conditions.forEach((c: any) => { if (c.outgoing === key) c.outgoing = '' })
    }
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
  const [suiteCode, methodCode] = val
  const suiteOption = apiOptions.value.find(s => s.value === suiteCode)
  const apiOption = suiteOption?.children?.find((a: any) => a.value === methodCode)
  if (apiOption?.api) {
    const api = apiOption.api
    Object.assign(selectedNode.value.method, {
      suiteCode, methodCode: api.methodCode, url: api.url,
      requestType: api.requestType, contentType: api.contentType
    })
  }
}

function addFillRule(type: 'input' | 'output') {
  if (!selectedNode.value?.method) return
  const rule = {
    source: '', sourceType: type === 'input' ? 'VARIABLE' : 'OUTPUT_PARAM',
    target: '', targetType: type === 'input' ? 'INPUT_PARAM' : 'VARIABLE'
  }
  if (type === 'input') selectedNode.value.method.inputFillRules.push(rule)
  else selectedNode.value.method.outputFillRules.push(rule)
}

function addAssignRule() {
  if (!selectedNode.value?.assignRules) return
  selectedNode.value.assignRules.push({ source: '', sourceType: 'CONSTANT', target: '', dataType: 'string' })
}

function addCondition() {
  if (!selectedNode.value?.conditions) return
  selectedNode.value.conditions.push({ conditionName: '新分支', conditionType: 'CUSTOM', expression: '', outgoing: '' })
}

// 变量管理
function addVariable() { varDialogVisible.value = true; varForm.value = { variableCode: '', variableName: '', variableType: 'VARIABLE', dataType: 'string', defaultValue: '' } }

function confirmAddVariable() {
  if (!varForm.value.variableCode) { ElMessage.warning('变量Code不能为空'); return }
  if (allVariables.value.some(v => v.variableCode === varForm.value.variableCode)) {
    ElMessage.warning('变量Code已存在'); return
  }
  allVariables.value.push({ ...varForm.value })
  varDialogVisible.value = false
}

async function saveVariables() {
  if (!flowInfo.value?.id) return
  await request.post('/flow/variable/save', {
    flowKey,
    flowDefinitionId: flowInfo.value.id,
    variables: allVariables.value
  })
  ElMessage.success('变量保存成功')
}

function varTypeName(type: string) {
  return { INPUT: '输入', OUTPUT: '输出', VARIABLE: '中间' }[type] || type
}

function varTypeColor(type: string) {
  return { INPUT: 'success', OUTPUT: 'warning', VARIABLE: 'info' }[type] || ''
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
  } catch (e: any) {
    debugResult.value = { success: false, errorMessage: e.message || '请求失败' }
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
  gap: 8px;
}

.toolbar-left { display: flex; align-items: center; gap: 12px; flex-shrink: 0; }
.toolbar-center { display: flex; gap: 6px; flex-wrap: nowrap; }
.toolbar-right { display: flex; gap: 8px; flex-shrink: 0; }
.flow-title { color: #fff; font-size: 14px; font-weight: 500; white-space: nowrap; }

/* 工具栏节点按钮颜色 */
.tb-btn-start { background: #52c41a !important; border-color: #52c41a !important; color: #fff !important; }
.tb-btn-end   { background: #ff4d4f !important; border-color: #ff4d4f !important; color: #fff !important; }
.tb-btn-method { background: #1890ff !important; border-color: #1890ff !important; color: #fff !important; }
.tb-btn-assign { background: #722ed1 !important; border-color: #722ed1 !important; color: #fff !important; }
.tb-btn-code   { background: #eb2f96 !important; border-color: #eb2f96 !important; color: #fff !important; }
.tb-btn-mysql  { background: #13c2c2 !important; border-color: #13c2c2 !important; color: #fff !important; }
.tb-btn-condition { background: #fa8c16 !important; border-color: #fa8c16 !important; color: #fff !important; }

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
.right-panel { border-right: none; border-left: 1px solid #eee; width: 300px; }

.panel-title {
  font-size: 13px;
  font-weight: 600;
  color: #333;
  padding: 12px 16px;
  border-bottom: 1px solid #eee;
  background: #f8f9fa;
  display: flex;
  align-items: center;
  gap: 6px;
}

/* 节点列表样式 */
.node-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  border-bottom: 1px solid #f0f0f0;
  transition: background 0.15s;
}
.node-item:hover { background: #f5f7ff; }
.node-item.node-selected { background: #e6f4ff; border-left: 3px solid #1890ff; }

.node-icon-badge {
  width: 30px; height: 30px;
  border-radius: 6px;
  display: flex; align-items: center; justify-content: center;
  font-size: 14px; color: #fff; flex-shrink: 0;
}
.badge-start     { background: #52c41a; }
.badge-end       { background: #ff4d4f; }
.badge-method    { background: #1890ff; }
.badge-assign    { background: #722ed1; }
.badge-code      { background: #eb2f96; font-size: 11px; }
.badge-mysql     { background: #13c2c2; }
.badge-condition { background: #fa8c16; }

.node-info { flex: 1; min-width: 0; }
.node-type-label { font-size: 12px; font-weight: 600; color: #333; }
.node-key-label  { font-size: 11px; color: #999; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* 中间画布 */
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
  display: flex; flex-direction: column; align-items: center;
  gap: 12px; margin-top: 80px; color: #999;
}
.flow-chart { display: flex; flex-direction: column; align-items: center; }
.flow-node-wrap { display: flex; flex-direction: column; align-items: center; }

.flow-node {
  width: 160px; padding: 12px 16px;
  border-radius: 8px; text-align: center; cursor: pointer;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  transition: all 0.2s;
  border: 2px solid transparent;
}
.flow-node:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.18); transform: translateY(-1px); }
.flow-node.fn-selected { border-color: #faad14 !important; box-shadow: 0 0 0 3px rgba(250,173,20,0.3); }

.fn-start     { background: #52c41a; color: #fff; }
.fn-end       { background: #ff4d4f; color: #fff; }
.fn-method    { background: #1890ff; color: #fff; }
.fn-assign    { background: #722ed1; color: #fff; }
.fn-code      { background: #eb2f96; color: #fff; }
.fn-mysql     { background: #13c2c2; color: #fff; }
.fn-condition { background: #fa8c16; color: #fff; }

.fn-icon  { font-size: 20px; margin-bottom: 4px; }
.fn-name  { font-size: 12px; font-weight: 500; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.fn-type  { font-size: 11px; opacity: 0.8; }
.fn-arrow { font-size: 20px; color: #aaa; margin: 4px 0; }

/* 右侧属性面板 */
.prop-content { padding: 12px; }
.prop-item { margin-bottom: 14px; }
.prop-item label { display: block; font-size: 12px; color: #555; margin-bottom: 4px; font-weight: 500; }
.prop-tip {
  font-size: 12px; color: #888; background: #f8f9fa;
  border-left: 3px solid #ddd; padding: 8px 10px;
  border-radius: 0 4px 4px 0; margin-bottom: 12px; line-height: 1.8;
}
.prop-tip code { background: #e8e8e8; padding: 1px 4px; border-radius: 3px; font-size: 11px; }
.prop-section-title {
  font-size: 12px; font-weight: 600; color: #555;
  margin: 8px 0 6px; padding-bottom: 4px; border-bottom: 1px solid #f0f0f0;
}

.fill-rule {
  display: flex; align-items: center; gap: 4px; margin-bottom: 6px;
}
.arrow-icon { color: #999; font-size: 16px; }

.assign-rule {
  border: 1px solid #f0f0f0; border-radius: 6px; padding: 8px;
  margin-bottom: 8px; background: #fafafa;
}
.assign-row { display: flex; gap: 4px; align-items: center; }

.condition-item {
  border: 1px solid #f0f0f0; border-radius: 6px;
  padding: 8px; margin-bottom: 8px; background: #fafafa;
}

.code-editor :deep(.el-textarea__inner) {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 12px;
  background: #1e1e1e;
  color: #d4d4d4;
  border-color: #444;
}

/* 节点类型颜色点 */
.type-dot-start     { color: #52c41a; }
.type-dot-end       { color: #ff4d4f; }
.type-dot-method    { color: #1890ff; }
.type-dot-assign    { color: #722ed1; }
.type-dot-code      { color: #eb2f96; }
.type-dot-mysql     { color: #13c2c2; }
.type-dot-condition { color: #fa8c16; }
</style>
