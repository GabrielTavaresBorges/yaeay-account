import { throwApiError } from '@/services/http/http-error'

export type AdministrationOverview = { totalUsers: number; pendingEmailConfirmation: number; activeUsers: number; suspendedUsers: number; disabledUsers: number; pendingOutboxMessages: number }
export type AdministrationUser = { userId: string; email: string; fullName: string; status: string; createdAt: string; emailConfirmedAt: string | null; lastLoginAt: string | null }
export type AdministrationAudit = { id: string; administratorId: string; targetUserId: string | null; action: string; justification: string; occurredAtUtc: string }

async function get<T>(path: string): Promise<T> {
  const response = await fetch(path, { credentials: 'same-origin' })
  if (response.ok) return await response.json() as T
  return throwApiError(response)
}

export const getAdministrationOverview = () => get<AdministrationOverview>('/api/administration/overview')
export const getAdministrationUsers = () => get<AdministrationUser[]>('/api/administration/users')
export const getAdministrationAudit = () => get<AdministrationAudit[]>('/api/administration/audit')
