<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import type { ApiError } from '@/services/http/api-error'
import {
  mdiAlertCircleOutline,
  mdiCheckCircleOutline,
  mdiEmailCheckOutline,
  mdiLockOutline,
} from '@mdi/js'
import {
  confirmEmail,
  getEmailConfirmationPreview,
} from '@/services/email-confirmation-service'
import StageEnvironmentBanner from '@/components/layout/StageEnvironmentBanner.vue'

type ConfirmationState =
  | 'loading'
  | 'ready'
  | 'submitting'
  | 'success'
  | 'already-confirmed'
  | 'error'

const alreadyConfirmedErrorCode = 'user.email.already-confirmed'

const state = ref<ConfirmationState>('loading')
const rawToken = ref('')
const maskedEmail = ref('')

const isSubmitting = computed(() => state.value === 'submitting')

const icon = computed(() => {
  if (state.value === 'success') return mdiCheckCircleOutline
  if (state.value === 'already-confirmed' || state.value === 'error') {
    return mdiAlertCircleOutline
  }
  return mdiEmailCheckOutline
})

const title = computed(() => {
  if (state.value === 'success') return 'E-mail confirmado'
  if (state.value === 'already-confirmed') return 'E-mail já foi confirmado'
  if (state.value === 'error') return 'Não foi possível confirmar'
  return 'Confirme seu e-mail'
})

const description = computed(() => {
  if (state.value === 'already-confirmed') {
    return 'Não é possível confirmar novamente.'
  }

  if (state.value === 'error') {
    return 'Este link é inválido ou não está mais disponível.'
  }

  return ''
})

onMounted(async () => {
  const fragment = window.location.hash.startsWith('#')
    ? window.location.hash.slice(1)
    : window.location.hash

  const token = new URLSearchParams(fragment).get('token')?.trim() ?? ''

  window.history.replaceState(
    null,
    document.title,
    `${window.location.pathname}${window.location.search}`,
  )

  if (!token) {
    state.value = 'error'
    return
  }

  rawToken.value = token

  try {
    const preview = await getEmailConfirmationPreview(token)
    maskedEmail.value = preview.maskedEmail
    state.value = 'ready'
  } catch (error: unknown) {
    rawToken.value = ''
    maskedEmail.value = ''
    state.value = resolveFailureState(error)
  }
})

async function submitConfirmation() {
  if (!rawToken.value || state.value !== 'ready') return

  state.value = 'submitting'

  try {
    await confirmEmail(rawToken.value)
    rawToken.value = ''
    state.value = 'success'
  } catch (error: unknown) {
    rawToken.value = ''
    state.value = resolveFailureState(error)
  }
}

function resolveFailureState(error: unknown): ConfirmationState {
  if (isApiError(error) && error.identifier) {
    if (error.identifier === alreadyConfirmedErrorCode) {
      return 'already-confirmed'
    }
  }

  return 'error'
}

function isApiError(error: unknown): error is ApiError {
  return typeof error === 'object' && error !== null && 'statusCode' in error
}

function closePage() {
  window.close()
}
</script>

<template>
  <v-main class="confirmation-page">
    <div class="confirmation-shell">
      <header class="confirmation-header">
        <div class="brand" aria-label="YaeaY Account">
          <span class="brand__strong">YaeaY</span>
          <span class="brand__light">Account</span>
        </div>

        <StageEnvironmentBanner class="confirmation-header__stage-banner" />
      </header>

      <main class="confirmation-content">
        <div class="confirmation-card-wrapper">
          <div class="confirmation-glow confirmation-glow--top" />
          <div class="confirmation-glow confirmation-glow--bottom" />

          <section class="confirmation-card" aria-live="polite">
            <v-icon :icon="icon" class="confirmation-icon" size="76" />

            <p class="confirmation-eyebrow">
              Confirmação de e-mail
            </p>

            <h1 class="confirmation-title">
              {{ title }}
            </h1>

            <p
              v-if="description"
              class="confirmation-description"
            >
              {{ description }}
            </p>

            <p
              v-if="state === 'already-confirmed'"
              class="confirmation-invalid-link"
            >
              Link inválido
            </p>

            <p
              v-if="state === 'ready' || state === 'submitting'"
              class="confirmation-masked-email"
              aria-label="Endereço de e-mail mascarado"
            >
              {{ maskedEmail }}
            </p>

            <div
              v-if="state === 'success'"
              class="confirmation-success-actions"
            >
              <RouterLink
                :to="{ name: 'login' }"
                class="confirmation-login-button"
              >
                ACESSAR ACCOUNT
              </RouterLink>

              <button
                type="button"
                class="confirmation-close-button"
                @click="closePage"
              >
                Fechar página
              </button>
            </div>

            <button
              v-if="state === 'ready' || state === 'submitting'"
              type="button"
              class="confirmation-button"
              :disabled="isSubmitting"
              @click="submitConfirmation"
            >
              {{ isSubmitting ? 'CONFIRMANDO...' : 'CONFIRMAR MEU E-MAIL' }}
            </button>

            <div
              v-if="state === 'ready' || state === 'submitting'"
              class="confirmation-security"
            >
              <v-icon :icon="mdiLockOutline" size="18" />
              <span>Ambiente protegido</span>
            </div>

            <div
              v-if="state === 'already-confirmed'"
              class="confirmation-security confirmation-security--already-confirmed"
            >
              <v-icon :icon="mdiLockOutline" size="18" />
              <span>Ambiente seguro</span>
            </div>
          </section>
        </div>
      </main>

      <footer class="confirmation-footer">
        <span class="confirmation-footer__company">YaeaY Software</span>
        <span aria-hidden="true">·</span>
        <span>Account</span>
      </footer>
    </div>
  </v-main>
