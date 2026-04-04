<template>
  <div ref="containerRef" :style="{ width: width, height: height, border: '1px solid #ddd', borderRadius: '4px' }"></div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import loader from '@monaco-editor/loader'

const props = withDefaults(defineProps<{
  modelValue?: string
  language?: string
  theme?: string
  width?: string
  height?: string
  readOnly?: boolean
}>(), {
  modelValue: '',
  language: 'javascript',
  theme: 'vs-dark',
  width: '100%',
  height: '300px',
  readOnly: false
})

const emit = defineEmits<{ (e: 'update:modelValue', val: string): void }>()

const containerRef = ref<HTMLElement>()
let monacoEditor: any = null

onMounted(async () => {
  // 配置 Monaco loader 使用 CDN（可自行更换为本地路径）
  loader.config({ paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.55.1/min/vs' } })

  await nextTick()
  const monaco = await loader.init()

  monacoEditor = monaco.editor.create(containerRef.value!, {
    value: props.modelValue,
    language: props.language,
    theme: props.theme,
    readOnly: props.readOnly,
    automaticLayout: true,
    minimap: { enabled: false },
    fontSize: 13,
    lineNumbers: 'on',
    scrollBeyondLastLine: false,
    wordWrap: 'on',
    tabSize: 2,
    // 代码提示：$var / $static 对象
    suggestOnTriggerCharacters: true
  })

  // 注册 Juggle JS 变量提示
  monaco.languages.registerCompletionItemProvider('javascript', {
    provideCompletionItems: (model: any, position: any) => {
      const word = model.getWordUntilPosition(position)
      const range = {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endColumn: word.endColumn
      }
      return {
        suggestions: [
          { label: '$var.getVariableValue', kind: monaco.languages.CompletionItemKind.Function,
            insertText: "$var.getVariableValue('${1:varName}')", insertTextRules: 4,
            documentation: '获取流程变量值', range },
          { label: '$var.setVariableValue', kind: monaco.languages.CompletionItemKind.Function,
            insertText: "$var.setVariableValue('${1:varName}', ${2:value})", insertTextRules: 4,
            documentation: '设置流程变量值', range },
          { label: '$static.getVariableValue', kind: monaco.languages.CompletionItemKind.Function,
            insertText: "$static.getVariableValue('${1:varCode}')", insertTextRules: 4,
            documentation: '获取全局静态变量', range },
          { label: '$static.setVariableValue', kind: monaco.languages.CompletionItemKind.Function,
            insertText: "$static.setVariableValue('${1:varCode}', ${2:value})", insertTextRules: 4,
            documentation: '设置全局静态变量', range },
          { label: 'JSON.parse', kind: monaco.languages.CompletionItemKind.Function,
            insertText: 'JSON.parse(${1:str})', insertTextRules: 4, range },
          { label: 'JSON.stringify', kind: monaco.languages.CompletionItemKind.Function,
            insertText: 'JSON.stringify(${1:obj})', insertTextRules: 4, range }
        ]
      }
    }
  })

  monacoEditor.onDidChangeModelContent(() => {
    emit('update:modelValue', monacoEditor.getValue())
  })
})

watch(() => props.modelValue, (newVal) => {
  if (monacoEditor && monacoEditor.getValue() !== newVal) {
    monacoEditor.setValue(newVal)
  }
})

onBeforeUnmount(() => {
  monacoEditor?.dispose()
})
</script>
