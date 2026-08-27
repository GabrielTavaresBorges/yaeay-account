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
  :deep(.v-field) {
    background-color: #ffffff;
  }

  :deep(.v-field__overlay) {
    background-color: transparent;
  }

  :deep(.v-field__input) {
    color: #183729;
  }

  :deep(.v-field__outline) {
    color: rgba(24, 55, 41, 0.42);
  }

  :deep(.v-label) {
    color: #424844;
    font-size: 0.72rem;
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    opacity: 1;
  }

  :deep(.v-field__prepend-inner),
  :deep(.v-field__append-inner),
  :deep(.v-field__clearable) {
    color: #183729;
  }

  :deep(input:-webkit-autofill),
  :deep(input:-webkit-autofill:hover),
  :deep(input:-webkit-autofill:focus),
  :deep(input:-webkit-autofill:active) {
    -webkit-box-shadow: 0 0 0 1000px #ffffff inset;
    -webkit-text-fill-color: #183729;
    caret-color: #183729;
    transition: background-color 9999s ease-in-out 0s;
  }
</style>
