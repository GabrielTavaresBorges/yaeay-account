<script setup lang="ts">
  import { computed, nextTick, ref } from 'vue'
  import {
    mdiAccountPlusOutline,
    mdiAlertCircleOutline,
    mdiArrowLeft,
    mdiAt,
    mdiChevronRight,
    mdiEmailCheckOutline,
    mdiEmailOutline,
    mdiFrequentlyAskedQuestions,
    mdiInformationOutline,
    mdiKeyOutline,
    mdiLightbulbOutline,
    mdiLockOutline,
    mdiLogin,
    mdiMagnify,
    mdiShieldAccountOutline,
    mdiShieldCheckOutline,
  } from '@mdi/js'
  import AppTopbar from '@/components/layout/AppTopbar.vue'
  import AppFooter from '@/components/layout/AppFooter.vue'

  type HelpCategoryId = 'all' | 'create' | 'login' | 'password' | 'email' | 'security'

  type HelpCategory = {
    id: HelpCategoryId
    label: string
    description: string
    icon: string
  }

  type HelpItem = {
    id: string
    category: Exclude<HelpCategoryId, 'all'>
    question: string
    summary: string
    keywords: string
    paragraphs?: string[]
    steps?: string[]
    tips?: string[]
  }

  const search = ref('')
  const selectedCategory = ref<HelpCategoryId>('all')
  const openedItems = ref<string[]>([])
  const answersSection = ref<HTMLElement | null>(null)

  const categories: HelpCategory[] = [
    {
      id: 'all',
      label: 'Todos',
      description: 'Veja todos os assuntos',
      icon: mdiFrequentlyAskedQuestions,
    },
    {
      id: 'create',
      label: 'Criar conta',
      description: 'Cadastro e dados necessários',
      icon: mdiAccountPlusOutline,
    },
    {
      id: 'login',
      label: 'Acessar',
      description: 'Login e mensagens de acesso',
      icon: mdiLogin,
    },
    {
      id: 'password',
      label: 'Senha',
      description: 'Regras e boas práticas',
      icon: mdiKeyOutline,
    },
    {
      id: 'email',
      label: 'E-mail',
      description: 'Formato e confirmação',
      icon: mdiEmailOutline,
    },
    {
      id: 'security',
      label: 'Segurança',
      description: 'Bloqueios e proteção',
      icon: mdiShieldAccountOutline,
    },
  ]

  const helpItems: HelpItem[] = [
    {
      id: 'create-account',
      category: 'create',
      question: 'Como criar uma conta?',
      summary: 'Saiba quais informações serão solicitadas e como concluir o cadastro.',
      keywords: 'cadastro registrar nova conta nome nascimento gênero telefone criar',
      steps: [
        'Na página de acesso, selecione “Criar conta”.',
        'Informe um endereço de e-mail válido e crie uma senha que atenda aos requisitos.',
        'Preencha seu nome, data de nascimento, gênero e telefone para contato.',
        'Revise as informações e selecione “Criar conta”.',
        'Depois do cadastro, confira sua caixa de entrada e confirme o e-mail quando solicitado.',
      ],
      tips: [
        'Use dados que você reconheça e consiga recuperar depois.',
        'Não crie mais de uma conta usando o mesmo endereço de e-mail.',
      ],
    },
    {
      id: 'registration-data',
      category: 'create',
      question: 'Quais dados preciso informar?',
      summary: 'E-mail, senha, dados pessoais básicos e um telefone para contato.',
      keywords: 'dados obrigatórios cadastro email senha nome nascimento gênero telefone',
      paragraphs: [
        'O cadastro solicita dados de acesso, dados pessoais e um telefone. Campos obrigatórios precisam ser preenchidos antes do envio.',
        'O nome deve ter entre 2 e 100 caracteres. A data de nascimento não pode estar no futuro. O telefone deve incluir país, código de área e número válidos.',
      ],
    },
    {
      id: 'sign-in',
      category: 'login',
      question: 'Como acessar minha conta?',
      summary: 'Use o mesmo e-mail e a mesma senha cadastrados na criação da conta.',
      keywords: 'entrar acessar login lembrar-me credenciais email senha',
      steps: [
        'Abra a página “Acessar”.',
        'Digite o endereço de e-mail completo usado no cadastro.',
        'Digite sua senha respeitando letras maiúsculas, minúsculas e caracteres especiais.',
        'Se o dispositivo for pessoal, você pode marcar “Lembrar-me”.',
        'Selecione “Entrar”.',
      ],
      tips: ['Em um computador compartilhado, não use a opção “Lembrar-me”.'],
    },
    {
      id: 'invalid-credentials',
      category: 'login',
      question: 'A mensagem diz “E-mail ou senha inválidos”',
      summary: 'Confira a digitação e confirme se está usando os dados da conta correta.',
      keywords: 'inválido credenciais erro entrar acesso não consigo login',
      steps: [
        'Confira se o e-mail está completo e sem espaços antes ou depois.',
        'Verifique se Caps Lock está ativado e se o teclado está no idioma esperado.',
        'Digite a senha novamente em vez de usar uma senha antiga salva pelo navegador.',
        'Se não lembrar a senha, use “Esqueci minha senha” na página de acesso.',
      ],
    },
    {
      id: 'password-rules',
      category: 'password',
      question: 'Quais são as regras para criar uma senha?',
      summary: 'A senha deve ter de 8 a 256 caracteres e combinar diferentes tipos de caracteres.',
      keywords: 'regra requisito senha maiúscula minúscula número especial tamanho exemplo',
      paragraphs: ['Sua senha precisa atender a todos os requisitos abaixo:'],
      steps: [
        'Ter no mínimo 8 e no máximo 256 caracteres.',
        'Conter pelo menos uma letra maiúscula, como A–Z.',
        'Conter pelo menos uma letra minúscula, como a–z.',
        'Conter pelo menos um número, como 0–9.',
        'Conter pelo menos um caractere especial, como !, @, #, $ ou %.',
      ],
      tips: [
        'Exemplo de formato válido: SolVerde!2026. Use apenas como referência e crie uma senha diferente.',
        '“senha123” não atende às regras porque não tem letra maiúscula nem caractere especial.',
      ],
    },
    {
      id: 'secure-password',
      category: 'password',
      question: 'Como escolher uma senha segura?',
      summary: 'Prefira uma senha longa, exclusiva e difícil de relacionar com você.',
      keywords: 'forte segura segurança senha exclusiva gerenciador',
      tips: [
        'Não reutilize a senha do seu e-mail, banco ou rede social.',
        'Evite nome, aniversário, telefone, sequências e palavras muito comuns.',
        'Uma frase longa e fácil de lembrar costuma ser melhor do que uma palavra curta.',
        'Considere usar um gerenciador de senhas confiável.',
      ],
    },
    {
      id: 'email-format',
      category: 'email',
      question: 'Qual formato de e-mail devo usar?',
      summary: 'Informe um endereço completo que você consiga acessar.',
      keywords: 'email formato arroba exemplo domínio inválido caixa entrada',
      paragraphs: [
        'O e-mail deve ter no máximo 254 caracteres e seguir o formato nome@dominio.com.',
        'Exemplo válido: nome@exemplo.com. Exemplos inválidos: nome@, @exemplo.com e nome exemplo.com.',
      ],
      tips: ['Revise erros de digitação antes de criar a conta; mensagens de confirmação serão enviadas para esse endereço.'],
    },
    {
      id: 'email-confirmation',
      category: 'email',
      question: 'Não recebi o e-mail de confirmação',
      summary: 'Confira pastas alternativas e aguarde alguns minutos antes de tentar novamente.',
      keywords: 'confirmação código token não chegou spam lixo eletrônico email',
      steps: [
        'Confirme se o endereço informado no cadastro está correto.',
        'Verifique Spam, Lixo eletrônico, Promoções e outras pastas filtradas.',
        'Pesquise por mensagens enviadas pela YaeaY.',
        'Aguarde alguns minutos, pois o provedor de e-mail pode atrasar a entrega.',
        'Se houver uma opção de reenvio, solicite somente uma nova mensagem e use o código ou link mais recente.',
      ],
    },
    {
      id: 'confirmation-required',
      category: 'login',
      question: 'O acesso pede confirmação do e-mail',
      summary: 'A conta foi criada, mas o endereço ainda precisa ser confirmado.',
      keywords: 'confirmar email login acesso obrigatório confirmação',
      paragraphs: [
        'Por segurança, algumas contas só podem ser acessadas depois que o endereço de e-mail é confirmado. Abra a mensagem recebida durante o cadastro e conclua a confirmação.',
        'Se o link ou código estiver expirado, solicite uma nova mensagem e use somente a mais recente.',
      ],
    },
    {
      id: 'locked-account',
      category: 'security',
      question: 'Minha conta está temporariamente bloqueada',
      summary: 'Muitas tentativas incorretas podem provocar um bloqueio temporário de segurança.',
      keywords: 'bloqueada bloqueio locked tentativas esperar segurança',
      paragraphs: [
        'Pare de repetir tentativas por alguns minutos. Novas tentativas consecutivas podem prolongar a proteção temporária.',
        'Depois, confira o e-mail e a senha ou inicie a recuperação de senha. A equipe de atendimento nunca deve pedir sua senha completa.',
      ],
    },
    {
      id: 'disabled-account',
      category: 'security',
      question: 'Minha conta aparece como suspensa ou desabilitada',
      summary: 'Esse estado é diferente de uma senha incorreta e pode exigir uma análise específica.',
      keywords: 'suspensa desabilitada desativada bloqueio análise suporte',
      paragraphs: [
        'Uma conta suspensa ou desabilitada não é liberada apenas trocando a senha. Leia atentamente a mensagem exibida no acesso e siga a orientação correspondente.',
        'Evite criar contas adicionais para contornar uma restrição. Isso pode dificultar a identificação da conta correta.',
      ],
    },
    {
      id: 'account-security',
      category: 'security',
      question: 'Acho que alguém tentou acessar minha conta',
      summary: 'Proteja primeiro seu e-mail e depois atualize as credenciais da conta.',
      keywords: 'invadida comprometida acesso suspeito fraude phishing segurança',
      steps: [
        'Troque imediatamente a senha do seu e-mail, caso também exista suspeita sobre ele.',
        'Crie uma nova senha exclusiva para o YaeaY Account.',
        'Não compartilhe links, códigos de verificação ou sua senha.',
        'Revise mensagens de segurança e desconsidere pedidos enviados por canais não oficiais.',
      ],
      tips: ['A YaeaY nunca solicitará sua senha completa nem um código de verificação por telefone, mensagem ou e-mail.'],
    },
  ]

  const normalizedSearch = computed(() => search.value.trim().toLocaleLowerCase('pt-BR'))

  const filteredItems = computed(() => helpItems.filter((item) => {
    const matchesCategory = selectedCategory.value === 'all'
      || item.category === selectedCategory.value

    if (!matchesCategory) return false
    if (!normalizedSearch.value) return true

    const searchableText = [
      item.question,
      item.summary,
      item.keywords,
      ...(item.paragraphs ?? []),
      ...(item.steps ?? []),
      ...(item.tips ?? []),
    ].join(' ').toLocaleLowerCase('pt-BR')

    return searchableText.includes(normalizedSearch.value)
  }))

  const selectedCategoryLabel = computed(() => categories.find(
    category => category.id === selectedCategory.value
  )?.label ?? 'Todos')

  async function selectCategory(categoryId: HelpCategoryId): Promise<void> {
    selectedCategory.value = categoryId
    openedItems.value = []
    await nextTick()

    answersSection.value?.scrollIntoView({
      behavior: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth',
      block: 'start',
    })
  }

  function clearFilters(): void {
    search.value = ''
    selectedCategory.value = 'all'
    openedItems.value = []
  }
