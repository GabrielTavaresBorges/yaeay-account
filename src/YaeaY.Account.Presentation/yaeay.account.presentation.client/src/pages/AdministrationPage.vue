<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  mdiAccountOutline,
  mdiAccountGroupOutline,
  mdiAlertCircleOutline,
  mdiBellOutline,
  mdiCalendarMonthOutline,
  mdiChevronDown,
  mdiChevronLeft,
  mdiChevronRight,
  mdiCogOutline,
  mdiEmailOutline,
  mdiFileDocumentOutline,
  mdiFilterOutline,
  mdiGavel,
  mdiHomeVariant,
  mdiMagnify,
  mdiMenu,
  mdiMenuOpen,
  mdiShieldCheckOutline,
  mdiShieldCrownOutline,
  mdiTrayArrowUp,
  mdiViewGridOutline,
} from '@mdi/js'
import StageEnvironmentBanner from '@/components/layout/StageEnvironmentBanner.vue'
import { useSidebarState } from '@/composables/use-sidebar-state'
import { getCachedCurrentSession, getCurrentSession, type CurrentSessionResponse } from '@/services/authentication-service'

type Section = 'home' | 'users' | 'email' | 'roles'

const route = useRoute()
const router = useRouter()
const { isSidebarCollapsed, isMobileSidebarOpen, toggleSidebar, closeSidebar } = useSidebarState()
const session = ref<CurrentSessionResponse | null>(getCachedCurrentSession())
const activeEmailTab = ref('pending')

const section = computed<Section>(() => {
  if (route.name === 'administration-manage-users') return 'users'
  if (route.name === 'administration-manage-email') return 'email'
  if (route.name === 'administration-roles-policies') return 'roles'
  return 'home'
})

const pageTitle = computed(() => ({
  home: 'Administração',
  users: 'Gerenciar usuários',
  email: 'Central de e-mail',
  roles: 'Regras e políticas',
})[section.value])

const breadcrumb = computed(() => ({
  home: '',
  users: 'Usuários',
  email: 'Central de e-mail',
  roles: 'Regras e políticas',
})[section.value])

const firstName = computed(() => session.value?.fullName.trim().split(/\s+/).at(0) ?? 'Gabriel')

const navigationItems = [
  { label: 'Home', icon: mdiHomeVariant, to: { name: 'home' } },
  { label: 'Meus dados', icon: mdiAccountOutline, to: { name: 'my-data-section', params: { section: 'basic' } } },
  { label: 'Apps', icon: mdiViewGridOutline, to: null },
  { label: 'Calendário', icon: mdiCalendarMonthOutline, to: null },
  { label: 'Administração', icon: mdiShieldCrownOutline, to: { name: 'administration' } },
]

const managementCards = [
  { title: 'Usuários', description: 'Consultas e administração de contas', icon: mdiAccountGroupOutline, to: { name: 'administration-manage-users' } },
  { title: 'Central de e-mail', description: 'Mensagens, pendências e templates', icon: mdiEmailOutline, to: { name: 'administration-manage-email' } },
  { title: 'Regras e políticas', description: 'Funções, permissões e segurança', icon: mdiShieldCheckOutline, to: { name: 'administration-roles-policies' } },
]

function navigate(to: { name: string; params?: Record<string, string> } | null): void {
  if (!to) return
  closeSidebar()
  void router.push(to)
}

onMounted(async () => {
  session.value = await getCurrentSession(true)
})
</script>

