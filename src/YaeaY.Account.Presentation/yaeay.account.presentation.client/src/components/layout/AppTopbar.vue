<!-- src/components/layout/AppTopbar.vue -->

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import type { RouteLocationRaw } from 'vue-router'
  import { useDisplay } from 'vuetify'
  import { getRuntimeConfiguration } from '@/services/runtime-configuration-service'

  const props = withDefaults(
    defineProps<{
      actionText?: string
      actionTo?: RouteLocationRaw
      showAction?: boolean
    }>(),
    {
      actionText: 'Ajuda',
      actionTo: '/help',
      showAction: true,
    }
  )

  const { width } = useDisplay()
  const showTestModeBanner = ref(false)
  const topbarHeight = computed(() => {
    if (!showTestModeBanner.value) return 65

    return width.value <= 600 ? 112 : 76
  })

  onMounted(async () => {
    try {
      const configuration = await getRuntimeConfiguration()
      showTestModeBanner.value = configuration.showTestModeBanner
    } catch {
      showTestModeBanner.value = false
    }
  })
</script>

<template>
  <v-app-bar class="app-topbar"
             :class="{ 'app-topbar--test-mode': showTestModeBanner }"
             flat
             :height="topbarHeight">
    <div class="app-topbar__content">
      <div class="app-topbar__brand">
        <span class="app-topbar__brand-strong">YaeaY</span>
        <span class="app-topbar__brand-light">Account</span>
      </div>

      <v-alert v-if="showTestModeBanner"
               class="app-topbar__environment-banner"
               density="compact"
               title="Ambiente STAGE"
               text="Modo para testes e homologação"
               type="warning" />

      <v-btn v-if="props.showAction"
             variant="text"
             class="app-topbar__action"
             :ripple="false"
             :to="props.actionTo">
        {{ props.actionText }}
      </v-btn>
    </div>
  </v-app-bar>
</template>

<style scoped>
  .app-topbar {
    backdrop-filter: blur(16px);
    background: rgba(255, 255, 255, 0.72) !important;
    border-bottom: 1px solid rgba(24, 55, 41, 0.08);
  }

  :deep(.app-topbar .v-toolbar__content) {
    padding: 0;
  }

  .app-topbar__content {
    width: 100%;
    height: 100%;
    padding: 18px 32px;
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
    align-items: center;
  }

  .app-topbar__environment-banner {
    grid-column: 2;
    justify-self: center;
    width: max-content;
    max-width: 100%;
  }

  :deep(.app-topbar__environment-banner .v-alert-title) {
    font-size: 0.8rem;
    font-weight: 800;
  }

  :deep(.app-topbar__environment-banner .v-alert__content) {
    font-size: 0.72rem;
    line-height: 1.2;
  }

  .app-topbar--test-mode .app-topbar__content {
    padding-top: 8px;
    padding-bottom: 8px;
  }

  .app-topbar__brand {
    grid-column: 1;
    justify-self: start;
    display: flex;
    align-items: baseline;
    color: #183729;
    line-height: 1;
    flex-shrink: 0;
  }

  .app-topbar__brand-strong {
    font-size: 1.5rem;
    font-weight: 800;
    letter-spacing: -0.04em;
  }

  .app-topbar__brand-light {
    font-size: 1.5rem;
    font-weight: 300;
    margin-left: 4px;
  }

  .app-topbar__action {
    grid-column: 3;
    justify-self: end;
    color: #3e564f;
    text-transform: none;
    letter-spacing: 0.01em;
    font-weight: 500;
    flex-shrink: 0;
  }

  @media (max-width: 600px) {
    .app-topbar__content {
      padding-left: 16px;
      padding-right: 16px;
    }

    .app-topbar--test-mode .app-topbar__content {
      grid-template-columns: 1fr auto;
      grid-template-rows: 48px 48px;
      padding-top: 0;
      padding-bottom: 8px;
    }

    .app-topbar--test-mode .app-topbar__environment-banner {
      grid-column: 1 / -1;
      grid-row: 2;
      justify-self: center;
    }

    .app-topbar--test-mode .app-topbar__brand {
      grid-column: 1;
      grid-row: 1;
    }

    .app-topbar--test-mode .app-topbar__action {
      grid-column: 2;
      grid-row: 1;
    }
  }
</style>
