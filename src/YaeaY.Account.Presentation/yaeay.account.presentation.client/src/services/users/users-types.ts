// src/services/users/users-types.ts

import type { Gender } from "../../constants/gender"
import type { PhoneType } from "@/constants/phoneType"

/** Identificador de User (GUID em string no client) */
export type UserId = string

/** Campos base usados no CREATE */
export type UserCreateCore = {
  emailAddress: string
  password: string
  fullName: string
  birthDate: string
  gender: Gender
  callingCode: string
  regionCode: string
  areaCode: string
  phoneType: PhoneType
  phoneNumber: string
}

/** ===== CREATE ===== */
export type CreateUserRequest = UserCreateCore

export type CreateUserResponse = {
  id: UserId
  fullName: string
  message: string
}

export type UpdateUserRequest = {
  fullName?: string
  birthDate?: string
  gender?: Gender
  phones?: UpdateUserPhoneRequest[]
}

export type UpdateUserPhoneRequest = {
  id?: string
  callingCode: string
  regionCode: string
  areaCode?: string
  phoneType: PhoneType
  phoneNumber: string
  isPrimary: boolean
}

/** Campos retornados pelo Update (conforme seu handler) */
export type UpdateUserResponse = {
  id: UserId
  updatedFields: string[]
  addedCpfDocuments: unknown[]
  message: string
}

export type MyDataPhoneResponse = {
  id: string
  callingCode: string
  country: string
  areaCode: string
  number: string
  phoneType: PhoneType
  isPrimary: boolean
  createdAt: string
}

export type MyDataDocumentResponse = {
  id: string
  type: string
  number: string | null
  createdAt: string
}

export type MyDataResponse = {
  userId: string
  email: string
  fullName: string
  birthDate: string
  gender: Gender
  status: string
  phones: MyDataPhoneResponse[]
  documents: MyDataDocumentResponse[]
  projectedAtUtc: string
}
