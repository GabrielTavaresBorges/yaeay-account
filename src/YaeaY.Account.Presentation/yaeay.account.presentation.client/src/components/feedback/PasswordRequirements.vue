<!-- src/components/feedback/PasswordRequirements.vue -->

<script setup lang="ts">
  import {
    mdiShieldLockOutline,
    mdiCheckCircle,
    mdiCloseCircle,
  } from '@mdi/js'

  type RuleItem = {
    text: string
    valid?: boolean
  }

  withDefaults(
    defineProps<{
      title?: string
      description?: string
      rules: RuleItem[]
    }>(),
    {
      title: 'Requisitos mínimos',
      description: 'Sua senha deve atender aos requisitos abaixo.',
    }
  )
</script>

<template>
  <div class="password-requirements">
    <div class="password-requirements__header">
      <div class="password-requirements__icon">
        <v-icon :icon="mdiShieldLockOutline" size="22" />
      </div>

      <div>
        <div class="password-requirements__title">
          {{ title }}
        </div>

        <div class="password-requirements__description">
          {{ description }}
        </div>
      </div>
    </div>

    <ul class="password-requirements__list">
      <li v-for="rule in rules"
          :key="rule.text"
          class="password-requirements__item">
        <v-icon size="18"
                :icon="rule.valid ? mdiCheckCircle : mdiCloseCircle"
                :class="rule.valid ? 'password-requirements__ok' : 'password-requirements__bad'" />

        <span class="password-requirements__text">
          {{ rule.text }}
        </span>
      </li>
    </ul>
  </div>
</template>

<style scoped>
  .password-requirements {
    margin-top: 6px;
    padding: 14px 16px;
    border-radius: 10px;
    background: rgba(226, 226, 226, 0.55);
    border: 1px solid rgba(31, 27, 22, 0.08);
  }

  .password-requirements__header {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    margin-bottom: 12px;
  }

  .password-requirements__icon {
    width: 34px;
    height: 34px;
    border-radius: 10px;
    display: grid;
    place-items: center;
    background: rgba(33, 75, 58, 0.1);
    color: #214b3a;
    flex: 0 0 auto;
  }

  .password-requirements__title {
    color: #214b3a;
    font-size: 0.9rem;
    font-weight: 700;
    line-height: 1.2;
  }

  .password-requirements__description {
    margin-top: 2px;
    color: #424844;
    font-size: 0.76rem;
    line-height: 1.35;
  }

  .password-requirements__list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px 18px;
  }

  @media (max-width: 600px) {
    .password-requirements__list {
      grid-template-columns: 1fr;
    }
  }

  .password-requirements__item {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .password-requirements__text {
    color: rgba(26, 28, 28, 0.88);
    font-size: 0.82rem;
    line-height: 1.25;
  }

  .password-requirements__ok {
    color: #214b3a;
  }

  .password-requirements__bad {
    color: #ba1a1a;
  }
</style>
