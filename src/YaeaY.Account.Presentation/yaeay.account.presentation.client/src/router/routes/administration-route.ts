import type { RouteRecordRaw } from 'vue-router'

const component = () => import('@/pages/AdministrationPage.vue')
const meta = { requiresAuthentication: true, requiresAdministration: true }

export const administrationRoutes: RouteRecordRaw[] = [
  { path: '/administration', name: 'administration', component, meta: { ...meta, title: 'Administração | YaeaY Account' } },
  { path: '/administration/manage-users', name: 'administration-manage-users', component, meta: { ...meta, title: 'Gerenciar usuários | YaeaY Account' } },
  { path: '/administration/manage-email', name: 'administration-manage-email', component, meta: { ...meta, title: 'Central de e-mail | YaeaY Account' } },
  { path: '/administration/roles-policies', name: 'administration-roles-policies', component, meta: { ...meta, title: 'Regras e políticas | YaeaY Account' } },
]
