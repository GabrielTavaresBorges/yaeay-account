<script setup lang="ts">
  import {
    mdiAccountCheckOutline,
    mdiAccountLockOutline,
    mdiAlertOctagonOutline,
    mdiCopyright,
    mdiFileDocumentCheckOutline,
    mdiProgressWrench,
    mdiScaleBalance,
  } from '@mdi/js'
  import PublicInformationLayout from '@/components/layout/PublicInformationLayout.vue'
</script>

<template>
  <PublicInformationLayout page="terms"
                           eyebrow="Regras claras de uso"
                           title="Termos de uso"
                           description="Estes termos estabelecem as condições essenciais para criar e utilizar uma identidade no YaeaY Account."
                           :icon="mdiFileDocumentCheckOutline"
                           updated-at="18 de agosto de 2026">
    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiAccountCheckOutline" size="21" /></span>
        <h2>Sua conta</h2>
      </div>
      <p>
        Você deve fornecer informações verdadeiras, atualizadas e pertencentes a você. A conta é pessoal:
        não compartilhe credenciais, não se passe por outra pessoa e não permita que terceiros utilizem seu acesso.
      </p>
      <p>Você é responsável por manter seus dados de recuperação atualizados e comunicar acessos suspeitos.</p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiAccountLockOutline" size="21" /></span>
        <h2>Uso permitido</h2>
      </div>
      <p>O Account deve ser utilizado de maneira lícita e compatível com sua finalidade. Não é permitido:</p>
      <ul>
        <li>tentar acessar contas, dados ou recursos sem autorização;</li>
        <li>automatizar cadastros, realizar fraude, spam ou contornar controles de segurança;</li>
        <li>explorar vulnerabilidades, interferir na disponibilidade ou introduzir código malicioso;</li>
        <li>utilizar o serviço para violar direitos, leis ou políticas aplicáveis.</li>
      </ul>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiAlertOctagonOutline" size="21" /></span>
        <h2>Bloqueio, suspensão e encerramento</h2>
      </div>
      <p>
        O acesso pode ser limitado temporariamente para proteger a conta, investigar atividade suspeita,
        corrigir risco técnico ou responder a uma violação destes termos. Quando apropriado, serão apresentadas
        orientações para regularização ou contestação.
      </p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiProgressWrench" size="21" /></span>
        <h2>Disponibilidade e mudanças</h2>
      </div>
      <p>
        O serviço pode receber manutenção, melhorias ou alterações. Buscamos continuidade e confiabilidade,
        mas interrupções podem ocorrer por manutenção, falhas, eventos externos ou medidas de segurança.
        Alterações relevantes destes termos devem ser comunicadas por meio adequado.
      </p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiCopyright" size="21" /></span>
        <h2>Propriedade e limites de uso</h2>
      </div>
      <p>
        Marcas, interfaces, textos, componentes e demais elementos do YaeaY Account são protegidos pela
        legislação aplicável. O acesso à conta não transfere direitos de propriedade nem autoriza cópia,
        modificação, engenharia reversa ou exploração comercial não permitida.
      </p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiScaleBalance" size="21" /></span>
        <h2>Privacidade e legislação</h2>
      </div>
      <p>
        O tratamento de dados é explicado no Aviso de Privacidade. Estes termos são interpretados conforme
        a legislação aplicável no Brasil, preservados os direitos obrigatórios do consumidor e do titular de dados.
      </p>
    </section>

    <div class="info-notice">
      <v-icon :icon="mdiFileDocumentCheckOutline" size="22" />
      <div>
        <strong>Leitura e concordância</strong>
        Ao utilizar o YaeaY Account, você declara ter lido estes termos e concordar em respeitar suas condições.
        Se não concordar, não prossiga com o uso da conta.
      </div>
    </div>

    <template #summary>
      <h2>Responsabilidade compartilhada</h2>
      <p>O serviço protege a plataforma; você protege o uso da sua identidade.</p>
      <ul>
        <li>Forneça dados corretos e próprios.</li>
        <li>Mantenha suas credenciais em sigilo.</li>
        <li>Não contorne controles de segurança.</li>
        <li>Respeite leis, direitos e políticas.</li>
      </ul>
    </template>
  </PublicInformationLayout>
</template>
