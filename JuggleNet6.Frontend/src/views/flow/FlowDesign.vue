<template>
  <div class="designer-container">
    <!-- 顶部工具栏 -->
    <div class="toolbar">
      <div class="toolbar-left">
        <el-button icon="ArrowLeft" link @click="router.back()" style="color:#fff">返回</el-button>
        <span class="flow-title">{{ flowInfo?.flowName }} - 流程设计器</span>
      </div>
      <div class="toolbar-center">
        <el-button size="small" @click="addNode('START')" :disabled="hasStart" class="tb-btn-start">▶ 开始</el-button>
        <el-button size="small" @click="addNode('METHOD')" class="tb-btn-method">⚙ 方法</el-button>
        <el-button size="small" @click="addNode('ASSIGN')" class="tb-btn-assign">← 赋值</el-button>
        <el-button size="small" @click="addNode('CODE')" class="tb-btn-code">{ } 代码</el-button>
        <el-button size="small" @click="addNode('MYSQL')" class="tb-btn-mysql">⊕ 数据库</el-button>
        <el-button size="small" @click="addNode('CONDITION')" class="tb-btn-condition">◆ 条件</el-button>
        <el-button size="small" @click="addNode('MERGE')" class="tb-btn-merge">⇒ 聚合</el-button>
        <el-button size="small" @click="addNode('END')" :disabled="hasEnd" class="tb-btn-end">⏹ 结束</el-button>
      </div>
      <div class="toolbar-right">
        <el-button size="small" @click="paramDrawer = true" icon="Setting">流程参数</el-button>
        <el-button size="small" @click="variableDrawer = true" icon="List">变量</el-button>
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
            <div class="node-key-label">{{ node.label || node.key }}</div>
          </div>
          <el-button size="small" type="danger" circle icon="Delete"
            @click.stop="removeNode(node.key)" style="margin-left:auto;flex-shrink:0" />
        </div>
        <el-empty v-if="nodes.length === 0" description="从工具栏添加节点" :image-size="60" />
      </div>

      <!-- 中间流程图 -->
      <div class="canvas-area">
        <div class="flow-hint" v-if="nodes.length === 0">
          <div style="font-size:48px;color:#ddd">⬡</div>
          <p>从工具栏添加节点，构建您的流程</p>
        </div>
        <div class="flow-chart" v-else>
          <div v-for="(node, idx) in nodes" :key="node.key" class="flow-node-wrap">
            <!-- CONDITION 节点：展示分支 -->
            <template v-if="node.elementType === 'CONDITION'">
              <div class="flow-node" :class="['fn-condition', selectedNodeKey === node.key ? 'fn-selected' : '']"
                @click="selectNode(node)">
                <div class="fn-icon">◆</div>
                <div class="fn-name">{{ node.label || node.key }}</div>
                <div class="fn-type">条件</div>
              </div>
              <div class="condition-branches" v-if="node.conditions && node.conditions.length">
                <div v-for="cond in node.conditions" :key="cond.conditionName" class="branch-line">
                  <div class="branch-arrow">→</div>
                  <div class="branch-label" :class="cond.conditionType === 'DEFAULT' ? 'branch-default' : 'branch-custom'">
                    {{ cond.conditionName || (cond.conditionType === 'DEFAULT' ? 'else' : cond.expression) }}
                  </div>
                  <div class="branch-target">{{ cond.outgoing || '未设置' }}</div>
                </div>
              </div>
            </template>
            <!-- MERGE 节点 -->
            <template v-else-if="node.elementType === 'MERGE'">
              <div class="flow-node fn-merge" :class="selectedNodeKey === node.key ? 'fn-selected' : ''"
                @click="selectNode(node)">
                <div class="fn-icon">⇒</div>
                <div class="fn-name">{{ node.label || node.key }}</div>
                <div class="fn-type">聚合</div>
              </div>
            </template>
            <!-- 其他节点 -->
            <template v-else>
              <div class="flow-node" :class="['fn-' + node.elementType.toLowerCase(), selectedNodeKey === node.key ? 'fn-selected' : '']"
                @click="selectNode(node)">
                <div class="fn-icon">{{ nodeIcon(node.elementType) }}</div>
                <div class="fn-name">{{ getNodeLabel(node) }}</div>
                <div class="fn-type">{{ nodeTypeName(node.elementType) }}</div>
              </div>
            </template>
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
          <div class="prop-item">
            <label>节点标签</label>
            <el-input v-model="selectedNode.label" placeholder="可选显示名称" size="small" />
          </div>

          <!-- START 节点 -->
          <template v-if="selectedNode.elementType === 'START'">
            <div class="prop-tip">开始节点是流程入口。可在「流程参数」中设置入参。</div>
          </template>

          <!-- END 节点 -->
          <template v-if="selectedNode.elementType === 'END'">
            <div class="prop-tip">结束节点是流程出口。可在「流程参数」中设置出参。</div>
          </template>

          <!-- MERGE 节点 -->
          <template v-if="selectedNode.elementType === 'MERGE'">
            <div class="prop-tip">聚合节点：将多个 CONDITION 分支汇聚到一个执行路径。所有分支执行完后继续往后。</div>
            <div class="prop-item" style="margin-top:16px">
              <label>后续节点</label>
              <el-select v-model="selectedNode.outgoings[0]" placeholder="选择下一节点" size="small"
                style="width:100%" clearable @change="onOutgoingChange">
                <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                  :label="`${nodeTypeName(n.elementType)}: ${n.label || n.key}`" />
              </el-select>
            </div>
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

            <!-- Header 配置 -->
            <div class="prop-section-title">
              Header 参数
              <el-button size="small" icon="Plus" link @click="addHeaderRule" style="margin-left:auto">添加</el-button>
            </div>
            <div v-for="(rule, i) in selectedNode.method?.headerFillRules" :key="'h'+i" class="fill-rule-row">
              <el-input v-model="rule.target" placeholder="Header名" size="small" style="width:40%" />
              <span class="arrow-icon">←</span>
              <el-select v-model="rule.sourceType" size="small" style="width:70px;flex-shrink:0">
                <el-option value="VARIABLE" label="变量" />
                <el-option value="CONSTANT" label="常量" />
              </el-select>
              <el-input v-if="rule.sourceType==='CONSTANT'" v-model="rule.source" placeholder="值" size="small" style="flex:1" />
              <el-select v-else v-model="rule.source" placeholder="变量" size="small" style="flex:1">
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
              <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.headerFillRules.splice(i, 1)" />
            </div>

            <!-- 输入参数配置 -->
            <div class="prop-section-title">
              输入参数（Body/Query）
              <el-button size="small" icon="Plus" link @click="addFillRule('input')" style="margin-left:auto">添加</el-button>
            </div>
            <div v-for="(rule, i) in selectedNode.method?.inputFillRules" :key="'i'+i" class="fill-rule-row">
              <el-select v-model="rule.sourceType" size="small" style="width:70px;flex-shrink:0">
                <el-option value="VARIABLE" label="变量" />
                <el-option value="CONSTANT" label="常量" />
              </el-select>
              <el-input v-if="rule.sourceType==='CONSTANT'" v-model="rule.source" placeholder="常量值" size="small" style="flex:1" />
              <el-select v-else v-model="rule.source" placeholder="来源变量" size="small" style="flex:1">
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
              <span class="arrow-icon">→</span>
              <el-input v-model="rule.target" placeholder="API入参名" size="small" style="width:36%" />
              <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.inputFillRules.splice(i, 1)" />
            </div>

            <!-- 输出参数配置 -->
            <div class="prop-section-title">
              输出映射（Response→变量）
              <el-button size="small" icon="Plus" link @click="addFillRule('output')" style="margin-left:auto">添加</el-button>
            </div>
            <div v-for="(rule, i) in selectedNode.method?.outputFillRules" :key="'o'+i" class="fill-rule-row">
              <el-input v-model="rule.source" placeholder="响应字段path" size="small" style="flex:1" />
              <span class="arrow-icon">→</span>
              <el-select v-model="rule.target" placeholder="目标变量" size="small" style="width:46%">
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
              <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.method!.outputFillRules.splice(i, 1)" />
            </div>
          </template>

          <!-- ASSIGN 节点属性 -->
          <template v-if="selectedNode.elementType === 'ASSIGN'">
            <div class="prop-tip">赋值节点：将常量或变量赋值给目标变量。</div>
            <div class="prop-section-title">
              赋值规则
              <el-button size="small" icon="Plus" link @click="addAssignRule" style="margin-left:auto">添加</el-button>
            </div>
            <div v-for="(rule, i) in selectedNode.assignRules" :key="i" class="assign-rule">
              <div class="assign-row">
                <el-select v-model="rule.sourceType" size="small" style="width:72px;flex-shrink:0">
                  <el-option value="CONSTANT" label="常量" />
                  <el-option value="VARIABLE" label="变量" />
                </el-select>
                <el-input v-if="rule.sourceType === 'CONSTANT'" v-model="rule.source" placeholder="常量值" size="small" style="flex:1" />
                <el-select v-else v-model="rule.source" placeholder="来源变量" size="small" style="flex:1">
                  <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
                </el-select>
              </div>
              <div class="assign-row" style="margin-top:4px">
                <span style="font-size:12px;color:#666;width:72px;flex-shrink:0">→ 赋值给</span>
                <el-select v-model="rule.target" placeholder="目标变量" size="small" style="flex:1">
                  <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
                </el-select>
                <el-select v-model="rule.dataType" size="small" style="width:72px;flex-shrink:0">
                  <el-option value="string" label="string" />
                  <el-option value="integer" label="integer" />
                  <el-option value="double" label="double" />
                  <el-option value="boolean" label="bool" />
                </el-select>
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.assignRules!.splice(i, 1)" />
              </div>
            </div>
          </template>

          <!-- CODE 节点属性 -->
          <template v-if="selectedNode.elementType === 'CODE'">
            <div class="prop-tip">
              代码节点：编写 JavaScript 脚本操作变量。<br>
              读取：<code>$var.getVariableValue('key')</code><br>
              写入：<code>$var.setVariableValue('key', val)</code>
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

          <!-- MYSQL/DB 节点属性 -->
          <template v-if="selectedNode.elementType === 'MYSQL'">
            <div class="prop-tip">数据库节点：执行 SQL，支持 <code>${varName}</code> 模板变量。</div>
            <div class="prop-item">
              <label>数据源</label>
              <el-select v-model="selectedNode.mysqlConfig.dataSourceName" placeholder="选择数据源" size="small" style="width:100%">
                <el-option v-for="ds in dataSources" :key="ds.id" :value="ds.dataSourceName"
                  :label="`${ds.dataSourceName} (${ds.dataSourceType})`" />
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
            <div class="prop-item" v-else>
              <label>影响行数写入变量</label>
              <el-select v-model="selectedNode.mysqlConfig.affectedRowsVariable" placeholder="可选" size="small" style="width:100%" clearable>
                <el-option v-for="v in allVariables" :key="v.variableCode" :value="v.variableCode" :label="v.variableCode" />
              </el-select>
            </div>
          </template>

          <!-- CONDITION 节点属性 -->
          <template v-if="selectedNode.elementType === 'CONDITION'">
            <div class="prop-tip">条件节点：根据表达式选择分支，分支可汇聚到 MERGE 聚合节点。</div>
            <div class="prop-section-title">
              条件分支
              <el-button size="small" icon="Plus" link @click="addCondition" style="margin-left:auto">添加</el-button>
            </div>
            <div v-for="(cond, i) in selectedNode.conditions" :key="i" class="condition-item">
              <div style="display:flex;gap:4px;align-items:center;margin-bottom:4px">
                <el-input v-model="cond.conditionName" placeholder="分支名称" size="small" style="flex:1" />
                <el-select v-model="cond.conditionType" size="small" style="width:90px;flex-shrink:0">
                  <el-option value="CUSTOM" label="自定义" />
                  <el-option value="DEFAULT" label="默认(else)" />
                </el-select>
                <el-button size="small" icon="Delete" circle type="danger" @click="selectedNode.conditions!.splice(i, 1)" />
              </div>
              <el-input v-if="cond.conditionType === 'CUSTOM'"
                v-model="cond.expression"
                placeholder="如: score >= 60 或 status == 'active'"
                size="small" style="margin-bottom:4px" />
              <div style="display:flex;align-items:center;gap:4px">
                <span style="font-size:12px;color:#666;white-space:nowrap;flex-shrink:0">跳转→</span>
                <el-select v-model="cond.outgoing" placeholder="下一节点" size="small" style="flex:1">
                  <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                    :label="`${nodeTypeName(n.elementType)}: ${n.label || n.key}`" />
                </el-select>
              </div>
            </div>
          </template>

          <!-- 后续节点（非CONDITION、非END、非MERGE） -->
          <div class="prop-item" style="margin-top:16px"
            v-if="!['CONDITION','END','MERGE'].includes(selectedNode.elementType)">
            <label>后续节点</label>
            <el-select v-model="selectedNode.outgoings[0]" placeholder="选择下一节点" size="small"
              style="width:100%" clearable @change="onOutgoingChange">
              <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                :label="`${nodeTypeName(n.elementType)}: ${n.label || n.key}`" />
            </el-select>
          </div>
        </div>
        <el-empty v-else description="点击节点查看/编辑属性" :image-size="60" style="padding-top:40px" />
      </div>
    </div>

    <!-- ========== 流程参数抽屉（入参/出参） ========== -->
    <el-drawer v-model="paramDrawer" title="📋 流程参数配置" size="640px" direction="rtl">
      <el-tabs v-model="paramTab" style="padding:0 8px">
        <el-tab-pane label="入参（Input）" name="input">
          <div style="margin-bottom:8px;text-align:right">
            <el-button size="small" type="primary" icon="Plus" @click="addFlowParam('input')">添加入参</el-button>
          </div>
          <el-table :data="flowInputParams" border size="small" empty-text="暂无入参">
            <el-table-column type="index" width="42" label="#" />
            <el-table-column label="参数Code" width="140">
              <template #default="{ row }">
                <el-input v-model="row.paramCode" size="small" placeholder="input_xxx" />
              </template>
            </el-table-column>
            <el-table-column label="参数名" width="100">
              <template #default="{ row }">
                <el-input v-model="row.paramName" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="类型" width="95">
              <template #default="{ row }">
                <el-select v-model="row.dataType" size="small" style="width:100%">
                  <el-option v-for="t in dataTypes" :key="t" :value="t" :label="t" />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column label="必填" width="55" align="center">
              <template #default="{ row }">
                <el-checkbox v-model="row.required" :true-value="1" :false-value="0" />
              </template>
            </el-table-column>
            <el-table-column label="默认值" width="90">
              <template #default="{ row }">
                <el-input v-model="row.defaultValue" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="描述">
              <template #default="{ row }">
                <el-input v-model="row.description" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="" width="50" align="center">
              <template #default="{ $index }">
                <el-button size="small" type="danger" link @click="flowInputParams.splice($index,1)">删</el-button>
              </template>
            </el-table-column>
          </el-table>
          <div style="margin-top:12px;text-align:right">
            <el-button type="primary" @click="saveFlowParams('input')">保存入参</el-button>
          </div>
        </el-tab-pane>

        <el-tab-pane label="出参（Output）" name="output">
          <div style="margin-bottom:8px;text-align:right">
            <el-button size="small" type="primary" icon="Plus" @click="addFlowParam('output')">添加出参</el-button>
          </div>
          <el-table :data="flowOutputParams" border size="small" empty-text="暂无出参">
            <el-table-column type="index" width="42" label="#" />
            <el-table-column label="参数Code" width="140">
              <template #default="{ row }">
                <el-input v-model="row.paramCode" size="small" placeholder="output_xxx" />
              </template>
            </el-table-column>
            <el-table-column label="参数名" width="100">
              <template #default="{ row }">
                <el-input v-model="row.paramName" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="类型" width="95">
              <template #default="{ row }">
                <el-select v-model="row.dataType" size="small" style="width:100%">
                  <el-option v-for="t in dataTypes" :key="t" :value="t" :label="t" />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column label="描述">
              <template #default="{ row }">
                <el-input v-model="row.description" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="" width="50" align="center">
              <template #default="{ $index }">
                <el-button size="small" type="danger" link @click="flowOutputParams.splice($index,1)">删</el-button>
              </template>
            </el-table-column>
          </el-table>
          <div style="margin-top:12px;text-align:right">
            <el-button type="primary" @click="saveFlowParams('output')">保存出参</el-button>
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-drawer>

    <!-- ========== 变量管理抽屉 ========== -->
    <el-drawer v-model="variableDrawer" title="🔧 流程变量管理" size="520px" direction="rtl">
      <div style="padding:0 4px">
        <div style="margin-bottom:12px;display:flex;justify-content:space-between;align-items:center">
          <span style="color:#666;font-size:13px">定义流程的输入/输出/中间变量（运行时上下文），在节点填充规则中引用。</span>
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
          <el-table-column prop="defaultValue" label="默认值" />
          <el-table-column label="操作" width="60">
            <template #default="{ $index }">
              <el-button size="small" type="danger" link @click="allVariables.splice($index, 1)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <div style="margin-top:16px;text-align:right">
          <el-button type="primary" @click="saveVariables">保存变量</el-button>
        </div>
      </div>
      <!-- 添加变量对话框 -->
      <el-dialog v-model="varDialogVisible" title="添加变量" width="420px" append-to-body>
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
              <el-option v-for="t in dataTypes" :key="t" :value="t" :label="t" />
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

    <!-- ========== 调试弹窗 ========== -->
    <el-dialog v-model="debugVisible" title="🐛 流程调试" width="660px">
      <div style="margin-bottom:8px;color:#666;font-size:13px">
        已定义的入参：
        <el-tag v-for="p in flowInputParams" :key="p.paramCode" size="small" style="margin-right:4px">
          {{ p.paramCode }}({{ p.dataType }})
        </el-tag>
        <span v-if="!flowInputParams.length" style="color:#aaa">无</span>
      </div>
      <el-form label-width="100px">
        <el-form-item label="输入参数">
          <el-input v-model="debugParams" type="textarea" :rows="6"
            placeholder='{"input_city": "北京", "input_name": "张三"}' class="code-editor" />
        </el-form-item>
      </el-form>
      <div v-if="debugResult !== null" style="margin-top:12px">
        <el-divider />
        <div style="font-weight:bold;margin-bottom:8px;color:#333;display:flex;align-items:center;gap:6px">
          <span :style="{ color: debugResult.success ? '#52c41a' : '#ff4d4f' }">
            {{ debugResult.success ? '✓ 执行成功' : '✗ 执行失败' }}
          </span>
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

