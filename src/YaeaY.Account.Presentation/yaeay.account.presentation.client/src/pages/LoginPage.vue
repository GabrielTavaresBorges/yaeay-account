<!-- src/pages/LoginPage.vue -->
<script setup lang="ts">
  import { reactive, ref } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import {
    mdiLogin,
    mdiArrowRight,
    mdiHelpCircleOutline,
  } from '@mdi/js'
  import AppTopbar from '@/components/layout/AppTopbar.vue'
  import AppFooter from '@/components/layout/AppFooter.vue'
  import { EmailField, PasswordField } from '@/components/inputs'
  import { rules } from '@/validators'
  import { login } from '@/services/authentication-service'
  import type { ApiError } from '@/services/http/api-error'

  type VForm = { validate: () => Promise<{ valid: boolean }> }

  const formRef = ref<VForm | null>(null)
  const route = useRoute()
  const router = useRouter()
  const rememberMe = ref<boolean>(false)
  const email = ref('')
  const password = ref('')
  const loading = ref(false)
  const feedback = reactive({ show: false, text: '', success: false })

  function loginErrorMessage(error: ApiError): string {
    switch (error.identifier) {
      case 'identity.credentials.invalid': return 'E-mail ou senha inválidos.'
      case 'identity.account.locked-out': return 'Acesso temporariamente bloqueado. Tente novamente mais tarde.'
      case 'user.login.email-confirmation-required': return 'Confirme seu e-mail antes de acessar o Account.'
      case 'user.login.account-suspended': return 'Seu Account está suspenso.'
      case 'user.login.account-disabled': return 'Seu Account está desabilitado.'
      default: return 'Não foi possível acessar o Account. Tente novamente.'
    }
  }

  async function submitLogin(): Promise<void> {
    const validation = await formRef.value?.validate()
    if (!validation?.valid) return

    loading.value = true
    feedback.show = false

    try {
      await login({
        emailAddress: email.value,
        password: password.value,
        rememberMe: rememberMe.value,
      })

      const redirect = typeof route.query.redirect === 'string'
        && route.query.redirect.startsWith('/')
        && !route.query.redirect.startsWith('//')
        ? route.query.redirect
        : '/home'

      await router.replace(redirect)
    } catch (error) {
      feedback.text = loginErrorMessage(error as ApiError)
      feedback.success = false
      feedback.show = true
    } finally {
      loading.value = false
    }
  }
</script>

