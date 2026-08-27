<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import { mdiArrowLeft, mdiInformationOutline, mdiLockReset, mdiCheckCircleOutline } from '@mdi/js'
import AppTopbar from '@/components/layout/AppTopbar.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import { EmailField, PasswordField } from '@/components/inputs'
import { PasswordRequirements } from '@/components/feedback'
import { rules } from '@/validators'
import { requestPasswordRecovery, resetPassword, verifyPasswordRecoveryCode } from '@/services/password-recovery-service'
import type { ApiError } from '@/services/http/api-error'

type Step = 'email' | 'code' | 'password' | 'success'
type VForm = { validate: () => Promise<{ valid: boolean }> }
type RecoveryAttemptState = {
  emailHash: string
  attemptCount: number
  windowStartedAtMs: number
  blockedUntilMs: number | null
  codeExpiresAtMs: number
  verified: boolean
}

const CODE_LIFETIME_SECONDS = 2 * 60
const MAXIMUM_RECOVERY_ATTEMPTS = 5
const RECOVERY_ATTEMPT_WINDOW_MS = 60 * 60 * 1000
const RECOVERY_ATTEMPT_STORAGE_KEY = 'yaeay.password-recovery.attempt-state.v2'

const step = ref<Step>('email')
const emailFormRef = ref<VForm | null>(null)
const passwordFormRef = ref<VForm | null>(null)
const email = ref('')
const issuedEmail = ref('')
const activeCodeNotice = ref(false)
const recoveryAttemptCount = ref(0)
const codeDigits = ref<string[]>(Array(6).fill(''))
const remainingSeconds = ref(0)
const newPassword = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const feedback = ref('')
let countdownInterval: number | undefined

const title = computed(() => ({
  email: 'Recuperar senha',
  code: 'Confirme o código',
  password: 'Crie uma nova senha',
  success: 'Senha alterada',
})[step.value])

const passwordValid = computed(() => {
  const value = newPassword.value
  return value.length >= 8 && /[A-Z]/.test(value) && /[a-z]/.test(value)
    && /\d/.test(value) && /[^A-Za-z0-9]/.test(value)
})

const passwordChecklist = computed(() => {
  const password = newPassword.value ?? ''
  return [
    { text: 'Mínimo de 8 caracteres', valid: password.trim().length >= 8 },
    { text: '1 letra maiúscula', valid: /[A-Z]/.test(password) },
    { text: '1 letra minúscula', valid: /[a-z]/.test(password) },
    { text: '1 número', valid: /\d/.test(password) },
    { text: '1 caractere especial (ex: @, #, $).', valid: /[^A-Za-z0-9]/.test(password) },
  ]
})

const code = computed(() => codeDigits.value.join(''))
const codeComplete = computed(() => /^\d{6}$/.test(code.value))
const countdownActive = computed(() => remainingSeconds.value > 0)
const countdownCritical = computed(() => countdownActive.value && remainingSeconds.value <= 10)
const formattedCountdown = computed(() => {
  const minutes = Math.floor(remainingSeconds.value / 60)
  const seconds = remainingSeconds.value % 60
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
})

function stopCountdown(): void {
  if (countdownInterval !== undefined) {
    window.clearInterval(countdownInterval)
    countdownInterval = undefined
  }
}

function startCountdown(expiresAtMs = Date.now() + CODE_LIFETIME_SECONDS * 1000): void {
  stopCountdown()
  const updateRemainingTime = () => {
    remainingSeconds.value = Math.max(0, Math.ceil((expiresAtMs - Date.now()) / 1000))
    if (remainingSeconds.value === 0) {
      activeCodeNotice.value = false
      stopCountdown()
    }
  }

  updateRemainingTime()
  if (remainingSeconds.value > 0)
    countdownInterval = window.setInterval(updateRemainingTime, 1000)
}

function resetCode(): void {
  codeDigits.value = Array(6).fill('')
}

function focusCodeInput(index: number): void {
  document.querySelector<HTMLInputElement>(`[data-code-index="${index}"]`)?.focus()
}