// 流程参数
const paramDrawer = ref(false)
const paramTab = ref('input')
const flowInputParams = ref<any[]>([])
const flowOutputParams = ref<any[]>([])

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

const dataTypes = ['string', 'integer', 'double', 'boolean', 'object', 'array']

const selectedNode = computed(() => nodes.value.find(n => n.key === selectedNodeKey.value) || null)
const hasStart = computed(() => nodes.value.some(n => n.elementType === 'START'))
const hasEnd = computed(() => nodes.value.some(n => n.elementType === 'END'))
const otherNodes = computed(() => nodes.value.filter(n => n.key !== selectedNodeKey.value))

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
    flowInputParams.value = res.data?.inputParams || []
    flowOutputParams.value = res.data?.outputParams || []
    // 同步调试参数默认值（用入参的 paramCode）
    if (flowInputParams.value.length > 0) {
      const defaultObj: Record<string, any> = {}
      for (const p of flowInputParams.value) {
        defaultObj[p.paramCode] = p.defaultValue || ''
      }
      debugParams.value = JSON.stringify(defaultObj, null, 2)
    }
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
    ASSIGN: '←', CODE: '{ }', MYSQL: '⊕', MERGE: '⇒'
  }
  return map[type] || '?'
}

function nodeTypeName(type: string) {
  const map: Record<string, string> = {
    START: '开始', END: '结束', METHOD: '方法', CONDITION: '条件',
    ASSIGN: '赋值', CODE: '代码', MYSQL: '数据库', MERGE: '聚合'
  }
  return map[type] || type
}

