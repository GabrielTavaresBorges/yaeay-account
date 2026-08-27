import { computed, ref } from 'vue'
import { useDisplay } from 'vuetify'

const sidebarPreferenceStorageKey = 'yaeay.account.sidebar.collapsed'

function readStoredPreference(): boolean | null {
  try {
    const storedValue = window.localStorage.getItem(sidebarPreferenceStorageKey)

    if (storedValue === 'true') return true
    if (storedValue === 'false') return false
  } catch {
    // A responsividade continua funcionando quando o armazenamento não está disponível.
  }

  return null
}

export function useSidebarState() {
  const { width } = useDisplay()
  const sidebarPreference = ref<boolean | null>(readStoredPreference())
  const isMobileSidebarOpen = ref(false)
  const usesMobileSidebar = computed(() => width.value <= 900)

  const isSidebarCollapsed = computed(() =>
    usesMobileSidebar.value
      ? !isMobileSidebarOpen.value
      : sidebarPreference.value ?? width.value <= 1200)

  function toggleSidebar(): void {
    if (usesMobileSidebar.value) {
      isMobileSidebarOpen.value = !isMobileSidebarOpen.value
      return
    }

    const nextValue = !isSidebarCollapsed.value
    sidebarPreference.value = nextValue

    try {
      window.localStorage.setItem(sidebarPreferenceStorageKey, String(nextValue))
    } catch {
      // Mantém o estado durante a página atual mesmo sem armazenamento persistente.
    }
  }

  function closeSidebar(): void {
    if (usesMobileSidebar.value) isMobileSidebarOpen.value = false
  }

  return {
    isSidebarCollapsed,
    isMobileSidebarOpen,
    toggleSidebar,
    closeSidebar,
  }
}
