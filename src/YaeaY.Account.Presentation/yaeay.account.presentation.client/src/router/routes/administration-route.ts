import type { RouteRecordRaw } from 'vue-router'

export const administrationRoutes: RouteRecordRaw[] = [{
  path: '/administration',
  name: 'administration',
  component: () => import('@/pages/AdministrationPage.vue'),
  meta: { requiresAuthentication: true, title: 'Administração | YaeaY Account' },
}]
