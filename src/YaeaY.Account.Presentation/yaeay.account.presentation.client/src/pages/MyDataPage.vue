<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { RouteLocationRaw } from 'vue-router'
import {
  mdiAccountCircleOutline,
  mdiAccountOutline,
  mdiArrowLeft,
  mdiBellOutline,
  mdiCalendarMonthOutline,
  mdiCardAccountDetailsOutline,
  mdiCheck,
  mdiChevronDown,
  mdiChevronLeft,
  mdiChevronRight,
  mdiCloudUploadOutline,
  mdiCogOutline,
  mdiDeleteOutline,
  mdiDeleteSweepOutline,
  mdiEyeOffOutline,
  mdiEyeOutline,
  mdiFileDocumentOutline,
  mdiHomeVariant,
  mdiImageOutline,
  mdiLogoutVariant,
  mdiMapMarkerOutline,
  mdiMenu,
  mdiMenuOpen,
  mdiPhoneOutline,
  mdiPencilOutline,
  mdiPlus,
  mdiShieldCheckOutline,
  mdiStar,
  mdiStarOutline,
  mdiViewGridOutline,
} from '@mdi/js'
import StageEnvironmentBanner from '@/components/layout/StageEnvironmentBanner.vue'
import { CpfField, UserPhonesField } from '@/components/inputs'
import { useSidebarState } from '@/composables/use-sidebar-state'
import { formatCpf, isValidCpf } from '@/validators/fields/cpf'
import type { PhoneModel } from '@/models/phone-model'
import { getPhoneDigitsRange } from '@/services/phoneFormat/phone-format-service'
import {
  getCachedCurrentSession,
  getCurrentSession,
  logout,
  type CurrentSessionResponse,
} from '@/services/authentication-service'

type ProfileSection = 'basic' | 'contact' | 'documents' | 'address'
type DocumentType = 'cpf' | 'rg' | 'driverLicense' | 'passport' | 'voterRegistration' | 'workCard'

interface DocumentImageDraft {
  id: string
  file: File
  previewUrl: string
}

interface UserDocumentDraft {
  id: string
  type: DocumentType
  number: string
  images: DocumentImageDraft[]
}

interface UserPhoneDraft {
  id: string
  phone: PhoneModel
  isPrimary: boolean
}

const MAX_DOCUMENT_IMAGES = 5
const MAX_DOCUMENT_IMAGE_SIZE_BYTES = 5 * 1024 * 1024
const MAX_USER_PHONES = 10
const ACCEPTED_DOCUMENT_IMAGE_TYPES = new Set(['image/jpeg', 'image/png', 'image/webp'])

const documentDefinitions: Array<{
  value: DocumentType
  title: string
  numberLabel: string
  placeholder: string
}> = [
  { value: 'cpf', title: 'CPF', numberLabel: 'Número do CPF', placeholder: '000.000.000-00' },
  { value: 'rg', title: 'RG', numberLabel: 'Número do RG', placeholder: 'Informe o número do RG' },
  { value: 'driverLicense', title: 'Carteira de Habilitação', numberLabel: 'Número de registro', placeholder: 'Informe o número da CNH' },
  { value: 'passport', title: 'Passaporte', numberLabel: 'Número do passaporte', placeholder: 'Informe o número do passaporte' },
  { value: 'voterRegistration', title: 'Título de Eleitor', numberLabel: 'Número do título', placeholder: 'Informe o número do título' },
  { value: 'workCard', title: 'Carteira de Trabalho', numberLabel: 'Número do documento', placeholder: 'Informe o número da carteira' },
]

const route = useRoute()
const router = useRouter()
const {
  isSidebarCollapsed,
  isMobileSidebarOpen,
  toggleSidebar,
  closeSidebar,
} = useSidebarState()
const session = ref<CurrentSessionResponse | null>(getCachedCurrentSession())
const isLoggingOut = ref(false)
const showLogoutError = ref(false)
const showLayoutNotice = ref(false)
const documentImageInput = ref<HTMLInputElement | null>(null)
const replacementImageInput = ref<HTMLInputElement | null>(null)
const documentUploadError = ref('')
const documentFormError = ref('')
const imageViewerDocumentId = ref<string | null>(null)
const imageViewerIndex = ref(0)
const phoneFormError = ref('')
const newPhoneIsPrimary = ref(false)

const profile = reactive({
  fullName: session.value?.fullName ?? '',
  birthDate: '',
  gender: '',
  socialName: '',
  postalCode: '',
  street: '',
  number: '',
  complement: '',
  district: '',
  city: '',
  state: '',
})

const phoneForm = ref<PhoneModel>(createDefaultPhone())
const registeredPhones = ref<UserPhoneDraft[]>([])
const phoneNumberVisibility = reactive<Record<string, boolean>>({})
const phoneVisibilityTimers = new Map<string, ReturnType<typeof setTimeout>>()

const documentForm = reactive<Pick<UserDocumentDraft, 'type' | 'number'>>({
  type: 'cpf',
  number: '',
})

const registeredDocuments = ref<UserDocumentDraft[]>([])
const documentNumberVisibility = reactive<Record<string, boolean>>({})
const documentVisibilityTimers = new Map<string, ReturnType<typeof setTimeout>>()

const availableDocumentDefinitions = computed(() =>
  documentDefinitions.filter((definition) =>
    !registeredDocuments.value.some((document) => document.type === definition.value)))

const selectedDocumentDefinition = computed(() =>
  documentDefinitions.find((document) => document.value === documentForm.type)
    ?? documentDefinitions[0]!)

const imageViewerDocument = computed(() =>
  registeredDocuments.value.find((document) => document.id === imageViewerDocumentId.value) ?? null)

const imageViewerImage = computed(() =>
  imageViewerDocument.value?.images[imageViewerIndex.value] ?? null)

const isImageViewerOpen = computed({
  get: () => imageViewerDocument.value !== null,
  set: (value: boolean) => {
    if (!value) closeImageViewer()
  },
})

const sectionDefinitions = [
  {
    id: 'basic' as const,
    label: 'Dados básicos',
    icon: mdiCardAccountDetailsOutline,
    fields: ['fullName', 'birthDate', 'gender', 'socialName'] as const,
  },
  {
    id: 'contact' as const,
    label: 'Contato',
    icon: mdiPhoneOutline,
    fields: [] as const,
  },
  {
    id: 'documents' as const,
    label: 'Documentos',
    icon: mdiFileDocumentOutline,
    fields: [] as const,
  },
  {
    id: 'address' as const,
    label: 'Endereço',
    icon: mdiMapMarkerOutline,
    fields: ['postalCode', 'street', 'number', 'district', 'city', 'state'] as const,
  },
]

const activeSection = computed<ProfileSection>(() => {
  const value = route.params.section
  return typeof value === 'string'
    && sectionDefinitions.some((section) => section.id === value)
    ? value as ProfileSection
    : 'basic'
})

function completionFor(fields: readonly (keyof typeof profile)[], sectionId: ProfileSection): number {
  if (sectionId === 'contact') {
    return registeredPhones.value.length > 0 ? 100 : 0
  }

  if (sectionId === 'documents') {
    return registeredDocuments.value.length > 0 ? 100 : 0
  }

  const completed = fields.filter((field) => profile[field].trim().length > 0).length
  return Math.round((completed / fields.length) * 100)
}

const sections = computed(() => sectionDefinitions.map((section) => ({
  ...section,
  completion: completionFor(section.fields, section.id),
  to: { name: 'my-data-section', params: { section: section.id } },
})))

const overallCompletion = computed(() => Math.round(
  sections.value.reduce((total, section) => total + section.completion, 0)
    / sections.value.length,
))

const firstName = computed(() =>
  session.value?.fullName.trim().split(/\s+/).at(0) ?? 'Usuário')

const navigationItems = [
  { label: 'Home', icon: mdiHomeVariant, to: { name: 'home' } },
  { label: 'Meus dados', icon: mdiAccountOutline, to: { name: 'my-data-section', params: { section: 'basic' } } },
  { label: 'Apps', icon: mdiViewGridOutline, to: null },
  { label: 'Calendário', icon: mdiCalendarMonthOutline, to: null },
]

async function navigateTo(to: RouteLocationRaw | null): Promise<void> {
  if (!to) return

  closeSidebar()
  await router.push(to)
}

onMounted(async () => {
  session.value ??= await getCurrentSession()
  profile.fullName ||= session.value.fullName
})