</script>

<template>
  <v-main class="help-page">
    <section class="help-shell">
      <AppTopbar :show-action="false" />

      <header class="help-hero">
        <v-container class="help-container">
          <v-btn class="help-back"
                 variant="text"
                 :prepend-icon="mdiArrowLeft"
                 :to="{ name: 'login' }"
                 :ripple="false">
            Acessar minha conta
          </v-btn>

          <div class="help-hero__content">
            <p class="help-eyebrow">Central de ajuda</p>
            <h1>Como podemos ajudar?</h1>
            <p class="help-hero__lead">
              Encontre orientações rápidas para criar sua conta, acessar com segurança
              e resolver as situações mais comuns.
            </p>

            <v-text-field v-model="search"
                          class="help-search"
                          :prepend-inner-icon="mdiMagnify"
                          placeholder="Busque por senha, e-mail, cadastro, bloqueio..."
                          aria-label="Buscar na central de ajuda"
                          variant="solo"
                          rounded="xl"
                          clearable
                          hide-details />
          </div>
        </v-container>
      </header>

      <v-container class="help-container help-content">
        <section aria-labelledby="help-categories-title">
          <div class="section-heading">
            <div>
              <p class="section-heading__eyebrow">Escolha um assunto</p>
              <h2 id="help-categories-title">O que você precisa resolver?</h2>
            </div>
            <span class="section-heading__count">
              {{ filteredItems.length }} {{ filteredItems.length === 1 ? 'orientação' : 'orientações' }}
            </span>
          </div>

          <v-row class="category-grid" justify="center" density="comfortable">
            <v-col v-for="category in categories"
                   :key="category.id"
                   cols="12"
                   sm="6"
                   md="4">
              <v-card tag="button"
                      type="button"
                      variant="flat"
                      ripple
                      class="category-card"
                      :class="{ 'category-card--active': selectedCategory === category.id }"
                      :aria-pressed="selectedCategory === category.id"
                      @click="selectCategory(category.id)">
                <span class="category-card__icon">
                  <v-icon :icon="category.icon" size="23" />
                </span>
                <span class="category-card__copy">
                  <strong>{{ category.label }}</strong>
                  <small>{{ category.description }}</small>
                </span>
                <v-icon :icon="mdiChevronRight" size="18" />
              </v-card>
            </v-col>
          </v-row>
        </section>

        <section ref="answersSection" class="answers-section" aria-labelledby="help-answers-title">
          <div class="section-heading section-heading--answers">
            <div>
              <p class="section-heading__eyebrow">{{ selectedCategoryLabel }}</p>
              <h2 id="help-answers-title">
                {{ normalizedSearch ? `Resultados para “${search.trim()}”` : 'Orientações úteis' }}
              </h2>
            </div>
          </div>

          <v-expansion-panels v-if="filteredItems.length"
                              v-model="openedItems"
                              multiple
                              class="help-answers">
            <v-expansion-panel v-for="item in filteredItems"
                               :key="item.id"
                               :value="item.id"
                               class="help-answer"
                               elevation="0">
              <v-expansion-panel-title class="help-answer__title">
                <div class="help-answer__heading">
                  <strong>{{ item.question }}</strong>
                  <span>{{ item.summary }}</span>
                </div>
              </v-expansion-panel-title>

              <v-expansion-panel-text class="help-answer__content">
                <p v-for="paragraph in item.paragraphs" :key="paragraph">
                  {{ paragraph }}
                </p>

                <ol v-if="item.steps?.length" class="help-steps">
                  <li v-for="(step, index) in item.steps" :key="step">
                    <span class="help-steps__number">{{ index + 1 }}</span>
                    <span>{{ step }}</span>
                  </li>
                </ol>

                <div v-if="item.tips?.length" class="help-tips">
                  <div class="help-tips__title">
                    <v-icon :icon="mdiLightbulbOutline" size="20" />
                    <strong>{{ item.tips.length === 1 ? 'Dica importante' : 'Dicas importantes' }}</strong>
                  </div>
                  <ul>
                    <li v-for="tip in item.tips" :key="tip">{{ tip }}</li>
                  </ul>
                </div>
              </v-expansion-panel-text>
            </v-expansion-panel>
          </v-expansion-panels>

          <div v-else class="empty-state">
            <span class="empty-state__icon">
              <v-icon :icon="mdiAlertCircleOutline" size="30" />
            </span>
            <h3>Nenhuma orientação encontrada</h3>
            <p>Tente usar outras palavras ou consulte todos os assuntos.</p>
            <v-btn variant="outlined" rounded="pill" @click="clearFilters">
              Limpar busca e filtros
            </v-btn>
          </div>
        </section>

        <section class="quick-guide" aria-labelledby="quick-guide-title">
          <div class="quick-guide__header">
            <span class="quick-guide__icon">
              <v-icon :icon="mdiInformationOutline" size="26" />
            </span>
            <div>
              <p class="section-heading__eyebrow">Antes de começar</p>
              <h2 id="quick-guide-title">Checklist rápido</h2>
            </div>
          </div>

          <div class="quick-guide__grid">
            <div class="quick-guide__item">
              <v-icon :icon="mdiAt" size="21" />
              <div><strong>E-mail completo</strong><span>Use o mesmo endereço informado no cadastro.</span></div>
            </div>
            <div class="quick-guide__item">
              <v-icon :icon="mdiLockOutline" size="21" />
              <div><strong>Senha exata</strong><span>Confira Caps Lock, idioma do teclado e espaços.</span></div>
            </div>
            <div class="quick-guide__item">
              <v-icon :icon="mdiEmailCheckOutline" size="21" />
              <div><strong>Caixa de entrada</strong><span>Verifique Spam e use apenas o código mais recente.</span></div>
            </div>
            <div class="quick-guide__item">
              <v-icon :icon="mdiShieldCheckOutline" size="21" />
              <div><strong>Canal seguro</strong><span>Nunca compartilhe sua senha ou código de verificação.</span></div>
            </div>
          </div>
        </section>

        <section class="help-actions" aria-label="Ações da conta">
          <div>
            <p class="section-heading__eyebrow">Pronto para continuar?</p>
            <h2>Acesse ou crie sua conta</h2>
            <p>Esta central contém apenas orientações e não armazena nenhuma informação.</p>
          </div>
          <div class="help-actions__buttons">
            <v-btn class="help-actions__primary"
                   rounded="pill"
                   size="large"
                   :prepend-icon="mdiLogin"
                   :to="{ name: 'login' }">
              Acessar
            </v-btn>
            <v-btn variant="outlined"
                   rounded="pill"
                   size="large"
                   :prepend-icon="mdiAccountPlusOutline"
                   :to="{ name: 'user-create' }">
              Criar conta
            </v-btn>
          </div>
        </section>
      </v-container>

      <AppFooter copyright="© 2026 YaeaY Software ®"
                 text-one="Privacidade"
                 :to-one="{ name: 'privacy' }"
                 text-two="Termos"
                 :to-two="{ name: 'terms' }"
                 text-three="Segurança"
                 :to-three="{ name: 'security' }" />
    </section>
  </v-main>
