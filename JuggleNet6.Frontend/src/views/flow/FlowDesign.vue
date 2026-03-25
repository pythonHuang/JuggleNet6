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
        <el-button size="small" @click="autoLayout" icon="Grid">自动布局</el-button>
        <el-button size="small" @click="paramDrawer = true" icon="Setting">流程参数</el-button>
        <el-button size="small" @click="variableDrawer = true" icon="List">变量</el-button>
        <el-button size="small" type="warning" @click="openDebug">调试</el-button>
        <el-button size="small" type="success" @click="saveFlow">保存</el-button>
        <el-button size="small" type="primary" @click="deployFlow">部署</el-button>
      </div>
    </div>

    <div class="designer-body">
      <!-- 中间 VueFlow 画布 -->
      <div class="canvas-area" ref="canvasRef">
        <VueFlow
          v-model:nodes="vfNodes"
          v-model:edges="vfEdges"
          :default-viewport="{ x: 60, y: 40, zoom: 1 }"
          :min-zoom="0.2"
          :max-zoom="2"
          :snap-to-grid="true"
          :snap-grid="[16, 16]"
          fit-view-on-init
          @node-click="onVfNodeClick"
          @edge-click="onVfEdgeClick"
          @connect="onVfConnect"
          @edge-update="onVfEdgeUpdate"
          class="vf-canvas"
        >
          <Background :variant="'dots'" :gap="20" :size="1.2" :color="'#d0d7e3'" />
          <Controls />
          <MiniMap :node-color="vfNodeColor" :node-border-radius="8" />

          <!-- 自定义节点模板 -->
          <template #node-juggle="{ data }">
            <div
              class="jg-node"
              :class="['jg-' + data.elementType.toLowerCase(), selectedNodeKey === data.nodeKey ? 'jg-selected' : '']"
              @click.stop="selectNodeByKey(data.nodeKey)"
            >
              <Handle type="target" :position="Position.Top" class="jg-handle jg-handle-top" />
              <div class="jg-icon">{{ nodeIcon(data.elementType) }}</div>
              <div class="jg-name">{{ data.label || data.nodeKey }}</div>
              <div class="jg-type">{{ nodeTypeName(data.elementType) }}</div>
              <Handle type="source" :position="Position.Bottom" class="jg-handle jg-handle-bottom" />
            </div>
          </template>
        </VueFlow>

        <!-- 空状态提示 -->
        <div class="flow-hint" v-if="vfNodes.length === 0">
          <div style="font-size:48px;color:#ddd">⬡</div>
          <p>从工具栏点击按钮添加节点，然后拖拽连接线建立流程</p>
        </div>
      </div>

      <!-- 右侧属性面板 -->
      <div class="right-panel">
        <div class="panel-title" v-if="selectedNode">
          <span :class="'type-dot-' + selectedNode.elementType.toLowerCase()">●</span>
          {{ nodeTypeName(selectedNode.elementType) }} 属性
          <el-button size="small" type="danger" link icon="Delete"
            style="margin-left:auto" @click="removeNode(selectedNode.key)">删除</el-button>
        </div>
        <div class="panel-title" v-else>节点属性</div>

        <div class="prop-content" v-if="selectedNode">
          <div class="prop-item">
            <label>节点Key</label>
            <el-input :value="selectedNode.key" disabled size="small" />
          </div>
          <div class="prop-item">
            <label>节点标签</label>
            <el-input v-model="selectedNode.label" placeholder="可选显示名称" size="small"
              @input="syncVfNodeLabel(selectedNode)" />
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
            <div class="prop-tip">聚合节点：将多个 CONDITION 分支汇聚到一个执行路径。通过画布连线设置入口和出口。</div>
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
            <div class="prop-tip">条件节点：每个分支连接到不同的目标节点（通过画布连线），在此设置判断表达式。</div>
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
                <el-select v-model="cond.outgoing" placeholder="下一节点（或从画布连线）" size="small" style="flex:1" clearable>
                  <el-option v-for="n in otherNodes" :key="n.key" :value="n.key"
                    :label="`${nodeTypeName(n.elementType)}: ${n.label || n.key}`" />
                </el-select>
              </div>
            </div>
          </template>

          <!-- 连线提示（所有节点通用） -->
          <div class="prop-tip" style="margin-top:12px;background:#e6f4ff;color:#1890ff">
            💡 <b>连线方式：</b>拖动节点底部蓝色连接点到目标节点顶部，即可建立连线。也可直接在画布上拖动节点改变位置。
          </div>
        </div>
        <el-empty v-else description="点击画布中的节点查看/编辑属性" :image-size="60" style="padding-top:40px" />
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
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import request from '../../utils/request'

