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
  mdiClose,
  mdiDeleteOutline,
  mdiDeleteSweepOutline,
  mdiEyeOffOutline,
  mdiEyeOutline,
  mdiFileDocumentOutline,
  mdiHomeVariant,
  mdiHistory,
  mdiImageOutline,
  mdiLogoutVariant,
  mdiMapMarkerOutline,
  mdiMenu,
  mdiMenuOpen,
  mdiPhoneOutline,
  mdiPencilOutline,
  mdiPlus,
  mdiStar,
  mdiStarOutline,
  mdiViewGridOutline,
} from '@mdi/js'
import StageEnvironmentBanner from '@/components/layout/StageEnvironmentBanner.vue'
import { CpfField, UserPhonesField } from '@/components/inputs'
import { useSidebarState } from '@/composables/use-sidebar-state'
import { formatCpf } from '@/validators/fields/cpf'
import type { PhoneModel } from '@/models/phone-model'
import { getPhoneDigitsRange } from '@/services/phoneFormat/phone-format-service'
import { getMyData, updateUser } from '@/services/users/users-service'
import { genderItems, type Gender } from '@/constants/gender'
import {
  getCachedCurrentSession,
  getCurrentSession,
  logout,
  type CurrentSessionResponse,
} from '@/services/authentication-service'

type ProfileSection = 'basic' | 'contact' | 'documents' | 'address'
type DocumentType = 'cpf' | 'rg' | 'cnh' | 'passport'

interface DocumentFieldDefinition {
  key: string
  label: string
  placeholder?: string
  type?: 'text' | 'date'
  fixed?: boolean
}

interface DocumentImageDraft {
  id: string
  file: File
  previewUrl: string
}

interface UserDocumentDraft {
  id: string
  type: DocumentType
  number: string
  details: Record<string, string>
  images: DocumentImageDraft[]
  history: UserDocumentHistoryDraft[]
}

interface UserDocumentHistoryDraft {
  id: string
  number: string
  details: Record<string, string>
  images: DocumentImageDraft[]
  registeredAt: string
}