<template>
  <v-main class="login-page">
    <section class="login-shell">
      <AppTopbar action-text="Ajuda" action-to="/help" />
      <v-container fluid class="login-content">
        <v-row class="login-row" justify="center" align="center">
          <v-col cols="12" class="login-column">
            <div class="login-grid">
              <section class="hero-panel">
                <div class="hero-panel__content">
                  <header class="hero-panel__header">
                    <h1 class="hero-title">
                      <span class="hero-title__strong">YaeaY</span>
                      <span class="hero-title__light">Account</span>
                    </h1>

                    <div class="hero-slogan">
                      <p>Uma conta.</p>
                      <p>Um ecossistema.</p>
                      <p class="hero-slogan__highlight">
                        Uma experiência simples, rápida e segura.
                      </p>
                    </div>
                  </header>

                  <div class="hero-cta desktop-only">
                    <p class="hero-cta__text">
                      Quer explorar nossos serviços?<br>
                      Junte-se a nós e simplifique sua rotina.<br>
                      Crie seu acesso gratuito em poucos segundos.
                    </p>

                    <v-btn size="large"
                           variant="flat"
                           class="hero-cta__button"
                           :append-icon="mdiArrowRight"
                           :to="{ name: 'user-create' }">
                      Criar conta
                    </v-btn>
                  </div>
                </div>
              </section>

              <section class="form-panel-wrapper">
                <div class="form-panel__blur form-panel__blur--top" />
                <div class="form-panel__blur form-panel__blur--bottom" />

                <v-card class="form-panel"
                        rounded="xl"
                        elevation="0">
                  <h2 class="form-panel__title">
                    Acessar
                  </h2>

                  <v-form ref="formRef" class="form-panel__form" @submit.prevent="submitLogin">
                    <EmailField v-model="email"
                                :rules="rules.email"
                                label="Email"
                                placeholder="nome@exemplo.com"
                                autocomplete="username"
                                density="comfortable" />

                    <PasswordField v-model="password"
                                   :rules="rules.password"
                                   label="Senha"
                                   placeholder="••••••••"
                                   autocomplete="current-password"
                                   density="comfortable" />

                    <div class="form-panel__options">
                      <v-checkbox v-model="rememberMe"
                                  label="Lembrar-me"
                                  density="compact"
                                  hide-details
                                  class="remember-checkbox" />

                      <v-btn variant="text"
                             class="forgot-link"
                             :prepend-icon="mdiHelpCircleOutline"
                             :ripple="false"
                             to="/forgot-password">
                        Esqueci minha senha
                      </v-btn>
                    </div>

                    <v-alert v-if="feedback.show"
                             :type="feedback.success ? 'success' : 'error'"
                             variant="tonal"
                             density="compact">
                      {{ feedback.text }}
                    </v-alert>

                    <v-btn block
                           size="x-large"
                           rounded="pill"
                           class="login-button"
                           :prepend-icon="mdiLogin"
                           type="submit"
                           :loading="loading"
                           :disabled="loading">
                      Entrar
                    </v-btn>
                  </v-form>

                  <div class="mobile-cta mobile-only">
                    <p class="mobile-cta__text">
                      Quer explorar nossos serviços?<br>
                      Crie seu acesso gratuito em poucos segundos.
                    </p>

                    <v-btn block
                           size="large"
                           variant="flat"
                           tile
                           class="mobile-cta__button"
                           :to="{ name: 'user-create' }">
                      Criar conta
                    </v-btn>
                  </div>
                </v-card>

                <div class="tech-stripe" aria-hidden="true">
                  <span class="tech-stripe__item tech-stripe__item--primary" />
                  <span class="tech-stripe__item tech-stripe__item--secondary" />
                  <span class="tech-stripe__item tech-stripe__item--muted" />
                </div>
              </section>
            </div>
          </v-col>
        </v-row>
      </v-container>

      <AppFooter copyright="© 2026 YaeaY Software ®"
                 text-one="Privacidade"
                 :to-one="{ name: 'privacy' }"
                 text-two="Termos"
                 :to-two="{ name: 'terms' }"
                 text-three="Segurança"
                 :to-three="{ name: 'security' }" />

    </section>
  </v-main>
</template>

