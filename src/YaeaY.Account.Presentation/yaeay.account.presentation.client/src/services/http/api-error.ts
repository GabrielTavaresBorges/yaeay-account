// src/services/http/api-error.ts

/**
 * Representa o corpo (body) de erro retornado pela API.
 *
 * No seu backend (ASP.NET), quando ocorre falha (ex.: 422),
 * o controller retorna algo como:
 * { code: "some.error-code", message: "Human readable message" }
 *
 * Observação: as propriedades são opcionais porque:
 * - o backend pode não retornar JSON em alguns erros (500/HTML)
 * - pode retornar um shape diferente dependendo do endpoint/middleware
 */
export type ApiErrorBody = {
  /** Código de erro usado atualmente pela API do Account. */
  code?: string

  /** Código/identificador de erro (útil para lógica e mapeamento de mensagens). */
  identifier?: string

  /** Mensagem humana, normalmente exibida na UI (snackbar/alert). */
  message?: string
}

/**
 * Representa um erro HTTP padronizado no client.
 *
 * É o tipo que o client "lança" (throw) após receber um status não-OK
 * e tentar extrair { code, message } do body.
 *
 * Por que existe?
 * - padroniza o formato de erro para todos os services
 * - garante que sempre teremos `statusCode` (HTTP status code)
 * - permite que a UI trate erros de forma consistente
 */
export type ApiError = ApiErrorBody & {
  /** HTTP status code (ex.: 400, 401, 404, 422, 500). */
  statusCode: number
}
