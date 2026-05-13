<!-- src/components/inputs/UserPhonesField.vue -->

<script setup lang="ts">
  import { computed, watch } from 'vue'

  import { callingCodeItems, type CallingCode } from '@/constants/callingCode'
  import { countryItems, type CountryCode } from '@/constants/country'
  import { phoneTypeItems } from '@/constants/phoneType'
  import { brazilAreaCodes } from '@/constants/areaCode'

  import type { PhoneModel } from '@/models/phone-model'

  import {
    getCallingCodeByCountry,
    resolveCountryFromCallingCode,
  } from '@/services/phoneCountry/phone-country-service'

  import {
    formatPhoneNumber,
    getPhoneNumberMaxLength,
  } from '@/services/phoneFormat/phone-format-service'

  const model = defineModel<PhoneModel>({
    default: {
      callingCode: '+55' as CallingCode,
      country: 'BR' as CountryCode,
      phoneType: 'Mobile',
      areaCode: '11',
      number: '',
    },
  })

  const callingCode = computed<PhoneModel['callingCode']>({
    get: () => model.value.callingCode,
    set: (value) => {
      model.value = {
        ...model.value,
        callingCode: value,
      }
    },
  })

  const country = computed<PhoneModel['country']>({
    get: () => model.value.country,
    set: (value) => {
      model.value = {
        ...model.value,
        country: value,
      }
    },
  })

  const phoneType = computed<PhoneModel['phoneType']>({
    get: () => model.value.phoneType,
    set: (value) => {
      model.value = {
        ...model.value,
        phoneType: value,
      }
    },
  })

  const areaCode = computed<PhoneModel['areaCode']>({
    get: () => {
      if (model.value.country !== 'BR') return ''

      return model.value.areaCode
    },
    set: (value) => {
      model.value = {
        ...model.value,
        areaCode: model.value.country === 'BR' ? value : '',
      }
    },
  })

  const number = computed<PhoneModel['number']>({
    get: () => model.value.number,
    set: (value) => {
      model.value = {
        ...model.value,
        number: formatPhoneNumber({
          callingCode: model.value.callingCode,
          country: model.value.country,
          phoneType: model.value.phoneType,
          value,
        }),
      }
    },
  })

  const isBrazil = computed(() => model.value.country === 'BR')

  const selectedCountryItem = computed(() =>
    countryItems.find((item) => item.value === model.value.country),
  )

  const numberPlaceholder = computed(() => {
    if (model.value.callingCode === '+55' && model.value.country === 'BR') {
      return model.value.phoneType === 'Landline'
        ? '0000-0000'
        : '00000-0000'
    }

    return ''
  })

  const numberMaxLength = computed(() =>
    getPhoneNumberMaxLength(
      model.value.callingCode,
      model.value.country,
      model.value.phoneType,
    ),
  )

  let syncing = false

  watch(
    () => model.value.country,
    (value) => {
      if (syncing) return

      syncing = true

      model.value = {
        ...model.value,
        callingCode: getCallingCodeByCountry(value),
        areaCode: value === 'BR' ? model.value.areaCode : '',
      }

      syncing = false
    },
  )

  watch(
    () => model.value.callingCode,
    (value) => {
      if (syncing) return

      syncing = true

      const resolvedCountry = resolveCountryFromCallingCode(value, model.value.country)

      if (resolvedCountry) {
        model.value = {
          ...model.value,
          country: resolvedCountry,
          areaCode: resolvedCountry === 'BR' ? model.value.areaCode : '',
        }
      }

      syncing = false
    },
  )

  watch(
    () => model.value.phoneType,
    () => {
      model.value = {
        ...model.value,
        number: '',
      }
    },
  )
</script>