<template>
  <v-main class="administration-page">
    <div class="administration-shell" :class="{ 'administration-shell--collapsed': isSidebarCollapsed }">
      <aside class="sidebar">
        <div class="brand" aria-label="YaeaY Account">
          <span class="brand__primary">YaeaY</span>
          <span class="brand__secondary">Account</span>
        </div>

        <nav class="sidebar__navigation" aria-label="Navegação principal">
          <v-tooltip v-for="item in navigationItems" :key="item.label" :text="item.label" :disabled="!isSidebarCollapsed" location="right">
            <template #activator="{ props: tooltipProps }">
              <button
                v-bind="tooltipProps"
                type="button"
                class="navigation-item"
                :class="{ 'navigation-item--active': item.label === 'Administração', 'navigation-item--disabled': !item.to }"
                :aria-current="item.label === 'Administração' ? 'page' : undefined"
                :aria-label="item.label"
                @click="navigate(item.to)"
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

      <button v-if="isMobileSidebarOpen" type="button" class="sidebar-backdrop" aria-label="Fechar menu lateral" @click="closeSidebar" />

      <section class="workspace">
        <header class="topbar">
          <div class="topbar__start">
            <v-btn class="sidebar-toggle" :icon="isSidebarCollapsed ? mdiMenu : mdiMenuOpen" variant="text" :aria-label="isSidebarCollapsed ? 'Expandir menu lateral' : 'Recolher menu lateral'" @click="toggleSidebar" />
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
            <button type="button" class="topbar__user-button" aria-label="Menu do usuário">
              <span>{{ firstName }}</span>
              <v-icon :icon="mdiChevronDown" size="21" />
            </button>
          </div>
        </header>

        <main class="administration-content">
          <div v-if="section !== 'home'" class="breadcrumb">Administração <span>/</span> {{ breadcrumb }}</div>
          <span v-else class="eyebrow">AMBIENTE ADMINISTRATIVO</span>
          <h1>{{ pageTitle }}</h1>

          <template v-if="section === 'home'">
            <section class="metrics-grid" aria-label="Indicadores administrativos">
              <article class="metric-card"><v-icon :icon="mdiAccountGroupOutline" size="27" /><span>Usuários</span><strong>1</strong></article>
              <article class="metric-card"><v-icon :icon="mdiAlertCircleOutline" size="27" /><span>Pendências de e-mail</span><strong>0</strong></article>
              <article class="metric-card"><v-icon :icon="mdiTrayArrowUp" size="27" /><span>Outbox pendente</span><strong>0</strong></article>
              <article class="metric-card"><v-icon :icon="mdiFileDocumentOutline" size="27" /><span>Contas suspensas</span><strong>0</strong></article>
            </section>

            <section class="management-grid" aria-label="Áreas de administração">
              <article v-for="card in managementCards" :key="card.title" class="management-card">
                <span class="management-card__icon"><v-icon :icon="card.icon" size="65" /></span>
                <h2>{{ card.title }}</h2>
                <p>{{ card.description }}</p>
                <v-btn class="primary-button" block @click="navigate(card.to)">GERENCIAR</v-btn>
              </article>
            </section>
          </template>

          <template v-else-if="section === 'users'">
            <section class="surface filters-surface">
              <h2>Filtros</h2>
              <div class="user-filters">
                <v-text-field label="Nome ou e-mail" placeholder="Digite o nome ou e-mail" :append-inner-icon="mdiMagnify" variant="outlined" hide-details />
                <v-select label="Status" :items="['Todos', 'Ativa', 'Pendente', 'Suspensa', 'Desabilitada']" model-value="Todos" variant="outlined" hide-details />
                <v-text-field label="Último login de" placeholder="dd/mm/aaaa" variant="outlined" hide-details />
                <v-text-field label="Último login até" placeholder="dd/mm/aaaa" variant="outlined" hide-details />
              </div>
              <div class="filter-actions"><v-btn class="primary-button" :prepend-icon="mdiFilterOutline">FILTRAR</v-btn><v-btn variant="text">Limpar filtros</v-btn></div>
            </section>

            <section class="surface users-surface">
              <h2>Usuários</h2>
              <div class="data-table">
                <div class="data-table__head"><span>NOME</span><span>E-MAIL</span><span>STATUS</span><span>ÚLTIMO LOGIN</span><span>AÇÕES</span></div>
                <div class="empty-state"><span class="empty-state__icon"><v-icon :icon="mdiAccountGroupOutline" size="46" /></span><strong>Nenhum usuário encontrado</strong><p>Tente ajustar os filtros para encontrar o que você procura.</p></div>
              </div>
              <footer class="table-footer"><span>0 resultado</span><div><v-btn size="small" variant="outlined">10 por página <v-icon :icon="mdiChevronDown" end /></v-btn><v-btn size="small" variant="outlined" disabled>Anterior</v-btn><v-btn size="small" class="page-current">1</v-btn><v-btn size="small" variant="outlined" disabled>Próxima</v-btn></div></footer>
            </section>
          </template>

          <template v-else-if="section === 'email'">
            <div class="email-tabs">
              <button :class="{ active: activeEmailTab === 'pending' }" @click="activeEmailTab = 'pending'"><v-icon :icon="mdiAccountGroupOutline" size="31" /><span><strong>Pendências de confirmação</strong><small>E-mails aguardando confirmação</small></span></button>
              <button :class="{ active: activeEmailTab === 'templates' }" @click="activeEmailTab = 'templates'"><v-icon :icon="mdiFileDocumentOutline" size="31" /><span><strong>Templates</strong><small>Gerencie modelos de e-mail</small></span></button>
              <button :class="{ active: activeEmailTab === 'outbox' }" @click="activeEmailTab = 'outbox'"><v-icon :icon="mdiTrayArrowUp" size="31" /><span><strong>Outbox</strong><small>E-mails enviados recentemente</small></span></button>
            </div>
            <section class="email-layout">
              <article class="surface email-list"><h2>Pendências de confirmação</h2><div class="email-filter"><v-text-field label="Período" model-value="01/07/2026  →  31/08/2026" variant="outlined" hide-details /><v-select label="Status" :items="['Todos']" model-value="Todos" variant="outlined" hide-details /><v-select label="Tipo" :items="['Todos']" model-value="Todos" variant="outlined" hide-details /></div><div class="filter-actions"><v-btn class="primary-button">Filtrar</v-btn><v-btn variant="outlined">Limpar</v-btn></div><div class="email-table"><div>DATA</div><div>DESTINATÁRIO</div><div>TIPO</div><div>STATUS</div><div>AÇÕES</div></div><div class="email-empty">Nenhum e-mail pendente nesta consulta.</div></article>
              <article class="surface template-editor"><h2>Editar template</h2><v-select label="Selecionar template" :items="['Confirmação de e-mail']" model-value="Confirmação de e-mail" variant="outlined" /><v-text-field label="Assunto" model-value="Confirme seu e-mail na YaeaY" variant="outlined" /><v-textarea label="Conteúdo HTML" model-value="Olá, {{nome}}.&#10;&#10;Recebemos sua solicitação. Clique no botão abaixo para continuar." rows="7" variant="outlined" /><p class="variables" v-text="'Variáveis disponíveis: {{nome}}, {{email}}, {{validade}}, {{link}}, {{empresa}}'" /><div class="editor-actions"><v-btn class="primary-button">SALVAR ALTERAÇÃO</v-btn></div></article>
            </section>
          </template>

          <template v-else>
            <section class="policy-notice"><v-icon :icon="mdiAlertCircleOutline" size="26" /><span>A criação de funções não atribui usuários nem concede acessos automaticamente.<br>As permissões são controladas pelas policies associadas a cada função.</span></section>
            <section class="surface roles-surface"><div class="surface-heading"><div><h2>Funções</h2><p>Gerencie as funções que podem ser atribuídas aos usuários.</p></div><v-btn class="primary-button">CRIAR FUNÇÃO</v-btn></div><div class="roles-table"><div>NOME DA FUNÇÃO</div><div>DESCRIÇÃO</div><div>USUÁRIOS</div><div>AÇÕES</div><strong>Admin</strong><span>Acesso total à plataforma e configurações administrativas.</span><span>1</span><v-btn icon="mdi-dots-horizontal" size="small" variant="tonal" /><strong>User</strong><span>Acesso padrão às funcionalidades da aplicação.</span><span>0</span><v-btn icon="mdi-dots-horizontal" size="small" variant="tonal" /></div><footer class="table-footer"><span>Mostrando 2 de 2 funções</span><div><v-btn size="small" variant="outlined" :icon="mdiChevronLeft" /><v-btn size="small" class="page-current">1</v-btn><v-btn size="small" variant="outlined" :icon="mdiChevronRight" /></div></footer></section>
            <section class="surface policies-surface"><h2>Policies</h2><p>Defina as políticas de acesso que determinam o que cada função pode fazer.</p><div class="empty-state"><span class="empty-state__icon"><v-icon :icon="mdiShieldCheckOutline" size="54" /></span><strong>Nenhuma policy cadastrada</strong><p>Crie uma função primeiro para configurar<br>as policies que vão definir as permissões de acesso.</p></div></section>
          </template>
        </main>
      </section>
    </div>
  </v-main>