// VueFlow
import { VueFlow, Position } from '@vue-flow/core'
import type { NodeMouseEvent, EdgeMouseEvent, Connection } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { Controls } from '@vue-flow/controls'
import { MiniMap } from '@vue-flow/minimap'
import { Handle } from '@vue-flow/core'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/core/dist/theme-default.css'
import '@vue-flow/controls/dist/style.css'
import '@vue-flow/minimap/dist/style.css'

const route = useRoute()
const router = useRouter()
const flowKey = route.params.flowKey as string

// ====== 业务节点数据（原格式） ======
const flowInfo = ref<any>(null)
const businessNodes = ref<any[]>([])   // 原始节点数据，与后端格式一致
const selectedNodeKey = ref<string | null>(null)
const allVariables = ref<any[]>([])
const apiOptions = ref<any[]>([])
const dataSources = ref<any[]>([])
const methodApiSelection = ref<any[]>([])

// ====== VueFlow 节点/边 ======
const vfNodes = ref<any[]>([])
const vfEdges = ref<any[]>([])

// 节点颜色（minimap用）
function vfNodeColor(node: any) {
  const map: Record<string, string> = {
    start: '#52c41a', end: '#ff4d4f', method: '#1890ff',
    assign: '#722ed1', code: '#eb2f96', mysql: '#13c2c2',
    condition: '#fa8c16', merge: '#7c3aed'
  }
  return map[node.data?.elementType?.toLowerCase()] || '#aaa'
}

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

const selectedNode = computed(() => businessNodes.value.find(n => n.key === selectedNodeKey.value) || null)
const hasStart = computed(() => businessNodes.value.some(n => n.elementType === 'START'))
const hasEnd = computed(() => businessNodes.value.some(n => n.elementType === 'END'))
const otherNodes = computed(() => businessNodes.value.filter(n => n.key !== selectedNodeKey.value))

watch(selectedNode, (node) => {
  if (node?.elementType === 'METHOD' && node.method?.suiteCode && node.method?.methodCode) {
    methodApiSelection.value = [node.method.suiteCode, node.method.methodCode]
  } else {
    methodApiSelection.value = []
  }
})

// ====== VueFlow 事件 ======
function onVfNodeClick(evt: NodeMouseEvent) {
  selectNodeByKey(evt.node.data.nodeKey)
}
function onVfEdgeClick(_evt: EdgeMouseEvent) {
  // 点击边时可以删除（预留）
}
function onVfConnect(params: Connection) {
  const srcKey = vfNodes.value.find((n: any) => n.id === params.source)?.data?.nodeKey
  const tgtKey = vfNodes.value.find((n: any) => n.id === params.target)?.data?.nodeKey
  if (!srcKey || !tgtKey) return

  // 更新业务节点的 outgoings/incomings
  const srcNode = businessNodes.value.find(n => n.key === srcKey)
  const tgtNode = businessNodes.value.find(n => n.key === tgtKey)
  if (!srcNode || !tgtNode) return

  if (!srcNode.outgoings) srcNode.outgoings = []
  if (!tgtNode.incomings) tgtNode.incomings = []
  if (!srcNode.outgoings.includes(tgtKey)) srcNode.outgoings.push(tgtKey)
  if (!tgtNode.incomings.includes(srcKey)) tgtNode.incomings.push(srcKey)

  // 如果源节点是CONDITION，自动填入第一个没有outgoing的分支
  if (srcNode.elementType === 'CONDITION' && srcNode.conditions) {
    const emptyCond = srcNode.conditions.find((c: any) => !c.outgoing)
    if (emptyCond) emptyCond.outgoing = tgtKey
  }

  // 添加边到VueFlow
  const edgeId = `e-${params.source}-${params.target}`
  if (!vfEdges.value.find(e => e.id === edgeId)) {
    vfEdges.value.push({
      id: edgeId,
      source: params.source,
      target: params.target,
      animated: true,
      style: { stroke: '#1890ff', strokeWidth: 2 },
      markerEnd: { type: 'arrowclosed', color: '#1890ff' }
    })
  }
}
function onVfEdgeUpdate(_evt: { edge: any; connection: any }) {
  // 边更新（预留）
}

