<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { RouteLocationRaw } from 'vue-router'
import {
  mdiAccountOutline,
  mdiBallotOutline,
  mdiBellOutline,
  mdiBriefcaseOutline,
  mdiCalendarMonthOutline,
  mdiCheck,
  mdiChevronDown,
  mdiCogOutline,
  mdiDotsVertical,
  mdiHeartPulse,
  mdiHomeVariant,
  mdiLogoutVariant,
  mdiMagnify,
  mdiMenu,
  mdiMenuOpen,
  mdiPiggyBankOutline,
  mdiSchoolOutline,
  mdiShieldCheckOutline,
  mdiViewGridOutline,
} from '@mdi/js'
import {
  getCachedCurrentSession,
  getCurrentSession,
  logout,
  type CurrentSessionResponse,
} from '@/services/authentication-service'
import StageEnvironmentBanner from '@/components/layout/StageEnvironmentBanner.vue'
import { useSidebarState } from '@/composables/use-sidebar-state'
import { getMyData } from '@/services/users/users-service'

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
const profileCompletion = ref(0)

const baseNavigationItems = [
  { label: 'Home', icon: mdiHomeVariant, active: true, to: { name: 'home' } },
  { label: 'Meus dados', icon: mdiAccountOutline, active: false, to: { name: 'my-data-section', params: { section: 'basic' } } },
  { label: 'Apps', icon: mdiViewGridOutline, active: false, to: null },
  { label: 'Calendário', icon: mdiCalendarMonthOutline, active: false, to: null },
]

const navigationItems = computed(() => [
  ...baseNavigationItems,
  ...(session.value?.canManageAccount
    ? [{ label: 'Administração', icon: mdiShieldCheckOutline, active: false, to: { name: 'administration' } }]
    : []),
])

const futureApps = [
  { title: 'Gerenciamento Financeiro', icon: mdiPiggyBankOutline },
  { title: 'Gerenciamento Educacional', icon: mdiSchoolOutline },
  { title: 'Gerenciamento Trabalhista', icon: mdiBriefcaseOutline },
  { title: 'Gerenciamento Político', icon: mdiBallotOutline },
]

const agendaItems = [
  { title: 'Medicamento', time: '08:00', color: '#2f86d8' },
  { title: 'Consulta', time: '14:30', color: '#3d9657' },
  { title: 'Revisar documentos', time: '19:00', color: '#e7bd29' },
]

const today = new Date()
const mondayOffset = (today.getDay() + 6) % 7
const weekDays = Array.from({ length: 7 }, (_, index) => {
  const date = new Date(today)
  date.setDate(today.getDate() - mondayOffset + index)

  return {
    label: new Intl.DateTimeFormat('pt-BR', { weekday: 'short' })
      .format(date)
      .replace('.', '')
      .toUpperCase(),
    day: date.getDate(),
    isToday: date.toDateString() === today.toDateString(),
  }
})

const firstName = computed(() =>
  session.value?.fullName.trim().split(/\s+/).at(0) ?? 'Usuário')

const formattedLastLogin = computed(() => {
  if (!session.value?.lastLoginAt) return 'Primeiro acesso'

  const value = new Date(session.value.lastLoginAt)
  const time = new Intl.DateTimeFormat('pt-BR', {
    hour: '2-digit',
    minute: '2-digit',
  }).format(value)

  return value.toDateString() === today.toDateString()
    ? `Hoje, ${time}`
    : new Intl.DateTimeFormat('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
      }).format(value)
})

const compactDate = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: 'short',
}).format(today).replace('.', '').toUpperCase()

const fullDate = `Hoje, ${new Intl.DateTimeFormat('pt-BR', {
  day: 'numeric',
  month: 'long',
}).format(today)}`

const completionMessage = computed(() => profileCompletion.value === 100
  ? 'Cadastro completo'
  : `${profileCompletion.value}% do cadastro concluído`)

function calculateProfileCompletion(data: Awaited<ReturnType<typeof getMyData>>): number {
  const basic = [data.fullName, data.birthDate, data.gender].filter(Boolean).length / 3
  const contact = data.phones.length > 0 ? 1 : 0
  const documents = data.documents.length > 0 ? 1 : 0
  const address = 0
  return Math.round(((basic + contact + documents + address) / 4) * 100)
}

