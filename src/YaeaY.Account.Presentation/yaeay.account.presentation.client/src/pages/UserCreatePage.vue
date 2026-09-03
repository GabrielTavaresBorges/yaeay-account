                               <!-- src/pages/UserCreatePage.vue -->

<script setup lang="ts">
  import { computed, reactive, ref, watch, nextTick } from 'vue'
  import {
    mdiAccountPlusOutline,
    mdiAccountOutline,
    mdiHelpCircleOutline,
    mdiShieldLockOutline,
    mdiCheckCircle,
    mdiCalendar,
    mdiEmailCheckOutline,
    mdiEmailOutline,
    mdiPhoneOutline,
    mdiInboxArrowDownOutline,
    mdiAlertCircleOutline,
    mdiArrowLeft,
    mdiArrowRight
  } from '@mdi/js'

  import { createUser } from '@/services/users/users-service'
  import { enqueueSnackbar } from '@/services/ui/snackbar-queue'
  import { rules } from '@/validators'
  import AppTopbar from '@/components/layout/AppTopbar.vue'
  import AppFooter from '@/components/layout/AppFooter.vue'
  import {
    EmailField,
    PasswordField,
    GenderSelect,
    UserPhonesField,
    FullNameField
  } from '@/components/inputs'
  import type { Gender } from '@/constants/gender'
  import type { PhoneModel } from '@/models/phone-model'
  import { PasswordRequirements } from '@/components/feedback'
  import { countryItems } from '@/constants/country'
  import { getPhoneDigitsRange } from '@/services/phoneFormat/phone-format-service'


  type VForm = { validate: () => Promise<{ valid: boolean }> }

  const birthDateFieldRules = computed(() => [
    () => {
      for (const r of rules.birthDate) {
        const response = r(form.birthDate)
        if (response !== true) return response
      }
      return true
    },
  ])

  /* panels */
  const openedPanels = ref<string[]>(['accessData'])

  /* refs */
  const formRef = ref<VForm | null>(null)
  const loading = ref(false)
  const accountCreated = ref(false)
  const registeredEmail = ref('')
  const registeredFullName = ref('')

  /* password */

  function notify(text: string, type: 'success' | 'info' | 'error' = 'info') {
    enqueueSnackbar(text, type)
  }

  /* form model (visual) */
  const form = reactive({
    email: '',
    password: '',
    confirmPassword: '',
    fullName: '',
    birthDate: null as Date | null,
    gender: null as Gender | null,
    phones: {
      callingCode: '+55',
      country: 'BR',
      phoneType: 'Mobile',
      areaCode: '11',
      number: '',
    } as PhoneModel,
  })

  /* password rules visuals */
  const upperRegex = /[A-Z]/
  const lowerRegex = /[a-z]/
  const digitRegex = /\d/
  const specialRegex = /[^A-Za-z0-9]/

  const passwordOk = computed(() => {
    const p = form.password ?? ''
    return (
      p.length >= 8 &&
      upperRegex.test(p) &&
      lowerRegex.test(p) &&
      digitRegex.test(p) &&
      specialRegex.test(p)
    )
  })

  const accessDataCompleted = computed(() =>
    runRules(rules.email, form.email) === true
    && runRules(rules.password, form.password) === true
    && form.confirmPassword.length > 0
    && form.confirmPassword === form.password,
  )

  const personalDataCompleted = computed(() =>
    runRules(rules.fullName, form.fullName) === true
    && runRules(rules.gender, form.gender) === true
    && birthDateFieldRules.value.every(rule => rule() === true),
  )

  const contactCompleted = computed(() => hasAtLeastOneValidPhone())

  /* birth date picker */
  const birthMenu = ref(false)

  const birthLabel = computed(() => {
    if (!form.birthDate) return 'Selecione uma data'
    return new Intl.DateTimeFormat('pt-BR').format(form.birthDate)
  })

  // string YYYY-MM-DD para enviar no backend
  const birthDateIso = computed(() => {
    if (!form.birthDate) return ''
    const y = form.birthDate.getFullYear()
    const m = String(form.birthDate.getMonth() + 1).padStart(2, '0')
    const d = String(form.birthDate.getDate()).padStart(2, '0')
    return `${y}-${m}-${d}`
  })

  const passwordChecklist = computed(() => {
    const pwd = form.password ?? ''
    return [
      { text: 'Mínimo de 8 caracteres', valid: pwd.trim().length >= 8 },
      { text: '1 letra maiúscula', valid: /[A-Z]/.test(pwd) },
      { text: '1 letra minúscula', valid: /[a-z]/.test(pwd) },
      { text: '1 número', valid: /\d/.test(pwd) },
      { text: '1 caractere especial (ex: @, #, $).', valid: /[^A-Za-z0-9]/.test(pwd) },
    ]
  })

  function runRules(ruleList: Array<(v: any) => true | string>, value: any) {
    for (const rule of ruleList) {
      const response = rule(value)
      if (response !== true) return response
    }
    return true
  }

  function hasAtLeastOneValidPhone(): boolean {
    const p = form.phones
    if (!p) return false

    const rawNumber = (p.number ?? '').replace(/\D/g, '')
    const rawAreaCode = (p.areaCode ?? '').replace(/\D/g, '')
    const callingOk = /^\+\d{1,3}$/.test((p.callingCode ?? '').trim())
    const countryOk = /^[A-Z]{2}$/.test((p.country ?? '').trim().toUpperCase())
    const typeOk = p.phoneType === 'Mobile' || p.phoneType === 'Landline'
    const areaCodeOk = p.country === 'BR'
      ? rawAreaCode.length === 2
      : rawAreaCode.length > 0
    const digitsRange = getPhoneDigitsRange(p.callingCode, p.country, p.phoneType)
    const numberOk = rawNumber.length >= digitsRange.minDigits
      && rawNumber.length <= digitsRange.maxDigits

    return callingOk && countryOk && typeOk && areaCodeOk && numberOk
  }

  function clearRegistrationForm(): void {
    form.email = ''
    form.password = ''
    form.confirmPassword = ''
    form.fullName = ''
    form.birthDate = null
    form.gender = null
    form.phones.callingCode = '+55'
    form.phones.country = 'BR'
    form.phones.phoneType = 'Mobile'
    form.phones.areaCode = '11'
    form.phones.number = ''
    birthMenu.value = false
    openedPanels.value = ['accessData']
  }

  function getPanelsWithErrors(): string[] {
    const panels = new Set<string>()

    // DADOS DE ACESSO
    if (runRules(rules.email, form.email) !== true) panels.add('accessData')

    // DADOS PESSOAIS
    if (runRules(rules.fullName, form.fullName) !== true) panels.add('personalData')
    if (runRules(rules.gender, form.gender) !== true) panels.add('personalData')

    for (const fn of birthDateFieldRules.value) {
      if (fn() !== true) {
        panels.add('personalData')
        break
      }
    }

    // CONTATO
    if (!hasAtLeastOneValidPhone()) {
      panels.add('contact')
    }

    return [...panels]
  }

  async function createAccount() {

    // 1) abre painéis que certamente têm erro (mesmo fechados)
    const panelsToOpen = getPanelsWithErrors()
    if (panelsToOpen.length) {
      openedPanels.value = Array.from(new Set([...openedPanels.value, ...panelsToOpen]))
      await nextTick() // espera o Vue renderizar os campos dos painéis abertos
    }

    // 2) valida o que estiver montado agora (inclui os painéis recém-abertos)
    const validation = await formRef.value?.validate()
    if (validation && !validation.valid) {
      notify('Revise os campos obrigatórios.', 'error')
      return
    }


    const rawPhoneNumber = form.phones.number.replace(/\D/g, '')

    // 3) segue fluxo normal
    const payload = {
      emailAddress: form.email.trim(),
      password: form.password.trim(),
      fullName: form.fullName.trim(),
      birthDate: birthDateIso.value,
      gender: form.gender as Gender,
      callingCode: form.phones.callingCode,
      regionCode: form.phones.country,
      areaCode: form.phones.areaCode,
      phoneType: form.phones.phoneType,
      phoneNumber: rawPhoneNumber,
    }

    try {
      loading.value = true
      const result = await createUser(payload)
      registeredEmail.value = payload.emailAddress
      registeredFullName.value = result.fullName
      clearRegistrationForm()
      accountCreated.value = true
      await nextTick()
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } catch (e: any) {
      notify(e?.message || 'Erro ao criar usuário.', 'error')
    } finally {
      loading.value = false
    }
  }
