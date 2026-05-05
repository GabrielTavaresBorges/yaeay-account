                        <!-- src/components/layout/AppFooter.vue -->

<script setup lang="ts">
  import { computed } from 'vue'
  import type { RouteLocationRaw } from 'vue-router'

  type FooterLink = {
    text?: string
    to?: RouteLocationRaw
    href?: string
  }

  const props = defineProps<{
    copyright?: string

    textOne?: string
    toOne?: RouteLocationRaw
    hrefOne?: string

    textTwo?: string
    toTwo?: RouteLocationRaw
    hrefTwo?: string

    textThree?: string
    toThree?: RouteLocationRaw
    hrefThree?: string

    textFour?: string
    toFour?: RouteLocationRaw
    hrefFour?: string
  }>()

  const links = computed<FooterLink[]>(() => {
    const items: FooterLink[] = [
      { text: props.textOne, to: props.toOne, href: props.hrefOne },
      { text: props.textTwo, to: props.toTwo, href: props.hrefTwo },
      { text: props.textThree, to: props.toThree, href: props.hrefThree },
      { text: props.textFour, to: props.toFour, href: props.hrefFour },
    ]

    return items.filter(link => Boolean(link.text?.trim()))
  })
</script>

<template>
  <v-footer class="app-footer">
    <div class="app-footer__content">
      <p v-if="props.copyright"
         class="app-footer__copy">
        {{ props.copyright }}
      </p>

      <nav v-if="links.length"
           class="app-footer__links">
        <v-btn v-for="link in links"
               :key="link.text"
               variant="text"
               class="app-footer__link"
               :to="link.to"
               :href="link.href"
               :ripple="false">
          {{ link.text }}
        </v-btn>
      </nav>
    </div>
  </v-footer>
</template>

  <style scoped>
    .app-footer {
      flex: 0 0 auto;
      margin-top: auto;
      padding: 0;
      min-height: auto;
      background: #f9f9f9;
      border-top: 1px solid rgba(24, 55, 41, 0.08);
    }

    .app-footer__content {
      width: 100%;
      padding: 28px 32px;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .app-footer__copy {
      margin: 0;
      color: #3e564f;
      font-size: 0.78rem;
      font-weight: 600;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      flex-shrink: 0;
    }

    .app-footer__links {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 12px;
      margin-left: auto;
      flex-shrink: 0;
    }

    .app-footer__link {
      color: #3e564f;
      font-size: 0.78rem;
      font-weight: 600;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      min-width: auto;
      padding-inline: 8px;
    }

      .app-footer__link:hover {
        color: #183729;
      }

    @media (max-width: 960px), (orientation: portrait) {
      .app-footer__content {
        flex-direction: column;
        align-items: center;
        gap: 16px;
      }

      .app-footer__links {
        margin-left: 0;
        justify-content: center;
        flex-wrap: wrap;
        gap: 8px;
      }
    }

    @media (max-width: 600px), (orientation: portrait) {
      .app-footer__content {
        padding-left: 16px;
        padding-right: 16px;
      }
    }
  </style>
