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
docker exec -it devsu-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'SuperPassw0rd' -C

# Ver logs del API
docker logs devsu-api

# Acceder al contenedor del frontend
docker exec -it devsu-frontend sh
```

## Inicializar la base de datos con datos de ejemplo

Si deseas cargar la base de datos con el esquema completo y datos de ejemplo, puedes ejecutar el script `BaseDatos.sql`:

### Opción 1: Usando sqlcmd desde el host
```bash
# Copiar el archivo SQL al contenedor
docker cp devsu/BaseDatos.sql devsu-sqlserver:/tmp/

# Ejecutar el script
docker exec -it devsu-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'SuperPassw0rd' -C -i /tmp/BaseDatos.sql
```

### Opción 2: Conectarse interactivamente y ejecutar comandos
```bash
# Conectarse a SQL Server
docker exec -it devsu-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'SuperPassw0rd' -C

# Una vez conectado, ejecutar:
1> USE master;
2> GO
1> :r /tmp/BaseDatos.sql
2> GO
```

### Opción 3: Usando un cliente SQL
Puedes conectarte con cualquier cliente SQL Server (Azure Data Studio, SSMS, DBeaver, etc.) usando:
- **Servidor**: localhost,1433
- **Usuario**: sa
- **Contraseña**: SuperPassw0rd
- **Base de datos**: devsu

Luego ejecutar el contenido del archivo `devsu/BaseDatos.sql`.

**Nota**: El script `BaseDatos.sql` incluye:
- Creación de la base de datos (si no existe)
- Esquema completo de tablas
- Índices para optimización
- Procedimiento almacenado `sp_GenerarReporteEstadoCuenta`
- Datos de ejemplo (25 clientes con cuentas y movimientos)
- Cliente de prueba "Victor" con movimientos en julio 2025

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