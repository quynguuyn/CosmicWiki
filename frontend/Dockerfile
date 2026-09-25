FROM node:20-alpine AS build

WORKDIR /app
ARG STRIPE_PUBLISHABLE_KEY=""

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration=production
RUN find dist/frontend -type f -name '*.js' -exec sed -i "s|__STRIPE_PUBLISHABLE_KEY__|${STRIPE_PUBLISHABLE_KEY}|g" {} +

FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
