# Configuración Docker para API Devsu

## Requisitos Previos
- Docker Desktop instalado
- Docker Compose instalado

## Inicio Rápido

1. **Construir y ejecutar los contenedores:**
   ```bash
   docker-compose up -d --build
   ```

2. **Acceder a la aplicación:**
   - API: http://localhost:5050
   - Swagger UI: http://localhost:5050/swagger
   - SQL Server: localhost:1433 (usuario: `sa`, contraseña: `SuperPassw0rd`)

## Comandos Docker

### Iniciar servicios
```bash
docker-compose up -d
```

### Detener servicios
```bash
docker-compose down
```

### Detener y eliminar volúmenes (elimina datos de la base de datos)
```bash
docker-compose down -v
```

### Ver logs
```bash
docker-compose logs -f api
docker-compose logs -f db
```

### Reconstruir después de cambios en el código
```bash
docker-compose up -d --build api
```

### Ejecutar comandos en el contenedor en ejecución
```bash
# Acceder al shell del contenedor API
docker exec -it devsu-api /bin/bash

# Acceder a SQL Server
docker exec -it devsu-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "SuperPassw0rd" -C
```

## Configuración

### Variables de Entorno
Las siguientes variables de entorno se pueden configurar en `docker-compose.yml`:

- `ASPNETCORE_ENVIRONMENT`: Establecer en `Development` o `Production`
- `ConnectionStrings__DefaultConnection`: Cadena de conexión a la base de datos
- `BusinessRules__LimiteDiarioRetiro`: Límite diario de retiro

### Base de Datos
- La base de datos se crea automáticamente y las migraciones se aplican al inicio
- Los datos se persisten en un volumen Docker llamado `sqlserver-data`
- Para reiniciar la base de datos, eliminar el volumen: `docker-compose down -v`

### Puertos
- API: 5050 (HTTP), 5051 (HTTPS en desarrollo)
- SQL Server: 1433

### Nota sobre la Plataforma
- SQL Server se ejecuta en la plataforma linux/amd64. En Macs con Apple Silicon (M1/M2), se ejecutará bajo emulación lo que puede impactar el rendimiento.

## Despliegue en Producción

Para despliegue en producción:

1. Actualizar contraseñas en `docker-compose.yml`
2. Usar variables de entorno o secretos para datos sensibles
3. Configurar certificados HTTPS correctamente
4. Considerar usar un proxy inverso (nginx, traefik)
5. Establecer `ASPNETCORE_ENVIRONMENT=Production`

## Solución de Problemas

### Problemas de conexión a la base de datos
- Asegurar que el contenedor SQL Server esté saludable: `docker-compose ps`
- Revisar logs: `docker-compose logs db`
- Verificar cadena de conexión en las variables de entorno

### La API no inicia
- Revisar logs: `docker-compose logs api`
- Asegurar que las migraciones se puedan aplicar
- Verificar que todas las variables de entorno requeridas estén configuradas

### El contenedor se reinicia constantemente
- Verificar logs para errores: `docker logs devsu-api`
- Si hay problemas con HTTPS, comentar las líneas de certificados en `docker-compose.override.yml`
- Asegurar que los puertos no estén en uso: `lsof -i:5050`