async function handleLogout(): Promise<void> {
  if (isLoggingOut.value) return

  isLoggingOut.value = true
  showLogoutError.value = false

  try {
    await logout()
    await router.replace({ name: 'login' })
  } catch {
    showLogoutError.value = true
  } finally {
    isLoggingOut.value = false
  }
}

function showPendingIntegration(): void {
  showLayoutNotice.value = true
}

function createDefaultPhone(): PhoneModel {
  return {
    callingCode: '+55',
    country: 'BR',
    phoneType: 'Mobile',
    areaCode: '11',
    number: '',
  }
}

function isValidPhone(phone: PhoneModel): boolean {
  const rawNumber = phone.number.replace(/\D/g, '')
  const rawAreaCode = phone.areaCode.replace(/\D/g, '')
  const digitsRange = getPhoneDigitsRange(phone.callingCode, phone.country, phone.phoneType)

  return /^\+\d{1,3}$/.test(phone.callingCode.trim())
    && /^[A-Z]{2}$/.test(phone.country.trim().toUpperCase())
    && (phone.phoneType === 'Mobile' || phone.phoneType === 'Landline')
    && (phone.country === 'BR' ? rawAreaCode.length === 2 : rawAreaCode.length > 0)
    && rawNumber.length >= digitsRange.minDigits
    && rawNumber.length <= digitsRange.maxDigits
}

function phoneIdentity(phone: PhoneModel): string {
  return [phone.callingCode, phone.areaCode, phone.number.replace(/\D/g, '')].join('|')
}

function displayPhoneNumber(phone: PhoneModel): string {
  return `${phone.callingCode} (${phone.areaCode}) ${phone.number}`
}

function maskPhoneNumber(phone: PhoneModel): string {
  const maskedAreaCode = [...phone.areaCode]
    .map((character) => (/\d/.test(character) ? '*' : character))
    .join('')
  const visibleDigitCount = 2
  const digitCount = [...phone.number].filter((character) => /\d/.test(character)).length
  let digitsToMask = Math.max(0, digitCount - visibleDigitCount)

  const maskedLocalNumber = [...phone.number].map((character) => {
    if (!/\d/.test(character) || digitsToMask === 0) return character
    digitsToMask -= 1
    return '*'
  }).join('')

  return `${phone.callingCode} (${maskedAreaCode}) ${maskedLocalNumber}`
}

function visiblePhoneNumber(phoneItem: UserPhoneDraft): string {
  return phoneNumberVisibility[phoneItem.id]
    ? displayPhoneNumber(phoneItem.phone)
    : maskPhoneNumber(phoneItem.phone)
}

function hidePhoneNumber(phoneId: string): void {
  phoneNumberVisibility[phoneId] = false
  const timer = phoneVisibilityTimers.get(phoneId)
  if (timer) clearTimeout(timer)
  phoneVisibilityTimers.delete(phoneId)
}

function togglePhoneNumberVisibility(phoneId: string): void {
  if (phoneNumberVisibility[phoneId]) {
    hidePhoneNumber(phoneId)
    return
  }

  phoneNumberVisibility[phoneId] = true
  const currentTimer = phoneVisibilityTimers.get(phoneId)
  if (currentTimer) clearTimeout(currentTimer)

  const timer = setTimeout(() => hidePhoneNumber(phoneId), 15000)
  phoneVisibilityTimers.set(phoneId, timer)
}

function phoneTypeLabel(phone: PhoneModel): string {
  return phone.phoneType === 'Landline' ? 'Fixo' : 'Celular'
}

function addPhone(): void {
  phoneFormError.value = ''

  if (registeredPhones.value.length >= MAX_USER_PHONES) {
    phoneFormError.value = `Você pode adicionar no máximo ${MAX_USER_PHONES} telefones.`
    return
  }

  if (!isValidPhone(phoneForm.value)) {
    phoneFormError.value = 'Informe um telefone válido antes de adicionar.'
    return
  }

  if (registeredPhones.value.some((item) => phoneIdentity(item.phone) === phoneIdentity(phoneForm.value))) {
    phoneFormError.value = 'Este telefone já foi adicionado.'
    return
  }

  const willBePrimary = registeredPhones.value.length === 0 || newPhoneIsPrimary.value
  if (willBePrimary) {
    registeredPhones.value.forEach((item) => { item.isPrimary = false })
  }

  const phoneId = crypto.randomUUID()
  registeredPhones.value.push({
    id: phoneId,
    phone: { ...phoneForm.value },
    isPrimary: willBePrimary,
  })
  phoneNumberVisibility[phoneId] = false

  phoneForm.value = createDefaultPhone()
  newPhoneIsPrimary.value = false
}

function makePhonePrimary(phoneId: string): void {
  registeredPhones.value.forEach((item) => {
    item.isPrimary = item.id === phoneId
  })
}

function openDocumentImagePicker(): void {
  documentUploadError.value = ''
  documentImageInput.value?.click()
}

function addDocumentImages(files: File[], images: DocumentImageDraft[]): void {
  documentUploadError.value = ''

  for (const file of files) {
    if (images.length >= MAX_DOCUMENT_IMAGES) {
      documentUploadError.value = `Você pode adicionar no máximo ${MAX_DOCUMENT_IMAGES} imagens por documento.`
      break
    }

    if (!ACCEPTED_DOCUMENT_IMAGE_TYPES.has(file.type)) {
      documentUploadError.value = 'Formato inválido. Selecione apenas imagens JPEG, PNG ou WebP.'
      continue
    }

    if (file.size > MAX_DOCUMENT_IMAGE_SIZE_BYTES) {
      documentUploadError.value = `A imagem ${file.name} ultrapassa o limite de 5 MB.`
      continue
    }

    const duplicate = images.some((image) =>
      image.file.name === file.name
      && image.file.size === file.size
      && image.file.lastModified === file.lastModified)

    if (duplicate) {
      documentUploadError.value = `A imagem ${file.name} já foi adicionada.`
      continue
    }

    images.push({
      id: crypto.randomUUID(),
      file,
      previewUrl: URL.createObjectURL(file),
    })
  }
}

function handleDocumentImageSelection(event: Event): void {
  const input = event.target as HTMLInputElement
  const document = imageViewerDocument.value
  if (document) addDocumentImages(Array.from(input.files ?? []), document.images)
  input.value = ''
}

function handleDocumentImageDrop(event: DragEvent): void {
  const document = imageViewerDocument.value
  if (document) addDocumentImages(Array.from(event.dataTransfer?.files ?? []), document.images)
}

function addDocument(): void {
  documentFormError.value = ''

  if (!documentForm.number.trim()) {
    documentFormError.value = 'Informe o número do documento.'
    return
  }

  if (documentForm.type === 'cpf' && !isValidCpf(documentForm.number)) {
    documentFormError.value = 'Informe um CPF válido.'
    return
  }

  if (registeredDocuments.value.some((document) => document.type === documentForm.type)) {
    documentFormError.value = 'Este tipo de documento já foi adicionado.'
    return
  }

  const documentId = crypto.randomUUID()
  registeredDocuments.value.push({
    id: documentId,
    type: documentForm.type,
    number: documentForm.number.trim(),
    images: [],
  })
  documentNumberVisibility[documentId] = false

  const nextDocumentType = documentDefinitions.find((definition) =>
    !registeredDocuments.value.some((document) => document.type === definition.value))
  if (nextDocumentType) documentForm.type = nextDocumentType.value
  documentForm.number = ''
  documentUploadError.value = ''
}

function documentTitle(type: DocumentType): string {
  return documentDefinitions.find((document) => document.value === type)?.title ?? type
}

function displayDocumentNumber(document: UserDocumentDraft): string {
  return document.type === 'cpf' ? formatCpf(document.number) : document.number
}

function maskDocumentNumber(document: UserDocumentDraft): string {
  const formattedNumber = displayDocumentNumber(document)
  const visibleCharacterCount = 2
  const alphanumericCount = [...formattedNumber].filter((character) => /[A-Za-z0-9]/.test(character)).length
  let charactersToMask = Math.max(0, alphanumericCount - visibleCharacterCount)

  return [...formattedNumber].map((character) => {
    if (!/[A-Za-z0-9]/.test(character) || charactersToMask === 0) return character
    charactersToMask -= 1
    return '*'
  }).join('')
}

function visibleDocumentNumber(document: UserDocumentDraft): string {
  return documentNumberVisibility[document.id]
    ? displayDocumentNumber(document)
    : maskDocumentNumber(document)
}

