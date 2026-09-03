// src/components/inputs/index.ts

import EmailField from './EmailField.vue'
import PasswordField from './PasswordField.vue'
import GenderSelect from './GenderSelect.vue'
import CpfField from './CpfField.vue'
import BrazilianStateSelect from './BrazilianStateSelect.vue'
import UserPhonesField from './UserPhonesField.vue'
import FullNameField from './FullNameField.vue'


export {
  EmailField,
  PasswordField,
  GenderSelect,
  CpfField,
  BrazilianStateSelect,
  UserPhonesField,
  FullNameField,
}

export const inputs = {
  EmailField,
  PasswordField,
  GenderSelect,
  CpfField,
  BrazilianStateSelect,
  UserPhonesField,
  FullNameField,
} as const
