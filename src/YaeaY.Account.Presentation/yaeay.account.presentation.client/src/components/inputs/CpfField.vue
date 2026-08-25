<script setup lang="ts">
import { computed, useAttrs } from 'vue'
import { cpfRules, digitsOnly, formatCpf, type Rule } from '@/validators/fields/cpf'

const props = withDefaults(
  defineProps<{
    modelValue: string
    rules?: Rule[]
    label?: string
    placeholder?: string
    clearable?: boolean
  }>(),
  {
    modelValue: '',
    label: 'CPF',
    placeholder: '000.000.000-00',
    clearable: true,
  },
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const attrs = useAttrs()

const activeRules = computed(() => props.rules ?? cpfRules)

const displayValue = computed<string>({
  get() {
    return formatCpf(props.modelValue)
  },
  set(val: string) {
    emit('update:modelValue', digitsOnly(val).slice(0, 11))
  },
})
</script>

<template>
  <v-text-field
    v-bind="attrs"
    v-model="displayValue"
    :label="label"
    :placeholder="placeholder"
    :rules="activeRules"
    :clearable="clearable"
    maxlength="14"
    inputmode="numeric"
    autocomplete="off"
  />
</template>