function hideDocumentNumber(documentId: string): void {
  documentNumberVisibility[documentId] = false
  const timer = documentVisibilityTimers.get(documentId)
  if (timer) clearTimeout(timer)
  documentVisibilityTimers.delete(documentId)
}

function toggleDocumentNumberVisibility(documentId: string): void {
  if (documentNumberVisibility[documentId]) {
    hideDocumentNumber(documentId)
    return
  }

  documentNumberVisibility[documentId] = true
  const currentTimer = documentVisibilityTimers.get(documentId)
  if (currentTimer) clearTimeout(currentTimer)

  const timer = setTimeout(() => hideDocumentNumber(documentId), 15000)
  documentVisibilityTimers.set(documentId, timer)
}

function openImageViewer(documentId: string, imageIndex = 0): void {
  const document = registeredDocuments.value.find((item) => item.id === documentId)
  if (!document) return

  imageViewerDocumentId.value = documentId
  imageViewerIndex.value = document.images.length
    ? Math.min(Math.max(imageIndex, 0), document.images.length - 1)
    : 0
  documentUploadError.value = ''
}

function closeImageViewer(): void {
  imageViewerDocumentId.value = null
  imageViewerIndex.value = 0
}

function showPreviousDocumentImage(): void {
  const imageCount = imageViewerDocument.value?.images.length ?? 0
  if (imageCount < 2) return
  imageViewerIndex.value = (imageViewerIndex.value - 1 + imageCount) % imageCount
}

function showNextDocumentImage(): void {
  const imageCount = imageViewerDocument.value?.images.length ?? 0
  if (imageCount < 2) return
  imageViewerIndex.value = (imageViewerIndex.value + 1) % imageCount
}

function removeViewedDocumentImage(): void {
  const document = imageViewerDocument.value
  const image = imageViewerImage.value
  if (!document || !image) return

  const imageIndex = document.images.findIndex((item) => item.id === image.id)
  if (imageIndex < 0) return

  document.images.splice(imageIndex, 1)
  URL.revokeObjectURL(image.previewUrl)

  if (document.images.length === 0) {
    imageViewerIndex.value = 0
    return
  }

  imageViewerIndex.value = Math.min(imageViewerIndex.value, document.images.length - 1)
}

function removeAllDocumentImages(): void {
  const document = imageViewerDocument.value
  if (!document) return

  document.images.forEach((image) => URL.revokeObjectURL(image.previewUrl))
  document.images.splice(0)
  imageViewerIndex.value = 0
  documentUploadError.value = ''
}

function openReplacementImagePicker(): void {
  documentUploadError.value = ''
  replacementImageInput.value?.click()
}

function handleReplacementImageSelection(event: Event): void {
  const input = event.target as HTMLInputElement
  const document = imageViewerDocument.value
  const currentImage = imageViewerImage.value
  const file = input.files?.[0]
  input.value = ''

  if (!document || !currentImage || !file) return

  const replacementImages: DocumentImageDraft[] = []
  addDocumentImages([file], replacementImages)
  const replacementImage = replacementImages[0]
  if (!replacementImage) return

  const currentIndex = document.images.findIndex((image) => image.id === currentImage.id)
  if (currentIndex < 0) {
    URL.revokeObjectURL(replacementImage.previewUrl)
    return
  }

  document.images.splice(currentIndex, 1, replacementImage)
  URL.revokeObjectURL(currentImage.previewUrl)
}

onBeforeUnmount(() => {
  phoneVisibilityTimers.forEach((timer) => clearTimeout(timer))
  phoneVisibilityTimers.clear()
  documentVisibilityTimers.forEach((timer) => clearTimeout(timer))
  documentVisibilityTimers.clear()
  registeredDocuments.value.forEach((document) => {
    document.images.forEach((image) => URL.revokeObjectURL(image.previewUrl))
  })
})
</script>

