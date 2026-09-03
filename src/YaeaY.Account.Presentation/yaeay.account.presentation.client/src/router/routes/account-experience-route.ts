import type { RouteRecordRaw } from 'vue-router'

const component = () => import('@/pages/HomePage.vue')
const meta = { requiresAuthentication: true }

export const accountExperienceRoutes: RouteRecordRaw[] = [
  {
    path: '/apps',
    name: 'account-apps',
    component,
    meta: { ...meta, title: 'Apps | YaeaY Account' },
  },
  {
    path: '/settings',
    name: 'account-settings',
    component,
    meta: { ...meta, title: 'Configurações | YaeaY Account' },
  },
]
