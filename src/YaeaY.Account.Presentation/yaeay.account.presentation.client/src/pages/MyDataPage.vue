<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { RouteLocationRaw } from 'vue-router'
import {
  mdiAccountCircleOutline,
  mdiAccountOutline,
  mdiBellOutline,
  mdiCalendarMonthOutline,
  mdiCardAccountDetailsOutline,
  mdiCheck,
  mdiChevronDown,
  mdiCogOutline,
  mdiEmailOutline,
  mdiFileDocumentOutline,
  mdiHomeVariant,
  mdiLogoutVariant,
  mdiMapMarkerOutline,
  mdiMenu,
  mdiMenuOpen,
  mdiPhoneOutline,
  mdiShieldCheckOutline,
  mdiViewGridOutline,
} from '@mdi/js'
import StageEnvironmentBanner from '@/components/layout/StageEnvironmentBanner.vue'
import { useSidebarState } from '@/composables/use-sidebar-state'
import {
  getCachedCurrentSession,
  getCurrentSession,
  logout,
  type CurrentSessionResponse,
} from '@/services/authentication-service'

type ProfileSection = 'basic' | 'contact' | 'documents' | 'address'

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

const profile = reactive({
  fullName: session.value?.fullName ?? '',
  birthDate: '',
  gender: '',
  socialName: '',
  emailAddress: '',
  phoneNumber: '',
  cpf: '',
  rg: '',
  postalCode: '',
  street: '',
  number: '',
  complement: '',
  district: '',
  city: '',
  state: '',
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
    fields: ['emailAddress', 'phoneNumber'] as const,
  },
  {
    id: 'documents' as const,
    label: 'Documentos',
    icon: mdiFileDocumentOutline,
    fields: ['cpf', 'rg'] as const,
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

function completionFor(fields: readonly (keyof typeof profile)[]): number {
  const completed = fields.filter((field) => profile[field].trim().length > 0).length
  return Math.round((completed / fields.length) * 100)
}

const sections = computed(() => sectionDefinitions.map((section) => ({
  ...section,
  completion: completionFor(section.fields),
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
              <div>
                <h2>Cadastro {{ overallCompletion }}% completo</h2>
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
                <h2>Contato</h2>
                <div class="form-grid">
                  <v-text-field
                    v-model="profile.emailAddress"
                    class="form-grid__full"
                    label="Endereço de e-mail"
                    placeholder="nome@exemplo.com"
                    :prepend-inner-icon="mdiEmailOutline"
                    type="email"
                    variant="outlined"
                    hide-details
                  />
                  <v-text-field
                    v-model="profile.phoneNumber"
                    class="form-grid__full"
                    label="Telefone"
                    placeholder="(00) 00000-0000"
                    :prepend-inner-icon="mdiPhoneOutline"
                    type="tel"
                    variant="outlined"
                    hide-details
                  />
                </div>
              </template>

              <template v-else-if="activeSection === 'documents'">
                <h2>Documentos</h2>
                <div class="form-grid">
                  <v-text-field
                    v-model="profile.cpf"
                    label="CPF"
                    placeholder="000.000.000-00"
                    :prepend-inner-icon="mdiFileDocumentOutline"
                    variant="outlined"
                    hide-details
                  />
                  <v-text-field
                    v-model="profile.rg"
                    label="RG"
                    :prepend-inner-icon="mdiFileDocumentOutline"
                    variant="outlined"
                    hide-details
                  />
                </div>
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

              <div class="privacy-note">
                <v-icon :icon="mdiShieldCheckOutline" size="23" />
                <span>Seus dados são protegidos e usados apenas para manter sua conta atualizada.</span>
              </div>

              <div class="form-actions">
                <v-btn variant="outlined" size="large" @click="showPendingIntegration">Cancelar</v-btn>
                <v-btn type="submit" size="large" color="#17543f">Salvar alterações</v-btn>
              </div>
            </v-form>
          </div>
        </div>
      </section>
    </div>

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

.privacy-note {
  min-height: 56px;
  display: flex;
  align-items: center;
  gap: 13px;
  margin-top: 28px;
  padding: 12px 17px;
  border-radius: 13px;
  color: #315f50;
  background: #edf4f0;
  font-size: 0.84rem;
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

  .form-actions {
    flex-direction: column-reverse;
  }

  .form-actions :deep(.v-btn) {
    width: 100%;
  }
}
</style>
