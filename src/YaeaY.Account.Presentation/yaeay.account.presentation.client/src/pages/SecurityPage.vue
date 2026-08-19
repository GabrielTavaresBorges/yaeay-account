<script setup lang="ts">
  import {
    mdiAccountKeyOutline,
    mdiAlertDecagramOutline,
    mdiBugOutline,
    mdiCookieLockOutline,
    mdiEmailAlertOutline,
    mdiLockCheckOutline,
    mdiShieldCheckOutline,
  } from '@mdi/js'
  import PublicInformationLayout from '@/components/layout/PublicInformationLayout.vue'
</script>

<template>
  <PublicInformationLayout page="security"
                           eyebrow="Proteção em todas as etapas"
                           title="Segurança"
                           description="Conheça os controles que protegem o Account e as atitudes que ajudam a manter sua identidade segura."
                           :icon="mdiShieldCheckOutline"
                           updated-at="18 de agosto de 2026">
    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiLockCheckOutline" size="21" /></span>
        <h2>Como protegemos o acesso</h2>
      </div>
      <ul>
        <li>senhas são protegidas por hash e não ficam disponíveis como texto legível;</li>
        <li>sessões utilizam cookies seguros, inacessíveis a scripts e restritos ao mesmo site;</li>
        <li>requisições sensíveis recebem proteção contra falsificação;</li>
        <li>tentativas repetidas podem provocar bloqueio temporário;</li>
        <li>o acesso exige confirmação do endereço de e-mail.</li>
      </ul>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiAccountKeyOutline" size="21" /></span>
        <h2>Proteja sua conta</h2>
      </div>
      <ul>
        <li>use uma senha longa, exclusiva e diferente da senha do seu e-mail;</li>
        <li>não salve credenciais em dispositivos públicos ou compartilhados;</li>
        <li>mantenha navegador, sistema e aplicativos atualizados;</li>
        <li>encerre a sessão ao terminar e revise mensagens de acesso inesperadas;</li>
        <li>proteja primeiro seu e-mail, pois ele é usado para confirmação e recuperação.</li>
      </ul>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiEmailAlertOutline" size="21" /></span>
        <h2>Evite fraude e phishing</h2>
      </div>
      <p>
        A YaeaY nunca solicitará sua senha completa ou um código de verificação por telefone, mensagem ou e-mail.
        Desconfie de urgência, ameaças, prêmios e links que levam a endereços diferentes do serviço oficial.
      </p>
      <p>Não informe credenciais depois de abrir links recebidos de remetentes desconhecidos.</p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiAlertDecagramOutline" size="21" /></span>
        <h2>Se notar atividade suspeita</h2>
      </div>
      <ul>
        <li>troque a senha do seu e-mail e depois a senha do Account;</li>
        <li>não continue tentando acessar por links recebidos na mensagem suspeita;</li>
        <li>preserve e-mails, horários e capturas de tela que ajudem na análise;</li>
        <li>consulte a Central de ajuda e utilize somente os canais oficiais disponíveis.</li>
      </ul>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiBugOutline" size="21" /></span>
        <h2>Vulnerabilidades de segurança</h2>
      </div>
      <p>
        Se encontrar uma possível vulnerabilidade, não acesse dados de terceiros, não interrompa o serviço
        e não explore o problema além do necessário para demonstrá-lo. Preserve evidências e procure o canal
        oficial indicado pela YaeaY para um relato responsável.
      </p>
    </section>

    <section class="info-section">
      <div class="info-section__heading">
        <span class="info-section__icon"><v-icon :icon="mdiCookieLockOutline" size="21" /></span>
        <h2>Limites importantes</h2>
      </div>
      <p>
        Nenhuma tecnologia elimina todos os riscos. O Account combina controles técnicos e operacionais,
        mas a segurança também depende do dispositivo, do e-mail e das escolhas de cada usuário.
      </p>
    </section>

    <template #summary>
      <h2>Regra de ouro</h2>
      <p>Credenciais e códigos são pessoais e nunca devem ser compartilhados.</p>
      <ul>
        <li>Uma senha exclusiva por serviço.</li>
        <li>Desconfie de pedidos urgentes.</li>
        <li>Confira o endereço antes de entrar.</li>
        <li>Comunique atividade suspeita.</li>
      </ul>
    </template>
  </PublicInformationLayout>
</template>
