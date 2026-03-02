FROM node:24-alpine AS build
WORKDIR /app

# Install dependencies first (layer caching)
COPY package.json package-lock.json* ./
RUN npm ci

# Build with empty API base so frontend uses relative URLs (nginx proxies /api)
COPY . .
ENV VITE_API_BASE=""
RUN npm run build

# Serve with nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