async function navigateTo(to: RouteLocationRaw | null): Promise<void> {
  if (!to) return

  closeSidebar()
  await router.push(to)
}

onMounted(async () => {
  session.value = await getCurrentSession(true)
  try {
    profileCompletion.value = calculateProfileCompletion(await getMyData())
  } catch {
    profileCompletion.value = 0
  }
})

async function handleLogout() {
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
</script>

<template>
  <v-main class="home-page">
    <div
      class="home-shell"
      :class="{ 'home-shell--collapsed': isSidebarCollapsed }"
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
                  'navigation-item--active': item.active,
                  'navigation-item--disabled': !item.to,
                }"
                :aria-current="item.active ? 'page' : undefined"
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
          <div class="navigation-item">
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

            <div class="search-box" aria-label="Pesquisa indisponível nesta etapa">
              <v-icon :icon="mdiMagnify" size="24" />
              <span>Pesquisar</span>
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

              <v-list
                class="account-menu"
                density="compact"
                min-width="190"
                bg-color="#ffffff"
              >
                <v-list-item
                  class="account-menu__logout"
                  :disabled="isLoggingOut"
                  :ripple="false"
                  base-color="#173d32"
                  color="#173d32"
                  title="Sair"
                  @click="handleLogout"
                >
                  <template #prepend>
                    <v-progress-circular
                      v-if="isLoggingOut"
                      indeterminate
                      size="20"
                      width="2"
                    />
                    <v-icon v-else :icon="mdiLogoutVariant" size="21" />
                  </template>
                </v-list-item>
              </v-list>
            </v-menu>
          </div>
        </header>

        <div class="dashboard">
          <div class="dashboard-grid">
            <section class="primary-content">
              <div class="summary-row">
                <article class="status-card">
                  <div class="status-card__content">
                    <span class="eyebrow eyebrow--light">Status da conta</span>
                    <h2>{{ completionMessage }}</h2>
                    <p>Complete seus dados para aproveitar todos os recursos do Account.</p>
                  </div>

                  <div class="status-card__illustration" aria-hidden="true">
                    <v-progress-circular :model-value="profileCompletion" :size="116" :width="8" color="#d5eadf" bg-color="#3e7765">
                      <strong>{{ profileCompletion }}%</strong>
                    </v-progress-circular>
                    <span class="status-card__check">
                      <v-icon :icon="mdiCheck" size="32" />
                    </span>
                  </div>
                </article>

                <article class="last-login-card">
                  <span class="eyebrow">Último login</span>
                  <strong>{{ formattedLastLogin }}</strong>
                  <div class="last-login-card__security">
                    <v-icon :icon="mdiShieldCheckOutline" size="22" />
                    <span>Sessão protegida</span>
                  </div>
                </article>
              </div>

              <section class="apps-section">
                <h2 class="section-title">Apps disponíveis</h2>

                <article class="available-app">
                  <div class="available-app__icon">
                    <v-icon :icon="mdiHeartPulse" size="56" />
                  </div>
                  <div class="available-app__content">
                    <h3>Gerenciamento de Saúde</h3>
                    <p>Cuide do corpo e da mente, organize informações de saúde, tratamentos e rotinas.</p>
                  </div>
                  <span class="static-button static-button--green">Acessar</span>
                </article>

                <h3 class="future-heading">Em breve</h3>
                <div class="future-apps">
                  <article v-for="app in futureApps" :key="app.title" class="future-app">
                    <span class="future-app__icon">
                      <v-icon :icon="app.icon" size="38" />
                    </span>
                    <h4>{{ app.title }}</h4>
                    <span class="future-app__badge">Em breve</span>
                  </article>
                </div>
              </section>
            </section>

            <aside class="agenda-card">
              <header class="agenda-card__header">
                <h2>Agenda</h2>
                <v-icon :icon="mdiCalendarMonthOutline" size="26" />
              </header>

              <div class="agenda-card__date-selector">
                <span class="agenda-card__date">{{ compactDate }}</span>
              </div>

              <div class="week-days">
                <div
                  v-for="day in weekDays"
                  :key="`${day.label}-${day.day}`"
                  class="week-day"
                  :class="{ 'week-day--today': day.isToday }"
                >
                  <span>{{ day.label }}</span>
                  <strong>{{ day.day }}</strong>
                </div>
              </div>

              <div class="agenda-divider" />
              <h3 class="agenda-card__today">{{ fullDate }}</h3>

              <div class="agenda-items">
                <article v-for="item in agendaItems" :key="item.title" class="agenda-item">
                  <span class="agenda-item__dot" :style="{ backgroundColor: item.color }" />
                  <span class="agenda-item__checkbox" aria-hidden="true" />
                  <span class="agenda-item__title">{{ item.title }}</span>
                  <time>{{ item.time }}</time>
                  <v-icon :icon="mdiDotsVertical" size="19" />
                </article>
              </div>

              <span class="agenda-card__add">Adicionar compromisso</span>
            </aside>
          </div>
        </div>
      </section>
    </div>

    <v-snackbar v-model="showLogoutError" color="error" timeout="5000">
      Não foi possível sair da conta. Tente novamente.
    </v-snackbar>
  </v-main>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
  background: #f8f9f7;
  color: #173d32;
}