</script>

<template>
  <v-main class="page">
    <AppTopbar
      :action-text="accountCreated ? 'Acessar' : 'Ajuda'"
      :action-to="accountCreated ? '/login' : '/help'"
    />

    <v-container fluid class="user-create-container py-6 py-md-10">
      <v-row class="user-create-row" justify="center" align="center">
        <v-col cols="12" class="user-create-column">

          <template v-if="accountCreated">
            <v-card class="registration-success" rounded="xl" elevation="0">
              <div class="registration-success__icon" aria-hidden="true">
                <v-icon :icon="mdiEmailCheckOutline" size="68" />
              </div>

              <p class="registration-success__eyebrow">Cadastro concluído</p>
              <h1 class="registration-success__title">Seu Account foi criado com sucesso.</h1>
              <p class="registration-success__greeting">
                Olá, {{ registeredFullName }}.
              </p>
              <p class="registration-success__lead">
                Enviamos um e-mail de confirmação para:
              </p>

              <div class="registration-success__email">
                {{ registeredEmail }}
              </div>

              <p class="registration-success__instruction">
                Abra a mensagem enviada pela YaeaY e selecione
                <strong>Confirmar meu e-mail</strong> para ativar seu Account.
              </p>

              <div class="registration-success__notice">
                <v-icon :icon="mdiInboxArrowDownOutline" size="30" />
                <div>
                  <strong>Não encontrou a mensagem?</strong>
                  <p>
                    Aguarde alguns minutos e verifique também as pastas de spam,
                    lixo eletrônico e promoções.
                  </p>
                </div>
              </div>

              <div class="registration-success__security">
                <v-icon :icon="mdiAlertCircleOutline" size="21" />
                <span>
                  O acesso será liberado somente após a confirmação do endereço de e-mail.
                </span>
              </div>

              <v-btn
                rounded="pill"
                size="large"
                class="registration-success__login"
                :append-icon="mdiArrowRight"
                :to="{ name: 'login' }"
              >
                Já confirmei — acessar Account
              </v-btn>
            </v-card>
          </template>

          <template v-else>
          <!-- CARD -->
          <v-card class="shell" rounded="xl" elevation="14">
            <!-- HEADER -->
            <div class="form-heading">
              <div class="form-heading__main">
                <span class="form-heading__icon" aria-hidden="true">
                  <v-icon :icon="mdiAccountPlusOutline" size="36" />
                </span>

                <h1 class="form-heading__title">
                  Crie sua conta
                </h1>
              </div>

              <p class="form-heading__subtitle">
                Informe seus dados básicos para criar seu acesso no Account.
              </p>
            </div>

            <!-- FORM -->
            <div class="pa-6 pa-md-8 pt-0">
              <v-form ref="formRef" @submit.prevent="createAccount">
                <v-expansion-panels v-model="openedPanels" multiple class="mb-6 panels">
                  <!-- DADOS DE ACESSO -->
                  <v-expansion-panel class="panel" value="accessData">
                    <v-expansion-panel-title class="section-title">
                      <span class="section-title__content">
                        <span class="section-title__label">
                          <span class="section-title__icon">
                            <v-icon :icon="mdiEmailOutline" size="23" />
                          </span>
                          <span>Dados de acesso</span>
                        </span>
                        <v-icon v-if="accessDataCompleted"
                                class="panel-completed-icon"
                                :icon="mdiCheckCircle"
                                size="24"
                                aria-label="Dados de acesso preenchidos corretamente" />
                      </span>
                    </v-expansion-panel-title>

                    <v-expansion-panel-text>
                      <EmailField v-model="form.email"
                                  :rules="rules.email"
                                  label="Endereço de e-mail"
                                  placeholder="exemplo@email.com"
                                  class="access-field"
                                  density="comfortable"
                                  clearable />

                      <v-row class="password-row">
                        <v-col cols="12" md="6">
                          <PasswordField v-model="form.password"
                                         :rules="rules.password"
                                         label="Senha"
                                         placeholder="********"
                                         class="access-field"
                                         density="comfortable"
                                         clearable>
                          </PasswordField>
                        </v-col>

                        <v-col cols="12" md="6">
                          <PasswordField v-model="form.confirmPassword"
                                         label="Confirmar senha"
                                         placeholder="********"
                                         :match="form.password"
                                         class="access-field"
                                         density="comfortable"
                                         clearable />
                        </v-col>
                      </v-row>

                      <!-- Requisitos Mínimos -->
                      <PasswordRequirements :rules="passwordChecklist" />

                    </v-expansion-panel-text>
                  </v-expansion-panel>

                  <!-- DADOS PESSOAIS -->
                  <v-expansion-panel class="panel" value="personalData">
                    <v-expansion-panel-title class="section-title">
                      <span class="section-title__content">
                        <span class="section-title__label">
                          <span class="section-title__icon">
                            <v-icon :icon="mdiAccountOutline" size="23" />
                          </span>
                          <span>Dados pessoais</span>
                        </span>
                        <v-icon v-if="personalDataCompleted"
                                class="panel-completed-icon"
                                :icon="mdiCheckCircle"
                                size="24"
                                aria-label="Dados pessoais preenchidos corretamente" />
                      </span>
                    </v-expansion-panel-title>

                    <v-expansion-panel-text>
                      <FullNameField v-model="form.fullName"
                                     :rules="rules.fullName"
                                     class="access-field" />

                      <v-row class="personal-data-row">
                        <v-col cols="12" sm="7">
                          <v-menu v-model="birthMenu"
                                  :close-on-content-click="false"
                                  location="bottom"
                                  transition="scale-transition"
                                  min-width="auto">
                            <template #activator="{ props }">
                              <v-text-field v-bind="props"
                                            :model-value="birthLabel"
                                            label="Data de nascimento"
                                            class="access-field"
                                             readonly
                                             variant="outlined"
                                             rounded="lg"
                                             density="comfortable"
                                            clearable
                                            :rules="birthDateFieldRules"
                                            :prepend-inner-icon="mdiCalendar" />
                            </template>

                            <v-card min-width="300" max-width="340" elevation="12" rounded="lg">
                              <v-date-picker :model-value="form.birthDate"
                                             locale="pt-BR"
                                             hide-header
                                             flat
                                             @update:model-value="(val) => { form.birthDate = val; birthMenu = false }" />
                            </v-card>
                          </v-menu>
                        </v-col>
                        <v-col cols="12" sm="5">
                          <GenderSelect v-model="form.gender"
                                        :rules="rules.gender"
                                        label="Gênero"
                                        clearable />
                        </v-col>
                      </v-row>
                    </v-expansion-panel-text>
                  </v-expansion-panel>

                  <!-- CONTATO -->
                  <v-expansion-panel class="panel" value="contact">
                    <v-expansion-panel-title class="section-title">
                      <span class="section-title__content">
                        <span class="section-title__label">
                          <span class="section-title__icon">
                            <v-icon :icon="mdiPhoneOutline" size="23" />
                          </span>
                          <span>Dados de contato</span>
                        </span>
                        <v-icon v-if="contactCompleted"
                                class="panel-completed-icon"
                                :icon="mdiCheckCircle"
                                size="24"
                                aria-label="Dados de contato preenchidos corretamente" />
                      </span>
                    </v-expansion-panel-title>
                    <v-expansion-panel-text>
                      <UserPhonesField v-model="form.phones"
                                       :multiple="false"
                                       :required="true" />
                    </v-expansion-panel-text>
                  </v-expansion-panel>
                </v-expansion-panels>

                <!-- BOTÃO CRIAR CONTA -->
                <v-btn size="default"
                       rounded="pill"
                       class="create-account-button"
                       type="submit"
                       :loading="loading"
                       :disabled="loading">
                  Criar conta
                </v-btn>

                <v-btn variant="text"
                       class="create-account-back"
                       :prepend-icon="mdiArrowLeft"
                       :to="{ name: 'login' }"
                       :ripple="false">
                  Voltar
                </v-btn>

              </v-form>
            </div>
          </v-card>
          </template>
        </v-col>
      </v-row>
    </v-container>

    <AppFooter copyright="© 2026 YaeaY Software ®"
               text-one="Privacidade"
               :to-one="{ name: 'privacy' }"
               text-two="Termos"
               :to-two="{ name: 'terms' }"
               text-three="Segurança"
               :to-three="{ name: 'security' }" />

  </v-main>