</template>

<style scoped>
  .help-page {
    min-height: 100vh;
    color: #183729;
    background: #f5f8f6;
  }

  .help-shell {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }

  .help-container {
    width: min(1120px, 100%);
    margin-inline: auto !important;
    padding-inline: 32px;
  }

  .help-hero {
    overflow: hidden;
    position: relative;
    background:
      radial-gradient(circle at 18% 8%, rgba(128, 185, 151, 0.2), transparent 32%),
      radial-gradient(circle at 84% 76%, rgba(24, 55, 41, 0.09), transparent 30%),
      linear-gradient(145deg, #edf5f0 0%, #f8faf9 72%);
    border-bottom: 1px solid rgba(24, 55, 41, 0.08);
  }

  .help-hero::after {
    content: '';
    position: absolute;
    width: 260px;
    height: 260px;
    right: -110px;
    top: -130px;
    border: 46px solid rgba(36, 92, 67, 0.05);
    border-radius: 50%;
    pointer-events: none;
  }

  .help-back {
    margin-top: 22px;
    margin-left: -14px;
    color: #3e564f;
    text-transform: none;
  }

  .help-hero__content {
    max-width: 760px;
    margin: 0 auto;
    padding: 42px 0 64px;
    text-align: center;
  }

  .help-eyebrow,
  .section-heading__eyebrow {
    margin: 0 0 8px;
    color: #39725a;
    font-size: 0.76rem;
    font-weight: 800;
    letter-spacing: 0.15em;
    text-transform: uppercase;
  }

  .help-hero h1 {
    margin: 0;
    color: #173f32;
    font-family: inherit;
    font-size: clamp(2rem, 4vw, 2.75rem);
    font-weight: 800;
    line-height: 1.1;
    letter-spacing: -0.04em;
  }

  .help-hero__lead {
    max-width: 650px;
    margin: 18px auto 28px;
    color: #53675e;
    font-size: 1.04rem;
    line-height: 1.65;
  }

  .help-search {
    max-width: 680px;
    margin: 0 auto;
  }

  :deep(.help-search .v-field) {
    min-height: 60px;
    color: #183729;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.11);
    box-shadow: 0 16px 38px rgba(24, 55, 41, 0.1);
  }

  :deep(.help-search .v-field__input) {
    min-height: 60px;
    font-size: 1rem;
  }

  .help-content {
    width: min(1120px, 100%);
    padding-top: 54px;
    padding-bottom: 70px;
  }

  .section-heading {
    margin-bottom: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    gap: 8px;
    text-align: center;
  }

  .section-heading h2,
  .quick-guide h2,
  .help-actions h2 {
    margin: 0;
    color: #183729;
    font-size: clamp(1.45rem, 3vw, 2rem);
    letter-spacing: -0.025em;
  }

  .section-heading__count {
    padding: 0;
    color: #6a7d74;
    font-size: 0.86rem;
    white-space: nowrap;
  }

  .category-grid {
    width: 100%;
    margin-inline: auto;
  }

  .category-card {
    width: 100%;
    height: 100%;
    min-height: 86px;
    padding: 16px;
    display: grid;
    grid-template-columns: 42px minmax(0, 1fr) auto;
    align-items: center;
    gap: 13px;
    color: #183729;
    text-align: left;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.11);
    border-radius: 16px;
    cursor: pointer;
    transition: border-color 160ms ease, transform 160ms ease, box-shadow 160ms ease;
  }

  .category-card:hover {
    transform: translateY(-2px);
    border-color: rgba(36, 92, 67, 0.4);
    box-shadow: 0 12px 26px rgba(24, 55, 41, 0.08);
  }

  .category-card:focus-visible {
    outline: 3px solid rgba(36, 92, 67, 0.24);
    outline-offset: 2px;
  }

  .category-card--active {
    color: #ffffff;
    background: #245c43;
    border-color: #245c43;
    box-shadow: 0 12px 28px rgba(36, 92, 67, 0.2);
  }

  .category-card__icon {
    width: 42px;
    height: 42px;
    display: grid;
    place-items: center;
    color: #245c43;
    background: #edf4ef;
    border-radius: 13px;
  }

  .category-card--active .category-card__icon {
    color: #ffffff;
    background: rgba(255, 255, 255, 0.15);
  }

  .category-card__copy {
    min-width: 0;
  }

  .category-card__copy strong,
  .category-card__copy small {
    display: block;
  }

  .category-card__copy strong {
    font-size: 0.96rem;
  }

  .category-card__copy small {
    margin-top: 3px;
    overflow: hidden;
    color: #6a7d74;
    font-size: 0.76rem;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .category-card--active .category-card__copy small {
    color: rgba(255, 255, 255, 0.76);
  }

  .answers-section {
    padding-top: 58px;
    scroll-margin-top: 24px;
  }

  .section-heading--answers {
    align-items: center;
  }

  .help-answers {
    gap: 10px;
  }

  .help-answer {
    overflow: hidden;
    background: #ffffff !important;
    border: 1px solid rgba(24, 55, 41, 0.1);
    border-radius: 16px !important;
  }

  .help-answer::after,
  .help-answer::before {
    display: none;
  }

  :deep(.help-answer__title) {
    min-height: 88px;
    padding: 19px 22px;
  }

  :deep(.help-answer__title:hover) {
    background: #f8faf9;
  }

  .help-answer__heading {
    padding-right: 16px;
  }

  .help-answer__heading strong,
  .help-answer__heading span {
    display: block;
  }

  .help-answer__heading strong {
    color: #183729;
    font-size: 1rem;
  }

  .help-answer__heading span {
    margin-top: 5px;
    color: #677970;
    font-size: 0.86rem;
    line-height: 1.45;
  }

  :deep(.help-answer__content .v-expansion-panel-text__wrapper) {
    padding: 2px 22px 24px;
  }

  .help-answer__content p {
    margin: 0 0 12px;
    color: #465c52;
    line-height: 1.65;
  }

  .help-steps {
    margin: 8px 0 0;
    padding: 0;
    display: grid;
    gap: 11px;
    list-style: none;
  }

  .help-steps li {
    display: flex;
    align-items: flex-start;
    gap: 11px;
    color: #344d42;
    line-height: 1.5;
  }

  .help-steps__number {
    width: 25px;
    height: 25px;
    display: grid;
    place-items: center;
    flex: 0 0 auto;
    color: #245c43;
    background: #eaf3ee;
    border-radius: 50%;
    font-size: 0.76rem;
    font-weight: 800;
  }

  .help-tips {
    margin-top: 19px;
    padding: 16px 18px;
    color: #6d5319;
    background: #fff8e8;
    border-radius: 13px;
  }

  .help-tips__title {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .help-tips ul {
    margin: 9px 0 0;
    padding-left: 20px;
    display: grid;
    gap: 6px;
    line-height: 1.5;
  }

  .empty-state {
    padding: 48px 24px;
    text-align: center;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.1);
    border-radius: 18px;
  }

  .empty-state__icon {
    width: 58px;
    height: 58px;
    display: grid;
    place-items: center;
    margin: 0 auto 15px;
    color: #527165;
    background: #edf4ef;
    border-radius: 17px;
  }

  .empty-state h3 {
    margin: 0;
    color: #183729;
  }

  .empty-state p {
    margin: 8px 0 20px;
    color: #6a7d74;
  }

  .quick-guide {
    margin-top: 58px;
    padding: 28px;
    background: #183729;
    border-radius: 22px;
    box-shadow: 0 20px 50px rgba(24, 55, 41, 0.16);
  }

  .quick-guide__header {
    display: flex;
    align-items: center;
    gap: 14px;
  }

  .quick-guide__header .section-heading__eyebrow {
    color: #9bc6ad;
  }

  .quick-guide h2 {
    color: #ffffff;
  }

  .quick-guide__icon {
    width: 48px;
    height: 48px;
    display: grid;
    place-items: center;
    color: #c8e5d3;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 14px;
  }

  .quick-guide__grid {
    margin-top: 24px;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
  }

  .quick-guide__item {
    padding: 16px;
    display: flex;
    align-items: flex-start;
    gap: 11px;
    color: #c8e5d3;
    background: rgba(255, 255, 255, 0.07);
    border-radius: 13px;
  }

  .quick-guide__item strong,
  .quick-guide__item span {
    display: block;
  }

  .quick-guide__item strong {
    color: #ffffff;
    font-size: 0.9rem;
  }

  .quick-guide__item span {
    margin-top: 3px;
    color: #bed1c6;
    font-size: 0.8rem;
    line-height: 1.45;
  }

  .help-actions {
    margin-top: 32px;
    padding: 28px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 28px;
    background: #ffffff;
    border: 1px solid rgba(24, 55, 41, 0.1);
    border-radius: 22px;
  }

  .help-actions p:not(.section-heading__eyebrow) {
    margin: 8px 0 0;
    color: #687a71;
  }

  .help-actions__buttons {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-shrink: 0;
  }

  .help-actions__primary {
    color: #ffffff;
    background: #245c43;
  }

  @media (max-width: 900px) {
    .help-actions {
      align-items: flex-start;
      flex-direction: column;
    }
  }

  @media (max-width: 600px) {
    .help-container {
      padding-inline: 16px;
    }

    .help-hero__content {
      padding: 30px 0 46px;
    }

    .help-back {
      margin-top: 12px;
    }

    .help-hero__lead {
      font-size: 0.96rem;
    }

    .help-content {
      padding-top: 38px;
      padding-bottom: 48px;
    }

    .quick-guide__grid {
      grid-template-columns: 1fr;
    }

    .category-card {
      min-height: 78px;
    }

    .answers-section {
      padding-top: 42px;
    }

    :deep(.help-answer__title) {
      padding: 17px;
    }

    :deep(.help-answer__content .v-expansion-panel-text__wrapper) {
      padding: 0 17px 20px;
    }

    .quick-guide,
    .help-actions {
      padding: 21px;
      border-radius: 18px;
    }

    .help-actions__buttons {
      width: 100%;
      align-items: stretch;
      flex-direction: column;
    }

    .help-actions__buttons :deep(.v-btn) {
      width: 100%;
    }
  }
</style>
