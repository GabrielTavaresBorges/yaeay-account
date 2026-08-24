<script setup lang="ts">
  import type { RouteLocationRaw } from 'vue-router'
  import {
    mdiArrowLeft,
    mdiHelpCircleOutline,
  } from '@mdi/js'
  import AppTopbar from '@/components/layout/AppTopbar.vue'
  import AppFooter from '@/components/layout/AppFooter.vue'

  type InformationPage = 'privacy' | 'terms' | 'security'

  const props = defineProps<{
    page: InformationPage
    eyebrow: string
    title: string
    description: string
    icon: string
    updatedAt: string
  }>()

  const pages: Array<{ id: InformationPage, label: string, to: RouteLocationRaw }> = [
    { id: 'privacy', label: 'Privacidade', to: { name: 'privacy' } },
    { id: 'terms', label: 'Termos', to: { name: 'terms' } },
    { id: 'security', label: 'Segurança', to: { name: 'security' } },
  ]
</script>

<template>
  <v-main class="public-info-page">
    <section class="public-info-shell">
      <AppTopbar action-text="Central de ajuda" :action-to="{ name: 'help' }" />

      <header class="public-info-hero">
        <div class="public-info-container">
          <v-btn class="public-info-back"
                 variant="text"
                 :prepend-icon="mdiArrowLeft"
                 :to="{ name: 'login' }"
                 :ripple="false">
            Voltar para acessar
          </v-btn>

          <div class="public-info-heading">
            <span class="public-info-heading__icon">
              <v-icon :icon="props.icon" size="30" />
            </span>
            <div>
              <p class="public-info-eyebrow">{{ props.eyebrow }}</p>
              <h1>{{ props.title }}</h1>
              <p class="public-info-description">{{ props.description }}</p>
              <p class="public-info-updated">Atualizado em {{ props.updatedAt }}</p>
            </div>
          </div>
        </div>
      </header>

      <div class="public-info-container public-info-content">
        <nav class="public-info-tabs" aria-label="Informações institucionais">
          <v-btn v-for="item in pages"
                 :key="item.id"
                 :to="item.to"
                 :variant="props.page === item.id ? 'flat' : 'text'"
                 :class="{ 'public-info-tabs__active': props.page === item.id }"
                 rounded="pill">
            {{ item.label }}
          </v-btn>
        </nav>

        <div class="public-info-grid">
          <article class="public-info-article">
            <slot />
          </article>

          <aside class="public-info-aside">
            <div class="public-info-aside__card">
              <slot name="summary" />
            </div>

            <div class="public-info-aside__help">
              <v-icon :icon="mdiHelpCircleOutline" size="22" />
              <div>
                <strong>Precisa de orientação?</strong>
                <p>Consulte respostas sobre cadastro, acesso, e-mail e proteção da conta.</p>
                <v-btn variant="text"
                       :to="{ name: 'help' }"
                       :ripple="false">
                  Abrir Central de ajuda
                </v-btn>
              </div>
            </div>
          </aside>
        </div>
      </div>

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
  .public-info-page {
    min-height: 100vh;
    color: #183729;
    background: #f5f8f6;
  }

  .public-info-shell {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }

  .public-info-container {
    width: min(1080px, 100%);
    margin-inline: auto;
    padding-inline: 32px;
  }

  .public-info-hero {
    background:
      radial-gradient(circle at 12% 12%, rgba(110, 167, 135, 0.19), transparent 34%),
      linear-gradient(145deg, #edf5f0 0%, #fafcfb 76%);
    border-bottom: 1px solid rgba(24, 55, 41, 0.08);
  }

  .public-info-back {
    margin-top: 20px;
    margin-left: -14px;
    color: #3e564f;
    text-transform: none;
  }

  .public-info-heading {
    max-width: 830px;
    padding: 38px 0 54px;
    display: flex;
    align-items: flex-start;
    gap: 20px;
  }

  .public-info-heading__icon {
    width: 62px;
    height: 62px;
    display: grid;
    place-items: center;
    flex: 0 0 auto;
    color: #245c43;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.1);
    border-radius: 18px;
    box-shadow: 0 12px 30px rgba(24, 55, 41, 0.08);
  }

  .public-info-eyebrow {
    margin: 2px 0 7px;
    color: #39725a;
    font-size: 0.75rem;
    font-weight: 800;
    letter-spacing: 0.15em;
    text-transform: uppercase;
  }

  .public-info-heading h1 {
    margin: 0;
    color: #173f32;
    font-family: inherit;
    font-size: clamp(2rem, 4vw, 2.65rem);
    font-weight: 800;
    line-height: 1.1;
    letter-spacing: -0.04em;
  }

  .public-info-description {
    max-width: 720px;
    margin: 14px 0 0;
    color: #53675e;
    font-size: 1rem;
    line-height: 1.65;
  }

  .public-info-updated {
    margin: 13px 0 0;
    color: #718078;
    font-size: 0.78rem;
  }

  .public-info-content {
    padding-top: 34px;
    padding-bottom: 62px;
  }

  .public-info-tabs {
    margin-bottom: 28px;
    display: flex;
    align-items: center;
    gap: 6px;
    flex-wrap: wrap;
  }

  .public-info-tabs :deep(.v-btn) {
    color: #3e564f;
    text-transform: none;
    letter-spacing: 0;
  }

  .public-info-tabs :deep(.public-info-tabs__active) {
    color: #ffffff;
    background: #245c43;
  }

  .public-info-grid {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 286px;
    align-items: start;
    gap: 28px;
  }

  .public-info-article {
    display: grid;
    gap: 12px;
  }

  :deep(.info-section) {
    padding: 25px 27px;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.1);
    border-radius: 17px;
  }

  :deep(.info-section__heading) {
    display: flex;
    align-items: center;
    gap: 11px;
  }

  :deep(.info-section__icon) {
    width: 38px;
    height: 38px;
    display: grid;
    place-items: center;
    flex: 0 0 auto;
    color: #245c43;
    background: #edf4ef;
    border-radius: 11px;
  }

  :deep(.info-section h2) {
    margin: 0;
    color: #183729;
    font-size: 1.13rem;
    letter-spacing: -0.015em;
  }

  :deep(.info-section p) {
    margin: 14px 0 0;
    color: #50655b;
    line-height: 1.65;
  }

  :deep(.info-section ul) {
    margin: 14px 0 0;
    padding-left: 21px;
    display: grid;
    gap: 8px;
    color: #435a4f;
    line-height: 1.55;
  }

  :deep(.info-section li::marker) {
    color: #39725a;
  }

  :deep(.info-notice) {
    padding: 18px 20px;
    display: flex;
    align-items: flex-start;
    gap: 11px;
    color: #604b1c;
    background: #fff8e8;
    border-radius: 14px;
    line-height: 1.55;
  }

  :deep(.info-notice strong) {
    display: block;
    margin-bottom: 3px;
  }

  .public-info-aside {
    position: sticky;
    top: 24px;
    display: grid;
    gap: 14px;
  }

  .public-info-aside__card {
    padding: 22px;
    color: #ffffff;
    background: #183729;
    border-radius: 18px;
    box-shadow: 0 18px 42px rgba(24, 55, 41, 0.15);
  }

  .public-info-aside__card :deep(h2) {
    margin: 0;
    color: #ffffff;
    font-size: 1.08rem;
  }

  .public-info-aside__card :deep(p) {
    margin: 9px 0 0;
    color: #c4d7cc;
    font-size: 0.84rem;
    line-height: 1.55;
  }

  .public-info-aside__card :deep(ul) {
    margin: 15px 0 0;
    padding: 0;
    display: grid;
    gap: 10px;
    list-style: none;
  }

  .public-info-aside__card :deep(li) {
    padding-top: 10px;
    color: #edf6f1;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
    font-size: 0.82rem;
    line-height: 1.45;
  }

  .public-info-aside__help {
    padding: 19px;
    display: flex;
    align-items: flex-start;
    gap: 11px;
    color: #245c43;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.1);
    border-radius: 16px;
  }

  .public-info-aside__help strong {
    display: block;
    color: #183729;
  }

  .public-info-aside__help p {
    margin: 5px 0 8px;
    color: #667970;
    font-size: 0.8rem;
    line-height: 1.45;
  }

  .public-info-aside__help :deep(.v-btn) {
    height: auto;
    min-width: 0;
    padding: 0;
    color: #245c43;
    font-size: 0.78rem;
    font-weight: 800;
    text-transform: none;
    letter-spacing: 0;
  }

  @media (max-width: 850px) {
    .public-info-grid {
      grid-template-columns: 1fr;
    }

    .public-info-aside {
      position: static;
      grid-template-columns: 1fr 1fr;
    }
  }

  @media (max-width: 600px) {
    .public-info-container {
      padding-inline: 16px;
    }

    .public-info-back {
      margin-top: 12px;
    }

    .public-info-heading {
      padding: 28px 0 40px;
      gap: 14px;
    }

    .public-info-heading__icon {
      width: 48px;
      height: 48px;
      border-radius: 14px;
    }

    .public-info-heading__icon :deep(.v-icon) {
      font-size: 24px !important;
    }

    .public-info-content {
      padding-top: 24px;
      padding-bottom: 44px;
    }

    .public-info-tabs {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 4px;
    }

    .public-info-tabs :deep(.v-btn) {
      min-width: 0;
      padding-inline: 9px;
    }

    :deep(.info-section) {
      padding: 20px;
    }

    .public-info-aside {
      grid-template-columns: 1fr;
    }
  }
</style>
