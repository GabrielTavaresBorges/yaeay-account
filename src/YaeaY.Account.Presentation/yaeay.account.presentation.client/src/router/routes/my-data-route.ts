import type { RouteRecordRaw } from 'vue-router'

export const myDataRoutes: RouteRecordRaw[] = [
  {
    path: '/user/my-data',
    redirect: { name: 'my-data-section', params: { section: 'basic' } },
  },
  {
    path: '/user/my-data/:section(basic|contact|documents|address)',
    name: 'my-data-section',
    component: () => import('@/pages/MyDataPage.vue'),
    meta: {
      requiresAuthentication: true,
      title: 'Meus Dados | YaeaY Account',
    },
  },
]
