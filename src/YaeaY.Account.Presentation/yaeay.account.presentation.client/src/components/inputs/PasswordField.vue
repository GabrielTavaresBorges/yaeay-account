           <!-- src/components/inputs/PasswordField.vue -->

<script setup lang="ts">import { computed, ref, useAttrs } from 'vue'
  import { mdiLock, mdiEye, mdiEyeOff } from '@mdi/js'

  type Rule = (value: any) => true | string

  const props = withDefaults(
    defineProps<{
      modelValue: string
      label?: string
      rules?: Rule[]
      clearable?: boolean

      match?: string
      matchMessage?: string
    }>(),
    {
      modelValue: '',
      label: 'Senha',
      clearable: true,
      matchMessage: 'As senhas não conferem.',
    }
  )

  const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void
  }>()

  const attrs = useAttrs()
  const show = ref(false)

  const model = computed<string>({
    get: () => props.modelValue ?? '',
    set: (val: string) => emit('update:modelValue', val),
  })

  const mergedRules = computed<Rule[]>(() => {
    const rs: Rule[] = []

    if (props.match !== undefined) {
      rs.push((v) => (String(v ?? '') === String(props.match ?? '') ? true : props.matchMessage))
    }

    if (props.rules?.length) rs.push(...props.rules)

    return rs
  })</script>

<template>
  <v-text-field v-bind="attrs"
                v-model="model"
                class="password-field"
                :label="label"
                :type="show ? 'text' : 'password'"
                :prepend-inner-icon="mdiLock"
                :append-inner-icon="show ? mdiEyeOff : mdiEye"
                @click:append-inner="show = !show"
                :rules="mergedRules"
                :clearable="clearable"
                variant="outlined"
                rounded="lg">

    <!-- Slot dentro do input (melhor visual) -->
    <template v-if="$slots['append-inner']" #append-inner>
      <slot name="append-inner" />
    </template>

    <!-- Mantém o append externo caso você queira usar -->
    <template v-if="$slots.append" #append>
      <slot name="append" />
    </template>
  </v-text-field>
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

  :global(.password-field input[type='password']::-ms-reveal),
  :global(.password-field input[type='password']::-ms-clear) {
    display: none !important;
    width: 0;
    height: 0;
  }

  :global(.password-field input[type='password']::-webkit-credentials-auto-fill-button) {
    visibility: hidden !important;
    display: none !important;
    pointer-events: none;
  }


  :deep(input:-webkit-autofill),
  :deep(input:-webkit-autofill:hover),
  :deep(input:-webkit-autofill:focus),
  :deep(input:-webkit-autofill:active) {
    background-color: transparent !important;
    -webkit-box-shadow: 0 0 0 1000px #ffffff inset !important;
    -webkit-text-fill-color: #183729 !important;
    caret-color: #183729;
    font-weight: 400;
    transition: background-color 9999s ease-in-out 0s;
  }
</style>