</template>

<style scoped>
.administration-page { min-height: 100vh; background: #fafbf9; color: #143e31; }
.administration-shell { min-height: 100vh; display: grid; grid-template-columns: 230px minmax(0, 1fr); }
.administration-shell--collapsed { grid-template-columns: 92px minmax(0, 1fr); }
.sidebar { position: sticky; top: 0; display: flex; height: 100vh; flex-direction: column; padding: 30px 10px 22px; background: #fff; border-right: 1px solid #e4e8e5; }
.brand { display: flex; align-items: baseline; gap: 6px; padding: 0 20px 44px; white-space: nowrap; }.brand__primary { font-size: 1.7rem; font-weight: 800; letter-spacing: -.055em; }.brand__secondary { color: #6f8b80; font-size: 1.08rem; font-weight: 300; }
.sidebar__navigation { display: flex; flex-direction: column; gap: 9px; }.navigation-item { display: flex; min-height: 54px; width: 100%; align-items: center; gap: 18px; padding: 0 22px; border: 0; border-radius: 18px; background: transparent; color: #4c5753; cursor: pointer; font: inherit; text-align: left; }.navigation-item--active { background: #e7ece9; color: #153f33; font-weight: 650; }.navigation-item--disabled { cursor: default; }.sidebar__footer { margin-top: auto; padding-top: 20px; border-top: 1px solid #e7e9e7; }.administration-shell--collapsed .brand { justify-content: center; padding-inline: 5px; }.administration-shell--collapsed .brand__secondary, .administration-shell--collapsed .navigation-item span { display: none; }.administration-shell--collapsed .navigation-item { justify-content: center; padding: 0; }
.workspace { min-width: 0; }.topbar { display: grid; min-height: 90px; grid-template-columns: minmax(260px, 1fr) auto minmax(150px, 1fr); align-items: center; gap: 24px; padding: 0 34px 0 42px; background: #fff; border-bottom: 1px solid #e5e8e5; }.topbar__start { display: flex; min-width: 0; align-items: center; gap: 12px; }.sidebar-toggle { color: #334d44; }.search-box { display: flex; width: min(410px, 100%); height: 52px; align-items: center; gap: 16px; padding: 0 18px; border: 1px solid #dce2de; border-radius: 13px; background: #fbfcfb; color: #77837e; }.topbar__stage-banner { justify-self: center; }.topbar__account { justify-self: end; display: flex; align-items: center; gap: 11px; }.notification { position: relative; display: grid; place-items: center; margin-right: 18px; color: #334d44; }.notification__badge { position: absolute; top: -8px; right: -8px; display: grid; min-width: 18px; height: 18px; place-items: center; border-radius: 9px; background: #e3b94f; color: #173d32; font-size: .67rem; font-weight: 800; }.topbar__user-button { display: inline-flex; min-height: 44px; align-items: center; gap: 11px; border: 0; border-radius: 10px; background: transparent; color: inherit; cursor: pointer; font-weight: 650; }
.administration-content { max-width: 1390px; margin: 0 auto; padding: 45px 42px 60px; }.eyebrow { color: #51766a; font-size: .82rem; font-weight: 800; letter-spacing: .14em; }.breadcrumb { margin-bottom: 18px; color: #315f52; font-size: .92rem; }.breadcrumb span { margin: 0 8px; color: #9ca9a2; } h1 { margin: 18px 0 38px; color: #123f31; font-size: clamp(2rem, 3vw, 2.7rem); font-weight: 400; letter-spacing: -.055em; } h2 { margin: 0; font-size: 1.16rem; font-weight: 500; } p { color: #647c73; }
.metrics-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; }.metric-card, .surface, .management-card { border: 1px solid #dbe3de; border-radius: 17px; background: rgba(255,255,255,.96); box-shadow: 0 8px 24px rgba(20,62,49,.035); }.metric-card { display: flex; min-height: 182px; flex-direction: column; gap: 18px; padding: 29px 25px; color: #518071; }.metric-card span { color: #315c4e; font-size: 1.04rem; }.metric-card strong { color: #063c2e; font-size: 2.35rem; font-weight: 400; }.management-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; margin-top: 32px; }.management-card { display: flex; min-height: 422px; flex-direction: column; align-items: center; padding: 38px 30px 36px; text-align: center; }.management-card__icon, .empty-state__icon { display: grid; width: 150px; height: 150px; place-items: center; border-radius: 50%; background: radial-gradient(circle at 30% 20%, #f6f8f7, #e1e8e4); color: #07533e; }.management-card h2 { margin-top: 28px; font-size: 1.7rem; color: #064535; }.management-card p { min-height: 48px; margin: 11px 0 24px; font-size: 1.02rem; }.primary-button { background: #07533e !important; color: #fff !important; font-weight: 750; letter-spacing: .02em; }.management-card .primary-button { margin-top: auto; min-height: 60px; border-radius: 11px; }
.surface { padding: 26px 22px; }.filters-surface h2 { margin-bottom: 28px; }.user-filters { display: grid; grid-template-columns: 1.2fr 1fr 1fr 1fr; gap: 20px; }.filter-actions { display: flex; align-items: center; gap: 12px; margin-top: 26px; }.filter-actions .v-btn { min-height: 48px; }.users-surface { margin-top: 28px; padding: 26px 22px 0; }.data-table__head, .email-table { display: grid; grid-template-columns: 1.2fr 1.4fr 1fr 1.1fr .55fr; gap: 12px; margin-top: 27px; padding: 0 10px 16px; border-bottom: 1px solid #e8ece9; color: #718078; font-size: .75rem; font-weight: 800; letter-spacing: .08em; }.empty-state { display: flex; min-height: 260px; flex-direction: column; align-items: center; justify-content: center; color: #0f4033; text-align: center; }.empty-state__icon { width: 105px; height: 105px; margin-bottom: 16px; color: #5f8a7b; }.empty-state p { margin: 10px 0 0; }.table-footer { display: flex; min-height: 82px; align-items: center; justify-content: space-between; margin: 0 -22px; padding: 0 22px; border-top: 1px solid #e8ece9; color: #315c4e; }.table-footer > div { display: flex; gap: 12px; }.page-current { min-width: 38px; background: #e9efeb !important; color: #164638 !important; }
.email-tabs { display: grid; grid-template-columns: repeat(3, 1fr); overflow: hidden; margin-bottom: 24px; border: 1px solid #dce4df; border-radius: 8px; background: #fff; }.email-tabs button { display: flex; min-height: 82px; align-items: center; gap: 20px; padding: 0 24px; border: 0; border-right: 1px solid #e6ebe8; background: #fff; color: #285d4e; text-align: left; }.email-tabs button.active { background: #f4f7f5; box-shadow: inset 0 0 0 1px #e5ebe7; }.email-tabs span { display: grid; gap: 3px; }.email-tabs small { color: #718278; font-size: .86rem; }.email-layout { display: grid; grid-template-columns: 1.12fr 1fr; gap: 16px; }.email-list h2, .template-editor h2 { margin-bottom: 24px; }.email-filter { display: grid; grid-template-columns: 1.5fr .75fr .75fr; gap: 10px; }.email-table { margin-top: 24px; grid-template-columns: .7fr 1.45fr 1.35fr .7fr .65fr; }.email-empty { display: grid; min-height: 280px; place-items: center; color: #7b8983; }.template-editor :deep(.v-field) { margin-bottom: 14px; }.variables { margin: -4px 0 22px; font-size: .8rem; }.editor-actions { display: flex; justify-content: flex-end; }.editor-actions .v-btn { min-height: 48px; }
.policy-notice { display: flex; gap: 18px; margin-bottom: 24px; padding: 27px 25px; border: 1px solid #b6d9e8; border-radius: 16px; background: #f4fbfe; color: #326779; line-height: 1.65; }.roles-surface { padding-bottom: 0; }.surface-heading { display: flex; align-items: start; justify-content: space-between; }.surface-heading p, .policies-surface > p { margin: 14px 0 28px; }.roles-table { display: grid; grid-template-columns: 1.1fr 2.6fr .65fr .35fr; align-items: center; gap: 0 18px; }.roles-table > * { min-height: 60px; display: flex; align-items: center; border-bottom: 1px solid #e7ece9; }.roles-table > :nth-child(-n+4) { color: #718078; font-size: .75rem; font-weight: 800; letter-spacing: .08em; }.roles-table strong { font-weight: 500; }.roles-table span { color: #456157; }.policies-surface { margin-top: 24px; }.policies-surface .empty-state { min-height: 290px; }.policies-surface .empty-state__icon { background: transparent; color: #adc8bd; }
@media (max-width: 1100px) { .metrics-grid { grid-template-columns: repeat(2, 1fr); }.management-grid { grid-template-columns: 1fr; }.management-card { min-height: 330px; }.user-filters, .email-layout { grid-template-columns: 1fr 1fr; }.email-layout .template-editor { grid-column: span 2; } }
@media (max-width: 760px) { .administration-shell { grid-template-columns: 1fr; }.sidebar { position: fixed; z-index: 10; width: 230px; transform: translateX(-100%); transition: transform .2s ease; }.topbar { grid-template-columns: 1fr auto; padding: 0 16px; }.topbar__stage-banner { display: none; }.search-box { width: 100%; }.topbar__account { grid-column: 2; }.notification { display: none; }.administration-content { padding: 30px 16px; }.metrics-grid, .user-filters, .email-layout { grid-template-columns: 1fr; }.email-layout .template-editor { grid-column: auto; }.email-tabs { grid-template-columns: 1fr; }.email-tabs button { border-bottom: 1px solid #e6ebe8; }.data-table__head, .email-table, .roles-table { overflow-x: auto; min-width: 700px; }.users-surface, .email-list { overflow-x: auto; }.table-footer { align-items: flex-start; flex-direction: column; gap: 14px; padding-block: 18px; }.sidebar-backdrop { position: fixed; z-index: 9; inset: 0; border: 0; background: rgba(8, 28, 20, .35); }.administration-shell:not(.administration-shell--collapsed) .sidebar { transform: translateX(0); } }
</style>