function distributeCodeDigits(startIndex: number, value: string): void {
  const digits = value.replace(/\D/g, '').slice(0, 6 - startIndex)
  if (!digits) return

  const nextDigits = [...codeDigits.value]
  digits.split('').forEach((digit, offset) => {
    nextDigits[startIndex + offset] = digit
  })
  codeDigits.value = nextDigits
  focusCodeInput(Math.min(startIndex + digits.length, 5))
}

function handleCodeInput(index: number, event: Event): void {
  const input = event.target as HTMLInputElement
  const digits = input.value.replace(/\D/g, '')

  if (!digits) {
    const nextDigits = [...codeDigits.value]
    nextDigits[index] = ''
    codeDigits.value = nextDigits
    input.value = ''
    return
  }

  distributeCodeDigits(index, digits)
}

function handleCodePaste(index: number, event: ClipboardEvent): void {
  event.preventDefault()
  distributeCodeDigits(index, event.clipboardData?.getData('text') ?? '')
}

function handleCodeKeydown(index: number, event: KeyboardEvent): void {
  if (event.key === 'Backspace' && !codeDigits.value[index] && index > 0) {
    const nextDigits = [...codeDigits.value]
    nextDigits[index - 1] = ''
    codeDigits.value = nextDigits
    focusCodeInput(index - 1)
  } else if (event.key === 'ArrowLeft' && index > 0) {
    event.preventDefault()
    focusCodeInput(index - 1)
  } else if (event.key === 'ArrowRight' && index < 5) {
    event.preventDefault()
    focusCodeInput(index + 1)
  }
}

function normalizeEmail(value: string): string {
  return value.trim().toLowerCase()
}

async function hashEmail(value: string): Promise<string> {
  const data = new TextEncoder().encode(value)
  const digest = await window.crypto.subtle.digest('SHA-256', data)
  return Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('')
}

async function loadAttemptState(normalizedEmail: string): Promise<RecoveryAttemptState | null> {
  try {
    const serialized = window.sessionStorage.getItem(RECOVERY_ATTEMPT_STORAGE_KEY)
    if (!serialized) return null

    const state = JSON.parse(serialized) as RecoveryAttemptState
    if (state.emailHash !== await hashEmail(normalizedEmail)) return null

    const resetAtMs = state.blockedUntilMs ?? state.windowStartedAtMs + RECOVERY_ATTEMPT_WINDOW_MS
    if (Date.now() >= resetAtMs) {
      window.sessionStorage.removeItem(RECOVERY_ATTEMPT_STORAGE_KEY)
      return null
    }

    return state
  } catch {
    window.sessionStorage.removeItem(RECOVERY_ATTEMPT_STORAGE_KEY)
    return null
  }
}

async function saveIssuedCodeState(
  normalizedEmail: string,
  previousState: RecoveryAttemptState | null,
): Promise<void> {
  const nowMs = Date.now()
  const withinCurrentWindow = previousState !== null
    && nowMs < (previousState.blockedUntilMs ?? previousState.windowStartedAtMs + RECOVERY_ATTEMPT_WINDOW_MS)
  const attemptCount = withinCurrentWindow ? previousState.attemptCount + 1 : 1
  const state: RecoveryAttemptState = {
    emailHash: await hashEmail(normalizedEmail),
    attemptCount,
    windowStartedAtMs: withinCurrentWindow ? previousState.windowStartedAtMs : nowMs,
    blockedUntilMs: attemptCount >= MAXIMUM_RECOVERY_ATTEMPTS
      ? nowMs + RECOVERY_ATTEMPT_WINDOW_MS
      : null,
    codeExpiresAtMs: nowMs + CODE_LIFETIME_SECONDS * 1000,
    verified: false,
  }

  window.sessionStorage.setItem(RECOVERY_ATTEMPT_STORAGE_KEY, JSON.stringify(state))
  recoveryAttemptCount.value = state.attemptCount
  startCountdown(state.codeExpiresAtMs)
}

async function markAttemptStateAsVerified(): Promise<void> {
  const state = await loadAttemptState(issuedEmail.value)
  if (!state) return

  state.verified = true
  window.sessionStorage.setItem(RECOVERY_ATTEMPT_STORAGE_KEY, JSON.stringify(state))
}

