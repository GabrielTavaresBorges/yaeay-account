<!-- src/pages/LoginPage.vue -->
<script setup lang="ts">
  import { ref } from 'vue'
  import {
    mdiEye,
    mdiEyeOff,
    mdiLogin,
    mdiArrowRight,
    mdiHelpCircleOutline,
  } from '@mdi/js'

  const showPassword = ref<boolean>(false)
  const rememberMe = ref<boolean>(false)
</script>

<template>
  <v-main class="login-page">
    <section class="login-shell">
      <div class="topbar">
        <div class="topbar__content">
          <div class="brand-mini">
            <span class="brand-mini__strong">YaeaY</span>
            <span class="brand-mini__light">Account</span>
          </div>

          <v-btn variant="text"
                 class="topbar__help"
                 :ripple="false"
                 to="/forgot-password">
            Ajuda
          </v-btn>
        </div>
      </div>

      <v-container class="login-content">
        <v-row class="ma-0" align="center" justify="center">
          <v-col cols="12" lg="11" xl="10">
            <div class="login-grid">
              <section class="hero-panel">
                <div class="hero-panel__content">
                  <header class="hero-panel__header">
                    <h1 class="hero-title">
                      <span class="hero-title__strong">YaeaY</span>
                      <span class="hero-title__light">Account</span>
                    </h1>

                    <div class="hero-slogan">
                      <p>Uma contAaa.</p>
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

                  <v-form class="form-panel__form">
                    <div class="field-block">
                      <label class="field-block__label">
                        Email
                      </label>

                      <v-text-field placeholder="nome@exemplo.com"
                                    type="email"
                                    variant="plain"
                                    density="comfortable"
                                    hide-details
                                    class="login-input" />
                    </div>

                    <div class="field-block">
                      <label class="field-block__label">
                        Senha
                      </label>

                      <v-text-field :type="showPassword ? 'text' : 'password'"
                                    placeholder="••••••••"
                                    variant="plain"
                                    density="comfortable"
                                    hide-details
                                    class="login-input"
                                    :append-inner-icon="showPassword ? mdiEyeOff : mdiEye"
                                    @click:append-inner="showPassword = !showPassword" />
                    </div>

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

                    <v-btn block
                           size="x-large"
                           rounded="pill"
                           class="login-button">
                      <v-icon :icon="mdiLogin" start />
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

      <footer class="footer-bar">
        <div class="footer-bar__content">
          <p class="footer-bar__copy">
            © 2026 YaeaY Software ®
          </p>

          <div class="footer-bar__links">
            <a href="#">Privacidade</a>
            <a href="#">Termos</a>
            <a href="#">Segurança</a>
          </div>
        </div>
      </footer>
    </section>
  </v-main>
</template>

<style scoped>
  .login-page {
    min-height: 100vh;
    background: #ebebeb;
  }

  .login-shell {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }

  .topbar {
    position: sticky;
    top: 0;
    z-index: 10;
    backdrop-filter: blur(16px);
    background: rgba(255, 255, 255, 0.72);
    border-bottom: 1px solid rgba(24, 55, 41, 0.08);
  }

  .topbar__content {
    width: 100%;
    padding: 18px 32px;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .brand-mini {
    display: flex;
    align-items: baseline;
    color: #183729;
    line-height: 1;
    flex-shrink: 0;
  }

  .brand-mini__strong {
    font-size: 1.5rem;
    font-weight: 800;
    letter-spacing: -0.04em;
  }

  .brand-mini__light {
    font-size: 1.5rem;
    font-weight: 300;
    margin-left: 4px;
  }

  .topbar__help {
    color: #3e564f;
    text-transform: none;
    letter-spacing: 0.01em;
    font-weight: 500;
    flex-shrink: 0;
  }

  .login-content {
    flex: 1;
    width: 100%;
    display: flex;
    align-items: center;
    padding-top: 40px;
    padding-bottom: 48px;
  }

  .login-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 48px;
    align-items: center;
  }

  .hero-panel__content {
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
    border-radius: 0;
    padding-inline: 28px;
  }

  .form-panel-wrapper {
    position: relative;
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

  .form-panel {
    position: relative;
    z-index: 1;
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

  .field-block {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .field-block__label {
    padding-left: 4px;
    font-size: 0.74rem;
    font-weight: 800;
    letter-spacing: 0.18em;
    text-transform: uppercase;
    color: #183729;
  }

  :deep(.login-input .v-field) {
    background: #f4f4f4;
    border-radius: 6px;
    box-shadow: none;
    border: none;
    padding-left: 12px;
    padding-right: 12px;
  }

  :deep(.login-input .v-field__input) {
    min-height: 56px;
    color: #183729;
    padding-left: 4px;
    padding-right: 4px;
  }

  :deep(.login-input input::placeholder) {
    color: rgba(24, 55, 41, 0.45);
    opacity: 1;
  }

  :deep(.login-input .v-field__outline) {
    display: none;
  }

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

  .login-button {
    margin-top: 4px;
    background: #183729;
    color: #ebebeb;
    font-weight: 800;
    text-transform: none;
    letter-spacing: 0.02em;
    box-shadow: 0 14px 28px rgba(24, 55, 41, 0.18);
  }

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

  .tech-stripe {
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

  .footer-bar {
    margin-top: auto;
    background: #f9f9f9;
    border-top: 1px solid rgba(24, 55, 41, 0.08);
  }

  .footer-bar__content {
    width: 100%;
    padding: 28px 32px;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .footer-bar__copy {
    margin: 0;
    color: #3e564f;
    font-size: 0.78rem;
    font-weight: 600;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    flex-shrink: 0;
  }

  .footer-bar__links {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 28px;
    margin-left: auto;
    flex-shrink: 0;
  }

    .footer-bar__links a {
      color: #3e564f;
      font-size: 0.78rem;
      font-weight: 600;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      text-decoration: none;
    }

      .footer-bar__links a:hover {
        color: #183729;
      }

  .mobile-only {
    display: none;
  }

  .desktop-only {
    display: block;
  }

  @media (max-width: 1264px) {
    .login-grid {
      gap: 32px;
    }

    .hero-panel__content {
      max-width: 100%;
    }
  }

  @media (max-width: 960px), (orientation: portrait) {
    .login-content {
      padding-top: 24px;
      padding-bottom: 32px;
    }

    .login-grid {
      grid-template-columns: 1fr;
      justify-items: center;
    }

    .hero-panel {
      order: 1;
      width: 100%;
      display: flex;
      justify-content: center;
    }

    .hero-panel__content {
      width: 100%;
      max-width: 720px;
      margin: 0 auto;
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

  @media (max-width: 600px), (orientation: portrait) {
    .topbar__content,
    .footer-bar__content {
      padding-left: 16px;
      padding-right: 16px;
    }

    .topbar__content {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

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
