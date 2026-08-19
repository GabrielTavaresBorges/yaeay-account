import type { RouteRecordRaw } from 'vue-router'

export const helpRoutes: RouteRecordRaw[] = [
  {
    path: '/help',
    name: 'help',
    component: () => import('@/pages/HelpPage.vue'),
  },
]