async function clearAttemptState(): Promise<void> {
  const state = await loadAttemptState(issuedEmail.value)
  if (state) window.sessionStorage.removeItem(RECOVERY_ATTEMPT_STORAGE_KEY)
  recoveryAttemptCount.value = 0
}

function attemptLimitMessage(state: RecoveryAttemptState): string {
  const waitUntilMs = state.blockedUntilMs ?? state.windowStartedAtMs + RECOVERY_ATTEMPT_WINDOW_MS
  const waitMinutes = Math.max(1, Math.ceil((waitUntilMs - Date.now()) / 60000))
  return `O limite de ${MAXIMUM_RECOVERY_ATTEMPTS} tentativas foi atingido. Aguarde ${waitMinutes} minuto${waitMinutes === 1 ? '' : 's'} para solicitar um novo código.`
}

function isApiError(error: unknown): error is ApiError {
  return typeof error === 'object' && error !== null && 'statusCode' in error
}

function recoveryErrorMessage(error: unknown): string {
  const fallback = 'Não foi possível concluir esta etapa. Tente novamente ou solicite um novo código.'
  if (!isApiError(error)) return fallback

  switch (error.identifier) {
    case 'password-recovery.invalid-or-expired':
      return 'A autorização para alterar a senha expirou ou não foi encontrada. Solicite um novo código.'
    case 'password-recovery.password-confirmation.does-not-match':
      return 'A confirmação da senha não corresponde à nova senha.'
    case 'account.password-text.required':
      return 'Informe a nova senha.'
    case 'account.password-text.too-short':
      return 'A nova senha deve ter no mínimo 8 caracteres.'
    case 'account.password-text.too-long':
      return 'A nova senha excede o tamanho máximo permitido.'
    case 'account.password-text.missing-uppercase':
      return 'A nova senha deve conter ao menos uma letra maiúscula.'
    case 'account.password-text.missing-lowercase':
      return 'A nova senha deve conter ao menos uma letra minúscula.'
    case 'account.password-text.missing-digit':
      return 'A nova senha deve conter ao menos um número.'
    case 'account.password-text.missing-special-character':
      return 'A nova senha deve conter ao menos um caractere especial.'
    case 'identity.password.reset-failed':
      return 'A senha não pôde ser alterada pela conta. Escolha outra senha e tente novamente.'
    default:
      return error.message || fallback
  }
}

function returnToEmail(): void {
  feedback.value = ''
  activeCodeNotice.value = false
  step.value = 'email'
}

function returnToActiveCode(): void {
  activeCodeNotice.value = false
  feedback.value = ''
  step.value = 'code'
}

async function requestCode(): Promise<void> {
  const validation = await emailFormRef.value?.validate()
  if (!validation?.valid) return

  const normalizedEmail = normalizeEmail(email.value)
  const attemptState = await loadAttemptState(normalizedEmail)

  if (attemptState && !attemptState.verified && Date.now() < attemptState.codeExpiresAtMs) {
    issuedEmail.value = normalizedEmail
    recoveryAttemptCount.value = attemptState.attemptCount
    startCountdown(attemptState.codeExpiresAtMs)
    activeCodeNotice.value = true
    return
  }

  if (attemptState && attemptState.attemptCount >= MAXIMUM_RECOVERY_ATTEMPTS) {
    recoveryAttemptCount.value = attemptState.attemptCount
    feedback.value = attemptLimitMessage(attemptState)
    return
  }

  await run(async () => {
    await requestPasswordRecovery(normalizedEmail)
    issuedEmail.value = normalizedEmail
    activeCodeNotice.value = false
    resetCode()
    await saveIssuedCodeState(normalizedEmail, attemptState)
    step.value = 'code'
  })
}

async function verifyCode(): Promise<void> {
  if (!countdownActive.value) {
    feedback.value = 'O código expirou. Solicite um novo código para continuar.'
    return
  }
  if (!codeComplete.value) {
    feedback.value = 'Informe os seis dígitos do código.'
    return
  }
  await run(async () => {
    await verifyPasswordRecoveryCode(email.value.trim(), code.value)
    await markAttemptStateAsVerified()
    stopCountdown()
    step.value = 'password'
  })
}

