import { throwApiError } from '@/services/http/http-error'

export type AdministrationOverview = { totalUsers: number; pendingEmailConfirmation: number; activeUsers: number; suspendedUsers: number; disabledUsers: number; pendingOutboxMessages: number }
export type AdministrationUser = { userId: string; email: string; fullName: string; status: string; createdAt: string; emailConfirmedAt: string | null; lastLoginAt: string | null }
export type AdministrationAudit = { id: string; administratorId: string; targetUserId: string | null; action: string; justification: string; occurredAtUtc: string }
export type AdministrationConfiguration = { emailConfirmationTemplate: { id: string; subject: string; bodyHtml: string; updatedAt: string } | null; roles: { id: string; name: string }[] }

async function get<T>(path: string): Promise<T> {
  const response = await fetch(path, { credentials: 'same-origin' })
  if (response.ok) return await response.json() as T
  return throwApiError(response)
}

async function mutate<T>(path: string, method: 'POST' | 'PUT', body: unknown): Promise<T> {
  const { getAntiforgeryToken } = await import('@/services/authentication-service')
  const response = await fetch(path, { method, credentials: 'same-origin', headers: { 'Content-Type': 'application/json', 'X-YaeaY-CSRF': await getAntiforgeryToken() }, body: JSON.stringify(body) })
  if (response.ok) return await response.json() as T
  return throwApiError(response)
}

export const getAdministrationOverview = () => get<AdministrationOverview>('/api/administration/overview')
export const getAdministrationUsers = () => get<AdministrationUser[]>('/api/administration/users')
export const getAdministrationAudit = () => get<AdministrationAudit[]>('/api/administration/audit')
export const getAdministrationConfiguration = () => get<AdministrationConfiguration>('/api/administration/configuration')
export const updateEmailConfirmationTemplate = (subject: string, bodyHtml: string, justification: string) => mutate('/api/administration/email-confirmation-template', 'PUT', { subject, bodyHtml, justification })
export const createIdentityRole = (name: string, justification: string) => mutate('/api/administration/roles', 'POST', { name, justification })
