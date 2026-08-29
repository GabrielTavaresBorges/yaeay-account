<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { mdiAccountGroupOutline, mdiArrowLeft, mdiEmailOutline, mdiFileDocumentOutline, mdiHistory, mdiTrayArrowUp } from '@mdi/js'
import { getCurrentSession } from '@/services/authentication-service'
import { getAdministrationAudit, getAdministrationOverview, getAdministrationUsers, type AdministrationAudit, type AdministrationOverview, type AdministrationUser } from '@/services/administration/administration-service'

const router = useRouter()
const overview = ref<AdministrationOverview | null>(null)
const users = ref<AdministrationUser[]>([])
const audit = ref<AdministrationAudit[]>([])
const loading = ref(true)
const error = ref('')

const cards = [
  { key: 'totalUsers', label: 'Usuários', icon: mdiAccountGroupOutline },
  { key: 'pendingEmailConfirmation', label: 'Confirmações pendentes', icon: mdiEmailOutline },
  { key: 'pendingOutboxMessages', label: 'Outbox pendente', icon: mdiTrayArrowUp },
  { key: 'suspendedUsers', label: 'Contas suspensas', icon: mdiFileDocumentOutline },
] as const

function formatDate(value: string | null) { return value ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : '—' }

onMounted(async () => {
  try {
    const session = await getCurrentSession(true)
    if (!session.canManageAccount) { await router.replace({ name: 'home' }); return }
    ;[overview.value, users.value, audit.value] = await Promise.all([getAdministrationOverview(), getAdministrationUsers(), getAdministrationAudit()])
  } catch { error.value = 'Não foi possível carregar os dados administrativos.' } finally { loading.value = false }
})
</script>

<template>
  <v-main class="administration-page">
    <section class="administration-shell">
      <button class="back" type="button" @click="router.push({ name: 'home' })"><v-icon :icon="mdiArrowLeft" size="18" /> Voltar</button>
      <header class="heading"><span class="heading__icon"><v-icon :icon="mdiAccountGroupOutline" size="34" /></span><div><h1>Administração</h1><p>Acompanhe usuários, confirmações, operação e auditoria do Account.</p></div></header>
      <v-alert v-if="error" type="error" variant="tonal">{{ error }}</v-alert>
      <v-progress-linear v-else-if="loading" indeterminate color="#17543f" />
      <template v-else-if="overview">
        <section class="metrics" aria-label="Indicadores administrativos">
          <article v-for="card in cards" :key="card.key" class="metric"><v-icon :icon="card.icon" size="25" /><span>{{ card.label }}</span><strong>{{ overview[card.key] }}</strong></article>
        </section>
        <section class="panel"><h2>Usuários</h2><div class="table-wrap"><table><thead><tr><th>Nome</th><th>E-mail</th><th>Status</th><th>Último login</th></tr></thead><tbody><tr v-for="user in users" :key="user.userId"><td>{{ user.fullName }}</td><td>{{ user.email }}</td><td><span class="status">{{ user.status }}</span></td><td>{{ formatDate(user.lastLoginAt) }}</td></tr><tr v-if="!users.length"><td colspan="4">Nenhum usuário projetado ainda.</td></tr></tbody></table></div></section>
        <section class="panel"><h2><v-icon :icon="mdiHistory" size="22" /> Auditoria recente</h2><div class="audit"><article v-for="entry in audit" :key="entry.id"><strong>{{ entry.action }}</strong><p>{{ entry.justification }}</p><small>{{ formatDate(entry.occurredAtUtc) }}</small></article><p v-if="!audit.length">Nenhuma operação administrativa registrada.</p></div></section>
      </template>
    </section>
  </v-main>
</template>

<style scoped>
.administration-page{min-height:100vh;background:#f8f9f7;color:#173d32}.administration-shell{max-width:1120px;margin:0 auto;padding:42px 28px 64px}.back{display:inline-flex;gap:6px;align-items:center;border:0;background:transparent;color:#31584a;cursor:pointer}.heading{display:flex;gap:18px;align-items:center;margin:24px 0 30px}.heading__icon{display:grid;place-items:center;width:70px;height:70px;border-radius:22px;color:#176143;background:#e3f3ea}.heading h1{margin:0;font-size:2.35rem;letter-spacing:-.05em}.heading p{margin:7px 0 0;color:#68766f}.metrics{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:15px}.metric,.panel{border:1px solid #dce4df;border-radius:18px;background:#fff;box-shadow:0 8px 24px rgba(20,62,49,.045)}.metric{min-height:145px;padding:21px;display:flex;flex-direction:column;gap:10px;color:#527568}.metric strong{margin-top:auto;color:#173d32;font-size:2.1rem}.panel{margin-top:22px;padding:25px}.panel h2{display:flex;align-items:center;gap:9px;margin:0 0 20px;font-size:1.2rem}.table-wrap{overflow:auto}table{width:100%;border-collapse:collapse;text-align:left}th,td{padding:13px 10px;border-bottom:1px solid #edf0ee;font-size:.9rem}th{color:#5b7067;font-size:.74rem;letter-spacing:.08em;text-transform:uppercase}.status{padding:5px 10px;border-radius:999px;background:#e7f0eb;color:#17543f;font-size:.76rem;font-weight:700}.audit{display:grid;gap:12px}.audit article{padding:15px;border-radius:12px;background:#f5f8f6}.audit p{margin:6px 0;color:#56665f}.audit small{color:#708078}@media(max-width:760px){.administration-shell{padding:28px 16px}.metrics{grid-template-columns:repeat(2,minmax(0,1fr))}.heading h1{font-size:2rem}}
</style>