async function changePassword(): Promise<void> {
  const validation = await passwordFormRef.value?.validate()
  if (!validation?.valid) return

  if (!passwordValid.value) {
    feedback.value = 'A nova senha ainda não atende aos requisitos de segurança.'
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    feedback.value = 'A confirmação da senha não corresponde.'
    return
  }
  await run(async () => {
    await resetPassword(newPassword.value, confirmPassword.value)
    newPassword.value = ''
    confirmPassword.value = ''
    resetCode()
    await clearAttemptState()
    step.value = 'success'
  })
}

async function resend(): Promise<void> {
  const normalizedEmail = normalizeEmail(email.value)
  const attemptState = await loadAttemptState(normalizedEmail)
  if (attemptState && attemptState.attemptCount >= MAXIMUM_RECOVERY_ATTEMPTS) {
    recoveryAttemptCount.value = attemptState.attemptCount
    feedback.value = attemptLimitMessage(attemptState)
    return
  }

  await run(async () => {
    await requestPasswordRecovery(normalizedEmail)
    issuedEmail.value = normalizedEmail
    activeCodeNotice.value = false
    resetCode()
    await saveIssuedCodeState(normalizedEmail, attemptState)
    feedback.value = 'Se a conta estiver elegível, um novo código será enviado.'
  }, false)
}

async function run(action: () => Promise<void>, clearFeedback = true): Promise<void> {
  loading.value = true
  if (clearFeedback) feedback.value = ''
  try {
    await action()
  } catch (error: unknown) {
    feedback.value = recoveryErrorMessage(error)
  } finally {
    loading.value = false
  }
}

watch(email, () => {
  activeCodeNotice.value = false
})

onUnmounted(stopCountdown)
</script>