</template>

<style scoped>
  .registration-success {
    width: 100%;
    padding: 52px 54px 48px;
    text-align: center;
    border: 1px solid rgba(24, 55, 41, 0.12);
    background: #ffffff;
    box-shadow: 0 28px 70px rgba(24, 55, 41, 0.1) !important;
  }

  .registration-success__icon {
    width: 112px;
    height: 112px;
    display: grid;
    place-items: center;
    margin: 0 auto 26px;
    border-radius: 50%;
    color: #176143;
    background: #eaf3ee;
    border: 1px solid #cfe0d6;
  }

  .registration-success__eyebrow {
    margin: 0 0 12px;
    color: #497064;
    font-size: 0.76rem;
    font-weight: 800;
    letter-spacing: 0.19em;
    text-transform: uppercase;
  }

  .registration-success__title {
    max-width: 590px;
    margin: 0 auto;
    color: #173f32;
    font-family: inherit;
    font-size: clamp(2rem, 5vw, 3rem);
    font-weight: 800;
    line-height: 1.1;
    letter-spacing: -0.04em;
  }

  .registration-success__greeting {
    margin: 24px 0 0;
    color: #273f37;
    font-size: 1.05rem;
  }

  .registration-success__lead {
    margin: 28px 0 12px;
    color: #4e5f59;
    font-size: 1rem;
  }

  .registration-success__email {
    max-width: 500px;
    margin: 0 auto;
    padding: 15px 22px;
    overflow-wrap: anywhere;
    border: 1px solid #b9cec2;
    border-radius: 12px;
    color: #143f31;
    background: #f3f7f5;
    font-size: 1.06rem;
    font-weight: 750;
  }

  .registration-success__instruction {
    max-width: 570px;
    margin: 28px auto 0;
    color: #344b43;
    font-size: 1rem;
    line-height: 1.65;
  }

  .registration-success__instruction strong {
    color: #174c39;
    font-weight: 750;
  }

  .registration-success__notice {
    max-width: 590px;
    display: flex;
    align-items: flex-start;
    gap: 17px;
    margin: 34px auto 0;
    padding: 22px 24px;
    text-align: left;
    border-left: 4px solid #d5a72f;
    border-radius: 10px;
    color: #594c2e;
    background: #fff8e5;
  }

  .registration-success__notice strong {
    display: block;
    margin-bottom: 5px;
    color: #493d25;
    font-weight: 750;
  }

  .registration-success__notice p {
    margin: 0;
    color: #6b6047;
    font-size: 0.93rem;
    line-height: 1.55;
  }

  .registration-success__security {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 9px;
    margin-top: 24px;
    color: #68756f;
    font-size: 0.85rem;
  }

  .registration-success__login {
    width: min(100%, 350px);
    margin-top: 34px;
    color: #ffffff !important;
    background: #183f31 !important;
    font-weight: 750;
    letter-spacing: 0;
    text-transform: none;
    box-shadow: 0 12px 24px rgba(24, 55, 41, 0.17);
  }

  .form-heading {
    margin: 0 auto;
    padding: clamp(32px, 5vw, 48px) 24px 14px;
    text-align: center;
  }

  .form-heading__title {
    margin: 0;
    color: #183729;
    font-family: 'Space Grotesk', sans-serif;
    font-size: clamp(2.2rem, 5vw, 3rem);
    font-weight: 800;
    line-height: 1.08;
    letter-spacing: -0.04em;
  }

  .form-heading__main {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 18px;
  }

  .form-heading__icon {
    width: 68px;
    height: 68px;
    display: grid;
    place-items: center;
    margin: 0;
    color: #218354;
    background: #edf6f1;
    border-radius: 19px;
  }

  .form-heading__subtitle {
    max-width: 470px;
    margin: 14px auto 0;
    color: #53675e;
    font-size: 0.98rem;
    line-height: 1.55;
  }

  .password-row {
    margin-top: 24px;
  }

  .personal-data-row {
    margin-top: 24px;
  }

  .create-account-button {
    width: min(100%, 280px);
    min-height: 44px;
    display: flex;
    margin: 28px auto 0;
    background-color: #183729 !important;
    color: #ffffff !important;
    font-weight: 700;
    letter-spacing: 0.2px;
    text-transform: none;
    box-shadow: 0 12px 24px rgba(24, 55, 41, 0.18);
  }

  .create-account-button:focus-visible {
    outline: 3px solid #8ea588;
    outline-offset: 3px;
  }

  .create-account-back {
    display: flex;
    width: max-content;
    margin: 18px auto 0;
    color: #5f6a65;
    font-weight: 500;
    text-transform: none;
    letter-spacing: 0;
  }


  /* ===== PAGE ===== */
  .page {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    background: #ebebeb;
  }

  .user-create-container {
    flex: 1;
    width: 100%;
    display: flex;
    align-items: safe center;
  }

  .user-create-row {
    width: 100%;
    margin-inline: 0;
    align-items: safe center;
  }

  .user-create-column {
    width: 100%;
    max-width: 760px;
    margin-inline: auto;
  }

  @media (max-width: 600px) {
    .registration-success {
      padding: 38px 22px 34px;
    }

    .registration-success__icon {
      width: 92px;
      height: 92px;
    }

    .registration-success__notice {
      padding: 19px 17px;
    }

    .registration-success__security {
      align-items: flex-start;
      text-align: left;
    }
  }

  .access-field {
    color: #183729;
  }

  :deep(.access-field .v-field) {
    box-shadow: none;
    background-color: #ffffff;
  }

  :deep(.access-field .v-label) {
    color: #424844;
    font-size: 0.72rem;
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
  }

  :deep(.access-field .v-field__input) {
    min-height: 56px;
    color: #183729;
    padding-inline-start: 18px;
  }

  /* ===== CARD ===== */
  .shell {
    width: min(100%, 700px);
    margin-inline: auto;
    overflow: hidden;
    border: 1px solid rgba(31, 27, 22, 0.08);
    background: rgba(255, 255, 255, 0.85);
    backdrop-filter: blur(10px);
  }

  /* ===== PANELS ===== */
  .panels {
    --v-theme-surface: transparent;
  }

  .panel {
    border: 1px solid rgba(31, 27, 22, 0.08);
    border-radius: 14px;
    overflow: hidden;
    margin-bottom: 12px;
    background: rgba(255, 255, 255, 0.72);
  }

  .section-title {
    color: #214b3a;
    font-family: 'Space Grotesk', sans-serif;
    font-size: 1.125rem;
    font-weight: 650;
    letter-spacing: 0.15px;
  }

  .section-title__content {
    min-width: 0;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  }

  .section-title__label {
    min-width: 0;
    display: flex;
    align-items: center;
    gap: 13px;
  }

  .section-title__icon {
    width: 42px;
    height: 42px;
    display: grid;
    place-items: center;
    flex: 0 0 auto;
    color: #218354;
    background: #edf6f1;
    border-radius: 13px;
  }

  .panel-completed-icon {
    flex: 0 0 auto;
    color: #218354;
  }

  /* ===== BUTTONS ===== */
  .btn-primary {
    background-color: #214b3a;
    color: #ffffff;
    font-weight: 650;
    letter-spacing: 0.2px;
    text-transform: none;
  }

  .btn-ghost {
    color: #214b3a;
    font-weight: 650;
    text-transform: none;
  }

  /* ===== BIRTH DATE ===== */
  .birth-activator {
    cursor: pointer;
    min-height: 56px;
  }

  :deep(.access-field .v-field__outline) {
    color: rgba(24, 55, 41, 0.42);
  }

  /* ===== PASSWORD DIALOG ===== */
  .password-dialog-title {
    color: #214b3a;
    font-weight: 700;
    display: flex;
    align-items: center;
  }

  .password-description {
    color: rgba(58, 47, 36, 0.88);
    margin-bottom: 14px;
    font-size: 0.95rem;
  }

  .password-rules {
    list-style: none;
    padding: 0;
    margin: 0;
  }

    .password-rules li {
      display: flex;
      align-items: center;
      margin-bottom: 8px;
      font-size: 0.92rem;
      color: rgba(31, 27, 22, 0.86);
    }

  .rule-ok {
    color: #214b3a;
  }

  .rule-pending {
    color: rgba(31, 27, 22, 0.35);
  }
</style>
