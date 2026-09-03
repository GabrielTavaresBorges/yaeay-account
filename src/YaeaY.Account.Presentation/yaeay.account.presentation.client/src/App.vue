<script setup lang="ts">
import { ref } from 'vue'
import type { SnackbarQueueMessage } from 'vuetify'
import { snackbarMessages } from '@/services/ui/snackbar-queue'

const snackbarQueue = ref<{ clear: () => void } | null>(null)

function snackbarActionClass(item: SnackbarQueueMessage): string {
  const color = typeof item === 'string' ? undefined : item.color
  return color === 'success' || color === 'error' || color === 'info' || color === 'warning'
    ? `app-snackbar-queue__action--${color}`
    : ''
}
</script>

<template>
  <v-app>
    <router-view />
    <v-snackbar-queue
      ref="snackbarQueue"
      v-model="snackbarMessages"
      :total-visible="5"
      class="app-snackbar-queue"
      location="bottom"
      rounded="pill"
    >
      <template #actions="{ item, props }">
        <v-btn
          v-bind="props"
          :class="[
            'app-snackbar-queue__action',
            snackbarActionClass(item),
          ]"
          size="small"
        >Fechar</v-btn>
      </template>
    </v-snackbar-queue>
  </v-app>
</template>

<style>
.v-snackbar.app-snackbar-queue {
  margin: 28px;
}

.v-snackbar.app-snackbar-queue .v-snackbar__wrapper {
  min-width: min(540px, calc(100vw - 56px));
  min-height: 92px;
  max-width: min(660px, calc(100vw - 56px));
  border-radius: 9999px !important;
  box-shadow: 0 14px 28px rgba(24, 55, 41, .18);
}

.v-snackbar.app-snackbar-queue .v-snackbar__content {
  min-height: 92px;
  padding: 18px 16px 18px 28px;
  font-size: 1.08rem;
  font-weight: 700;
}

.app-snackbar-queue__action {
  min-width: 78px;
  min-height: 38px;
  border-radius: 999px;
  padding-inline: 13px;
  color: #ebebeb;
  margin-left: 18px;
  margin-right: 4px;
  background: #183729;
  font-weight: 800;
  font-size: .82rem;
  letter-spacing: 0;
  text-transform: none;
  box-shadow: none;
}

.app-snackbar-queue__action:hover { filter: brightness(.86); }
.app-snackbar-queue__action:focus-visible { outline: 3px solid #fff; outline-offset: 3px; }

.app-snackbar-queue__action--success { background: #176c3d; }
.app-snackbar-queue__action--error { background: #a61f2d; }
.app-snackbar-queue__action--info { background: #0876bf; }
.app-snackbar-queue__action--warning { background: #9a6700; }

@media (max-width: 560px) {
  .v-snackbar.app-snackbar-queue { margin: 20px; }
  .v-snackbar.app-snackbar-queue .v-snackbar__wrapper { min-width: calc(100vw - 40px); max-width: calc(100vw - 40px); min-height: 78px; border-radius: 32px !important; }
  .v-snackbar.app-snackbar-queue .v-snackbar__content { min-height: 78px; padding: 14px 14px 14px 20px; font-size: .96rem; }
  .app-snackbar-queue__action { min-width: 72px; min-height: 36px; margin-left: 12px; margin-right: 2px; padding-inline: 11px; }
}
</style>