<template>
  <v-main class="recovery-page">
    <AppTopbar action-text="Voltar para acessar" :action-to="{ name: 'login' }" />
    <v-container fluid class="recovery-container">
      <v-card class="recovery-card" rounded="xl" elevation="10">
        <div class="recovery-icon">
          <v-icon :icon="step === 'success' ? mdiCheckCircleOutline : mdiLockReset" size="42" />
        </div>
        <p class="eyebrow">Redefinir a senha</p>
        <h1 v-if="step === 'success'">{{ title }}</h1>

        <template v-if="step === 'email'">
          <h1 class="step-title">Informe o e-mail utilizado no Account</h1>
          <v-alert class="eligibility-info" color="blue" variant="tonal" rounded="lg" border="start" :icon="mdiInformationOutline">
            Se houver uma conta associada a este e-mail, enviaremos um código válido por 2 minutos.
          </v-alert>
          <v-form ref="emailFormRef" class="recovery-form" @submit.prevent="requestCode">
            <EmailField v-model="email"
                        :rules="rules.email"
                        label="Endereço de e-mail"
                        placeholder="exemplo@email.com"
                        class="access-field"
                        density="comfortable"
                        clearable />
            <v-alert v-if="activeCodeNotice"
                     class="active-code-notice"
                     type="info"
                     variant="tonal"
                     density="compact">
              Já existe um código válido enviado para o e-mail informado. Não é possível enviar outro código enquanto ele estiver ativo.
            </v-alert>
            <v-btn v-if="activeCodeNotice"
                   rounded="pill"
                   size="large"
                   class="primary-action existing-code-button"
                   type="button"
                   @click="returnToActiveCode">
              Confirmar código
            </v-btn>
            <v-btn v-else
                   rounded="pill"
                   size="large"
                   class="primary-action send-code-button"
                   type="submit"
                   :loading="loading">
              Enviar código
            </v-btn>
          </v-form>
        </template>

        <template v-else-if="step === 'code'">
          <h1 class="step-title">Confirme o código</h1>
          <p class="lead code-instruction">Digite o código de seis dígitos enviado para o endereço informado.</p>
          <p class="countdown" :class="{ 'countdown--critical': countdownCritical || !countdownActive }" aria-live="polite">
            <span>Tempo restante:</span>
            <strong>{{ formattedCountdown }}</strong>
          </p>
          <p class="attempt-count" aria-live="polite">
            <span>Tentativas de redefinição:</span>
            <strong>{{ recoveryAttemptCount }}/{{ MAXIMUM_RECOVERY_ATTEMPTS }}</strong>
          </p>
          <v-form class="code-form" @submit.prevent="verifyCode">
            <div class="code-inputs" role="group" aria-label="Código de confirmação com seis dígitos">
              <input v-for="(_, index) in codeDigits"
                     :key="index"
                     :value="codeDigits[index]"
                     :data-code-index="index"
                     :aria-label="`Dígito ${index + 1} do código`"
                     :autocomplete="index === 0 ? 'one-time-code' : 'off'"
                     class="code-input"
                     inputmode="numeric"
                     pattern="[0-9]*"
                     maxlength="1"
                     @input="handleCodeInput(index, $event)"
                     @paste="handleCodePaste(index, $event)"
                     @keydown="handleCodeKeydown(index, $event)" />
            </div>
            <v-btn rounded="pill"
                   size="large"
                   class="primary-action confirm-code-button"
                   type="submit"
                   :loading="loading"
                   :disabled="loading || !codeComplete || !countdownActive">
              Confirmar código
            </v-btn>
            <v-btn variant="text"
                   class="resend-code-button"
                   :disabled="loading || countdownActive"
                   @click="resend">
              Reenviar código
            </v-btn>
          </v-form>
        </template>

        <template v-else-if="step === 'password'">
          <h1 class="step-title">Crie uma nova senha</h1>
          <v-form ref="passwordFormRef" class="password-form" @submit.prevent="changePassword">
            <v-row class="password-row">
              <v-col cols="12" md="6">
                <PasswordField v-model="newPassword"
                               :rules="rules.password"
                               label="Senha"
                               autocomplete="new-password"
                               class="access-field"
                               density="comfortable"
                               clearable />
              </v-col>
              <v-col cols="12" md="6">
                <PasswordField v-model="confirmPassword"
                               label="Confirmar senha"
                               autocomplete="new-password"
                               :match="newPassword"
                               class="access-field"
                               density="comfortable"
                               clearable />
              </v-col>
            </v-row>
            <PasswordRequirements :rules="passwordChecklist" />
            <v-btn rounded="pill"
                   size="large"
                   class="primary-action change-password-button"
                   type="submit"
                   :loading="loading">
              Alterar senha
            </v-btn>
          </v-form>
        </template>

        <template v-else>
          <p class="lead">Sua senha foi alterada com segurança. As sessões anteriores serão invalidadas e um aviso será enviado ao seu e-mail.</p>
          <v-btn block rounded="pill" size="large" class="primary-action" :to="{ name: 'login' }">Acessar Account</v-btn>
        </template>

        <v-alert v-if="feedback" class="feedback-alert" type="info" variant="tonal" density="compact">{{ feedback }}</v-alert>
        <v-btn v-if="step === 'code'"
               class="back-link"
               variant="text"
               :prepend-icon="mdiArrowLeft"
               @click="returnToEmail">
          Voltar
        </v-btn>
        <v-btn v-else-if="step !== 'success'"
               class="back-link"
               variant="text"
               :prepend-icon="mdiArrowLeft"
               :to="{ name: 'login' }">
          Voltar
        </v-btn>
      </v-card>
    </v-container>
    <AppFooter copyright="© 2026 YaeaY Software ®" text-one="Privacidade" :to-one="{ name: 'privacy' }" text-two="Termos" :to-two="{ name: 'terms' }" text-three="Segurança" :to-three="{ name: 'security' }" />
  </v-main>
</template>