<template>
  <v-main class="my-data-page">
    <div
      class="account-shell"
      :class="{ 'account-shell--collapsed': isSidebarCollapsed }"
    >
      <aside class="sidebar">
        <div class="brand" aria-label="YaeaY Account">
          <span class="brand__primary">YaeaY</span>
          <span class="brand__secondary">Account</span>
        </div>

        <nav class="sidebar__navigation" aria-label="Navegação principal">
          <v-tooltip
            v-for="item in navigationItems"
            :key="item.label"
            :text="item.label"
            :disabled="!isSidebarCollapsed"
            location="right"
          >
            <template #activator="{ props: tooltipProps }">
              <button
                v-bind="tooltipProps"
                type="button"
                class="navigation-item"
                :class="{
                  'navigation-item--active': item.label === 'Meus dados',
                  'navigation-item--disabled': !item.to,
                }"
                :aria-current="item.label === 'Meus dados' ? 'page' : undefined"
                :aria-label="item.label"
                :aria-disabled="!item.to"
                @click="navigateTo(item.to)"
              >
                <v-icon :icon="item.icon" size="22" />
                <span>{{ item.label }}</span>
              </button>
            </template>
          </v-tooltip>
        </nav>

        <div class="sidebar__footer">
          <div class="navigation-item navigation-item--static">
            <v-icon :icon="mdiCogOutline" size="23" />
            <span>Configurações</span>
          </div>
        </div>
      </aside>

      <button
        v-if="isMobileSidebarOpen"
        type="button"
        class="sidebar-backdrop"
        aria-label="Fechar menu lateral"
        @click="closeSidebar"
      />

      <section class="workspace">
        <header class="topbar">
          <div class="topbar__start">
            <v-tooltip
              :text="isSidebarCollapsed ? 'Expandir menu' : 'Recolher menu'"
              location="bottom"
            >
              <template #activator="{ props: tooltipProps }">
                <v-btn
                  v-bind="tooltipProps"
                  class="sidebar-toggle"
                  :icon="isSidebarCollapsed ? mdiMenu : mdiMenuOpen"
                  variant="text"
                  :aria-label="isSidebarCollapsed ? 'Expandir menu lateral' : 'Recolher menu lateral'"
                  :aria-expanded="!isSidebarCollapsed"
                  @click="toggleSidebar"
                />
              </template>
            </v-tooltip>

            <div class="topbar__context">
              <v-icon :icon="mdiAccountCircleOutline" size="24" />
              <span>Minha conta</span>
            </div>
          </div>

          <StageEnvironmentBanner class="topbar__stage-banner" />

          <div class="topbar__account">
            <div class="notification" aria-label="Notificações">
              <v-icon :icon="mdiBellOutline" size="25" />
              <span class="notification__badge">3</span>
            </div>

            <v-menu location="bottom end" :close-on-content-click="!isLoggingOut">
              <template #activator="{ props }">
                <button
                  v-bind="props"
                  type="button"
                  class="topbar__user-button"
                  aria-label="Abrir menu do usuário"
                >
                  <span class="topbar__name">{{ firstName }}</span>
                  <v-icon :icon="mdiChevronDown" size="21" />
                </button>
              </template>

              <v-list density="compact" min-width="190" bg-color="#ffffff">
                <v-list-item
                  :disabled="isLoggingOut"
                  :ripple="false"
                  base-color="#173d32"
                  color="#173d32"
                  title="Sair"
                  @click="handleLogout"
                >
                  <template #prepend>
                    <v-progress-circular v-if="isLoggingOut" indeterminate size="20" width="2" />
                    <v-icon v-else :icon="mdiLogoutVariant" size="21" />
                  </template>
                </v-list-item>
              </v-list>
            </v-menu>
          </div>
        </header>

        <div class="profile-page">
          <v-btn
            class="profile-back"
            variant="text"
            :prepend-icon="mdiArrowLeft"
            :to="{ name: 'home' }"
            :ripple="false"
          >
            Voltar
          </v-btn>

          <header class="profile-heading">
            <div class="profile-heading__title">
              <span class="profile-heading__icon">
                <v-icon :icon="mdiAccountOutline" size="35" />
              </span>
              <div>
                <h1>Meus Dados</h1>
                <p>Mantenha suas informações pessoais sempre atualizadas.</p>
              </div>
            </div>

            <article class="completion-summary">
              <v-progress-circular
                :model-value="overallCompletion"
                :size="82"
                :width="7"
                color="#21644d"
                bg-color="#e4e9e6"
              >
                <strong>{{ overallCompletion }}%</strong>
              </v-progress-circular>
              <div class="completion-summary__content">
                <h2>Cadastro completo</h2>
                <p>Complete seus dados para aproveitar todos os recursos da sua conta.</p>
                <div class="completion-summary__privacy">
                  <v-icon :icon="mdiShieldCheckOutline" size="19" />
                  <span>Seus dados são protegidos e usados apenas para manter sua conta atualizada.</span>
                </div>
              </div>
              <v-btn variant="text" class="completion-summary__action" @click="showPendingIntegration">
                Ver pendências
              </v-btn>
            </article>
          </header>

          <div class="profile-workspace">
            <nav class="section-navigation" aria-label="Seções dos meus dados">
              <router-link
                v-for="section in sections"
                :key="section.id"
                :to="section.to"
                class="section-card"
                :class="{ 'section-card--active': activeSection === section.id }"
              >
                <span class="section-card__icon">
                  <v-icon :icon="section.icon" size="26" />
                </span>
                <span class="section-card__content">
                  <strong>{{ section.label }}</strong>
                  <small>{{ section.completion }}% concluído</small>
                </span>
                <span v-if="section.completion === 100" class="section-card__complete">
                  <v-icon :icon="mdiCheck" size="23" />
                </span>
                <v-progress-circular
                  v-else
                  :model-value="section.completion"
                  :size="46"
                  :width="4"
                  :color="activeSection === section.id ? '#ffffff' : '#ef9800'"
                  :bg-color="activeSection === section.id ? '#67927f' : '#e3e7e4'"
                >
                  <small>{{ section.completion }}%</small>
                </v-progress-circular>
              </router-link>
            </nav>

            <v-form class="data-panel" @submit.prevent="showPendingIntegration">
              <template v-if="activeSection === 'basic'">
                <h2>Dados básicos</h2>
                <div class="form-grid">
                  <v-text-field
                    v-model="profile.fullName"
                    class="form-grid__full"
                    label="Nome completo"
                    :prepend-inner-icon="mdiAccountOutline"
                    variant="outlined"
                    hide-details
                  />
                  <v-text-field
                    v-model="profile.birthDate"
                    label="Data de nascimento"
                    :prepend-inner-icon="mdiCalendarMonthOutline"
                    type="date"
                    variant="outlined"
                    hide-details
                  />
                  <v-select
                    v-model="profile.gender"
                    label="Gênero"
                    :prepend-inner-icon="mdiAccountOutline"
                    :items="['Feminino', 'Masculino', 'Não binário', 'Prefiro não informar']"
                    variant="outlined"
                    hide-details
                  />
                  <v-text-field
                    v-model="profile.socialName"
                    class="form-grid__full"
                    label="Nome social"
                    placeholder="Como prefere ser chamado?"
                    :prepend-inner-icon="mdiAccountOutline"
                    variant="outlined"
                    hide-details
                  />
                </div>
              </template>

              <template v-else-if="activeSection === 'contact'">
                <div class="contact-heading">
                  <div>
                    <h2>Telefones</h2>
                    <p>Adicione seus telefones e escolha qual será o contato principal.</p>
                  </div>
                  <span class="contact-heading__count">
                    {{ registeredPhones.length }}/{{ MAX_USER_PHONES }} telefones
                  </span>
                </div>

                <section class="phone-editor" aria-labelledby="phone-editor-title">
                  <h3 id="phone-editor-title" class="sr-only">Adicionar telefone</h3>
                  <UserPhonesField
                    v-model="phoneForm"
                    @update:model-value="phoneFormError = ''"
                  />

                  <div class="phone-editor__actions">
                    <div class="phone-editor__primary-option">
                      <v-switch
                        v-if="registeredPhones.length"
                        v-model="newPhoneIsPrimary"
                        color="#1c644b"
                        density="compact"
                        hide-details
                        label="Definir como principal"
                      />
                      <span v-else>O primeiro telefone será definido como principal.</span>
                    </div>
                    <v-btn
                      :prepend-icon="mdiPlus"
                      rounded="pill"
                      color="#17543f"
                      variant="flat"
                      :disabled="registeredPhones.length >= MAX_USER_PHONES"
                      @click="addPhone"
                    >
                      Adicionar telefone
                    </v-btn>
                  </div>

                  <p v-if="phoneFormError" class="phone-form-error" role="alert">
                    {{ phoneFormError }}
                  </p>
                </section>

                <section v-if="registeredPhones.length" class="registered-phones" aria-labelledby="registered-phones-title">
                  <div class="registered-phones__heading">
                    <h3 id="registered-phones-title">Telefones adicionados</h3>
                    <span>{{ registeredPhones.length }}</span>
                  </div>

                  <article
                    v-for="phoneItem in registeredPhones"
                    :key="phoneItem.id"
                    class="registered-phone-card"
                  >
                    <span class="registered-phone-card__icon">
                      <v-icon :icon="mdiPhoneOutline" size="24" />
                    </span>
                    <div class="registered-phone-card__identity">
                      <div class="registered-phone-card__number">
                        <strong>{{ visiblePhoneNumber(phoneItem) }}</strong>
                        <v-tooltip
                          :text="phoneNumberVisibility[phoneItem.id] ? 'Ocultar número do telefone' : 'Mostrar número do telefone'"
                          location="top"
                        >
                          <template #activator="{ props: tooltipProps }">
                            <v-btn
                              v-bind="tooltipProps"
                              :icon="phoneNumberVisibility[phoneItem.id] ? mdiEyeOffOutline : mdiEyeOutline"
                              variant="text"
                              color="#315f50"
                              density="comfortable"
                              :aria-label="phoneNumberVisibility[phoneItem.id] ? 'Ocultar número do telefone' : 'Mostrar número do telefone'"
                              :aria-pressed="phoneNumberVisibility[phoneItem.id] === true"
                              @click="togglePhoneNumberVisibility(phoneItem.id)"
                            />
                          </template>
                        </v-tooltip>
                      </div>
                      <span>{{ phoneTypeLabel(phoneItem.phone) }} · {{ phoneItem.phone.country }}</span>
                    </div>
                    <v-chip
                      v-if="phoneItem.isPrimary"
                      :prepend-icon="mdiStar"
                      color="#1c644b"
                      variant="tonal"
                      size="small"
                    >
                      Principal
                    </v-chip>
                    <v-btn
                      v-else
                      :prepend-icon="mdiStarOutline"
                      rounded="pill"
                      variant="text"
                      color="#315f50"
                      @click="makePhonePrimary(phoneItem.id)"
                    >
                      Tornar principal
                    </v-btn>
                  </article>
                </section>
              </template>

              <template v-else-if="activeSection === 'documents'">
                <div class="documents-heading">
                  <div>
                    <h2>Documentos</h2>
                    <p>Adicione cada documento com seu tipo, número e imagens.</p>
                  </div>
                  <span class="documents-heading__count">
                    {{ registeredDocuments.length }} documento(s)
                  </span>
                </div>

                <div class="document-fields">
                  <v-select
                    v-model="documentForm.type"
                    class="document-fields__type"
                    label="Tipo de documento"
                    :items="availableDocumentDefinitions"
                    item-title="title"
                    item-value="value"
                    :prepend-inner-icon="mdiFileDocumentOutline"
                    variant="outlined"
                    hide-details
                    :disabled="availableDocumentDefinitions.length === 0"
                    @update:model-value="documentFormError = ''"
                  />
                  <CpfField
                    v-if="documentForm.type === 'cpf'"
                    v-model="documentForm.number"
                    label="Número do CPF"
                    :prepend-inner-icon="mdiCardAccountDetailsOutline"
                    variant="outlined"
                    hide-details="auto"
                    validate-on="blur"
                    :disabled="availableDocumentDefinitions.length === 0"
                    @update:model-value="documentFormError = ''"
                  />
                  <v-text-field
                    v-else
                    v-model="documentForm.number"
                    :label="selectedDocumentDefinition.numberLabel"
                    :placeholder="selectedDocumentDefinition.placeholder"
                    :prepend-inner-icon="mdiCardAccountDetailsOutline"
                    variant="outlined"
                    hide-details
                    :disabled="availableDocumentDefinitions.length === 0"
                    @update:model-value="documentFormError = ''"
                  />
                  <v-btn
                    class="document-add-inline"
                    :prepend-icon="mdiPlus"
                    color="#17543f"
                    variant="flat"
                    rounded="pill"
                    :disabled="availableDocumentDefinitions.length === 0"
                    @click="addDocument"
                  >
                    Adicionar documento
                  </v-btn>
                </div>

                <p v-if="documentFormError" class="document-form-error" role="alert">
                  {{ documentFormError }}
                </p>

                <section v-if="registeredDocuments.length" class="registered-documents" aria-labelledby="registered-documents-title">
                  <div class="registered-documents__heading">
                    <h3 id="registered-documents-title">Documentos adicionados</h3>
                    <span>{{ registeredDocuments.length }}</span>
                  </div>

                  <article
                    v-for="document in registeredDocuments"
                    :key="document.id"
                    class="registered-document-card"
                  >
                    <span class="registered-document-card__icon">
                      <v-icon :icon="mdiFileDocumentOutline" size="25" />
                    </span>
                    <div class="registered-document-card__identity">
                      <strong>{{ documentTitle(document.type) }}</strong>
                      <div class="registered-document-card__number">
                        <span>{{ visibleDocumentNumber(document) }}</span>
                        <v-tooltip
                          :text="documentNumberVisibility[document.id] ? 'Ocultar número do documento' : 'Mostrar número do documento'"
                          location="top"
                        >
                          <template #activator="{ props: tooltipProps }">
                            <v-btn
                              v-bind="tooltipProps"
                              :icon="documentNumberVisibility[document.id] ? mdiEyeOffOutline : mdiEyeOutline"
                              size="x-small"
                              variant="text"
                              color="#315f50"
                              :aria-label="documentNumberVisibility[document.id] ? 'Ocultar número do documento' : 'Mostrar número do documento'"
                              :aria-pressed="documentNumberVisibility[document.id] === true"
                              @click="toggleDocumentNumberVisibility(document.id)"
                            />
                          </template>
                        </v-tooltip>
                      </div>
                    </div>
                    <div class="registered-document-card__images">
                      <v-tooltip
                        text="Adicionar Imagem"
                        location="top"
                      >
                        <template #activator="{ props: tooltipProps }">
                          <v-btn
                            v-bind="tooltipProps"
                            class="document-image-trigger"
                            icon
                            variant="flat"
                            color="#21644d"
                            aria-label="Adicionar Imagem"
                            @click="openImageViewer(document.id)"
                          >
                            <span class="cloud-image-icon" aria-hidden="true">
                              <v-icon :icon="mdiCloudUploadOutline" size="23" />
                              <v-icon class="cloud-image-icon__image" :icon="mdiImageOutline" size="10" />
                            </span>
                            <span v-if="document.images.length" class="document-image-trigger__count">
                              {{ document.images.length }}
                            </span>
                          </v-btn>
                        </template>
                      </v-tooltip>
                    </div>
                  </article>

                </section>
              </template>

              <template v-else>
                <h2>Endereço</h2>
                <div class="form-grid form-grid--address">
                  <v-text-field v-model="profile.postalCode" label="CEP" variant="outlined" hide-details />
                  <v-text-field v-model="profile.street" class="form-grid__wide" label="Logradouro" variant="outlined" hide-details />
                  <v-text-field v-model="profile.number" label="Número" variant="outlined" hide-details />
                  <v-text-field v-model="profile.complement" label="Complemento" variant="outlined" hide-details />
                  <v-text-field v-model="profile.district" label="Bairro" variant="outlined" hide-details />
                  <v-text-field v-model="profile.city" label="Cidade" variant="outlined" hide-details />
                  <v-text-field v-model="profile.state" label="Estado" variant="outlined" hide-details />
                </div>
              </template>

              <div class="form-actions">
                <v-btn variant="outlined" size="large" @click="showPendingIntegration">Cancelar</v-btn>
                <v-btn
                  type="submit"
                  size="large"
                  color="#17543f"
                  :disabled="(activeSection === 'documents' && registeredDocuments.length === 0)
                    || (activeSection === 'contact' && registeredPhones.length === 0)"
                >
                  Salvar alterações
                </v-btn>
              </div>
            </v-form>
          </div>
        </div>
      </section>
    </div>

    <v-dialog v-model="isImageViewerOpen" max-width="960" scrollable>
      <v-card v-if="imageViewerDocument" class="document-viewer">
        <header class="document-viewer__header">
          <div>
            <strong>{{ documentTitle(imageViewerDocument.type) }} - Imagens</strong>
            <span>{{ visibleDocumentNumber(imageViewerDocument) }}</span>
          </div>
          <div class="document-viewer__header-actions">
            <v-btn
              class="document-viewer__back"
              :prepend-icon="mdiArrowLeft"
              variant="text"
              :ripple="false"
              @click="closeImageViewer"
            >
              Voltar
            </v-btn>
          </div>
        </header>

        <div
          v-if="imageViewerImage"
          class="document-viewer__stage"
          @dragover.prevent
          @drop.prevent="handleDocumentImageDrop"
        >
          <v-btn
            class="document-viewer__arrow document-viewer__arrow--previous"
            :icon="mdiChevronLeft"
            color="#ffffff"
            variant="tonal"
            :disabled="imageViewerDocument.images.length <= 1"
            aria-label="Imagem anterior"
            @click="showPreviousDocumentImage"
          />

          <img
            :src="imageViewerImage.previewUrl"
            :alt="`Imagem ${imageViewerIndex + 1} de ${documentTitle(imageViewerDocument.type)}`"
          >

          <v-btn
            class="document-viewer__arrow document-viewer__arrow--next"
            :icon="mdiChevronRight"
            color="#ffffff"
            variant="tonal"
            :disabled="imageViewerDocument.images.length <= 1"
            aria-label="Próxima imagem"
            @click="showNextDocumentImage"
          />
        </div>

        <button
          v-else
          type="button"
          class="document-viewer__empty"
          @click="openDocumentImagePicker"
          @dragover.prevent
          @drop.prevent="handleDocumentImageDrop"
        >
          <span class="document-viewer__empty-icon">
            <span class="cloud-image-icon cloud-image-icon--empty" aria-hidden="true">
              <v-icon :icon="mdiCloudUploadOutline" size="44" />
              <v-icon class="cloud-image-icon__image" :icon="mdiImageOutline" size="16" />
            </span>
          </span>
          <strong>Este documento ainda não possui imagens</strong>
          <small>Adicione JPEG, PNG ou WebP de até 5 MB.</small>
        </button>

        <input
          ref="documentImageInput"
          class="document-image-input"
          type="file"
          accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
          multiple
          @change="handleDocumentImageSelection"
        >

        <input
          ref="replacementImageInput"
          class="document-image-input"
          type="file"
          accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
          @change="handleReplacementImageSelection"
        >

        <p v-if="documentUploadError" class="document-viewer__error" role="alert">
          {{ documentUploadError }}
        </p>

        <nav
          v-if="imageViewerDocument.images.length > 1"
          class="document-viewer__navigation"
          aria-label="Navegação entre imagens do documento"
        >
          <button
            v-for="(image, index) in imageViewerDocument.images"
            :key="image.id"
            type="button"
            :class="{ 'document-viewer__navigation-item--active': imageViewerIndex === index }"
            :aria-label="`Visualizar imagem ${index + 1}`"
            :aria-current="imageViewerIndex === index ? 'true' : undefined"
            @click="imageViewerIndex = index"
          >
            <img :src="image.previewUrl" alt="">
            <span>{{ index + 1 }}</span>
          </button>
        </nav>

        <footer v-if="imageViewerImage" class="document-viewer__footer">
          <div class="document-viewer__file-details">
            <strong>{{ imageViewerImage.file.name }}</strong>
            <span>
              {{ imageViewerIndex + 1 }} de {{ imageViewerDocument.images.length }}
              · {{ (imageViewerImage.file.size / 1024 / 1024).toFixed(2) }} MB
            </span>
          </div>
          <v-btn
            v-if="imageViewerDocument.images.length < MAX_DOCUMENT_IMAGES"
            class="document-viewer__add-more"
            rounded="pill"
            color="#1c644b"
            variant="flat"
            @click="openDocumentImagePicker"
          >
            <template #prepend>
              <span class="cloud-image-icon" aria-hidden="true">
                <v-icon :icon="mdiCloudUploadOutline" size="22" />
                <v-icon class="cloud-image-icon__image" :icon="mdiImageOutline" size="9" />
              </span>
            </template>
            Adicionar mais imagens
          </v-btn>
          <div class="document-viewer__image-actions">
            <v-tooltip text="Alterar imagem" location="top">
              <template #activator="{ props: tooltipProps }">
                <v-btn
                  v-bind="tooltipProps"
                  :icon="mdiPencilOutline"
                  variant="text"
                  aria-label="Alterar imagem"
                  @click="openReplacementImagePicker"
                />
              </template>
            </v-tooltip>
            <v-tooltip text="Remover imagem" location="top">
              <template #activator="{ props: tooltipProps }">
                <v-btn
                  v-bind="tooltipProps"
                  :icon="mdiDeleteOutline"
                  variant="text"
                  color="#a13f3f"
                  aria-label="Remover imagem"
                  @click="removeViewedDocumentImage"
                />
              </template>
            </v-tooltip>
            <v-tooltip text="Remover todas as imagens" location="top">
              <template #activator="{ props: tooltipProps }">
                <v-btn
                  v-bind="tooltipProps"
                  :icon="mdiDeleteSweepOutline"
                  variant="text"
                  color="#a13f3f"
                  aria-label="Remover todas as imagens"
                  @click="removeAllDocumentImages"
                />
              </template>
            </v-tooltip>
          </div>
        </footer>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="showLayoutNotice" color="#315f50" timeout="5000">
      O layout está pronto. A persistência desta seção ainda não foi conectada ao back-end.
    </v-snackbar>
    <v-snackbar v-model="showLogoutError" color="error" timeout="5000">
      Não foi possível sair da conta. Tente novamente.
    </v-snackbar>
  </v-main>