function selectNodeByKey(key: string) {
  selectedNodeKey.value = key
}

// ====== 业务节点 → VueFlow 节点转换 ======
function buildVfNode(bNode: any, x: number, y: number) {
  return {
    id: bNode.key,
    type: 'juggle',
    position: { x: bNode._x ?? x, y: bNode._y ?? y },
    data: {
      nodeKey: bNode.key,
      elementType: bNode.elementType,
      label: bNode.label || ''
    },
    draggable: true
  }
}

function buildVfEdge(srcKey: string, tgtKey: string) {
  return {
    id: `e-${srcKey}-${tgtKey}`,
    source: srcKey,
    target: tgtKey,
    animated: true,
    style: { stroke: '#1890ff', strokeWidth: 2 },
    markerEnd: { type: 'arrowclosed', color: '#1890ff' }
  }
}

// 将业务节点数组同步到VueFlow
function syncBusinessNodesToVf() {
  // 计算布局（如果没有保存位置）
  const cols = 1
  const xBase = 100
  const yBase = 60
  const xGap = 200
  const yGap = 120

  const newVfNodes: any[] = []
  const newVfEdges: any[] = []
  const edgeSet = new Set<string>()

  businessNodes.value.forEach((bNode, idx) => {
    const x = bNode._x ?? xBase + (idx % cols) * xGap
    const y = bNode._y ?? yBase + idx * yGap
    newVfNodes.push(buildVfNode(bNode, x, y))
  })

  // 从 outgoings 构建边
  businessNodes.value.forEach(bNode => {
    const outs: string[] = bNode.outgoings || []
    outs.forEach((tgt: string) => {
      const eid = `e-${bNode.key}-${tgt}`
      if (!edgeSet.has(eid)) {
        edgeSet.add(eid)
        newVfEdges.push(buildVfEdge(bNode.key, tgt))
      }
    })
    // CONDITION 分支也要建边
    if (bNode.elementType === 'CONDITION' && bNode.conditions) {
      bNode.conditions.forEach((c: any) => {
        if (c.outgoing) {
          const eid = `e-${bNode.key}-${c.outgoing}`
          if (!edgeSet.has(eid)) {
            edgeSet.add(eid)
            newVfEdges.push({
              ...buildVfEdge(bNode.key, c.outgoing),
              label: c.conditionName || '',
              style: { stroke: '#fa8c16', strokeWidth: 2, strokeDasharray: '5,3' },
              labelStyle: { fill: '#fa8c16', fontWeight: 600, fontSize: 11 },
              markerEnd: { type: 'arrowclosed', color: '#fa8c16' }
            })
          }
        }
      })
    }
  })

  vfNodes.value = newVfNodes
  vfEdges.value = newVfEdges
}

// 从VueFlow节点位置反同步回业务节点
function syncVfPositionsToBusinessNodes() {
  vfNodes.value.forEach(vfn => {
    const bNode = businessNodes.value.find(n => n.key === vfn.id)
    if (bNode) {
      bNode._x = vfn.position.x
      bNode._y = vfn.position.y
    }
  })
}

// 同步标签到VueFlow节点data
function syncVfNodeLabel(bNode: any) {
  const vfn = vfNodes.value.find(n => n.id === bNode.key)
  if (vfn) vfn.data = { ...vfn.data, label: bNode.label }
}

// ====== 自动布局（自动排成整齐竖排） ======
function autoLayout() {
  const xBase = 200
  const yBase = 60
  const yGap = 130
  businessNodes.value.forEach((bNode, idx) => {
    bNode._x = xBase
    bNode._y = yBase + idx * yGap
  })
  syncBusinessNodesToVf()
  ElMessage.success('已自动布局')
}

// ====== 数据加载 ======
onMounted(async () => {
  await Promise.all([loadFlowInfo(), loadSuiteApis(), loadDataSources()])
})

