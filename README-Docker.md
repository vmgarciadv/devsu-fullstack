# Configuración Docker para Devsu Fullstack

Este proyecto incluye una configuración completa de Docker para ejecutar todos los servicios necesarios:
- **Base de datos**: SQL Server 2022
- **Backend**: API ASP.NET Core 8.0
- **Frontend**: Aplicación Angular

## Requisitos previos

- Docker Desktop instalado
- Docker Compose instalado
- Puertos disponibles: 1433 (DB), 5050 (API), 4200 (Frontend)

## Estructura del proyecto

```
devsu-fullstack/
├── devsu/                 # Backend ASP.NET Core
│   ├── Dockerfile
│   └── ...
├── devsu-front/          # Frontend Angular
│   ├── Dockerfile
│   ├── nginx.conf
│   └── ...
└── docker-compose.yml    # Configuración de todos los servicios
```

## Iniciar los servicios

### 1. Construir e iniciar todos los servicios

```bash
# En el directorio raíz del proyecto
docker-compose up --build -d
```

### 2. Verificar el estado de los servicios

```bash
docker-compose ps
```

### 3. Ver los logs

```bash
# Todos los servicios
docker-compose logs -f

# Servicio específico
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f db
```

## Acceder a los servicios

- **Frontend Angular**: http://localhost:4200
- **API Backend**: http://localhost:5050
- **SQL Server**: localhost:1433

## Comandos útiles

### Detener todos los servicios
```bash
docker-compose down
```

### Detener y eliminar volúmenes (incluida la base de datos)
```bash
docker-compose down -v
```

### Reconstruir un servicio específico
```bash
docker-compose build api
docker-compose up -d api
```

### Ejecutar comandos en un contenedor
```bash
# Acceder a la base de datos
docker exec -it devsu-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U user -P password -C

# Ver logs del API
docker logs devsu-api

# Acceder al contenedor del frontend
docker exec -it devsu-frontend sh
```

## Configuración de red

Todos los servicios están conectados a través de la red `devsu-network`, lo que permite:
- El frontend puede comunicarse con el API usando `http://api:80`
- El API puede comunicarse con la base de datos usando `Server=db`
- Los servicios están aislados del host excepto por los puertos expuestos

## Solución de problemas

### El frontend no puede conectarse al API
- Verificar que el API esté ejecutándose: `docker-compose ps`
- Revisar los logs del frontend: `docker-compose logs frontend`
- Asegurarse de que nginx esté configurado correctamente

### La base de datos no inicia
- Verificar los requisitos de memoria para SQL Server
- Revisar los logs: `docker-compose logs db`
- Asegurarse de que el puerto 1433 no esté en uso

### Cambios en el código no se reflejan
- Reconstruir la imagen: `docker-compose build [servicio]`
- Reiniciar el servicio: `docker-compose up -d [servicio]`