</template>

<style scoped>
.my-data-page {
  min-height: 100vh;
  color: #173d32;
  background:
    radial-gradient(circle at 0 100%, rgba(94, 150, 120, 0.11), transparent 24%),
    #f8f9f7;
}

.account-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 230px minmax(0, 1fr);
}

.account-shell--collapsed {
  grid-template-columns: 92px minmax(0, 1fr);
}

.account-shell--collapsed .brand {
  justify-content: center;
  padding-inline: 5px;
}

.account-shell--collapsed .brand__secondary,
.account-shell--collapsed .navigation-item span {
  display: none;
}

.account-shell--collapsed .navigation-item {
  justify-content: center;
  padding: 0;
}

.sidebar-backdrop {
  display: none;
}

.sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  display: flex;
  flex-direction: column;
  padding: 30px 10px 22px;
  background: #fff;
  border-right: 1px solid #e5e8e5;
}

.brand {
  display: flex;
  align-items: baseline;
  gap: 6px;
  padding: 0 20px 44px;
  white-space: nowrap;
}

.brand__primary {
  color: #143e31;
  font-size: 1.7rem;
  font-weight: 800;
  letter-spacing: -0.055em;
}

.brand__secondary {
  color: #6f8b80;
  font-size: 1.08rem;
  font-weight: 300;
}

