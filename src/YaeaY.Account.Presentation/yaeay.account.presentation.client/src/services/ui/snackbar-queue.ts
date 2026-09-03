import { ref } from 'vue'
import type { SnackbarQueueMessage } from 'vuetify'

export type SnackbarType = 'success' | 'info' | 'error' | 'warning'

export const snackbarMessages = ref<SnackbarQueueMessage[]>([])

export function enqueueSnackbar(message: string, type: SnackbarType): void {
  snackbarMessages.value.push({
    text: message,
    color: type,
    timeout: 6000,
  })
}
