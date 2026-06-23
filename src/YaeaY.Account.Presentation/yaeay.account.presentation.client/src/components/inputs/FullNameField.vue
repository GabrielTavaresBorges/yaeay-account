<!-- src/components/inputs/FullNameField.vue -->

<script setup lang="ts">
  import { computed, useAttrs } from 'vue'
  import { mdiAccountOutline } from '@mdi/js'

  type Rule = (value: any) => true | string

  const props = withDefaults(
    defineProps<{
      modelValue: string
      rules?: Rule[]
      label?: string
      placeholder?: string
      clearable?: boolean
      trimOnBlur?: boolean
    }>(),
    {
      modelValue: '',
      label: 'Nome completo',
      placeholder: 'Informe seu nome completo',
      clearable: true,
      trimOnBlur: true,
    }
  )

  const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void
  }>()

  const attrs = useAttrs()

  const model = computed<string>({
    get: () => props.modelValue ?? '',
    set: (val: string) => emit('update:modelValue', val),
  })

  function onBlur() {
    if (!props.trimOnBlur) return
    emit('update:modelValue', (props.modelValue ?? '').trim())
  }
</script>

<template>
  <v-text-field v-bind="attrs"
                v-model="model"
                class="full-name-field"
                :label="label"
                :placeholder="placeholder"
                :rules="rules"
                :clearable="clearable"
                :prepend-inner-icon="mdiAccountOutline"
                type="text"
                autocomplete="name"
                spellcheck="false"
                autocapitalize="words"
                @blur="onBlur"
                variant="outlined"
                rounded="lg" />
</template>

<style scoped>
  :deep(.full-name-field .v-field) {
    background-color: #e2e2e2;
  }

  :deep(.full-name-field .v-field__overlay) {
    background-color: transparent;
  }

  :deep(.full-name-field .v-field__input) {
    color: #183729;
  }

  :deep(.full-name-field .v-field__prepend-inner),
  :deep(.full-name-field .v-field__append-inner),
  :deep(.full-name-field .v-field__clearable) {
    color: #183729;
  }

  :deep(.full-name-field input:-webkit-autofill),
  :deep(.full-name-field input:-webkit-autofill:hover),
  :deep(.full-name-field input:-webkit-autofill:focus),
  :deep(.full-name-field input:-webkit-autofill:active) {
    -webkit-box-shadow: 0 0 0 1000px #e2e2e2 inset;
    -webkit-text-fill-color: #183729;
    caret-color: #183729;
    transition: background-color 9999s ease-in-out 0s;
  }
</style>
