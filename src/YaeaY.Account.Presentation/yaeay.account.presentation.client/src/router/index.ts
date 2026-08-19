// src/router/index.ts

import { createRouter, createWebHistory } from 'vue-router'
import { loginRoutes } from './routes/login-route'
import { userCreateRoutes } from './routes/user-create-route'
import { emailConfirmationRoutes } from './routes/email-confirmation-route'
import { homeRoutes } from './routes/home-route'
import { helpRoutes } from './routes/help-route'
import { publicInformationRoutes } from './routes/public-information-route'
import { getCurrentSession } from '@/services/authentication-service'

const routes = [
  { path: '/', redirect: '/login' },
  ...loginRoutes,
  ...userCreateRoutes,
  ...emailConfirmationRoutes,
  ...homeRoutes,
  ...helpRoutes,
  ...publicInformationRoutes,
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  if (!to.meta.requiresAuthentication) return true

  try {
    await getCurrentSession()
    return true
  } catch {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }
})

export default router