async function loadFlowInfo() {
  try {
    const res: any = await request.get(`/flow/definition/infoByKey/${flowKey}`)
    const def = res.data?.definition || res.data
    flowInfo.value = def
    if (def?.flowContent && def.flowContent !== '[]') {
      try {
        businessNodes.value = JSON.parse(def.flowContent)
      } catch { businessNodes.value = [] }
    }
    allVariables.value = res.data?.variables || []
    flowInputParams.value = res.data?.inputParams || []
    flowOutputParams.value = res.data?.outputParams || []
    if (flowInputParams.value.length > 0) {
      const defaultObj: Record<string, any> = {}
      for (const p of flowInputParams.value) {
        defaultObj[p.paramCode] = p.defaultValue || ''
      }
      debugParams.value = JSON.stringify(defaultObj, null, 2)
    }
    // 同步到 VueFlow
    await nextTick()
    syncBusinessNodesToVf()
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

// ====== 节点操作 ======
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

function addNode(type: string) {
  const key = `${type.toLowerCase()}_${Date.now()}`
  // 新节点放在最后一个节点右边，或默认位置
  const lastVfNode = vfNodes.value[vfNodes.value.length - 1]
  const x = lastVfNode ? lastVfNode.position.x + 220 : 200
  const y = lastVfNode ? lastVfNode.position.y : 200

  const bNode: any = { key, elementType: type, incomings: [], outgoings: [], label: '', _x: x, _y: y }
  if (type === 'METHOD') {
    bNode.method = {
      suiteCode: '', methodCode: '', url: '', requestType: 'GET', contentType: 'JSON',
      inputFillRules: [], outputFillRules: [], headerFillRules: []
    }
  }
  if (type === 'CONDITION') {
    bNode.conditions = [
      { conditionName: '分支1', conditionType: 'CUSTOM', expression: '', outgoing: '' },
      { conditionName: '默认', conditionType: 'DEFAULT', expression: '', outgoing: '' }
    ]
  }
  if (type === 'ASSIGN') bNode.assignRules = []
  if (type === 'CODE') bNode.codeConfig = { scriptType: 'javascript', script: '' }
  if (type === 'MYSQL') bNode.mysqlConfig = {
    dataSourceName: '', dataSourceType: '', sql: '', operationType: 'QUERY',
    outputVariable: '', affectedRowsVariable: ''
  }

  businessNodes.value.push(bNode)

  // 同步到VueFlow
  vfNodes.value.push(buildVfNode(bNode, x, y))

  selectNodeByKey(key)
}

function removeNode(key: string) {
  businessNodes.value = businessNodes.value.filter(n => n.key !== key)
  if (selectedNodeKey.value === key) selectedNodeKey.value = null
  businessNodes.value.forEach(n => {
    n.outgoings = (n.outgoings || []).filter((k: string) => k !== key)
    n.incomings = (n.incomings || []).filter((k: string) => k !== key)
    if (n.conditions) n.conditions.forEach((c: any) => { if (c.outgoing === key) c.outgoing = '' })
  })
  // 同步到VueFlow
  vfNodes.value = vfNodes.value.filter(n => n.id !== key)
  vfEdges.value = vfEdges.value.filter(e => e.source !== key && e.target !== key)
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

// ====== 保存/部署/调试 ======
async function saveFlow() {
  if (!flowInfo.value?.id) return ElMessage.error('流程信息未加载')
  // 保存前先把VueFlow位置同步回业务节点
  syncVfPositionsToBusinessNodes()
  // 把VueFlow的边同步回业务节点的 outgoings/incomings
  syncVfEdgesToBusinessNodes()
  // 序列化业务节点（去掉 _x _y 字段可选保留，此处保留以便下次还原位置）
  await request.put('/flow/definition/save', { id: flowInfo.value.id, flowContent: JSON.stringify(businessNodes.value) })
  ElMessage.success('保存成功')
}

function syncVfEdgesToBusinessNodes() {
  // 重置所有节点的 outgoings/incomings
  businessNodes.value.forEach(n => { n.outgoings = []; n.incomings = [] })
  // 从VueFlow边重建
  vfEdges.value.forEach(edge => {
    const srcKey = edge.source
    const tgtKey = edge.target
    const srcNode = businessNodes.value.find(n => n.key === srcKey)
    const tgtNode = businessNodes.value.find(n => n.key === tgtKey)
    if (srcNode && !srcNode.outgoings.includes(tgtKey)) srcNode.outgoings.push(tgtKey)
    if (tgtNode && !tgtNode.incomings.includes(srcKey)) tgtNode.incomings.push(srcKey)
  })
  // CONDITION分支的 outgoing 保持在conditions里，不需要额外处理
  // （outgoings已在onVfConnect里同步了）
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

/* 中间画布 */
.canvas-area {
  flex: 1;
  position: relative;
  background: #f0f2f5;
  overflow: hidden;
}

.vf-canvas {
  width: 100%;
  height: 100%;
}

.flow-hint {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -60%);
  text-align: center;
  color: #aaa;
  pointer-events: none;
  z-index: 10;
}
.flow-hint p { font-size: 14px; margin-top: 8px; }

/* 右侧属性面板 */
.right-panel {
  width: 340px;
  background: #fff;
  border-left: 1px solid #eee;
  overflow-y: auto;
  flex-shrink: 0;
}

.panel-title {
  font-size: 13px; font-weight: 600; color: #333;
  padding: 12px 16px; border-bottom: 1px solid #eee;
  background: #f8f9fa; display: flex; align-items: center; gap: 6px;
  position: sticky; top: 0; z-index: 1;
}

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

.fill-rule-row {
  display: flex; align-items: center; gap: 4px;
  margin-bottom: 6px; padding: 6px; background: #fafafa;
  border-radius: 6px; border: 1px solid #f0f0f0;
}
.arrow-icon { color: #1890ff; font-weight: bold; flex-shrink: 0; }

.assign-rule { background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 6px; margin-bottom: 8px; }
.assign-row { display: flex; align-items: center; gap: 4px; }

.condition-item { background: #fffbe6; border: 1px solid #ffe58f; border-radius: 6px; padding: 8px; margin-bottom: 8px; }

.code-editor :deep(textarea) { font-family: 'Consolas', 'Monaco', monospace !important; font-size: 12px !important; line-height: 1.6; background: #1e1e1e !important; color: #d4d4d4 !important; }
</style>

<!-- 自定义节点全局样式（非scoped） -->
<style>
/* VueFlow 自定义节点 jg-node */
.jg-node {
  width: 140px;
  min-height: 64px;
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  border: 2px solid transparent;
  padding: 8px 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.12);
  background: #fff;
  position: relative;
  transition: box-shadow 0.15s, border-color 0.15s;
  user-select: none;
}
.jg-node:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.2); }
.jg-node.jg-selected { border-color: #1890ff !important; box-shadow: 0 0 0 3px rgba(24,144,255,0.25); }

.jg-icon { font-size: 20px; margin-bottom: 4px; }
.jg-name { font-size: 12px; font-weight: 600; color: #333; max-width: 120px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; text-align: center; }
.jg-type { font-size: 11px; color: #999; }

.jg-start     { background: linear-gradient(135deg, #f6ffed, #d9f7be); border-color: #52c41a; }
.jg-end       { background: linear-gradient(135deg, #fff1f0, #ffccc7); border-color: #ff4d4f; }
.jg-method    { background: linear-gradient(135deg, #e6f4ff, #bae0ff); border-color: #1890ff; }
.jg-assign    { background: linear-gradient(135deg, #f9f0ff, #d3adf7); border-color: #722ed1; }
.jg-code      { background: linear-gradient(135deg, #fff0f6, #ffadd2); border-color: #eb2f96; }
.jg-mysql     { background: linear-gradient(135deg, #e6fffb, #b5f5ec); border-color: #13c2c2; }
.jg-condition { background: linear-gradient(135deg, #fff7e6, #ffd591); border-color: #fa8c16; }
.jg-merge     { background: linear-gradient(135deg, #f5f0ff, #c4b5fd); border-color: #7c3aed; }

/* Handle 连接点样式 */
.jg-handle {
  width: 10px !important;
  height: 10px !important;
  background: #1890ff !important;
  border: 2px solid #fff !important;
  border-radius: 50% !important;
}
.jg-handle-top  { top: -6px !important; }
.jg-handle-bottom { bottom: -6px !important; }

/* VueFlow 画布背景 */
.vue-flow__background { background: #f0f2f5; }

/* 覆盖 VueFlow 控件颜色 */
.vue-flow__controls-button {
  background: #fff;
  border: 1px solid #ddd;
  color: #333;
}
.vue-flow__controls-button:hover { background: #e6f4ff; }
</style>
