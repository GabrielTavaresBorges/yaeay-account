// src/services/users/users-service.ts

import type { CreateUserRequest, CreateUserResponse, MyDataResponse, UpdateBasicDataRequest, UpdateBasicDataResponse, UpdateDocumentsRequest, UpdateDocumentsResponse, UpdatePhonesRequest, UpdatePhonesResponse, UploadedDocumentImageResponse } from './users-types'
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


async function updateMyData<TResponse>(path: string, payload: object): Promise<TResponse> {
  const antiforgeryToken = await getAntiforgeryToken()
  const res = await fetch(`/api/User/my-data/${path}`, {
    method: 'PUT',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-YaeaY-CSRF': antiforgeryToken,
    },
    body: JSON.stringify(payload),
  })

  if (res.ok) return (await res.json()) as TResponse
  return throwApiError(res)
}

export function updateBasicData(payload: UpdateBasicDataRequest): Promise<UpdateBasicDataResponse> {
  return updateMyData('basic', payload)
}

export function updatePhones(payload: UpdatePhonesRequest): Promise<UpdatePhonesResponse> {
  return updateMyData('contact', payload)
}

export function updateDocuments(payload: UpdateDocumentsRequest): Promise<UpdateDocumentsResponse> {
  return updateMyData('documents', payload)
}

export async function getMyData(): Promise<MyDataResponse> {
  const response = await fetch('/api/User/me', { credentials: 'same-origin' })
  if (response.ok) return (await response.json()) as MyDataResponse
  return throwApiError(response)
}

export async function uploadCpfDocumentImage(file: File): Promise<UploadedDocumentImageResponse> {
  const antiforgeryToken = await getAntiforgeryToken()
  const formData = new FormData()
  formData.append('image', file, file.name)
  const response = await fetch('/api/User/documents/cpf/images', {
    method: 'POST',
    credentials: 'same-origin',
    headers: { 'X-YaeaY-CSRF': antiforgeryToken },
    body: formData,
  })

  if (response.ok) return (await response.json()) as UploadedDocumentImageResponse
  return throwApiError(response)
}
