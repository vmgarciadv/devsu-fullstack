# Etapa 1: Construir la aplicación Angular
FROM node:20-alpine AS build
WORKDIR /app

# Copiar archivos de paquetes
COPY package*.json ./

# Instalar dependencias
RUN npm ci

# Copiar el resto de la aplicación
COPY . .

# Construir la aplicación Angular para producción
RUN npm run build

# Etapa 2: Servir la aplicación con nginx
FROM nginx:alpine

# Copiar configuración personalizada de nginx
COPY nginx.conf /etc/nginx/nginx.conf

# Copiar la aplicación Angular construida desde la etapa de build
COPY --from=build /app/dist/devsu-front/browser /usr/share/nginx/html

# Exponer puerto 80
EXPOSE 80

# Iniciar nginx
CMD ["nginx", "-g", "daemon off;"]