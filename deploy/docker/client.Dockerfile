FROM node:20 AS build

WORKDIR /app

COPY src/YaeaY.Account.Presentation/yaeay.account.presentation.client/package*.json ./
RUN npm ci

COPY src/YaeaY.Account.Presentation/yaeay.account.presentation.client/ .

# The development config initializes HTTPS certificates for Vite on Windows.
# The production image only needs a static build and must not depend on dev-certs.
RUN printf "%s\\n" \
  "import { fileURLToPath, URL } from 'node:url'" \
  "import { defineConfig } from 'vite'" \
  "import vue from '@vitejs/plugin-vue'" \
  "import vuetify from 'vite-plugin-vuetify'" \
  "export default defineConfig({ plugins: [vue(), vuetify({ autoImport: true })], resolve: { alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) } } })" \
  > vite.config.container.ts \
  && npx vite build --config vite.config.container.ts

FROM nginx:alpine

RUN rm /etc/nginx/conf.d/default.conf
COPY deploy/docker/client.nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --quiet --spider http://127.0.0.1/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
