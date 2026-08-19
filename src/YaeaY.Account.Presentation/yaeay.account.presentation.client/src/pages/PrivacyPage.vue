<script setup lang="ts">
  import {
    mdiAccountDetailsOutline,
    mdiCookieOutline,
    mdiDatabaseClockOutline,
    mdiScaleBalance,
    mdiShareVariantOutline,
    mdiShieldLockOutline,
    mdiTextBoxCheckOutline,
  } from '@mdi/js'
  import PublicInformationLayout from '@/components/layout/PublicInformationLayout.vue'
</script>

<template>
  <PublicInformationLayout page="privacy"
                           eyebrow="Seus dados, suas escolhas"
                           title="Privacidade"
                           description="Este aviso explica, de forma objetiva, quais dados o YaeaY Account utiliza, para quais finalidades e quais escolhas você possui."
                           :icon="mdiShieldLockOutline"
                           updated-at="18 de agosto de 2026">
    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiAccountDetailsOutline" size="21" /></span>
        <h2>Dados utilizados pelo Account</h2>
      </div>
      <p>Utilizamos somente informações relacionadas à criação, proteção e funcionamento da conta:</p>
      <ul>
        <li>dados cadastrais, como nome, data de nascimento, gênero, e-mail e telefone;</li>
        <li>dados da conta, como situação, datas de criação, confirmação e acessos;</li>
        <li>credenciais protegidas e informações necessárias para autenticar sua sessão;</li>
        <li>registros técnicos indispensáveis à prevenção de abuso, diagnóstico e segurança.</li>
      </ul>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiTextBoxCheckOutline" size="21" /></span>
        <h2>Por que utilizamos esses dados</h2>
      </div>
      <ul>
        <li>criar e administrar uma identidade única no ecossistema YaeaY;</li>
        <li>confirmar o e-mail, autenticar acessos e manter a sessão;</li>
        <li>proteger contas, investigar falhas e impedir atividades indevidas;</li>
        <li>enviar comunicações operacionais necessárias ao funcionamento da conta;</li>
        <li>cumprir obrigações legais e atender solicitações válidas dos titulares.</li>
      </ul>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiShareVariantOutline" size="21" /></span>
        <h2>Compartilhamento</h2>
      </div>
      <p>
        Os dados podem ser tratados por fornecedores técnicos necessários à operação, como infraestrutura,
        banco de dados e entrega de e-mails. Também poderão ser fornecidos quando houver obrigação legal,
        ordem válida ou necessidade de proteger direitos e a segurança do serviço.
      </p>
      <p>O YaeaY Account não comercializa dados pessoais nem os disponibiliza para publicidade de terceiros.</p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiDatabaseClockOutline" size="21" /></span>
        <h2>Proteção e retenção</h2>
      </div>
      <p>
        Aplicamos controles compatíveis com o risco, incluindo proteção das credenciais, sessões seguras,
        restrição de acesso e mecanismos contra solicitações indevidas. Nenhum sistema é totalmente imune a riscos.
      </p>
      <p>
        Mantemos dados pelo período necessário às finalidades informadas e às obrigações legais, de segurança
        e de defesa de direitos. Quando aplicável, dados podem ser eliminados ou anonimizados.
      </p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiScaleBalance" size="21" /></span>
        <h2>Seus direitos</h2>
      </div>
      <p>Nos limites da LGPD e após validação segura da identidade, você pode solicitar:</p>
      <ul>
        <li>confirmação do tratamento e acesso aos dados;</li>
        <li>correção de informações incompletas, inexatas ou desatualizadas;</li>
        <li>informações sobre finalidades e compartilhamentos;</li>
        <li>anonimização, bloqueio, portabilidade ou eliminação, quando aplicáveis;</li>
        <li>revogação do consentimento e oposição ao tratamento, quando cabíveis.</li>
      </ul>
      <p>Consulte a Central de ajuda para identificar os canais oficiais disponíveis para sua solicitação.</p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiCookieOutline" size="21" /></span>
        <h2>Cookies essenciais</h2>
      </div>
      <p>
        O Account utiliza cookies estritamente necessários para autenticação, proteção contra requisições
        indevidas e, se você escolher “Lembrar-me”, continuidade do acesso. Esta versão não utiliza cookies
        publicitários no fluxo de conta.
      </p>
    </section>

    <template #summary>
      <h2>Em poucas palavras</h2>
      <p>Transparência sobre o ciclo de vida dos dados pessoais.</p>
      <ul>
        <li>Coleta limitada ao funcionamento da conta.</li>
        <li>Uso vinculado a finalidades informadas.</li>
        <li>Direitos exercidos com validação de identidade.</li>
        <li>Cookies restritos a funções essenciais.</li>
      </ul>
    </template>
  </PublicInformationLayout>
</template>
