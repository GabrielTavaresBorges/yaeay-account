<!-- src/components/layout/AppTopbar.vue -->

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import type { RouteLocationRaw } from 'vue-router'
  import { mdiAlertCircleOutline } from '@mdi/js'
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

  const { smAndDown } = useDisplay()
  const showTestModeBanner = ref(false)
  const testModeBannerText = ref('MODO DE TESTES - HOMOLOGAÇÃO')
  const topbarHeight = computed(() => showTestModeBanner.value && smAndDown.value ? 96 : 65)

  onMounted(async () => {
    try {
      const configuration = await getRuntimeConfiguration()
      showTestModeBanner.value = configuration.showTestModeBanner
      testModeBannerText.value = configuration.testModeBannerText
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

      <v-chip v-if="showTestModeBanner"
              class="app-topbar__environment-banner"
              color="error"
              variant="tonal"
              size="small"
              :prepend-icon="mdiAlertCircleOutline">
        {{ testModeBannerText }}
      </v-chip>

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
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .app-topbar__environment-banner {
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
    font-size: 0.72rem;
    font-weight: 800;
    letter-spacing: 0.08em;
  }

  .app-topbar__brand {
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
    color: #3e564f;
    text-transform: none;
    letter-spacing: 0.01em;
    font-weight: 500;
    flex-shrink: 0;
  }

  @media (max-width: 600px), (orientation: portrait) {
    .app-topbar__content {
      padding-left: 16px;
      padding-right: 16px;
    }

    .app-topbar--test-mode .app-topbar__content {
      display: grid;
      grid-template-columns: 1fr auto;
      grid-template-rows: 48px 32px;
      padding-top: 0;
      padding-bottom: 8px;
    }

    .app-topbar--test-mode .app-topbar__environment-banner {
      position: static;
      grid-column: 1 / -1;
      grid-row: 2;
      justify-self: center;
      transform: none;
    }
  }
</style>
