import type { RouteRecordRaw } from 'vue-router'

export const publicInformationRoutes: RouteRecordRaw[] = [
  {
    path: '/privacy',
    name: 'privacy',
    component: () => import('@/pages/PrivacyPage.vue'),
    meta: { title: 'Privacidade' },
  },
  {
    path: '/terms',
    name: 'terms',
    component: () => import('@/pages/TermsPage.vue'),
    meta: { title: 'Termos' },
  },
  {
    path: '/security',
    name: 'security',
    component: () => import('@/pages/SecurityPage.vue'),
    meta: { title: 'Segurança' },
  },
]
