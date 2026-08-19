<!-- src/components/layout/AppTopbar.vue -->

<script setup lang="ts">
  import type { RouteLocationRaw } from 'vue-router'

  const props = withDefaults(
    defineProps<{
      actionText?: string
      actionTo?: RouteLocationRaw
    }>(),
    {
      actionText: 'Ajuda',
      actionTo: '/help',
    }
  )
</script>

<template>
  <v-app-bar class="app-topbar"
             flat
             height="65">
    <div class="app-topbar__content">
      <div class="app-topbar__brand">
        <span class="app-topbar__brand-strong">YaeaY</span>
        <span class="app-topbar__brand-light">Account</span>
      </div>

      <v-btn variant="text"
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
  }
</style>
