import type { RouteRecordRaw } from 'vue-router'

export const publicInformationRoutes: RouteRecordRaw[] = [
  {
    path: '/privacy',
    name: 'privacy',
    component: () => import('@/pages/PrivacyPage.vue'),
  },
  {
    path: '/terms',
    name: 'terms',
    component: () => import('@/pages/TermsPage.vue'),
  },
  {
    path: '/security',
    name: 'security',
    component: () => import('@/pages/SecurityPage.vue'),
  },
]