.sidebar__navigation {
  display: flex;
  flex-direction: column;
  gap: 9px;
}

.navigation-item {
  width: 100%;
  min-height: 54px;
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 0 22px;
  border: 0;
  border-radius: 18px;
  color: #4c5753;
  background: transparent;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.navigation-item--disabled,
.navigation-item--static {
  cursor: default;
}

.navigation-item--active {
  color: #153f33;
  background: #e7ece9;
  font-weight: 650;
}

.sidebar__footer {
  margin-top: auto;
  padding-top: 20px;
  border-top: 1px solid #e7e9e7;
}

.workspace {
  min-width: 0;
}

.topbar {
  position: relative;
  min-height: 90px;
  display: grid;
  grid-template-columns: minmax(220px, 1fr) auto minmax(150px, 1fr);
  align-items: center;
  gap: 24px;
  padding: 8px 34px 8px 42px;
  background: #fff;
  border-bottom: 1px solid #e5e8e5;
}

.topbar__context,
.topbar__account {
  display: flex;
  align-items: center;
}

.topbar__start {
  grid-column: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.sidebar-toggle {
  flex: 0 0 auto;
  color: #334d44;
}

.topbar__context {
  gap: 12px;
  color: #60736a;
}

.topbar__stage-banner {
  grid-column: 2;
  justify-self: center;
}

.topbar__account {
  grid-column: 3;
  justify-self: end;
  gap: 11px;
}

.notification {
  position: relative;
  display: grid;
  place-items: center;
  margin-right: 18px;
  color: #334d44;
}

.notification__badge {
  position: absolute;
  top: -8px;
  right: -8px;
  min-width: 18px;
  height: 18px;
  display: grid;
  place-items: center;
  border-radius: 9px;
  color: #173d32;
  background: #e3b94f;
  font-size: 0.67rem;
  font-weight: 800;
}

.topbar__user-button {
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  gap: 11px;
  padding: 0 4px 0 10px;
  border: 0;
  border-radius: 10px;
  color: inherit;
  background: transparent;
  cursor: pointer;
}

.topbar__user-button:hover,
.topbar__user-button:focus-visible {
  background: #f1f5f2;
}

.topbar__name {
  font-weight: 650;
}

.profile-page {
  width: min(1370px, 100%);
  margin-inline: auto;
  padding: 34px 42px 46px;
  box-sizing: border-box;
}

.profile-back {
  margin: -8px 0 18px -14px;
  color: #3e564f;
  text-transform: none;
  letter-spacing: 0;
}

.profile-heading {
  display: grid;
  grid-template-columns: minmax(300px, 1fr) minmax(470px, 620px);
  align-items: center;
  gap: 32px;
  margin-bottom: 28px;
}

.profile-heading__title {
  display: flex;
  align-items: center;
  gap: 20px;
}

.profile-heading__icon {
  width: 76px;
  height: 76px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: 24px;
  color: #176148;
  background: #e3eee8;
}

.profile-heading h1 {
  margin: 0;
  color: #123e31;
  font-size: clamp(2rem, 3.3vw, 3rem);
  line-height: 1;
  letter-spacing: -0.045em;
}

.profile-heading__title p,
.completion-summary p {
  margin: 9px 0 0;
  color: #64746d;
  line-height: 1.45;
}

.completion-summary {
  min-height: 118px;
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 22px;
  padding: 18px 22px;
  border: 1px solid #dfe5e1;
  border-radius: 18px;
  background: #fff;
  box-shadow: 0 8px 24px rgba(20, 62, 49, 0.045);
}

.completion-summary h2 {
  margin: 0;
  font-size: 1.13rem;
}

.completion-summary p {
  font-size: 0.84rem;
}

.completion-summary__content {
  min-width: 0;
}

.completion-summary__privacy {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 11px;
  color: #315f50;
  font-size: 0.77rem;
  line-height: 1.4;
}

.completion-summary__privacy :deep(.v-icon) {
  flex: 0 0 auto;
}

.completion-summary__action {
  color: #1d5c46;
  text-transform: none;
  letter-spacing: 0;
}

.profile-workspace {
  display: grid;
  grid-template-columns: minmax(270px, 365px) minmax(0, 1fr);
  align-items: start;
  gap: 26px;
}

.section-navigation {
  display: grid;
  gap: 12px;
}

.section-card {
  min-height: 94px;
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 16px;
  padding: 15px 20px;
  border: 1px solid #dfe5e1;
  border-radius: 17px;
  color: #173d32;
  background: #fff;
  text-decoration: none;
  transition: transform 160ms ease, box-shadow 160ms ease;
}

.section-card:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 24px rgba(20, 62, 49, 0.08);
}

.section-card--active {
  border-color: #1c644b;
  color: #fff;
  background: linear-gradient(120deg, #176047, #1c513f);
  box-shadow: 0 10px 24px rgba(20, 73, 54, 0.16);
}

.section-card__icon {
  width: 48px;
  height: 48px;
  display: grid;
  place-items: center;
  border-radius: 14px;
  color: #21644d;
  background: #edf4f0;
}

.section-card--active .section-card__icon {
  color: #fff;
  background: rgba(255, 255, 255, 0.16);
}

.section-card__content {
  min-width: 0;
  display: grid;
  gap: 5px;
}

.section-card__content strong,
.section-card__content small {
  overflow-wrap: anywhere;
  line-height: 1.35;
}

.section-card > .v-progress-circular,
.section-card__complete {
  flex: 0 0 auto;
}

.section-card__content small {
  color: #728078;
}

.section-card--active .section-card__content small {
  color: #dcebe4;
}

.section-card__complete {
  width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  color: #fff;
  background: #1b6d50;
}

.data-panel {
  min-height: 478px;
  display: flex;
  flex-direction: column;
  padding: 34px 36px 30px;
  border: 1px solid #dfe5e1;
  border-radius: 18px;
  background: #fff;
  box-shadow: 0 9px 25px rgba(20, 62, 49, 0.045);
}

.data-panel h2 {
  margin: 0 0 30px;
  font-size: 1.55rem;
  letter-spacing: -0.025em;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.contact-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 22px;
}

.contact-heading h2 {
  margin-bottom: 5px;
}

.contact-heading p {
  margin: 0;
  color: #6b7973;
  font-size: 0.86rem;
  line-height: 1.45;
}

.contact-heading__count {
  flex: 0 0 auto;
  padding: 7px 11px;
  border-radius: 999px;
  color: #225f4a;
  background: #edf4f0;
  font-size: 0.78rem;
  font-weight: 700;
}

.phone-editor {
  padding: 4px 18px 18px;
  border: 1px solid #e0e7e3;
  border-radius: 16px;
  background: #fbfdfc;
}

.phone-editor__actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  margin-top: 14px;
}

.phone-editor__primary-option {
  min-width: 0;
  color: #687870;
  font-size: 0.82rem;
}

.phone-editor__primary-option :deep(.v-label) {
  color: #315f50;
  opacity: 1;
}

.phone-editor__actions :deep(.v-btn) {
  flex: 0 0 auto;
  min-height: 44px;
  padding-inline: 20px;
  text-transform: none;
  letter-spacing: 0;
  box-shadow: 0 4px 10px rgba(20, 73, 54, 0.16);
}

.phone-form-error {
  margin: 12px 0 0;
  color: #a13f3f;
  font-size: 0.82rem;
}

.registered-phones {
  display: grid;
  gap: 11px;
  margin-top: 28px;
  padding-top: 24px;
  border-top: 1px solid #e4e8e5;
}

.registered-phones__heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2px;
}

