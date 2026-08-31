// src/services/users/users-service.ts

import type { CreateUserRequest, CreateUserResponse, MyDataResponse, UpdateUserRequest, UpdateUserResponse } from './users-types'
import { throwApiError } from '@/services/http/http-error'
import { getAntiforgeryToken } from '@/services/authentication-service'

export async function createUser(payload: CreateUserRequest): Promise<CreateUserResponse> {
  const response = await fetch('/api/User', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })

  if (response.ok) return (await response.json()) as CreateUserResponse
  return throwApiError(response)
}


export async function updateUser(payload: UpdateUserRequest): Promise<UpdateUserResponse> {
  const antiforgeryToken = await getAntiforgeryToken()
  const res = await fetch('/api/User', {
    method: 'PUT',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-YaeaY-CSRF': antiforgeryToken,
    },
    body: JSON.stringify(payload),
  })

  if (res.ok) return (await res.json()) as UpdateUserResponse
  return throwApiError(res)
}

export async function getMyData(): Promise<MyDataResponse> {
  const response = await fetch('/api/User/me', { credentials: 'same-origin' })
  if (response.ok) return (await response.json()) as MyDataResponse
  return throwApiError(response)
}
