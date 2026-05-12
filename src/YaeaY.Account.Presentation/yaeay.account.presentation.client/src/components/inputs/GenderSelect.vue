                   <!-- src/components/inputs/GenderSelect.vue -->

<script setup lang="ts">
  import { genderItems, type Gender } from '@/constants/gender'

  const model = defineModel<Gender | null>({ default: null })

  defineProps<{
    label?: string
    clearable?: boolean
    disabled?: boolean
    rules?: Array<(v: unknown) => true | string>
  }>()
</script>

<template>
  <v-select v-model="model"
            class="gender-select"
            :label="label ?? 'Gênero'"
            :items="genderItems"
            item-title="title"
            item-value="value"
            variant="outlined"
            rounded="lg"
            density="comfortable"
            :clearable="clearable ?? true"
            :disabled="disabled ?? false"
            :rules="rules" />
</template>

<style scoped>
    .gender-select {
      color: #183729;
    }

    .gender-select :deep(.v-field) {
      box-shadow: none;
    }

    .gender-select :deep(.v-field__overlay) {
      background-color: transparent;
    }

    .gender-select :deep(.v-field__input) {
      min-height: 56px;
      color: #183729;
      padding-inline-start: 18px;
    }

    .gender-select :deep(.v-field-label:not(.v-field-label--floating)) {
      margin-inline-start: 18px;
    }

    .gender-select :deep(.v-label) {
      color: #424844;
      font-size: 0.72rem;
      font-weight: 700;
      letter-spacing: 0.12em;
      text-transform: uppercase;
    }

    .gender-select :deep(.v-field__append-inner),
    .gender-select :deep(.v-field__clearable) {
      color: #183729;
    }
</style>
