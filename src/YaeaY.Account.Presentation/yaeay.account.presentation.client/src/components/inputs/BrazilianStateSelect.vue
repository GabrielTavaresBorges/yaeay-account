<script setup lang="ts">
const model = defineModel<string>({ default: '' })

withDefaults(defineProps<{
  label?: string
  required?: boolean
}>(), {
  label: 'UF de expedição',
  required: true,
})

const states = [
  ['AC', 'Acre'], ['AL', 'Alagoas'], ['AP', 'Amapá'], ['AM', 'Amazonas'], ['BA', 'Bahia'], ['CE', 'Ceará'], ['DF', 'Distrito Federal'], ['ES', 'Espírito Santo'], ['GO', 'Goiás'], ['MA', 'Maranhão'], ['MT', 'Mato Grosso'], ['MS', 'Mato Grosso do Sul'], ['MG', 'Minas Gerais'], ['PA', 'Pará'], ['PB', 'Paraíba'], ['PR', 'Paraná'], ['PE', 'Pernambuco'], ['PI', 'Piauí'], ['RJ', 'Rio de Janeiro'], ['RN', 'Rio Grande do Norte'], ['RS', 'Rio Grande do Sul'], ['RO', 'Rondônia'], ['RR', 'Roraima'], ['SC', 'Santa Catarina'], ['SP', 'São Paulo'], ['SE', 'Sergipe'], ['TO', 'Tocantins'],
].map(([value, title]) => ({ value, title: `${value} — ${title}` }))
</script>

<template>
  <v-select
    v-model="model"
    :items="states"
    item-title="title"
    item-value="value"
    :label="label"
    :rules="required ? [(value: string) => !!value || `Informe ${label.toLocaleLowerCase('pt-BR')}.`] : []"
  />
</template>