<style scoped>
.recovery-page { min-height: 100vh; background: #ebebeb; display: flex; flex-direction: column; }
.recovery-container { flex: 1; width: 100%; max-width: none; display: flex; align-items: safe center; justify-content: center; padding: 40px 20px; }
.recovery-card { width: min(100%, 720px); padding: clamp(28px, 6vw, 52px); text-align: center; color: #183729; background: rgba(255, 255, 255, 0.96); border: 1px solid rgba(24, 55, 41, 0.06); box-shadow: 0 32px 80px rgba(24, 55, 41, 0.08) !important; }
.recovery-icon { width: 76px; height: 76px; margin: 0 auto 18px; border-radius: 22px; display: grid; place-items: center; color: #176b46; background: #e3f5eb; }
.eyebrow { margin: 0 0 14px; color: #3e564f; font-weight: 800; letter-spacing: .2em; text-transform: uppercase; font-size: .78rem; }
h1 { margin: 0; color: #183729; font-size: clamp(1.8rem, 5vw, 2.4rem); }
.lead { margin: 16px auto 28px; color: #5c6963; line-height: 1.6; }
.step-title { max-width: 620px; margin: 6px auto 24px; color: #183729; font-size: clamp(2rem, 5vw, 3rem); font-weight: 800; letter-spacing: -.04em; line-height: 1.1; }
.eligibility-info { margin-bottom: 24px; font-size: .82rem; text-align: left; }
.eligibility-info :deep(.v-alert__content) { white-space: nowrap; }
.eligibility-info :deep(.v-alert__prepend) { margin-inline-end: 12px; }
.recovery-form { width: 100%; }
.access-field { color: #183729; }
.access-field :deep(.v-field) { box-shadow: none; }
.access-field :deep(.v-label) { color: #424844; font-size: .72rem; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; }
.access-field :deep(.v-field__input) { min-height: 56px; color: #183729; padding-inline-start: 18px; }
.primary-action { background: #183729; color: #ebebeb; font-weight: 800; text-transform: none; letter-spacing: .02em; box-shadow: 0 14px 28px rgba(24, 55, 41, .18); }
.send-code-button { display: flex; width: min(100%, 300px); margin: 18px auto 0; }
.active-code-notice { margin-top: 20px; text-align: left; }
.existing-code-button { display: flex; width: min(100%, 300px); margin: 22px auto 0; }
.code-instruction { margin-bottom: 12px; }
.countdown { display: flex; align-items: baseline; justify-content: center; gap: 8px; margin: 0 0 24px; color: #3e564f; font-size: 1rem; }
.countdown strong { min-width: 52px; color: #183729; font-size: 1.1rem; font-variant-numeric: tabular-nums; }
.countdown--critical, .countdown--critical strong { color: #c62828; }
.attempt-count { display: flex; align-items: baseline; justify-content: center; gap: 8px; margin: -14px 0 24px; color: #3e564f; font-size: .92rem; }
.attempt-count strong { color: #183729; font-variant-numeric: tabular-nums; }
.code-form { display: flex; flex-direction: column; align-items: center; }
.code-inputs { display: grid; grid-template-columns: repeat(6, 64px); justify-content: center; gap: 16px; width: 100%; margin-bottom: 28px; }
.code-input { width: 64px; height: 70px; color: #183729; background: #fff; border: 1px solid #aeb7b2; border-radius: 7px; outline: none; font: inherit; font-size: 1.6rem; font-weight: 800; text-align: center; caret-color: #183729; transition: border-color .2s, box-shadow .2s; }
.code-input:focus { border-color: #183729; box-shadow: 0 0 0 3px rgba(24, 55, 41, .14); }
.confirm-code-button { width: min(100%, 300px); }
.resend-code-button { margin-top: 10px; }
.password-form { width: 100%; text-align: left; }
.password-row { margin-top: 0; }
.password-form :deep(.password-requirements) { margin-top: 0; }
.change-password-button { display: flex; width: min(100%, 300px); margin: 28px auto 0; }
.feedback-alert { margin: 30px 0 12px; text-align: left; }
.back-link { margin-top: 18px; color: #5f6a65; }

@media (max-width: 600px) {
  .recovery-container { padding: 28px 16px; }
  .recovery-card { padding: 32px 20px; }
  .eligibility-info { font-size: .8rem; }
  .eligibility-info :deep(.v-alert__content) { white-space: normal; }
  .code-inputs { grid-template-columns: repeat(6, minmax(40px, 1fr)); gap: 8px; }
  .code-input { width: 100%; height: 58px; }
}
</style>