.registered-phones__heading h3 {
  margin: 0;
  font-size: 1rem;
}

.registered-phones__heading span {
  min-width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  color: #1e5d47;
  background: #e9f1ed;
  font-size: 0.78rem;
  font-weight: 700;
}

.registered-phone-card {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 14px;
  padding: 13px 14px;
  border: 1px solid #dfe5e1;
  border-radius: 14px;
  background: #fff;
}

.registered-phone-card__icon {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  color: #21644d;
  background: #edf4f0;
}

.registered-phone-card__identity {
  min-width: 0;
  display: grid;
  gap: 4px;
}

.registered-phone-card__identity strong,
.registered-phone-card__identity span {
  overflow-wrap: anywhere;
}

.registered-phone-card__identity strong {
  color: #173f31;
  font-size: 0.96rem;
}

.registered-phone-card__number {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 5px;
}

.registered-phone-card__number strong {
  min-width: 0;
  font-variant-numeric: tabular-nums;
  letter-spacing: 0.025em;
}

.registered-phone-card__number :deep(.v-btn) {
  flex: 0 0 auto;
}

.registered-phone-card__identity span {
  color: #748078;
  font-size: 0.8rem;
}

.registered-phone-card :deep(.v-btn) {
  text-transform: none;
  letter-spacing: 0;
}

.documents-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 26px;
}

.documents-heading h2 {
  margin-bottom: 5px;
}

.documents-heading p {
  margin: 0;
  color: #6b7973;
  font-size: 0.86rem;
  line-height: 1.45;
}

.documents-heading__count {
  flex: 0 0 auto;
  padding: 7px 11px;
  border-radius: 999px;
  color: #225f4a;
  background: #edf4f0;
  font-size: 0.78rem;
  font-weight: 700;
}

.document-fields {
  display: grid;
  grid-template-columns: 230px minmax(260px, 1fr) auto;
  align-items: start;
  gap: 14px;
}

.document-fields__type {
  width: 230px;
  max-width: 100%;
}

.document-image-input {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
  clip-path: inset(50%);
  white-space: nowrap;
}

.document-form-error,
.document-viewer__error {
  margin: 12px 0 0;
  color: #a13f3f;
  font-size: 0.82rem;
}

.document-add-inline {
  min-width: 188px;
  height: 48px;
  align-self: start;
  margin-top: 4px;
  padding-inline: 20px;
  text-transform: none;
  letter-spacing: 0;
  box-shadow: 0 4px 10px rgba(20, 73, 54, 0.16);
}

.registered-documents {
  display: grid;
  gap: 11px;
  margin-top: 28px;
  padding-top: 24px;
  border-top: 1px solid #e4e8e5;
}

.registered-documents__heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2px;
}

.registered-documents__heading h3 {
  margin: 0;
  font-size: 1rem;
}

.registered-documents__heading span {
  min-width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  color: #1e5d47;
  background: #e9f1ed;
  font-size: 0.78rem;
  font-weight: 700;
}

.registered-document-card {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 14px;
  padding: 13px 14px;
  border: 1px solid #dfe5e1;
  border-radius: 14px;
  background: #fff;
}

.registered-document-card__icon {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  color: #21644d;
  background: #edf4f0;
}

.registered-document-card__identity {
  min-width: 0;
  display: grid;
  gap: 4px;
}

.registered-document-card__identity strong {
  font-size: 0.94rem;
}

.registered-document-card__identity span {
  color: #66766f;
  font-size: 0.82rem;
}

.registered-document-card__number {
  display: flex;
  align-items: center;
  gap: 5px;
}

.registered-document-card__number > span {
  min-width: 112px;
  font-variant-numeric: tabular-nums;
  letter-spacing: 0.025em;
}

.registered-document-card__number :deep(.v-btn) {
  flex: 0 0 auto;
}

.registered-document-card__images :deep(.v-btn) {
  display: inline-flex;
  gap: 7px;
  text-transform: none;
  letter-spacing: 0;
}

.registered-document-card__images :deep(.document-image-trigger) {
  position: relative;
  width: 58px;
  height: 58px;
  min-width: 58px;
  padding: 0;
  border-radius: 20px !important;
  color: #155a43 !important;
  background: #e3eee8 !important;
}

.document-image-trigger__count {
  position: absolute;
  top: -4px;
  right: -4px;
  min-width: 20px;
  height: 20px;
  display: grid;
  place-items: center;
  padding-inline: 5px;
  border: 2px solid #fff;
  border-radius: 10px;
  color: #fff;
  background: #1c644b;
  font-size: 0.66rem;
  font-weight: 800;
}

.cloud-image-icon {
  position: relative;
  width: 25px;
  height: 24px;
  display: inline-grid;
  place-items: center;
  flex: 0 0 auto;
}

.cloud-image-icon__image {
  position: absolute !important;
  right: 0;
  bottom: 0;
  border-radius: 2px;
  background: #21644d;
  color: #fff;
}

.cloud-image-icon--large {
  width: 30px;
  height: 28px;
}

.cloud-image-icon--empty {
  width: 48px;
  height: 45px;
}

.document-viewer {
  overflow: hidden;
  border-radius: 18px !important;
  background: #fff;
}

.document-viewer__header,
.document-viewer__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  padding: 16px 20px;
}

.document-viewer__header {
  border-bottom: 1px solid #e2e7e4;
}

.document-viewer__header > div:first-child,
.document-viewer__footer > div {
  min-width: 0;
  display: grid;
  gap: 3px;
}

.document-viewer__header strong {
  display: block;
  color: #173d32 !important;
  opacity: 1 !important;
  font-size: 1.05rem;
  font-weight: 750;
  line-height: 1.35;
}

.document-viewer__header span,
.document-viewer__footer span {
  color: #6a7972;
  font-size: 0.8rem;
}

.document-viewer__header-actions {
  display: flex !important;
  grid-auto-flow: column;
  align-items: center;
}

.document-viewer__back {
  color: #3e564f;
  text-transform: none;
  letter-spacing: 0;
}

.document-viewer__stage {
  position: relative;
  min-height: min(62vh, 600px);
  display: grid;
  place-items: center;
  padding: 24px 76px;
  background: #17221e;
}

.document-viewer__stage img {
  width: 100%;
  height: min(58vh, 560px);
  display: block;
  object-fit: contain;
}

.document-viewer__arrow {
  position: absolute !important;
  top: 50%;
  z-index: 1;
  transform: translateY(-50%);
}

.document-viewer__arrow--previous {
  left: 18px;
}

.document-viewer__arrow--next {
  right: 18px;
}

.document-viewer__empty {
  min-height: 330px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 9px;
  margin: 24px;
  padding: 32px;
  border: 1px dashed #8eaaa0;
  border-radius: 16px;
  color: #265e4a;
  background: #f5f9f7;
  font: inherit;
  cursor: pointer;
}