function getNodeLabel(node: any) {
  if (node.label) return node.label
  if (node.elementType === 'METHOD') return node.method?.methodCode || node.key
  if (node.elementType === 'MYSQL') return node.mysqlConfig?.dataSourceName || node.key
  return node.key
}

function addNode(type: string) {
  const key = `${type.toLowerCase()}_${Date.now()}`
  const node: any = { key, elementType: type, incomings: [], outgoings: [], label: '' }
  if (type === 'METHOD') {
    node.method = {
      suiteCode: '', methodCode: '', url: '', requestType: 'GET', contentType: 'JSON',
      inputFillRules: [], outputFillRules: [], headerFillRules: []
    }
  }
  if (type === 'CONDITION') {
    node.conditions = [
      { conditionName: '分支1', conditionType: 'CUSTOM', expression: '', outgoing: '' },
      { conditionName: '默认', conditionType: 'DEFAULT', expression: '', outgoing: '' }
    ]
  }
  if (type === 'ASSIGN') node.assignRules = []
  if (type === 'CODE') node.codeConfig = { scriptType: 'javascript', script: '' }
  if (type === 'MYSQL') node.mysqlConfig = { dataSourceName: '', dataSourceType: '', sql: '', operationType: 'QUERY', outputVariable: '', affectedRowsVariable: '' }
  nodes.value.push(node)
  selectNode(node)
}

