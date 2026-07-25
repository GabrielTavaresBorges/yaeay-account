// src/services/users/users-types.ts

import type { Gender } from "../../constants/gender"
import type { PhoneType } from "@/constants/phoneType"

/** Identificador de User (GUID em string no client) */
export type UserId = string

/** Campos base usados no CREATE */
export type UserCreateCore = {
  email: string
  password: string
  fullName: string
  birthDate: string
  gender: Gender
  callingCode: string
  regionCode: string
  areaCode: string
  phoneType: PhoneType
  phoneNumber: string
  e164: string,
}

/** ===== CREATE ===== */
export type CreateUserRequest = UserCreateCore

export type CreateUserResponse = {
  id: UserId
  fullName: string
  message: string
}

/** ===== UPDATE =====
 * Update parcial:
 * - Envie apenas o que quer alterar (campos opcionais)
 * - CPF NÃO é atualizado (não existe no payload)
 * - Id vai na URL: PUT /api/users/{id}
 */
export type UpdateUserRequest = {
  fullName?: string
  email?: string
}

/** Campos retornados pelo Update (conforme seu handler) */
export type UpdateUserResponse = {
  id: UserId
  updatedFields: string[]
  message: string
}
