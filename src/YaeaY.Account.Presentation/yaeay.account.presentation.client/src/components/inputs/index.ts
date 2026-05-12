// src/components/inputs/index.ts

import EmailField from './EmailField.vue'
import PasswordField from './PasswordField.vue'
import GenderSelect from './GenderSelect.vue'
import CpfField from './CpfField.vue'
import UserPhonesField from './UserPhonesField.vue'
import FullNameField from './FullNameField.vue'


export {
  EmailField,
  PasswordField,
  GenderSelect,
  CpfField,
  UserPhonesField,
  FullNameField,
}

export const inputs = {
  EmailField,
  PasswordField,
  GenderSelect,
  CpfField,
  UserPhonesField,
  FullNameField,
} as const