.home-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 230px minmax(0, 1fr);
}

.home-shell--collapsed {
  grid-template-columns: 92px minmax(0, 1fr);
}

.home-shell--collapsed .brand {
  justify-content: center;
  padding-inline: 5px;
}

.home-shell--collapsed .brand__secondary,
.home-shell--collapsed .navigation-item span {
  display: none;
}

.home-shell--collapsed .navigation-item {
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
  min-height: 54px;
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 0 22px;
  border-radius: 18px;
  color: #4c5753;
  font-size: 1rem;
  user-select: none;
  width: 100%;
  border: 0;
  background: transparent;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.navigation-item--disabled {
  cursor: default;
}

.navigation-item--active {
  background: #e7ece9;
  color: #153f33;
  font-weight: 650;
}

.navigation-item--active span {
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
  min-height: 90px;
  display: grid;
  grid-template-columns: minmax(260px, 1fr) auto minmax(150px, 1fr);
  align-items: center;
  gap: 24px;
  padding: 0 34px 0 42px;
  background: #fff;
  border-bottom: 1px solid #e5e8e5;
}

.topbar__start {
  grid-column: 1;
  min-width: 0;
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
}

.sidebar-toggle {
  flex: 0 0 auto;
  color: #334d44;
}

.topbar__stage-banner {
  grid-column: 2;
  justify-self: center;
}

.search-box {
  width: min(480px, 100%);
  height: 52px;
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 0 18px;
  border: 1px solid #dce2de;
  border-radius: 13px;
  color: #77837e;
  background: #fbfcfb;
}

.topbar__account {
  grid-column: 3;
  justify-self: end;
  display: flex;
  align-items: center;
  gap: 11px;
}

.topbar__user-button {
  display: inline-flex;
  align-items: center;
  gap: 11px;
  min-height: 44px;
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

.topbar__user-button:focus-visible {
  outline: 2px solid #1f6b55;
  outline-offset: 2px;
}

.account-menu {
  color: #173d32;
  background: #fff;
}

:deep(.account-menu__logout .v-list-item__overlay) {
  background-color: #183729 !important;
}

:deep(.account-menu__logout:hover .v-list-item__overlay),
:deep(.account-menu__logout:focus-visible .v-list-item__overlay) {
  opacity: 0.08;
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
  background: #e3b94f;
  color: #173d32;
  font-size: 0.67rem;
  font-weight: 800;
}

.topbar__name {
  font-weight: 650;
}

.dashboard {
  max-width: 1370px;
  margin: 0 auto;
  padding: 46px 42px 54px;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: minmax(600px, 1fr) minmax(320px, 390px);
  gap: 42px;
  align-items: stretch;
}

.primary-content {
  min-width: 0;
}

.summary-row {
  display: grid;
  grid-template-columns: minmax(430px, 1fr) 210px;
  gap: 18px;
}

.status-card,
.last-login-card,
.available-app,
.future-app,
.agenda-card {
  border-radius: 18px;
}

.status-card {
  min-height: 254px;
  display: flex;
  justify-content: space-between;
  overflow: hidden;
  padding: 40px 36px;
  color: #fff;
  background: linear-gradient(128deg, #0b4b38, #123e31 70%, #163a31);
  box-shadow: 0 10px 24px rgba(19, 62, 49, 0.13);
}

.eyebrow {
  display: block;
  color: #668077;
  font-size: 0.7rem;
  font-weight: 800;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.eyebrow--light {
  color: #a7c2b8;
}

.status-card h2 {
  margin: 25px 0 24px;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(2rem, 3vw, 2.65rem);
  font-weight: 700;
  line-height: 1.12;
}

.status-card p {
  max-width: 340px;
  margin: 0;
  color: #c9ddd4;
  font-size: .94rem;
  line-height: 1.5;
}

.status-card__illustration strong {
  color: #fff;
  font-size: 1.25rem;
}

.static-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 43px;
  padding: 0 28px;
  border-radius: 999px;
  font-size: 0.76rem;
  font-weight: 800;
  letter-spacing: 0.035em;
  text-transform: uppercase;
  user-select: none;
}

.static-button--green {
  min-width: 150px;
  color: #fff;
  background: linear-gradient(110deg, #064d38, #0a3f31);
}

.status-card__illustration {
  position: relative;
  align-self: center;
  color: rgba(173, 201, 190, 0.35);
}

.status-card__check {
  position: absolute;
  right: -6px;
  bottom: 10px;
  width: 61px;
  height: 61px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #fff;
  color: #0b523c;
}

.last-login-card {
  min-height: 254px;
  display: flex;
  flex-direction: column;
  padding: 40px 28px 30px;
  border: 1px solid #dfe4e1;
  background: #fff;
  box-shadow: 0 7px 18px rgba(23, 61, 50, 0.045);
}

.last-login-card strong {
  margin-top: 24px;
  color: #174838;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: 1.55rem;
  font-weight: 700;
  line-height: 1.2;
}

.last-login-card__security {
  display: flex;
  align-items: center;
  gap: 9px;
  margin-top: auto;
  color: #65736e;
  font-size: 0.87rem;
}

.last-login-card__security .v-icon {
  color: #83ac8d;
}

.apps-section {
  margin-top: 31px;
}

.section-title {
  margin: 0 0 14px;
  color: #173e32;
  font-size: 1.08rem;
  font-weight: 800;
  text-transform: uppercase;
}

.available-app {
  min-height: 138px;
  display: grid;
  grid-template-columns: 112px minmax(0, 1fr) auto;
  align-items: center;
  gap: 24px;
  padding: 22px 28px;
  border: 1px solid #d8dedb;
  background: #fff;
  box-shadow: 0 6px 15px rgba(17, 60, 46, 0.045);
}

.available-app__icon {
  width: 90px;
  height: 90px;
  display: grid;
  place-items: center;
  border-radius: 18px;
  color: #628e76;
  background: linear-gradient(145deg, #e0e9e3, #f3f5f3);
}

.available-app__content h3 {
  margin: 0 0 8px;
  color: #153f33;
  font-size: 1.28rem;
  font-weight: 750;
}

.available-app__content p {
  max-width: 510px;
  margin: 0;
  color: #65706c;
  font-size: 0.94rem;
  line-height: 1.55;
}

.future-heading {
  margin: 23px 0 12px;
  color: #4d5c56;
  font-size: 0.99rem;
  font-weight: 650;
}

.future-apps {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 18px;
}

.future-app {
  min-height: 190px;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20px 14px 15px;
  text-align: center;
  border: 1px solid #e0e4e1;
  background: #fff;
  box-shadow: 0 5px 13px rgba(17, 60, 46, 0.04);
}

.future-app__icon {
  width: 64px;
  height: 64px;
  display: grid;
  place-items: center;
  border-radius: 15px;
  color: #557f6a;
  background: #edf1ee;
}

.future-app h4 {
  margin: 12px 0;
  color: #173e32;
  font-size: 0.9rem;
  font-weight: 650;
  line-height: 1.25;
}

.future-app__badge {
  margin-top: auto;
  padding: 6px 17px;
  border-radius: 999px;
  color: #64706b;
  background: #eff1ef;
  font-size: 0.67rem;
  font-weight: 700;
  text-transform: uppercase;
}

.agenda-card {
  height: 100%;
  min-height: 690px;
  padding: 31px 27px 28px;
  border: 1px solid #dfe4e1;
  background: #fff;
  box-shadow: 0 8px 20px rgba(17, 60, 46, 0.045);
}

.agenda-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.agenda-card__header h2 {
  margin: 0;
  color: #173e32;
  font-size: 1.5rem;
  font-weight: 750;
}

.agenda-card__date-selector {
  display: flex;
  justify-content: center;
  margin: 31px 0 24px;
}

.agenda-card__date {
  min-width: 120px;
  padding: 10px 18px;
  text-align: center;
  border: 1px solid #e0e4e1;
  border-radius: 12px;
  color: #284c41;
  font-size: 0.9rem;
  font-weight: 750;
}

.week-days {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 2px;
}

.week-day {
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 11px;
  color: #5f6b67;
}

.week-day span {
  font-size: 0.65rem;
  font-weight: 750;
}

.week-day strong {
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  color: #345148;
  font-size: 0.88rem;
  font-weight: 650;
}

.week-day--today strong {
  color: #fff;
  background: #174c3a;
}

.week-day--today::after {
  width: 5px;
  height: 5px;
  content: '';
  border-radius: 50%;
  background: #174c3a;
}

.agenda-divider {
  height: 1px;
  margin: 22px 0;
  background: #e3e6e4;
}

.agenda-card__today {
  margin: 0 0 16px;
  color: #20483b;
  font-size: 0.9rem;
  font-weight: 650;
}

.agenda-items {
  display: flex;
  flex-direction: column;
  gap: 13px;
}

.agenda-item {
  min-height: 68px;
  display: grid;
  grid-template-columns: 10px 22px minmax(0, 1fr) auto 18px;
  align-items: center;
  gap: 12px;
  padding: 0 12px;
  border: 1px solid #e0e4e1;
  border-radius: 13px;
  color: #334940;
  box-shadow: 0 3px 9px rgba(15, 57, 43, 0.045);
}

.agenda-item__dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
}

.agenda-item__checkbox {
  width: 20px;
  height: 20px;
  border: 1.5px solid #87938e;
  border-radius: 4px;
}

.agenda-item__title,
.agenda-item time {
  font-size: 0.82rem;
}

.agenda-item time {
  color: #61706a;
}

.agenda-card__add {
  width: 78%;
  min-height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 34px auto 0;
  border-radius: 999px;
  color: #44655a;
  background: #eef1ef;
  font-size: 0.88rem;
  font-weight: 600;
  user-select: none;
}

@media (max-width: 1200px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
  }

  .agenda-card {
    min-height: auto;
  }
}

@media (max-width: 900px) {
  .home-shell {
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

  .home-shell--collapsed .sidebar {
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
    min-height: 72px;
    padding-inline: 20px;
  }

  .topbar__stage-banner {
    grid-column: 2;
  }

  .search-box {
    width: min(390px, 62vw);
  }

  .dashboard {
    padding: 34px 22px 42px;
  }

  .summary-row {
    grid-template-columns: 1fr;
  }

  .last-login-card {
    min-height: 160px;
  }

  .future-apps {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 760px) {
  .search-box {
    display: none;
  }

  .topbar {
    grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  }
}

@media (max-width: 620px) {
  .search-box,
  .notification {
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

  .status-card__illustration {
    display: none;
  }

  .available-app {
    grid-template-columns: 74px 1fr;
    padding: 20px;
  }

  .available-app__icon {
    width: 68px;
    height: 68px;
  }

  .available-app .static-button {
    grid-column: 1 / -1;
    width: 100%;
  }

  .future-apps {
    grid-template-columns: 1fr;
  }

  .agenda-card {
    padding-inline: 16px;
  }

  .agenda-item {
    grid-template-columns: 9px 20px minmax(0, 1fr) auto;
  }

  .agenda-item .v-icon {
    display: none;
  }
}
</style>