function removeNode(key: string) {
  nodes.value = nodes.value.filter(n => n.key !== key)
  if (selectedNodeKey.value === key) selectedNodeKey.value = null
  nodes.value.forEach(n => {
    n.outgoings = (n.outgoings || []).filter((k: string) => k !== key)
    if (n.conditions) n.conditions.forEach((c: any) => { if (c.outgoing === key) c.outgoing = '' })
  })
}

function selectNode(node: any) { selectedNodeKey.value = node.key }

function onOutgoingChange(val: string) {
  if (selectedNode.value) selectedNode.value.outgoings = val ? [val] : []
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

function addHeaderRule() {
  if (!selectedNode.value?.method) return
  selectedNode.value.method.headerFillRules.push({ source: '', sourceType: 'CONSTANT', target: '', targetType: 'HEADER' })
}

function addAssignRule() {
  if (!selectedNode.value?.assignRules) return
  selectedNode.value.assignRules.push({ source: '', sourceType: 'CONSTANT', target: '', dataType: 'string' })
}

function addCondition() {
  if (!selectedNode.value?.conditions) return
  selectedNode.value.conditions.push({ conditionName: '新分支', conditionType: 'CUSTOM', expression: '', outgoing: '' })
}

// 流程参数
function addFlowParam(type: 'input' | 'output') {
  const prefix = type === 'input' ? 'input_' : 'output_'
  const param = { paramCode: prefix, paramName: '', dataType: 'string', required: type === 'input' ? 1 : 0, defaultValue: '', description: '', sortNum: 0 }
  if (type === 'input') flowInputParams.value.push(param)
  else flowOutputParams.value.push(param)
}

async function saveFlowParams(type: 'input' | 'output') {
  if (!flowInfo.value?.id) return
  const paramType = type === 'input' ? 5 : 6
  const params = type === 'input' ? flowInputParams.value : flowOutputParams.value
  const payload = {
    ownerId: flowInfo.value.id,
    ownerCode: flowKey,
    paramType,
    parameters: params.map((p: any, i: number) => ({ ...p, sortNum: i }))
  }
  await request.post('/parameter/save', payload)
  ElMessage.success(`${type === 'input' ? '入参' : '出参'}保存成功`)
  // 同步变量列表
  await loadFlowInfo()
}

// 变量管理
function addVariable() {
  varDialogVisible.value = true
  varForm.value = { variableCode: '', variableName: '', variableType: 'VARIABLE', dataType: 'string', defaultValue: '' }
}

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

function varTypeName(type: string) { return { INPUT: '输入', OUTPUT: '输出', VARIABLE: '中间' }[type] || type }
function varTypeColor(type: string) { return { INPUT: 'success', OUTPUT: 'warning', VARIABLE: 'info' }[type] || '' }

async function saveFlow() {
  if (!flowInfo.value?.id) return ElMessage.error('流程信息未加载')
  await request.put('/flow/definition/save', { id: flowInfo.value.id, flowContent: JSON.stringify(nodes.value) })
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
.toolbar-center { display: flex; gap: 5px; flex-wrap: nowrap; overflow-x: auto; }
.toolbar-right { display: flex; gap: 8px; flex-shrink: 0; }
.flow-title { color: #fff; font-size: 14px; font-weight: 500; white-space: nowrap; }

/* 工具栏按钮颜色 */
.tb-btn-start     { background: #52c41a !important; border-color: #52c41a !important; color: #fff !important; }
.tb-btn-end       { background: #ff4d4f !important; border-color: #ff4d4f !important; color: #fff !important; }
.tb-btn-method    { background: #1890ff !important; border-color: #1890ff !important; color: #fff !important; }
.tb-btn-assign    { background: #722ed1 !important; border-color: #722ed1 !important; color: #fff !important; }
.tb-btn-code      { background: #eb2f96 !important; border-color: #eb2f96 !important; color: #fff !important; }
.tb-btn-mysql     { background: #13c2c2 !important; border-color: #13c2c2 !important; color: #fff !important; }
.tb-btn-condition { background: #fa8c16 !important; border-color: #fa8c16 !important; color: #fff !important; }
.tb-btn-merge     { background: #7c3aed !important; border-color: #7c3aed !important; color: #fff !important; }

.designer-body { flex: 1; display: flex; overflow: hidden; }

.left-panel, .right-panel {
  width: 260px;
  background: #fff;
  border-right: 1px solid #eee;
  overflow-y: auto;
  flex-shrink: 0;
}
.right-panel { border-right: none; border-left: 1px solid #eee; width: 310px; }

.panel-title {
  font-size: 13px; font-weight: 600; color: #333;
  padding: 12px 16px; border-bottom: 1px solid #eee;
  background: #f8f9fa; display: flex; align-items: center; gap: 6px;
  position: sticky; top: 0; z-index: 1;
}

.node-item {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 12px; cursor: pointer;
  border-bottom: 1px solid #f0f0f0; transition: background 0.15s;
}
.node-item:hover { background: #f5f7ff; }
.node-item.node-selected { background: #e6f4ff; border-left: 3px solid #1890ff; }

.node-icon-badge {
  width: 30px; height: 30px; border-radius: 6px;
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
.badge-merge     { background: #7c3aed; }

.node-info { flex: 1; min-width: 0; }
.node-type-label { font-size: 12px; font-weight: 600; color: #333; }
.node-key-label  { font-size: 11px; color: #999; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* 中间画布 */
.canvas-area {
  flex: 1; overflow-y: auto; background: #f5f7fa;
  display: flex; align-items: flex-start; justify-content: center; padding: 24px 0;
}
.flow-hint { text-align: center; color: #aaa; padding-top: 80px; font-size: 15px; }
.flow-chart { display: flex; flex-direction: column; align-items: center; gap: 0; min-width: 320px; }
.flow-node-wrap { display: flex; flex-direction: column; align-items: center; }

.flow-node {
  width: 160px; min-height: 64px; border-radius: 10px;
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  cursor: pointer; transition: all 0.15s; border: 2px solid transparent;
  padding: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  background: #fff;
}
.flow-node:hover { transform: scale(1.03); }
.flow-node.fn-selected { border-color: #1890ff !important; box-shadow: 0 0 0 3px rgba(24,144,255,0.2); }
.fn-icon { font-size: 20px; margin-bottom: 4px; }
.fn-name { font-size: 12px; font-weight: 600; color: #333; max-width: 140px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.fn-type { font-size: 11px; color: #999; }
.fn-arrow { font-size: 20px; color: #bbb; margin: 2px 0; }

.fn-start     { background: linear-gradient(135deg, #f6ffed, #d9f7be); border-color: #52c41a; }
.fn-end       { background: linear-gradient(135deg, #fff1f0, #ffccc7); border-color: #ff4d4f; }
.fn-method    { background: linear-gradient(135deg, #e6f4ff, #bae0ff); border-color: #1890ff; }
.fn-assign    { background: linear-gradient(135deg, #f9f0ff, #d3adf7); border-color: #722ed1; }
.fn-code      { background: linear-gradient(135deg, #fff0f6, #ffadd2); border-color: #eb2f96; }
.fn-mysql     { background: linear-gradient(135deg, #e6fffb, #b5f5ec); border-color: #13c2c2; }
.fn-condition { background: linear-gradient(135deg, #fff7e6, #ffd591); border-color: #fa8c16; }
.fn-merge     { background: linear-gradient(135deg, #f5f0ff, #c4b5fd); border-color: #7c3aed; }

/* 条件分支展示 */
.condition-branches {
  display: flex; flex-direction: column; gap: 4px;
  background: #fffbe6; border: 1px dashed #fa8c16;
  border-radius: 8px; padding: 8px 12px; margin: 4px 0;
  min-width: 200px;
}
.branch-line { display: flex; align-items: center; gap: 6px; font-size: 12px; }
.branch-arrow { color: #fa8c16; font-weight: bold; }
.branch-label { padding: 2px 6px; border-radius: 4px; font-size: 11px; max-width: 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.branch-custom { background: #fff0f0; color: #cf1322; }
.branch-default { background: #e6fffb; color: #08979c; }
.branch-target { color: #1890ff; font-size: 11px; }

/* 右侧属性面板 */
.prop-content { padding: 12px; }
.prop-item { margin-bottom: 12px; }
.prop-item label { display: block; font-size: 12px; color: #666; margin-bottom: 4px; font-weight: 500; }
.prop-tip { font-size: 12px; color: #888; background: #f8f9fa; padding: 8px 10px; border-radius: 6px; margin-bottom: 12px; line-height: 1.8; }
.prop-tip code { background: #e8f4fd; padding: 1px 4px; border-radius: 3px; color: #1890ff; font-size: 11px; }
.prop-section-title {
  font-size: 12px; font-weight: 600; color: #444;
  margin: 12px 0 6px; padding-bottom: 4px; border-bottom: 1px solid #eee;
  display: flex; align-items: center;
}
.type-dot-start { color: #52c41a; }
.type-dot-end { color: #ff4d4f; }
.type-dot-method { color: #1890ff; }
.type-dot-assign { color: #722ed1; }
.type-dot-code { color: #eb2f96; }
.type-dot-mysql { color: #13c2c2; }
.type-dot-condition { color: #fa8c16; }
.type-dot-merge { color: #7c3aed; }

/* 填充规则行 */
.fill-rule-row {
  display: flex; align-items: center; gap: 4px;
  margin-bottom: 6px; padding: 6px; background: #fafafa;
  border-radius: 6px; border: 1px solid #f0f0f0;
}
.arrow-icon { color: #1890ff; font-weight: bold; flex-shrink: 0; }

/* 赋值规则 */
.assign-rule { background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 6px; margin-bottom: 8px; }
.assign-row { display: flex; align-items: center; gap: 4px; }

/* 条件分支属性 */
.condition-item { background: #fffbe6; border: 1px solid #ffe58f; border-radius: 6px; padding: 8px; margin-bottom: 8px; }

/* 代码编辑器 */
.code-editor :deep(textarea) { font-family: 'Consolas', 'Monaco', monospace !important; font-size: 12px !important; line-height: 1.6; background: #1e1e1e !important; color: #d4d4d4 !important; }
</style>
