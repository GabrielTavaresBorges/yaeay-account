import { throwApiError } from '@/services/http/http-error'

export type ConfirmEmailResponse = {
  userId: string
  status: string
  emailConfirmedAt: string
}

export type EmailConfirmationPreviewResponse = {
  maskedEmail: string
}

export async function getEmailConfirmationPreview(
  token: string,
): Promise<EmailConfirmationPreviewResponse> {
  const response = await fetch('/api/email-confirmations/preview', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token }),
  })

  if (response.ok) return (await response.json()) as EmailConfirmationPreviewResponse
  return throwApiError(response)
}

export async function confirmEmail(token: string): Promise<ConfirmEmailResponse> {
  const response = await fetch('/api/email-confirmations/confirm', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token }),
  })

  if (response.ok) return (await response.json()) as ConfirmEmailResponse
  return throwApiError(response)
}