<style scoped>
  /* =========================================================
     PAGE / SHELL
     Estrutura base da página de login
  ========================================================= */

  .login-page {
    min-height: 100dvh;
    background: #ebebeb;
  }

  .login-shell {
    min-height: 100dvh;
    display: flex;
    flex-direction: column;
  }

  .login-content {
    flex: 1;
    width: 100%;
    display: flex;
    align-items: center;
    padding-top: 40px;
    padding-bottom: 48px;
  }

  .login-row {
    width: 100%;
    margin-inline: 0;
  }

  /*
    Área total do conteúdo.
    Aumentei de 800px para 1040px para o texto respirar mais à esquerda,
    sem deslocar demais o form.
  */
  .login-column {
    width: 100%;
    max-width: 1120px;
    margin-inline: auto;
  }

  /*
    Layout desktop:
    - Texto/hero à esquerda
    - Form à direita
    - Conjunto centralizado na tela
  */
  .login-grid {
    width: 100%;
    display: grid;
    grid-template-columns: minmax(320px, 380px) minmax(420px, 520px);
    gap: 96px;
    align-items: center;
  }


  /* =========================================================
     HERO / TEXTO DA ESQUERDA
     YaeaY Account + slogan + chamada para criar conta
  ========================================================= */

  .hero-panel {
    width: 100%;
    display: flex;
    justify-content: flex-start;
  }

  .hero-panel__content {
    width: 100%;
    max-width: 560px;
  }

  .hero-title {
    color: #183729;
    line-height: 0.95;
    margin: 0;
  }

  .hero-title__strong {
    display: block;
    font-size: clamp(3.8rem, 8vw, 5.8rem);
    font-weight: 800;
    letter-spacing: -0.06em;
  }

  .hero-title__light {
    display: block;
    margin-top: 8px;
    font-size: clamp(2rem, 4vw, 3.2rem);
    font-weight: 300;
    opacity: 0.82;
  }

  .hero-slogan {
    margin-top: 32px;
    padding-left: 20px;
    border-left: 4px solid #8ea588;
    color: #183729;
    font-size: clamp(1.05rem, 1.8vw, 1.45rem);
    line-height: 1.45;
  }

    .hero-slogan p {
      margin: 0;
    }

  .hero-slogan__highlight {
    font-weight: 600;
  }

  .hero-cta {
    margin-top: 56px;
    max-width: 460px;
  }

  .hero-cta__text {
    margin: 0 0 24px;
    color: #3e564f;
    font-size: 1.05rem;
    line-height: 1.7;
    font-weight: 500;
  }

  .hero-cta__button {
    background: #183729;
    color: #ebebeb;
    text-transform: uppercase;
    letter-spacing: 0.16em;
    font-weight: 800;
    padding-inline: 28px;
  }


  /* =========================================================
     FORM WRAPPER
     Container do card de login + efeitos visuais
  ========================================================= */

  .form-panel-wrapper {
    position: relative;
    width: 100%;
    max-width: 520px;
  }

  .form-panel__blur {
    position: absolute;
    border-radius: 999px;
    filter: blur(48px);
    z-index: 0;
  }

  .form-panel__blur--top {
    top: -36px;
    right: -24px;
    width: 220px;
    height: 220px;
    background: rgba(142, 165, 136, 0.24);
  }

  .form-panel__blur--bottom {
    left: -16px;
    bottom: -28px;
    width: 160px;
    height: 160px;
    background: rgba(62, 86, 79, 0.12);
  }


  /* =========================================================
     FORM CARD
     Card principal de acesso
  ========================================================= */

  .form-panel {
    position: relative;
    z-index: 1;
    width: 100%;
    background: rgba(255, 255, 255, 0.96);
    padding: 36px;
    box-shadow: 0 32px 80px rgba(24, 55, 41, 0.08);
    border: 1px solid rgba(24, 55, 41, 0.06);
  }

  .form-panel__title {
    margin: 0 0 28px;
    font-size: 2rem;
    font-weight: 800;
    color: #183729;
    letter-spacing: -0.03em;
  }

  .form-panel__form {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }


  /* =========================================================
     FORM OPTIONS
     Lembrar-me + Esqueci minha senha
  ========================================================= */

  .form-panel__options {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    flex-wrap: wrap;
  }

  .remember-checkbox {
    margin: 0;
    color: #3e564f;
  }

  :deep(.remember-checkbox .v-label) {
    opacity: 1;
    color: #3e564f;
    font-size: 0.95rem;
    white-space: nowrap;
  }

  .forgot-link {
    color: #183729;
    text-transform: none;
    font-weight: 600;
    letter-spacing: 0;
  }


  /* =========================================================
     BUTTONS
     Botão de login e botão de criar conta
  ========================================================= */

  .login-button {
    margin-top: 4px;
    background: #183729;
    color: #ebebeb;
    font-weight: 800;
    text-transform: none;
    letter-spacing: 0.02em;
    box-shadow: 0 14px 28px rgba(24, 55, 41, 0.18);
  }

  /* =========================================================
     MOBILE CTA
     Chamada para criar conta exibida em telas menores
  ========================================================= */

  .mobile-cta {
    margin-top: 32px;
    padding-top: 24px;
    border-top: 1px solid rgba(24, 55, 41, 0.08);
  }

  .mobile-cta__text {
    margin: 0 0 20px;
    text-align: center;
    color: #3e564f;
    line-height: 1.6;
  }

  .mobile-cta__button {
    background: #183729;
    color: #ebebeb;
    text-transform: uppercase;
    letter-spacing: 0.14em;
    font-weight: 800;
    border-radius: 0;
  }


  /* =========================================================
     TECH STRIPE
     Barrinhas decorativas abaixo do card
  ========================================================= */

  .tech-stripe {
    width: 100%;
    max-width: 520px;
    display: flex;
    gap: 8px;
    margin-top: 20px;
    padding-left: 6px;
  }

  .tech-stripe__item {
    display: inline-block;
    height: 4px;
  }

  .tech-stripe__item--primary {
    width: 48px;
    background: #183729;
  }

  .tech-stripe__item--secondary {
    width: 18px;
    background: #8ea588;
  }

  .tech-stripe__item--muted {
    width: 10px;
    background: #bfc8c4;
  }


  /* =========================================================
     VISIBILITY HELPERS
     Controle de elementos desktop/mobile
  ========================================================= */

  .mobile-only {
    display: none;
  }

  .desktop-only {
    display: block;
  }


  /* =========================================================
     RESPONSIVE - NOTEBOOKS / TELAS MÉDIAS
  ========================================================= */

  @media (max-width: 1264px) {
    .login-column {
      max-width: 960px;
    }

    .login-grid {
      grid-template-columns: minmax(300px, 360px) minmax(400px, 500px);
      gap: 56px;
    }

    .hero-panel__content {
      max-width: 100%;
    }
  }

  /* Em notebooks e monitores, todo o login cabe na área visível. */
  @media (min-width: 961px) and (orientation: landscape) {
    .login-page,
    .login-shell {
      height: 100dvh;
      min-height: 0;
    }

    .login-page {
      overflow: hidden;
    }

    .login-shell {
      /* O v-main já reserva os 65px da AppTopbar fixa. */
      height: calc(100dvh - 65px);
      overflow: visible;
    }

    .login-content {
      min-height: 0;
      padding-top: clamp(14px, 2.5vh, 28px);
      padding-bottom: clamp(14px, 2.5vh, 28px);
    }

    .hero-slogan {
      margin-top: clamp(20px, 3vh, 32px);
    }

    .hero-cta {
      margin-top: clamp(24px, 4vh, 42px);
    }

    .hero-cta__text {
      margin-bottom: 18px;
      line-height: 1.55;
    }

    .form-panel {
      padding: clamp(25px, 4vh, 34px);
    }

    .form-panel__title {
      margin-bottom: clamp(18px, 3vh, 26px);
    }

    .form-panel__form {
      gap: clamp(14px, 2.2vh, 19px);
    }

    .tech-stripe {
      margin-top: 12px;
    }

    :deep(.app-footer__content) {
      padding-top: clamp(14px, 2.3vh, 22px);
      padding-bottom: clamp(14px, 2.3vh, 22px);
    }
  }


  /* =========================================================
     RESPONSIVE - TABLET / PORTRAIT
     Aqui vira coluna única igual ao UserCreatePage
  ========================================================= */

  @media (max-width: 960px), (orientation: portrait) {
    .login-content {
      padding-top: 24px;
      padding-bottom: 32px;
    }

    .login-column {
      max-width: 760px;
    }

    .login-grid {
      display: grid;
      grid-template-columns: 1fr;
      justify-items: center;
      gap: 32px;
    }

    .hero-panel {
      width: 100%;
      display: flex;
      justify-content: center;
      order: 1;
    }

    .hero-panel__content {
      width: 100%;
      max-width: 720px;
      margin-inline: auto;
      text-align: center;
    }

    .hero-title,
    .hero-slogan,
    .hero-cta,
    .mobile-cta {
      text-align: center;
    }

    .hero-slogan {
      border-left: none;
      padding-left: 0;
    }

    .form-panel-wrapper {
      order: 2;
      width: 100%;
      max-width: 720px;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .form-panel {
      width: 100%;
      max-width: 720px;
      margin-inline: auto;
    }

    .tech-stripe {
      justify-content: center;
      padding-left: 0;
    }

    .desktop-only {
      display: none;
    }

    .mobile-only {
      display: block;
    }

    .footer-bar__content {
      flex-direction: column;
      align-items: center;
      gap: 16px;
    }

    .footer-bar__links {
      margin-left: 0;
      justify-content: center;
      flex-wrap: wrap;
      gap: 16px;
    }
  }


  /* =========================================================
     RESPONSIVE - MOBILE
  ========================================================= */

  @media (max-width: 600px) {
    .form-panel {
      padding: 24px;
    }

    .form-panel__options {
      flex-direction: column;
      align-items: flex-start;
    }

    .hero-slogan {
      margin-top: 24px;
    }
  }
</style>
