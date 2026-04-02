<template>
  <div class="page-container">
    <div class="page-header">
      <el-button icon="ArrowLeft" link @click="router.back()">返回</el-button>
      <div class="header-info">
        <h2>接口详情</h2>
        <el-tag :type="methodColor(apiInfo?.requestType)" size="small">{{ apiInfo?.requestType }}</el-tag>
        <span class="api-url">{{ apiInfo?.url }}</span>
      </div>
    </div>

    <el-card v-loading="loading">
      <el-descriptions title="基本信息" :column="2" border size="small" style="margin-bottom:24px">
        <el-descriptions-item label="接口名称">{{ apiInfo?.methodName }}</el-descriptions-item>
        <el-descriptions-item label="接口Code">{{ apiInfo?.methodCode }}</el-descriptions-item>
        <el-descriptions-item label="接口类型">
          <el-tag :type="apiInfo?.methodType === 'WEBSERVICE' ? 'warning' : 'info'" size="small">
            {{ apiInfo?.methodType === 'WEBSERVICE' ? 'WebService（SOAP）' : 'API 接口（HTTP）' }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="请求方式">
          <el-tag v-if="apiInfo?.methodType !== 'WEBSERVICE'" :type="methodColor(apiInfo?.requestType)" size="small">{{ apiInfo?.requestType }}</el-tag>
          <el-tag v-else type="warning" size="small">SOAP 1.1</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="内容类型">{{ apiInfo?.methodType === 'WEBSERVICE' ? 'text/xml' : apiInfo?.contentType }}</el-descriptions-item>
        <el-descriptions-item label="URL" :span="2">{{ apiInfo?.url }}</el-descriptions-item>
        <el-descriptions-item label="描述" :span="2">{{ apiInfo?.methodDesc || '-' }}</el-descriptions-item>
      </el-descriptions>

      <!-- 入参管理 -->
      <div class="param-section">
        <div class="section-header">
          <span class="section-title">
            <el-icon color="#1890ff"><Upload /></el-icon> 入参配置
          </span>
          <el-button size="small" type="primary" icon="Plus" @click="addParam('input')">添加入参</el-button>
        </div>
        <el-table :data="inputParams" border size="small" empty-text="暂无入参">
          <el-table-column type="index" width="50" label="#" />
          <el-table-column label="参数Code" width="160">
            <template #default="{ row }">
              <el-input v-model="row.paramCode" size="small" placeholder="如: city" />
            </template>
          </el-table-column>
          <el-table-column label="参数名" width="130">
            <template #default="{ row }">
              <el-input v-model="row.paramName" size="small" placeholder="如: 城市名" />
            </template>
          </el-table-column>
          <el-table-column label="数据类型" width="110">
            <template #default="{ row }">
              <el-select v-model="row.dataType" size="small" style="width:100%">
                <el-option value="string" label="string" />
                <el-option value="integer" label="integer" />
                <el-option value="double" label="double" />
                <el-option value="boolean" label="boolean" />
                <el-option value="date" label="date（日期）" />
                <el-option value="json" label="json（JSON对象）" />
                <el-option value="object" label="object（对象类型）" />
                <el-option value="array" label="array（对象数组）" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="必填" width="70" align="center">
            <template #default="{ row }">
              <el-checkbox v-model="row.required" :true-value="1" :false-value="0" />
            </template>
          </el-table-column>
          <el-table-column label="默认值" width="130">
            <template #default="{ row }">
              <el-input v-model="row.defaultValue" size="small" placeholder="可选" />
            </template>
          </el-table-column>
          <el-table-column label="描述">
            <template #default="{ row }">
              <el-input v-model="row.description" size="small" placeholder="可选" />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="70" align="center">
            <template #default="{ $index }">
              <el-button size="small" type="danger" link @click="inputParams.splice($index, 1)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div style="margin-top:8px;text-align:right">
          <el-button type="primary" size="small" @click="saveParams('input')">保存入参</el-button>
        </div>
      </div>

      <!-- 出参管理 -->
      <div class="param-section" style="margin-top:24px">
        <div class="section-header">
          <span class="section-title">
            <el-icon color="#52c41a"><Download /></el-icon> 出参配置
          </span>
          <el-button size="small" type="success" icon="Plus" @click="addParam('output')">添加出参</el-button>
        </div>
        <el-table :data="outputParams" border size="small" empty-text="暂无出参">
          <el-table-column type="index" width="50" label="#" />
          <el-table-column label="参数Code" width="160">
            <template #default="{ row }">
              <el-input v-model="row.paramCode" size="small" placeholder="如: temperature" />
            </template>
          </el-table-column>
          <el-table-column label="参数名" width="130">
            <template #default="{ row }">
              <el-input v-model="row.paramName" size="small" placeholder="如: 温度" />
            </template>
          </el-table-column>
          <el-table-column label="数据类型" width="110">
            <template #default="{ row }">
              <el-select v-model="row.dataType" size="small" style="width:100%">
                <el-option value="string" label="string" />
                <el-option value="integer" label="integer" />
                <el-option value="double" label="double" />
                <el-option value="boolean" label="boolean" />
                <el-option value="date" label="date（日期）" />
                <el-option value="json" label="json（JSON对象）" />
                <el-option value="object" label="object（对象类型）" />
                <el-option value="array" label="array（对象数组）" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column label="JSON路径" width="160">
            <template #default="{ row }">
              <el-input v-model="row.objectCode" size="small" placeholder="如: data.temp" />
            </template>
          </el-table-column>
          <el-table-column label="描述">
            <template #default="{ row }">
              <el-input v-model="row.description" size="small" placeholder="可选" />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="70" align="center">
            <template #default="{ $index }">
              <el-button size="small" type="danger" link @click="outputParams.splice($index, 1)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div style="margin-top:8px;text-align:right">
          <el-button type="success" size="small" @click="saveParams('output')">保存出参</el-button>
        </div>
      </div>

      <!-- HTTP 头部参数 -->
      <div class="param-section" style="margin-top:24px">
        <div class="section-header">
          <span class="section-title">
            <el-icon color="#fa8c16"><Key /></el-icon> Header 配置（可选）
          </span>
          <el-button size="small" icon="Plus" @click="addParam('header')">添加 Header</el-button>
        </div>
        <el-table :data="headerParams" border size="small" empty-text="暂无 Header">
          <el-table-column type="index" width="50" label="#" />
          <el-table-column label="Header名" width="200">
            <template #default="{ row }">
              <el-input v-model="row.paramCode" size="small" placeholder="如: Authorization" />
            </template>
          </el-table-column>
          <el-table-column label="参数说明">
            <template #default="{ row }">
              <el-input v-model="row.paramName" size="small" placeholder="如: 认证Token" />
            </template>
          </el-table-column>
          <el-table-column label="必填" width="70" align="center">
            <template #default="{ row }">
              <el-checkbox v-model="row.required" :true-value="1" :false-value="0" />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="70" align="center">
            <template #default="{ $index }">
              <el-button size="small" type="danger" link @click="headerParams.splice($index, 1)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div style="margin-top:8px;text-align:right">
          <el-button size="small" @click="saveParams('header')">保存 Header</el-button>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '../../utils/request'

const route = useRoute()
const router = useRouter()
const suiteCode = route.params.suiteCode as string
const apiId = Number(route.params.apiId)

const loading = ref(false)
const apiInfo = ref<any>(null)
const inputParams = ref<any[]>([])
const outputParams = ref<any[]>([])
const headerParams = ref<any[]>([])

// paramType: 1=入参, 2=出参, 3=header
const PARAM_TYPE = { input: 1, output: 2, header: 3 }

onMounted(async () => {
  loading.value = true
  try {
    // 加载接口基本信息
    const apisRes: any = await request.post('/suite/api/list', { suiteCode })
    const apis = apisRes.data || []
    apiInfo.value = apis.find((a: any) => a.id === apiId) || null

    // 加载各类参数
    await Promise.all([
      loadParams('input'),
      loadParams('output'),
      loadParams('header')
    ])
  } finally {
    loading.value = false
  }
})

async function loadParams(type: 'input' | 'output' | 'header') {
  const paramType = PARAM_TYPE[type]
  const res: any = await request.get('/parameter/list', { params: { ownerId: apiId, paramType } })
  const list = (res.data || []).map((p: any) => ({ ...p }))
  if (type === 'input') inputParams.value = list
  else if (type === 'output') outputParams.value = list
  else headerParams.value = list
}

function addParam(type: 'input' | 'output' | 'header') {
  const param = {
    paramCode: '', paramName: '', dataType: 'string',
    objectCode: '', required: 0, defaultValue: '', description: ''
  }
  if (type === 'input') inputParams.value.push(param)
  else if (type === 'output') outputParams.value.push(param)
  else headerParams.value.push(param)
}

async function saveParams(type: 'input' | 'output' | 'header') {
  if (!apiInfo.value) return
  const paramType = PARAM_TYPE[type]
  const params = type === 'input' ? inputParams.value : type === 'output' ? outputParams.value : headerParams.value

  // 验证 paramCode 不为空
  const invalid = params.find(p => !p.paramCode?.trim())
  if (invalid) { ElMessage.warning('参数Code不能为空'); return }

  await request.post('/parameter/save', {
    ownerId: apiId,
    ownerCode: apiInfo.value.methodCode,
    paramType,
    parameters: params
  })
  ElMessage.success(`${type === 'input' ? '入参' : type === 'output' ? '出参' : 'Header'} 保存成功`)
  await loadParams(type)
}

function methodColor(type: string) {
  const map: Record<string, string> = { GET: 'success', POST: 'primary', PUT: 'warning', DELETE: 'danger' }
  return map[type] || 'info'
}
</script>

<style scoped>
.page-container { padding: 20px; }
.page-header {
  display: flex; align-items: center; gap: 12px;
  margin-bottom: 16px;
}
.header-info { display: flex; align-items: center; gap: 10px; flex: 1; }
.header-info h2 { font-size: 18px; color: #333; margin: 0; }
.api-url { font-size: 13px; color: #888; font-family: monospace; }

.param-section { }
.section-header {
  display: flex; justify-content: space-between; align-items: center;
  margin-bottom: 10px;
}
.section-title {
  font-size: 14px; font-weight: 600; color: #333;
  display: flex; align-items: center; gap: 6px;
}
</style>
