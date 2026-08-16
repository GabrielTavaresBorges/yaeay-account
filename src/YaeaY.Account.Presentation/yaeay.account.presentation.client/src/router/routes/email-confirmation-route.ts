import type { RouteRecordRaw } from 'vue-router'

export const emailConfirmationRoutes: RouteRecordRaw[] = [
  {
    path: '/confirm-email',
    name: 'email-confirmation',
    component: () => import('@/pages/EmailConfirmationPage.vue'),
  },
]
