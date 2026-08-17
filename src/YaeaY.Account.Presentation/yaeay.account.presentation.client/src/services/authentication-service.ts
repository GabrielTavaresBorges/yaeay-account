import { throwApiError } from '@/services/http/http-error'

export type LoginRequest = {
  emailAddress: string
  password: string
  rememberMe: boolean
}

export type LoginResponse = {
  userId: string
  fullName: string
  loggedInAt: string
}

export type CurrentSessionResponse = {
  userId: string
  fullName: string
  lastLoginAt: string | null
}

type AntiforgeryTokenResponse = { token: string }

let currentSession: CurrentSessionResponse | null = null

async function getAntiforgeryToken(): Promise<string> {
  const response = await fetch('/api/authentication/antiforgery-token', {
    credentials: 'same-origin',
  })

  if (!response.ok) return throwApiError(response)
  return ((await response.json()) as AntiforgeryTokenResponse).token
}

export async function login(payload: LoginRequest): Promise<LoginResponse> {
  const antiforgeryToken = await getAntiforgeryToken()
  const response = await fetch('/api/authentication/login', {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-YaeaY-CSRF': antiforgeryToken,
    },
    body: JSON.stringify(payload),
  })

  if (response.ok) {
    const loginResponse = (await response.json()) as LoginResponse
    currentSession = {
      userId: loginResponse.userId,
      fullName: loginResponse.fullName,
      lastLoginAt: loginResponse.loggedInAt,
    }

    return loginResponse
  }

  return throwApiError(response)
}

export function getCachedCurrentSession(): CurrentSessionResponse | null {
  return currentSession
}

export async function getCurrentSession(): Promise<CurrentSessionResponse> {
  if (currentSession) return currentSession

  const response = await fetch('/api/authentication/session', {
    credentials: 'same-origin',
  })

  if (!response.ok) return throwApiError(response)

  currentSession = (await response.json()) as CurrentSessionResponse
  return currentSession
}

export async function logout(): Promise<void> {
  const antiforgeryToken = await getAntiforgeryToken()
  const response = await fetch('/api/authentication/logout', {
    method: 'POST',
    credentials: 'same-origin',
    headers: { 'X-YaeaY-CSRF': antiforgeryToken },
  })

  if (!response.ok) return throwApiError(response)
  currentSession = null
}