interface UserPhoneDraft {
  id: string
  isPersisted: boolean
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
  description: string
  available: boolean
  numberIsFixed: boolean
  fields: DocumentFieldDefinition[]
}> = [
  {
    value: 'cpf',
    title: 'CPF',
    numberLabel: 'Número do CPF',
    placeholder: '000.000.000-00',
    description: 'Cadastro de Pessoa Física',
    available: true,
    numberIsFixed: true,
    fields: [],
  },
  {
    value: 'rg',
    title: 'RG',
    numberLabel: 'Número do RG',
    placeholder: 'Informe o número do RG',
    description: 'Registro Geral',
    available: false,
    numberIsFixed: true,
    fields: [
      { key: 'holderName', label: 'Nome no documento', placeholder: 'Nome conforme impresso no RG' },
      { key: 'birthDate', label: 'Data de nascimento', type: 'date' },
      { key: 'birthPlace', label: 'Naturalidade', placeholder: 'Cidade e estado' },
      { key: 'parentageOne', label: 'Filiação 1', placeholder: 'Nome conforme impresso' },
      { key: 'parentageTwo', label: 'Filiação 2', placeholder: 'Nome conforme impresso' },
      { key: 'issueDate', label: 'Data de emissão', type: 'date' },
      { key: 'expirationDate', label: 'Data de validade', type: 'date' },
      { key: 'issuingAuthority', label: 'Órgão expedidor', placeholder: 'Ex.: SSP' },
      { key: 'issuingState', label: 'Estado expedidor', placeholder: 'Ex.: SP' },
    ],
  },
  {
    value: 'cnh',
    title: 'CNH',
    numberLabel: 'Número de registro',
    placeholder: 'Informe o número da CNH',
    description: 'Carteira Nacional de Habilitação',
    available: false,
    numberIsFixed: true,
    fields: [
      { key: 'holderName', label: 'Nome no documento', placeholder: 'Nome conforme impresso na CNH' },
      { key: 'cpfNumber', label: 'CPF apresentado na CNH', placeholder: '000.000.000-00' },
      { key: 'identityDocument', label: 'Documento de identidade', placeholder: 'Número e órgão emissor' },
      { key: 'birthDate', label: 'Data de nascimento', type: 'date' },
      { key: 'category', label: 'Categoria', placeholder: 'Ex.: AB' },
      { key: 'firstLicenseDate', label: 'Primeira habilitação', type: 'date', fixed: true },
      { key: 'issueDate', label: 'Data de emissão', type: 'date' },
      { key: 'expirationDate', label: 'Data de validade', type: 'date' },
      { key: 'issuingState', label: 'Estado expedidor', placeholder: 'Ex.: SP' },
      { key: 'renach', label: 'RENACH', placeholder: 'Informe o RENACH' },
      { key: 'observations', label: 'Observações', placeholder: 'Restrições e observações impressas' },
    ],
  },
  {
    value: 'passport',
    title: 'Passaporte',
    numberLabel: 'Número do passaporte',
    placeholder: 'Informe o número do passaporte',
    description: 'Documento de viagem',
    available: false,
    numberIsFixed: false,
    fields: [
      { key: 'holderName', label: 'Nome no documento', placeholder: 'Nome conforme impresso no passaporte' },
      { key: 'birthDate', label: 'Data de nascimento', type: 'date' },
      { key: 'birthPlace', label: 'Local de nascimento', placeholder: 'Cidade e país' },
      { key: 'sex', label: 'Sexo', placeholder: 'Conforme impresso' },
      { key: 'nationality', label: 'Nacionalidade', placeholder: 'Ex.: Brasileira' },
      { key: 'issuingCountry', label: 'País emissor', placeholder: 'Ex.: Brasil' },
      { key: 'issuingAuthority', label: 'Autoridade emissora', placeholder: 'Ex.: Polícia Federal' },
      { key: 'issueDate', label: 'Data de emissão', type: 'date' },
      { key: 'expirationDate', label: 'Data de validade', type: 'date' },
    ],
  },
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
const imageViewerDocumentId = ref<string | null>(null)
const imageViewerIndex = ref(0)
const historyDocumentType = ref<DocumentType | null>(null)
const openedDocumentCards = ref<DocumentType | null>(null)
const phoneFormError = ref('')
const newPhoneIsPrimary = ref(false)
const isLoadingMyData = ref(true)
const myDataLoadError = ref('')
const isSaving = ref(false)
const saveMessage = ref('')
const saveError = ref('')
const isSaveMessageVisible = ref(false)
const isSaveErrorVisible = ref(false)

const profile = reactive({
  fullName: '',
  birthDate: '',
  gender: '' as Gender | '',
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
const editingPhoneId = ref<string | null>(null)
const phoneNumberVisibility = reactive<Record<string, boolean>>({})
const phoneVisibilityTimers = new Map<string, ReturnType<typeof setTimeout>>()

const registeredDocuments = ref<UserDocumentDraft[]>(documentDefinitions.map((definition) => ({
  id: definition.value,
  type: definition.value,
  number: '',
  details: Object.fromEntries(definition.fields.map((field) => [field.key, ''])),
  images: [],
  history: [],
})))
const documentNumberVisibility = reactive<Record<string, boolean>>({})
const documentVisibilityTimers = new Map<string, ReturnType<typeof setTimeout>>()

const completedDocumentCount = computed(() =>
  registeredDocuments.value.filter((document) =>
    document.number.trim().length > 0 || document.history.length > 0).length)

const cpfDocument = computed(() =>
  registeredDocuments.value.find((document) => document.type === 'cpf')!)

function documentDefinition(type: DocumentType) {
  return documentDefinitions.find((definition) => definition.value === type)
    ?? documentDefinitions[0]!
}

function isDocumentNumberLocked(document: UserDocumentDraft): boolean {
  return document.history.length > 0 && documentDefinition(document.type).numberIsFixed
}

function openDocumentHistory(type: DocumentType): void {
  historyDocumentType.value = type
}

function formatHistoryDate(value: string): string {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

function filledHistoryDetails(document: UserDocumentDraft, history: UserDocumentHistoryDraft) {
  return documentDefinition(document.type).fields
    .map((field) => ({ ...field, value: history.details[field.key] ?? '' }))
    .filter((field) => field.value.trim().length > 0)
}

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

const historyDocument = computed(() =>
  registeredDocuments.value.find((document) => document.type === historyDocumentType.value) ?? null)

const isHistoryDialogOpen = computed({
  get: () => historyDocument.value !== null,
  set: (value: boolean) => {
    if (!value) historyDocumentType.value = null
  },
})

const sectionDefinitions = [
  {
    id: 'basic' as const,
    label: 'Dados básicos',
    icon: mdiCardAccountDetailsOutline,
    fields: ['fullName', 'birthDate', 'gender'] as const,
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
    return completedDocumentCount.value > 0 ? 100 : 0
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

async function loadMyData(): Promise<void> {
  isLoadingMyData.value = true
  myDataLoadError.value = ''
  try {
    // A tela autenticada é preenchida exclusivamente pela projeção account_read.
    const myData = await getMyData()
    profile.fullName = myData.fullName
    profile.birthDate = myData.birthDate
    profile.gender = myData.gender
    registeredPhones.value = myData.phones.map((phone) => ({
      id: phone.id,
      isPersisted: true,
      phone: {
        callingCode: phone.callingCode,
        country: phone.country,
        areaCode: phone.areaCode,
        phoneType: phone.phoneType,
        number: phone.number,
      } as PhoneModel,
      isPrimary: phone.isPrimary,
    }))

    for (const document of myData.documents) {
      const type = document.type.toLowerCase() as DocumentType
      const draft = registeredDocuments.value.find((item) => item.type === type)
      if (!draft || !document.number) continue

      draft.number = document.number
      draft.history = [{
        id: document.id,
        number: document.number,
        details: {},
        images: [],
        registeredAt: document.createdAt,
      }]
    }
  } catch {
    myDataLoadError.value = 'Não foi possível carregar seus dados básicos. Atualize a página para tentar novamente.'
  } finally {
    isLoadingMyData.value = false
  }
}

onMounted(async () => {
  session.value ??= await getCurrentSession()
  await loadMyData()
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
    isPersisted: false,
    phone: { ...phoneForm.value },
    isPrimary: willBePrimary,
  })
  phoneNumberVisibility[phoneId] = false

  phoneForm.value = createDefaultPhone()
  newPhoneIsPrimary.value = false
}

function resetPhoneEditor(): void {
  editingPhoneId.value = null
  phoneForm.value = createDefaultPhone()
  newPhoneIsPrimary.value = false
  phoneFormError.value = ''
}

function beginPhoneEdit(phoneItem: UserPhoneDraft): void {
  editingPhoneId.value = phoneItem.id
  phoneForm.value = { ...phoneItem.phone }
  newPhoneIsPrimary.value = phoneItem.isPrimary
  phoneFormError.value = ''
}

function updatePhone(): void {
  const phoneId = editingPhoneId.value
  const phoneItem = registeredPhones.value.find((item) => item.id === phoneId)
  if (!phoneId || !phoneItem) {
    resetPhoneEditor()
    return
  }

  if (!isValidPhone(phoneForm.value)) {
    phoneFormError.value = 'Informe um telefone vÃ¡lido antes de atualizar.'
    return
  }

  if (registeredPhones.value.some((item) =>
    item.id !== phoneId && phoneIdentity(item.phone) === phoneIdentity(phoneForm.value))) {
    phoneFormError.value = 'Este telefone jÃ¡ foi adicionado.'
    return
  }

  phoneItem.phone = { ...phoneForm.value }
  if (newPhoneIsPrimary.value)
    makePhonePrimary(phoneId)

  resetPhoneEditor()
}

function removePhone(phoneId: string): void {
  const phoneItem = registeredPhones.value.find((item) => item.id === phoneId)
  if (!phoneItem) return

  if (registeredPhones.value.length <= 1) {
    phoneFormError.value = 'Ã‰ necessÃ¡rio manter ao menos um telefone cadastrado.'
    return
  }

  if (phoneItem.isPrimary) {
    phoneFormError.value = 'Defina outro telefone como principal antes de removÃª-lo.'
    return
  }

  registeredPhones.value = registeredPhones.value.filter((item) => item.id !== phoneId)
  hidePhoneNumber(phoneId)

  if (editingPhoneId.value === phoneId)
    resetPhoneEditor()
}

function makePhonePrimary(phoneId: string): void {
  registeredPhones.value.forEach((item) => {
    item.isPrimary = item.id === phoneId
  })
}

async function saveChanges(): Promise<void> {
  if (isSaving.value) return

  if (activeSection.value === 'documents' || activeSection.value === 'address') {
    showPendingIntegration()
    return
  }

  isSaving.value = true
  saveMessage.value = ''
  saveError.value = ''
  isSaveMessageVisible.value = false
  isSaveErrorVisible.value = false

  try {
    const response = activeSection.value === 'basic'
      ? await updateUser({
        fullName: profile.fullName,
        birthDate: profile.birthDate,
        gender: profile.gender || undefined,
      })
      : await updateUser({
        phones: registeredPhones.value.map((item) => ({
          ...(item.isPersisted ? { id: item.id } : {}),
          callingCode: item.phone.callingCode,
          regionCode: item.phone.country,
          areaCode: item.phone.areaCode,
          phoneType: item.phone.phoneType,
          phoneNumber: item.phone.number,
          isPrimary: item.isPrimary,
        })),
      })

    if (activeSection.value === 'basic' && session.value) {
      session.value = { ...session.value, fullName: profile.fullName }
    }

    saveMessage.value = response.updatedFields.length
      ? 'Alterações salvas. Seus dados serão atualizados em instantes.'
      : 'Não há alterações para salvar.'
    isSaveMessageVisible.value = true
  } catch (error) {
    saveError.value = error instanceof Error || (typeof error === 'object' && error !== null && 'message' in error)
      ? String((error as { message: unknown }).message)
      : 'Não foi possível salvar suas alterações. Tente novamente.'
    isSaveErrorVisible.value = true
  } finally {
    isSaving.value = false
  }
}

async function cancelChanges(): Promise<void> {
  if (isSaving.value) return
  await loadMyData()
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

function documentTitle(type: DocumentType): string {
  return documentDefinitions.find((document) => document.value === type)?.title ?? type
}

function displayDocumentNumber(document: Pick<UserDocumentDraft, 'type' | 'number'>): string {
  return document.type === 'cpf' ? formatCpf(document.number) : document.number
}

function maskDocumentNumber(document: Pick<UserDocumentDraft, 'type' | 'number'>): string {
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
  const previewUrls = new Set<string>()
  registeredDocuments.value.forEach((document) => {
    document.images.forEach((image) => previewUrls.add(image.previewUrl))
    document.history.forEach((history) =>
      history.images.forEach((image) => previewUrls.add(image.previewUrl)))
  })
  previewUrls.forEach((previewUrl) => URL.revokeObjectURL(previewUrl))
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

            <v-form class="data-panel" @submit.prevent="saveChanges">
              <template v-if="activeSection === 'basic'">
                <h2>Dados básicos</h2>
                <v-alert
                  v-if="myDataLoadError"
                  class="my-data-load-error"
                  type="error"
                  variant="tonal"
                  role="alert"
                >
                  {{ myDataLoadError }}
                </v-alert>
                <div v-else class="form-grid" :aria-busy="isLoadingMyData">
                  <v-text-field
                    v-model="profile.fullName"
                    class="form-grid__full"
                    label="Nome completo"
                    :prepend-inner-icon="mdiAccountOutline"
                    variant="outlined"
                    hide-details
                    :loading="isLoadingMyData"
                    :disabled="isLoadingMyData"
                  />
                  <v-text-field
                    v-model="profile.birthDate"
                    label="Data de nascimento"
                    :prepend-inner-icon="mdiCalendarMonthOutline"
                    type="date"
                    variant="outlined"
                    hide-details
                    :loading="isLoadingMyData"
                    :disabled="isLoadingMyData"
                  />
                  <v-select
                    v-model="profile.gender"
                    label="Gênero"
                    :prepend-inner-icon="mdiAccountOutline"
                    :items="genderItems"
                    item-title="title"
                    item-value="value"
                    variant="outlined"
                    hide-details
                    :loading="isLoadingMyData"
                    :disabled="isLoadingMyData"
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
                      :prepend-icon="editingPhoneId ? mdiCheck : mdiPlus"
                      rounded="pill"
                      color="#17543f"
                      variant="flat"
                      :disabled="!editingPhoneId && registeredPhones.length >= MAX_USER_PHONES"
                      @click="editingPhoneId ? updatePhone() : addPhone()"
                    >
                      {{ editingPhoneId ? 'Atualizar telefone' : 'Adicionar telefone' }}
                    </v-btn>
                    <v-btn
                      v-if="editingPhoneId"
                      rounded="pill"
                      variant="text"
                      color="#315f50"
                      @click="resetPhoneEditor"
                    >
                      Cancelar
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
                    <div class="registered-phone-card__controls">
                      <v-chip
                        v-if="phoneItem.isPrimary"
                        :prepend-icon="mdiStar"
                        color="#1c644b"
                        variant="tonal"
                        size="small"
                      >
                        Principal
                      </v-chip>
                      <div class="registered-phone-card__actions">
                      <v-btn
                        v-if="!phoneItem.isPrimary"
                        :prepend-icon="mdiStarOutline"
                        rounded="pill"
                        variant="text"
                        color="#315f50"
                        @click="makePhonePrimary(phoneItem.id)"
                      >
                        Tornar principal
                      </v-btn>
                      <v-tooltip text="Editar telefone" location="top">
                        <template #activator="{ props: tooltipProps }">
                          <v-btn
                            v-bind="tooltipProps"
                            :icon="mdiPencilOutline"
                            variant="text"
                            color="#315f50"
                            aria-label="Editar telefone"
                            @click="beginPhoneEdit(phoneItem)"
                          />
                        </template>
                      </v-tooltip>
                      <v-tooltip
                        :text="phoneItem.isPrimary
                          ? 'Defina outro telefone como principal antes de remover este'
                          : registeredPhones.length <= 1
                            ? 'Mantenha ao menos um telefone cadastrado'
                            : 'Remover telefone'"
                        location="top"
                      >
                        <template #activator="{ props: tooltipProps }">
                          <span v-bind="tooltipProps">
                            <v-btn
                              :icon="mdiDeleteOutline"
                              variant="text"
                              color="#a13f3f"
                              aria-label="Remover telefone"
                              :disabled="phoneItem.isPrimary || registeredPhones.length <= 1"
                              @click="removePhone(phoneItem.id)"
                            />
                          </span>
                        </template>
                      </v-tooltip>
                      </div>
                    </div>
                  </article>
                </section>
              </template>

              <template v-else-if="activeSection === 'documents'">
                <div class="documents-heading">
                  <h2>Documentos</h2>
                </div>

                <v-expansion-panels
                  v-model="openedDocumentCards"
                  class="document-type-cards"
                  aria-label="Tipos de documentos disponíveis"
                >
                  <v-expansion-panel
                    :value="cpfDocument.type"
                    class="document-type-card"
                    elevation="0"
                  >
                    <v-expansion-panel-title class="document-type-card__header">
                      <span class="document-type-card__icon">
                        <v-icon :icon="mdiFileDocumentOutline" size="25" />
                      </span>
                      <span class="document-type-card__heading">
                        <strong>CPF</strong>
                      </span>
                    </v-expansion-panel-title>

                    <v-expansion-panel-text class="document-type-card__content">
                      <div class="cpf-document-fields">
                        <v-text-field
                          model-value="CPF"
                          label="Tipo de documento"
                          :prepend-inner-icon="mdiFileDocumentOutline"
                          variant="outlined"
                          readonly
                          hide-details
                        />
                        <CpfField
                          v-model="cpfDocument.number"
                          label="Número do CPF"
                          :prepend-inner-icon="mdiCardAccountDetailsOutline"
                          variant="outlined"
                          hide-details="auto"
                          validate-on="blur"
                          :readonly="isDocumentNumberLocked(cpfDocument)"
                        />
                        <v-tooltip
                          :text="cpfDocument.images.length ? 'Visualizar imagens' : 'Adicionar imagens'"
                          location="top"
                        >
                          <template #activator="{ props: tooltipProps }">
                            <v-btn
                              v-bind="tooltipProps"
                              class="document-image-trigger cpf-document-fields__images"
                              icon
                              variant="flat"
                              color="#21644d"
                              :aria-label="cpfDocument.images.length ? 'Visualizar imagens' : 'Adicionar imagens'"
                              @click="openImageViewer(cpfDocument.id)"
                            >
                              <span class="cloud-image-icon" aria-hidden="true">
                                <v-icon :icon="mdiCloudUploadOutline" size="23" />
                                <v-icon class="cloud-image-icon__image" :icon="mdiImageOutline" size="10" />
                              </span>
                              <span v-if="cpfDocument.images.length" class="document-image-trigger__count">
                                {{ cpfDocument.images.length }}
                              </span>
                            </v-btn>
                          </template>
                        </v-tooltip>
                      </div>

                      <div class="cpf-document-history">
                        <v-tooltip text="Visualizar histórico do CPF" location="top">
                          <template #activator="{ props: tooltipProps }">
                            <v-btn
                              v-bind="tooltipProps"
                              :icon="mdiEyeOutline"
                              variant="text"
                              color="#315f50"
                              aria-label="Visualizar histórico do CPF"
                              @click="openDocumentHistory('cpf')"
                            />
                          </template>
                        </v-tooltip>
                        <span v-if="cpfDocument.history.length">{{ cpfDocument.history.length }}</span>
                      </div>
                    </v-expansion-panel-text>
                  </v-expansion-panel>
                </v-expansion-panels>
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
                <v-btn variant="outlined" size="large" :disabled="isSaving" @click="cancelChanges">Cancelar</v-btn>
                <v-btn
                  type="submit"
                  size="large"
                  color="#17543f"
                  :loading="isSaving"
                  :disabled="isSaving"
                >
                  Salvar alterações
                </v-btn>
              </div>
            </v-form>
          </div>
        </div>
      </section>
    </div>

    <v-dialog v-model="isHistoryDialogOpen" max-width="980" scrollable>
      <v-card v-if="historyDocument" class="document-history-dialog">
        <header class="document-history-dialog__header">
          <span class="document-history-dialog__icon">
            <v-icon :icon="mdiHistory" size="25" />
          </span>
          <div>
            <strong>Histórico de {{ documentTitle(historyDocument.type) }}</strong>
            <span>Cada registro preserva os dados e as imagens daquela emissão.</span>
          </div>
          <v-btn
            :icon="mdiClose"
            variant="text"
            aria-label="Fechar histórico"
            @click="isHistoryDialogOpen = false"
          />
        </header>

        <div class="document-history-dialog__body">
          <div v-if="historyDocument.history.length" class="document-history-list">
            <article
              v-for="(history, index) in historyDocument.history"
              :key="history.id"
              class="document-history-entry"
            >
              <header class="document-history-entry__header">
                <div>
                  <span class="document-history-entry__sequence">
                    Emissão {{ historyDocument.history.length - index }}
                  </span>
                  <strong>{{ maskDocumentNumber({ type: historyDocument.type, number: history.number }) }}</strong>
                </div>
                <time :datetime="history.registeredAt">Registrada em {{ formatHistoryDate(history.registeredAt) }}</time>
              </header>

              <dl v-if="filledHistoryDetails(historyDocument, history).length" class="document-history-entry__details">
                <div v-for="field in filledHistoryDetails(historyDocument, history)" :key="field.key">
                  <dt>{{ field.label }}</dt>
                  <dd>{{ field.value }}</dd>
                </div>
              </dl>

              <section class="document-history-entry__images">
                <div>
                  <strong>Imagens preservadas</strong>
                  <span>{{ history.images.length }} de {{ MAX_DOCUMENT_IMAGES }}</span>
                </div>
                <div v-if="history.images.length" class="document-history-entry__thumbnails">
                  <img
                    v-for="image in history.images"
                    :key="image.id"
                    :src="image.previewUrl"
                    :alt="`Imagem histórica de ${documentTitle(historyDocument.type)}`"
                  >
                </div>
                <p v-else>Nenhuma imagem foi vinculada a esta emissão.</p>
              </section>
            </article>
          </div>

          <div v-else class="document-history-empty">
            <span><v-icon :icon="mdiHistory" size="30" /></span>
            <strong>Nenhuma emissão registrada</strong>
            <p>Preencha os dados atuais e selecione “Registrar emissão” para montar o histórico visual.</p>
          </div>
        </div>
      </v-card>
    </v-dialog>

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
    <v-snackbar v-model="isSaveMessageVisible" color="#315f50" timeout="5000">
      {{ saveMessage }}
    </v-snackbar>
    <v-snackbar v-model="isSaveErrorVisible" color="error" timeout="5000">
      {{ saveError }}
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

.registered-phone-card__actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 2px;
}

.registered-phone-card__controls {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
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

.document-type-cards {
  display: grid;
  gap: 18px;
}

.document-type-card {
  overflow: hidden;
  border: 1px solid #dfe5e1;
  border-radius: 18px;
  background: #fff;
  box-shadow: 0 8px 24px rgba(31, 75, 59, 0.06);
}

.document-type-card::before,
.document-type-card::after {
  display: none;
}

:deep(.document-type-card__header) {
  min-height: 82px;
  display: flex;
  align-items: center;
  gap: 13px;
  padding: 16px 18px;
  background: linear-gradient(135deg, #f7faf8 0%, #eef5f1 100%);
}

:deep(.document-type-card__header:hover) {
  background: #eef5f1;
}

.document-type-card__icon {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  color: #21644d;
  background: #dfece6;
}

.document-type-card__heading {
  flex: 1 1 auto;
  min-width: 0;
  display: grid;
  gap: 2px;
}

.document-type-card__heading strong {
  color: #173f32;
  font-size: 1rem;
}

.document-type-card__heading small {
  color: #6d7c75;
  font-size: 0.8rem;
}

.document-type-card__summary {
  display: flex;
  align-items: center;
  gap: 8px;
}

.document-type-card__history-count {
  color: #547067;
  font-size: 0.72rem;
  font-weight: 700;
  white-space: nowrap;
}

.document-type-card__status {
  padding: 6px 10px;
  border: 1px solid #ded9c7;
  border-radius: 999px;
  color: #796d42;
  background: #fbf8eb;
  font-size: 0.72rem;
  font-weight: 700;
  white-space: nowrap;
}

.document-type-card__status--available {
  border-color: #c9e1d6;
  color: #1f684e;
  background: #e8f4ee;
}

:deep(.document-type-card__content .v-expansion-panel-text__wrapper) {
  padding: 0;
}

.document-data-group {
  padding: 20px 18px 4px;
  border-top: 1px solid #e7ebe8;
}

.document-data-group--versioned {
  margin-top: 12px;
  padding-top: 18px;
  background: #fcfdfc;
}

.document-data-group__heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 15px;
  color: #477061;
}

.document-data-group__heading h3 {
  margin: 0;
  color: #284b3e;
  font-size: 0.9rem;
}

.document-data-group__heading p,
.document-data-group__empty {
  margin: 4px 0 0;
  color: #74827c;
  font-size: 0.78rem;
  line-height: 1.45;
}

.document-data-group__empty {
  padding: 2px 0 18px;
}

.document-type-card__fields {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
  padding: 0 0 18px;
}

.document-type-card__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 14px 18px;
  border-top: 1px solid #edf0ee;
  background: #fbfcfb;
}

.document-type-card__image-copy {
  display: grid;
  gap: 2px;
}

.document-type-card__footer strong {
  color: #345347;
  font-size: 0.82rem;
}

.document-type-card__footer span {
  color: #79857f;
  font-size: 0.75rem;
}

.document-type-card__actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
}

.document-type-card__actions :deep(.v-btn) {
  text-transform: none;
  letter-spacing: 0;
}

.document-history-button__count {
  min-width: 20px;
  height: 20px;
  display: inline-grid;
  place-items: center;
  margin-left: 7px;
  padding-inline: 5px;
  border-radius: 10px;
  color: #fff !important;
  background: #315f50;
  font-size: 0.66rem !important;
  font-weight: 800;
}

.document-type-card__footer :deep(.document-image-trigger) {
  position: relative;
  width: 54px;
  height: 54px;
  min-width: 54px;
  border-radius: 18px !important;
  color: #155a43 !important;
  background: #e3eee8 !important;
}

.document-type-card :deep(.v-expansion-panel-title) {
  height: 78px;
  min-height: 78px;
}

:deep(.document-type-card__content .v-expansion-panel-text__wrapper) {
  min-height: 150px;
  padding: 20px 18px 14px;
  border-top: 1px solid #e7ebe8;
}

.cpf-document-fields {
  display: grid;
  grid-template-columns: 190px minmax(260px, 1fr) 56px;
  align-items: start;
  gap: 14px;
}

.cpf-document-fields :deep(.v-field) {
  min-height: 56px;
}

.cpf-document-fields :deep(.cpf-document-fields__images) {
  position: relative;
  width: 56px;
  height: 56px;
  min-width: 56px;
  border-radius: 16px !important;
  color: #155a43 !important;
  background: #e3eee8 !important;
}

.cpf-document-history {
  min-height: 44px;
  display: flex;
  align-items: center;
  gap: 2px;
  margin-top: 14px;
  padding-top: 8px;
  border-top: 1px solid #edf0ee;
}

.cpf-document-history > span {
  min-width: 20px;
  height: 20px;
  display: grid;
  place-items: center;
  border-radius: 10px;
  color: #fff;
  background: #315f50;
  font-size: 0.66rem;
  font-weight: 800;
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

.document-history-dialog {
  overflow: hidden;
  border-radius: 20px !important;
}

.document-history-dialog__header {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 13px;
  padding: 17px 20px;
  border-bottom: 1px solid #e3e8e5;
  background: #f6f9f7;
}

.document-history-dialog__icon {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  color: #1d634a;
  background: #e2eee8;
}

.document-history-dialog__header > div {
  min-width: 0;
  display: grid;
  gap: 2px;
}

.document-history-dialog__header strong {
  color: #173f32;
  font-size: 1rem;
}

.document-history-dialog__header span {
  color: #6c7b74;
  font-size: 0.8rem;
}

.document-history-dialog__body {
  max-height: 72vh;
  overflow-y: auto;
  padding: 18px;
  background: #f7f9f8;
}

.document-history-list {
  display: grid;
  gap: 14px;
}

.document-history-entry {
  overflow: hidden;
  border: 1px solid #dde4e0;
  border-radius: 16px;
  background: #fff;
}

.document-history-entry__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 14px 16px;
  border-bottom: 1px solid #edf0ee;
}

.document-history-entry__header > div {
  display: grid;
  gap: 3px;
}

.document-history-entry__sequence {
  color: #237156;
  font-size: 0.72rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.document-history-entry__header strong {
  color: #314f43;
  font-size: 0.9rem;
  font-variant-numeric: tabular-nums;
}

.document-history-entry__header time {
  color: #74827c;
  font-size: 0.75rem;
}

.document-history-entry__details {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
  margin: 0;
  padding: 15px 16px;
}

.document-history-entry__details div {
  min-width: 0;
  padding: 10px 11px;
  border-radius: 10px;
  background: #f6f8f7;
}

.document-history-entry__details dt {
  color: #77847e;
  font-size: 0.68rem;
  font-weight: 700;
}

.document-history-entry__details dd {
  margin: 3px 0 0;
  overflow-wrap: anywhere;
  color: #345347;
  font-size: 0.8rem;
}

.document-history-entry__images {
  padding: 13px 16px 16px;
  border-top: 1px solid #edf0ee;
}

.document-history-entry__images > div:first-child {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.document-history-entry__images strong {
  color: #365448;
  font-size: 0.78rem;
}

.document-history-entry__images span,
.document-history-entry__images p {
  color: #7a8781;
  font-size: 0.72rem;
}

.document-history-entry__images p {
  margin: 9px 0 0;
}

.document-history-entry__thumbnails {
  display: flex !important;
  justify-content: flex-start !important;
  gap: 8px !important;
  margin-top: 10px;
  overflow-x: auto;
}

.document-history-entry__thumbnails img {
  width: 84px;
  height: 62px;
  flex: 0 0 auto;
  object-fit: cover;
  border: 1px solid #dce3df;
  border-radius: 9px;
}

.document-history-empty {
  min-height: 280px;
  display: grid;
  place-items: center;
  align-content: center;
  gap: 8px;
  padding: 30px;
  text-align: center;
}

.document-history-empty > span {
  width: 58px;
  height: 58px;
  display: grid;
  place-items: center;
  border-radius: 18px;
  color: #28654f;
  background: #e5f0ea;
}

.document-history-empty strong {
  color: #264a3c;
}

.document-history-empty p {
  max-width: 440px;
  margin: 0;
  color: #718079;
  font-size: 0.82rem;
  line-height: 1.5;
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

  .document-type-card__fields {
    grid-template-columns: 1fr;
  }

  .cpf-document-fields {
    grid-template-columns: 1fr 1fr 56px;
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

  .registered-phone-card__controls {
    grid-column: 1 / -1;
    justify-self: stretch;
    justify-content: flex-start;
  }

  .documents-heading {
    display: grid;
    gap: 12px;
  }

  .documents-heading__count {
    justify-self: start;
  }

  .cpf-document-fields {
    grid-template-columns: 1fr 56px;
  }

  .cpf-document-fields > :first-child {
    grid-column: 1 / -1;
  }

  :deep(.document-type-card__header) {
    align-items: flex-start;
    flex-wrap: wrap;
  }

  .document-type-card__summary {
    width: 100%;
    padding-left: 57px;
  }

  .document-type-card__footer {
    flex-direction: column;
    align-items: flex-start;
  }

  .document-type-card__actions {
    width: 100%;
    justify-content: flex-start;
    flex-wrap: wrap;
  }

  .document-history-entry__header {
    align-items: flex-start;
    flex-direction: column;
  }

  .document-history-entry__details {
    grid-template-columns: 1fr;
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
