import { throwApiError } from '@/services/http/http-error'

type AntiforgeryTokenResponse = { token: string }

async function antiforgeryToken(): Promise<string> {
  const response = await fetch('/api/password-recoveries/antiforgery-token', { credentials: 'same-origin' })
  if (!response.ok) return throwApiError(response)
  return ((await response.json()) as AntiforgeryTokenResponse).token
}

async function post<T>(path: string, payload: unknown): Promise<T> {
  const token = await antiforgeryToken()
  const response = await fetch(`/api/password-recoveries/${path}`, {
    method: 'POST',
    credentials: 'same-origin',
    headers: { 'Content-Type': 'application/json', 'X-YaeaY-CSRF': token },
    body: JSON.stringify(payload),
  })
  if (!response.ok) return throwApiError(response)
  return (await response.json()) as T
}

export function requestPasswordRecovery(emailAddress: string): Promise<{ message: string }> {
  return post('request', { emailAddress })
}

export function verifyPasswordRecoveryCode(emailAddress: string, code: string): Promise<{ verified: boolean }> {
  return post('verify', { emailAddress, code })
}

export function resetPassword(newPassword: string, confirmPassword: string): Promise<{ changedAtUtc: string }> {
  return post('reset', { newPassword, confirmPassword })
}