</template>

<style scoped>
.confirmation-page {
  min-height: 100vh;
  background: #ebebeb;
  color: #183729;
}

.confirmation-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.confirmation-header {
  position: relative;
  min-height: 68px;
  display: flex;
  align-items: center;
  padding: 0 32px;
  background: rgba(255, 255, 255, 0.96);
  border-bottom: 1px solid rgba(24, 55, 41, 0.08);
}

.confirmation-header__stage-banner {
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
}

.brand {
  display: flex;
  align-items: baseline;
  gap: 5px;
  color: #183729;
}

.brand__strong {
  font-size: 1.65rem;
  font-weight: 800;
  letter-spacing: -0.05em;
}

.brand__light {
  font-size: 1.4rem;
  font-weight: 300;
  opacity: 0.82;
}

.confirmation-content {
  flex: 1;
  display: grid;
  place-items: center;
  width: 100%;
  padding: 48px 20px;
}

.confirmation-card-wrapper {
  position: relative;
  width: 100%;
  max-width: 600px;
}

.confirmation-glow {
  position: absolute;
  border-radius: 999px;
  filter: blur(52px);
}

.confirmation-glow--top {
  top: -42px;
  right: -28px;
  width: 230px;
  height: 230px;
  background: rgba(142, 165, 136, 0.24);
}

.confirmation-glow--bottom {
  bottom: -32px;
  left: -24px;
  width: 180px;
  height: 180px;
  background: rgba(62, 86, 79, 0.12);
}

.confirmation-card {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
  padding: 48px 56px;
  text-align: center;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(24, 55, 41, 0.06);
  border-radius: 28px;
  box-shadow: 0 32px 80px rgba(24, 55, 41, 0.08);
}

.confirmation-icon {
  margin-bottom: 22px;
  color: #183729;
}

.confirmation-eyebrow {
  margin: 0 0 18px;
  color: #3e564f;
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0.2em;
  text-transform: uppercase;
}

.confirmation-title {
  margin: 0;
  color: #183729;
  font-size: clamp(2rem, 5vw, 3rem);
  font-weight: 800;
  letter-spacing: -0.04em;
  line-height: 1.1;
}

.confirmation-description {
  max-width: 440px;
  margin: 24px 0 0;
  color: #3e564f;
  font-size: 1.05rem;
  line-height: 1.7;
}

.confirmation-invalid-link {
  margin: 6px 0 0;
  color: #707875;
  font-size: 0.98rem;
  line-height: 1.5;
}

.confirmation-masked-email {
  margin: 24px 0 0;
  color: #4f5753;
  font-size: 1.08rem;
  font-weight: 600;
  line-height: 1.5;
}

.confirmation-button {
  width: 100%;
  margin-top: 32px;
  padding: 18px 28px;
  color: #ffffff;
  background: #183729;
  border: 0;
  border-radius: 999px;
  box-shadow: 0 14px 28px rgba(24, 55, 41, 0.18);
  cursor: pointer;
  font: inherit;
  font-size: 0.9rem;
  font-weight: 800;
  letter-spacing: 0.12em;
}

.confirmation-button:focus-visible {
  outline: 3px solid #8ea588;
  outline-offset: 4px;
}

.confirmation-button:disabled {
  cursor: wait;
  opacity: 0.72;
}

.confirmation-success-actions {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
  margin-top: 32px;
}

.confirmation-login-button {
  width: min(100%, 400px);
  padding: 18px 28px;
  color: #ffffff;
  background: #183729;
  border-radius: 999px;
  box-shadow: 0 14px 28px rgba(24, 55, 41, 0.18);
  font-size: 0.9rem;
  font-weight: 800;
  letter-spacing: 0.12em;
  text-align: center;
  text-decoration: none;
}

.confirmation-login-button:focus-visible {
  outline: 3px solid #8ea588;
  outline-offset: 4px;
}

.confirmation-close-button {
  margin-top: 20px;
  padding: 4px 8px;
  color: #8a918e;
  background: transparent;
  border: 0;
  cursor: pointer;
  font: inherit;
  font-size: 0.95rem;
}

.confirmation-close-button:hover {
  color: #6c7d76;
}

.confirmation-close-button:focus-visible {
  outline: 2px solid #8ea588;
  outline-offset: 3px;
}

.confirmation-security {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 24px;
  color: #6c7d76;
  font-size: 0.88rem;
}

.confirmation-security--already-confirmed {
  margin-top: 24px;
}

.confirmation-footer {
  min-height: 74px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: #6c7d76;
  font-size: 0.9rem;
}

.confirmation-footer__company {
  color: #183729;
  font-weight: 800;
}

@media (max-width: 600px) {
  .confirmation-header {
    min-height: 112px;
    display: grid;
    grid-template-rows: 48px 56px;
    padding: 0 20px 8px;
  }

  .confirmation-header__stage-banner {
    position: static;
    grid-row: 2;
    justify-self: center;
    transform: none;
  }

  .confirmation-content {
    padding: 28px 16px;
  }

  .confirmation-card {
    padding: 38px 24px;
    border-radius: 22px;
  }

  .confirmation-security {
    align-items: flex-start;
    text-align: left;
  }
}
</style>
