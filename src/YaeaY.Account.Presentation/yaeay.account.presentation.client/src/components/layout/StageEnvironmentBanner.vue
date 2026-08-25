<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getRuntimeConfiguration } from '@/services/runtime-configuration-service'

const emit = defineEmits<{
  visibilityChange: [isVisible: boolean]
}>()

const isVisible = ref(false)

onMounted(async () => {
  try {
    const configuration = await getRuntimeConfiguration()
    isVisible.value = configuration.showTestModeBanner
  } catch {
    isVisible.value = false
  }

  emit('visibilityChange', isVisible.value)
})
</script>

<template>
  <v-alert
    v-if="isVisible"
    class="stage-environment-banner"
    density="compact"
    title="Ambiente STAGE"
    text="Modo para testes e homologação"
    type="warning"
  />
</template>

<style scoped>
.stage-environment-banner {
  width: max-content;
  max-width: 100%;
}

:deep(.stage-environment-banner .v-alert-title) {
  font-size: 0.8rem;
  font-weight: 800;
}

:deep(.stage-environment-banner .v-alert__content) {
  font-size: 0.72rem;
  line-height: 1.2;
}

</style>
