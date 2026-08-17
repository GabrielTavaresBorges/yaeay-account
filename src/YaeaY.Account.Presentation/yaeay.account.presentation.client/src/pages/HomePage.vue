<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
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
  mdiMagnify,
  mdiPiggyBankOutline,
  mdiSchoolOutline,
  mdiShieldCheckOutline,
  mdiViewGridOutline,
} from '@mdi/js'
import {
  getCachedCurrentSession,
  getCurrentSession,
  type CurrentSessionResponse,
} from '@/services/authentication-service'

const session = ref<CurrentSessionResponse | null>(getCachedCurrentSession())

const navigationItems = [
  { label: 'Início', icon: mdiHomeVariant, active: true },
  { label: 'Meus dados', icon: mdiAccountOutline, active: false },
  { label: 'Apps', icon: mdiViewGridOutline, active: false },
  { label: 'Agenda', icon: mdiCalendarMonthOutline, active: false },
]

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

onMounted(async () => {
  session.value ??= await getCurrentSession()
})
</script>

<template>
  <v-main class="home-page">
    <div class="home-shell">
      <aside class="sidebar">
        <div class="brand" aria-label="YaeaY Account">
          <span class="brand__primary">YaeaY</span>
          <span class="brand__secondary">Account</span>
        </div>

        <nav class="sidebar__navigation" aria-label="Navegação principal">
          <div
            v-for="item in navigationItems"
            :key="item.label"
            class="navigation-item"
            :class="{ 'navigation-item--active': item.active }"
            :aria-current="item.active ? 'page' : undefined"
          >
            <v-icon :icon="item.icon" size="22" />
            <span>{{ item.label }}</span>
          </div>
        </nav>

        <div class="sidebar__footer">
          <div class="navigation-item">
            <v-icon :icon="mdiCogOutline" size="23" />
            <span>Configurações</span>
          </div>
        </div>
      </aside>

      <section class="workspace">
        <header class="topbar">
          <div class="search-box" aria-label="Pesquisa indisponível nesta etapa">
            <v-icon :icon="mdiMagnify" size="24" />
            <span>Pesquisar</span>
          </div>

          <div class="topbar__account">
            <div class="notification" aria-label="Notificações">
              <v-icon :icon="mdiBellOutline" size="25" />
              <span class="notification__badge">3</span>
            </div>
            <span class="topbar__name">{{ firstName }}</span>
            <v-icon :icon="mdiChevronDown" size="21" />
          </div>
        </header>

        <div class="dashboard">
          <h1>Olá, {{ firstName }}.</h1>

          <div class="dashboard-grid">
            <section class="primary-content">
              <div class="summary-row">
                <article class="status-card">
                  <div class="status-card__content">
                    <span class="eyebrow eyebrow--light">Status da conta</span>
                    <h2>Seu ambiente<br>está pronto.</h2>
                    <span class="static-button static-button--light">Gerenciar perfil</span>
                  </div>

                  <div class="status-card__illustration" aria-hidden="true">
                    <v-icon :icon="mdiShieldCheckOutline" size="116" />
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
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  padding: 0 34px 0 42px;
  background: #fff;
  border-bottom: 1px solid #e5e8e5;
}

.search-box {
  width: min(480px, 46vw);
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
  display: flex;
  align-items: center;
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

.dashboard > h1 {
  margin: 0 0 30px;
  color: #0f4838;
  font-family: Georgia, 'Times New Roman', serif;
  font-size: clamp(2.65rem, 4vw, 4.4rem);
  font-weight: 700;
  line-height: 1;
  letter-spacing: -0.045em;
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

.static-button--light {
  background: #fff;
  color: #173d32;
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
  .home-shell {
    grid-template-columns: 92px minmax(0, 1fr);
  }

  .brand {
    padding-inline: 5px;
    justify-content: center;
  }

  .brand__secondary,
  .navigation-item span {
    display: none;
  }

  .navigation-item {
    justify-content: center;
    padding: 0;
  }

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
    position: static;
    height: auto;
    flex-direction: row;
    align-items: center;
    padding: 12px 18px;
    border-right: 0;
    border-bottom: 1px solid #e5e8e5;
  }

  .brand {
    padding: 0;
  }

  .sidebar__navigation {
    flex-direction: row;
    margin-left: auto;
  }

  .sidebar__footer {
    display: none;
  }

  .navigation-item {
    width: 44px;
    min-height: 44px;
  }

  .topbar {
    min-height: 72px;
    padding-inline: 20px;
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

@media (max-width: 620px) {
  .sidebar__navigation .navigation-item:nth-child(n+3),
  .search-box,
  .notification {
    display: none;
  }

  .topbar {
    justify-content: flex-end;
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