<template>
  <div class="user-phones-field">
    <div class="phone-grid">
      <!-- DDI -->
      <label class="phone-field">
        <span>DDI</span>

        <div class="phone-select-wrapper">
          <select v-model="callingCode">
            <option v-for="item in callingCodeItems"
                    :key="item.value"
                    :value="item.value">
              {{ item.title }}
            </option>
          </select>
        </div>
      </label>

      <!-- País -->
      <!-- País -->
      <div class="phone-field phone-field--country">
        <span>País</span>

        <v-select v-model="country"
                  :items="countryItems"
                  item-title="title"
                  item-value="value"
                  variant="outlined"
                  rounded="lg"
                  density="comfortable"
                  hide-details
                  class="phone-country-select">
          <template #selection>
            <div class="country-selection country-selection--flag-only">
              <v-img v-if="selectedCountryItem"
                     :src="selectedCountryItem.flagSrc"
                     :alt="selectedCountryItem.alt"
                     width="24"
                     height="16"
                     cover />
            </div>
          </template>
        </v-select>
      </div>


      <!-- Tipo -->
      <label class="phone-field">
        <span>Tipo</span>

        <div class="phone-select-wrapper">
          <select v-model="phoneType">
            <option v-for="item in phoneTypeItems"
                    :key="item.value"
                    :value="item.value">
              {{ item.title }}
            </option>
          </select>
        </div>
      </label>

      <!-- DDD / Área -->
      <label class="phone-field">
        <span>{{ isBrazil ? 'DDD' : 'Área' }}</span>

        <div v-if="isBrazil" class="phone-select-wrapper">
          <select v-model="areaCode">
            <option value="">
              Selecione
            </option>

            <option v-for="item in brazilAreaCodes"
                    :key="item"
                    :value="item">
              {{ item }}
            </option>
          </select>
        </div>

        <input v-else
               v-model="areaCode"
               type="text"
               placeholder="Área" />
      </label>

      <!-- Número -->
      <label class="phone-field phone-field--number">
        <span>Número</span>

        <input v-model="number"
               class="phone-number-input"
               type="text"
               :maxlength="numberMaxLength"
               :placeholder="numberPlaceholder" />
      </label>
    </div>
  </div>
</template>

<style scoped>
  .user-phones-field {
    width: 100%;
    padding: 16px 0 4px;
  }

  .phone-grid {
    display: grid;
    grid-template-columns: repeat(12, 1fr);
    gap: 16px;
  }

  .phone-field {
    grid-column: span 2;
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .phone-field--number {
    grid-column: span 4;
  }

  .phone-field > span {
    color: #424844;
    font-size: 0.72rem;
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
  }

  .phone-field input {
    width: 100%;
    min-height: 56px;
    padding: 0 16px;
    border: 1px solid rgba(24, 55, 41, 0.42);
    border-radius: 12px;
    background-color: #ffffff;
    color: #183729;
    outline: none;
    font-size: 1rem;
  }

  .phone-select-wrapper {
    width: 100%;
    min-height: 56px;
    border: 1px solid rgba(24, 55, 41, 0.42);
    border-radius: 12px;
    background-color: #ffffff;
    overflow: hidden;
  }

    .phone-select-wrapper select {
      width: 100%;
      min-height: 56px;
      padding: 0 40px 0 16px;
      border: none;
      background-color: #ffffff;
      color: #183729;
      outline: none;
      font-size: 1rem;
      cursor: pointer;
      appearance: none;
      -webkit-appearance: none;
      -moz-appearance: none;
    }

      .phone-select-wrapper select option {
        background-color: #ffffff;
        color: #183729;
      }

  .phone-field input:focus {
    border-color: #183729;
  }

  .phone-select-wrapper:focus-within {
    border-color: #183729;
  }

  .phone-country-select {
    width: 100%;
  }

  :deep(.phone-country-select .v-field) {
    min-height: 56px;
    background-color: #ffffff;
    color: #183729;
    border-radius: 12px;
  }

  :deep(.phone-country-select .v-field__input) {
    min-height: 56px;
    color: #183729;
  }

  :deep(.phone-country-select .v-field__outline) {
    color: rgba(24, 55, 41, 0.42);
  }

  :deep(.phone-country-select .v-label) {
    color: #424844;
    font-size: 0.72rem;
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
  }

  .country-selection {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    color: #183729;
    width: 100%;
    white-space: nowrap;
  }

    .country-selection span {
      color: #183729;
      font-size: 1rem;
      font-weight: 400;
      letter-spacing: normal;
      text-transform: none;
    }

  .phone-number-input {
    background-color: #ffffff !important;
  }

  @media (max-width: 960px) {
    .phone-field,
    .phone-field--number {
      grid-column: span 6;
    }
  }

  @media (max-width: 600px) {
    .phone-field,
    .phone-field--number {
      grid-column: span 12;
    }
  }
</style>
