import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    component: () => import('../views/Login.vue')
  },
  {
    path: '/',
    component: () => import('../views/Layout.vue'),
    redirect: '/flow/define',
    children: [
      { path: 'flow/define', component: () => import('../views/flow/FlowDefinitionList.vue') },
      { path: 'flow/dashboard', component: () => import('../views/flow/Dashboard.vue') },
      { path: 'flow/list',   component: () => import('../views/flow/FlowInfoList.vue') },
      { path: 'flow/version/:flowKey', component: () => import('../views/flow/FlowVersionList.vue') },
      { path: 'suite/list',  component: () => import('../views/suite/SuiteList.vue') },
      { path: 'suite/api/:suiteCode/:suiteId', component: () => import('../views/suite/ApiList.vue') },
      { path: 'suite/api/:suiteCode/:apiId/detail', component: () => import('../views/suite/ApiDetail.vue') },
      { path: 'object/list', component: () => import('../views/object/ObjectList.vue') },
      { path: 'object/attr/:objectId/:objectCode', component: () => import('../views/object/ObjectAttrList.vue') },
      { path: 'system/token',      component: () => import('../views/system/TokenList.vue') },
      { path: 'system/datasource', component: () => import('../views/system/DataSourceList.vue') },
      { path: 'system/static-var', component: () => import('../views/system/StaticVariable.vue') },
      { path: 'system/schedule',   component: () => import('../views/system/ScheduleTask.vue') },
      { path: 'system/webhook',    component: () => import('../views/system/WebhookList.vue') },
      { path: 'system/users',      component: () => import('../views/system/UserManage.vue') },
      { path: 'system/role',       component: () => import('../views/system/RoleManage.vue') },
      { path: 'system/tenant',     component: () => import('../views/system/TenantManage.vue') },
      { path: 'system/config',     component: () => import('../views/system/SystemConfig.vue') },
      { path: 'flow/log',          component: () => import('../views/flow/FlowLog.vue') },
      { path: 'flow/async-result', component: () => import('../views/flow/AsyncFlowResult.vue') },
      { path: 'flow/testcase',     component: () => import('../views/flow/FlowTestCase.vue') }
    ]
  },
  {
    path: '/design/:flowKey',
    component: () => import('../views/flow/FlowDesign.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫
router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  if (to.path !== '/login' && !token) {
    return '/login'
  }
})

export default router