.document-viewer__empty-icon {
  width: 72px;
  height: 72px;
  display: grid;
  place-items: center;
  margin-bottom: 5px;
  border-radius: 22px;
  background: #e3eee8;
}

.document-viewer__empty small {
  color: #6a7972;
}

.document-viewer__error {
  padding-inline: 20px;
}

.document-viewer__navigation {
  display: flex;
  justify-content: center;
  gap: 9px;
  overflow-x: auto;
  padding: 12px 18px;
  border-top: 1px solid #e2e7e4;
  background: #f7f9f8;
  scrollbar-width: thin;
}

.document-viewer__navigation button {
  position: relative;
  width: 64px;
  height: 52px;
  flex: 0 0 auto;
  overflow: hidden;
  padding: 0;
  border: 2px solid transparent;
  border-radius: 9px;
  background: #e3e8e5;
  cursor: pointer;
}

.document-viewer__navigation button:hover,
.document-viewer__navigation button:focus-visible,
.document-viewer__navigation-item--active {
  border-color: #257257 !important;
}

.document-viewer__navigation img {
  width: 100%;
  height: 100%;
  display: block;
  object-fit: cover;
}

.document-viewer__navigation span {
  position: absolute;
  right: 3px;
  bottom: 3px;
  min-width: 18px;
  height: 18px;
  display: grid;
  place-items: center;
  border-radius: 9px;
  color: #fff;
  background: rgba(15, 48, 37, 0.82);
  font-size: 0.66rem;
  font-weight: 700;
}

.document-viewer__footer {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  border-top: 1px solid #e2e7e4;
}

.document-viewer__file-details {
  justify-self: start;
}

.document-viewer__add-more {
  min-width: 220px;
  justify-self: center;
  color: #fff;
  text-transform: none;
  letter-spacing: 0;
  box-shadow: 0 5px 12px rgba(20, 73, 54, 0.22);
}

.document-viewer__add-more .cloud-image-icon__image {
  background: #fff;
  color: #1c644b;
}

.document-viewer__image-actions {
  display: flex !important;
  grid-auto-flow: column;
  align-items: center;
  justify-self: end;
  gap: 2px !important;
}

.document-viewer__arrow:disabled {
  opacity: 0.5 !important;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 22px;
}

.form-grid--address {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.form-grid__full {
  grid-column: 1 / -1;
}

.form-grid__wide {
  grid-column: span 2;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 13px;
  margin-top: auto;
  padding-top: 28px;
}

.form-actions :deep(.v-btn) {
  min-width: 168px;
  text-transform: none;
  letter-spacing: 0;
}

@media (max-width: 1200px) {
  .profile-heading {
    grid-template-columns: 1fr;
  }

  .profile-workspace {
    grid-template-columns: 1fr;
  }

  .section-navigation {
    grid-template-columns: repeat(4, minmax(260px, 1fr));
    overflow-x: auto;
    padding-bottom: 8px;
    scrollbar-width: thin;
    scroll-snap-type: x proximity;
  }

  .section-card {
    scroll-snap-align: start;
  }

  .data-panel {
    width: 100%;
    min-width: 0;
  }
}

@media (max-width: 900px) {
  .account-shell {
    display: block;
  }

  .sidebar {
    position: fixed;
    inset: 0 auto 0 0;
    z-index: 1202;
    width: min(280px, 86vw);
    height: 100dvh;
    padding: 28px 10px 20px;
    border-right: 1px solid #e5e8e5;
    border-bottom: 0;
    box-shadow: 18px 0 44px rgba(18, 56, 43, 0.16);
    transform: translateX(0);
    transition: transform 180ms ease;
  }

  .account-shell--collapsed .sidebar {
    transform: translateX(-105%);
  }

  .brand {
    justify-content: flex-start;
    padding: 0 20px 38px;
  }

  .sidebar__navigation {
    flex-direction: column;
    margin-left: 0;
  }

  .sidebar__footer {
    display: block;
  }

  .navigation-item {
    width: 100%;
    min-height: 54px;
    justify-content: flex-start;
    padding: 0 22px;
  }

  .navigation-item span {
    display: inline;
  }

  .sidebar-backdrop {
    position: fixed;
    inset: 0;
    z-index: 1201;
    display: block;
    border: 0;
    background: rgba(15, 40, 31, 0.32);
    backdrop-filter: blur(2px);
  }

  .topbar {
    min-height: 78px;
    padding-inline: 20px;
  }

  .topbar__stage-banner {
    grid-column: 2;
  }

  .profile-page {
    padding: 28px 22px 40px;
  }

}

@media (max-width: 760px) {
  .topbar {
    grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  }

  .completion-summary {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .completion-summary__action {
    grid-column: 1 / -1;
    justify-self: start;
  }

  .form-grid,
  .form-grid--address {
    grid-template-columns: 1fr;
    gap: 16px;
  }

  .form-grid__wide,
  .form-grid__full {
    grid-column: auto;
  }

  .document-fields {
    grid-template-columns: 1fr;
  }

  .phone-editor__actions {
    align-items: stretch;
    flex-direction: column;
  }

  .phone-editor__actions :deep(.v-btn) {
    width: 100%;
  }

  .document-fields__type {
    width: 100%;
  }

  .document-add-inline {
    width: 100%;
    margin-top: 0;
  }

  .registered-document-card {
    grid-template-columns: auto minmax(0, 1fr) auto;
  }

  .registered-document-card__images {
    grid-column: 1 / -1;
  }

  .document-viewer__stage {
    min-height: 50vh;
    padding-inline: 58px;
  }
}

@media (max-width: 620px) {
  .topbar__context,
  .notification,
  .topbar__name {
    display: none;
  }

  .topbar {
    min-height: 112px;
    display: grid;
    grid-template-columns: 1fr auto;
    grid-template-rows: 48px 56px;
    padding: 0 16px 8px;
  }

  .topbar__start {
    grid-column: 1;
    grid-row: 1;
    display: flex;
  }

  .topbar__stage-banner {
    grid-column: 1 / -1;
    grid-row: 2;
    justify-self: center;
  }

  .topbar__account {
    grid-column: 2;
    grid-row: 1;
  }

  .profile-heading__title {
    align-items: flex-start;
  }

  .profile-heading__icon {
    width: 56px;
    height: 56px;
    border-radius: 18px;
  }

  .completion-summary {
    gap: 15px;
    padding: 16px;
  }

  .data-panel {
    min-height: 0;
    padding: 24px 18px;
  }

  .contact-heading {
    display: grid;
    gap: 12px;
  }

  .contact-heading__count {
    justify-self: start;
  }

  .phone-editor {
    padding-inline: 12px;
  }

  .registered-phone-card {
    grid-template-columns: auto minmax(0, 1fr);
    gap: 10px;
    padding: 12px;
  }

  .registered-phone-card > :deep(.v-chip),
  .registered-phone-card > :deep(.v-btn) {
    grid-column: 1 / -1;
    justify-self: stretch;
  }

  .registered-phone-card > :deep(.v-btn) {
    width: 100%;
  }

  .documents-heading {
    display: grid;
    gap: 12px;
  }

  .documents-heading__count {
    justify-self: start;
  }

  .registered-document-card {
    grid-template-columns: auto minmax(0, 1fr) auto;
    gap: 10px;
    padding: 12px;
  }

  .registered-document-card__images {
    grid-column: 1 / -1;
  }

  .registered-document-card__images :deep(.v-btn) {
    width: 100%;
  }

  .document-viewer__header,
  .document-viewer__footer {
    align-items: flex-start;
    padding: 13px 14px;
  }

  .document-viewer__header-actions :deep(.v-btn:first-child) {
    min-width: 0;
    padding-inline: 8px;
  }

  .document-viewer__stage {
    min-height: 48vh;
    padding: 14px 46px;
  }

  .document-viewer__stage img {
    height: 46vh;
  }

  .document-viewer__arrow--previous {
    left: 6px;
  }

  .document-viewer__arrow--next {
    right: 6px;
  }

  .document-viewer__empty {
    min-height: 280px;
    margin: 14px;
    padding: 24px 16px;
    text-align: center;
  }

  .document-viewer__footer {
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 12px;
  }

  .document-viewer__add-more {
    grid-column: 1 / -1;
    grid-row: 2;
    width: min(100%, 280px);
  }

  .document-viewer__image-actions {
    grid-column: 2;
    grid-row: 1;
  }

  .form-actions {
    flex-direction: column-reverse;
  }

  .form-actions :deep(.v-btn) {
    width: 100%;
  }
}
</style>
