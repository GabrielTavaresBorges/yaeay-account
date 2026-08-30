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
    get: () => model.value.areaCode,
    set: (value) => {
      model.value = {
        ...model.value,
        areaCode: value,
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

  function onPhoneNumberChange(value: string | number | null) {
    const formattedNumber = formatPhoneNumber({
      callingCode: model.value.callingCode,
      country: model.value.country,
      phoneType: model.value.phoneType,
      value: String(value ?? ''),
    })

    model.value.number = formattedNumber
  }

  let syncing = false

  watch(
    () => model.value.country,
    (value) => {
      if (syncing) return

      syncing = true

      model.value = {
        ...model.value,
        callingCode: getCallingCodeByCountry(value),
        areaCode: '',
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
          areaCode: '',
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
      <v-select v-model="callingCode"
                :items="callingCodeItems"
                item-title="title"
                item-value="value"
                label="DDI"
                variant="outlined"
                rounded="lg"
                density="comfortable"
                hide-details
                class="phone-field phone-field--wide" />

      <v-select v-model="country"
                :items="countryItems"
                item-title="title"
                item-value="value"
                label="País"
                variant="outlined"
                rounded="lg"
                density="comfortable"
                hide-details
                class="phone-field phone-field--country">
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

      <v-select v-model="phoneType"
                :items="phoneTypeItems"
                item-title="title"
                item-value="value"
                label="Tipo"
                variant="outlined"
                rounded="lg"
                density="comfortable"
                hide-details
                class="phone-field phone-field--wide" />

      <v-select v-if="isBrazil"
                v-model="areaCode"
                :items="brazilAreaCodes"
                label="DDD"
                placeholder="Selecione"
                variant="outlined"
                rounded="lg"
                density="comfortable"
                hide-details
                class="phone-field" />

      <v-text-field v-else
                    v-model="areaCode"
                    label="Área"
                    placeholder="Área"
                    variant="outlined"
                    rounded="lg"
                    density="comfortable"
                    hide-details
                    class="phone-field" />

      <v-text-field :model-value="model.number"
                    label="Número"
                    :maxlength="numberMaxLength"
                    :placeholder="numberPlaceholder"
                    inputmode="numeric"
                    autocomplete="tel-national"
                    variant="outlined"
                    rounded="lg"
                    density="comfortable"
                    hide-details
                    class="phone-field phone-field--number"
                    @update:model-value="onPhoneNumberChange" />
    </div>
  </div>
</template>

<style scoped>
  .user-phones-field {
    width: 100%;
    padding: 16px 0 4px;
    container-type: inline-size;
  }

  .phone-grid {
    display: grid;
    grid-template-columns:
      minmax(105px, 1.15fr)
      minmax(96px, 0.9fr)
      minmax(115px, 1.25fr)
      minmax(78px, 0.85fr)
      minmax(150px, 1.8fr);
    gap: 12px;
  }

  .phone-field {
    min-width: 0;
    color: #183729;
  }

  .phone-field--number {
    min-width: 0;
  }

  :deep(.phone-field .v-field) {
    min-height: 56px;
    border-radius: 12px;
    background-color: #ffffff;
    color: #183729;
  }

  :deep(.phone-field .v-field__input) {
    min-height: 56px;
    color: #183729;
  }

  :deep(.phone-field .v-field__outline) {
    color: rgba(24, 55, 41, 0.42);
  }

  :deep(.phone-field .v-label) {
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

  @container (max-width: 720px) {
    .phone-grid {
      grid-template-columns: repeat(4, minmax(0, 1fr));
    }

    .phone-field--number {
      grid-column: 1 / -1;
    }
  }

  @container (max-width: 520px) {
    .phone-grid {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .phone-field--number {
      grid-column: 1 / -1;
    }
  }

  @container (max-width: 300px) {
    .phone-grid {
      grid-template-columns: minmax(0, 1fr);
    }

    .phone-field--number {
      grid-column: auto;
    }
  }
</style>
