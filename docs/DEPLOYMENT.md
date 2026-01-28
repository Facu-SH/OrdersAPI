# Guía de Deployment

Instrucciones para desplegar Order Integration API en servicios cloud gratuitos.

## Opciones de Deployment

| Plataforma | PostgreSQL | Docker | Free Tier |
|------------|------------|--------|-----------|
| **Railway** | ✅ Incluido | ✅ Sí | ✅ $5/mes créditos |
| **Render** | ✅ Incluido | ✅ Sí | ✅ Limitado |
| **Fly.io** | ⚠️ Separado | ✅ Sí | ✅ Generoso |

---

## Railway (Recomendado)

Railway ofrece la experiencia más simple con PostgreSQL incluido.

### Paso 1: Preparar el Repositorio

```bash
# Asegurarse de que el código está en GitHub
git push origin main
```

### Paso 2: Crear Proyecto en Railway

1. Ir a [railway.app](https://railway.app)
2. Click en **"Start a New Project"**
3. Seleccionar **"Deploy from GitHub repo"**
4. Autorizar acceso al repositorio

### Paso 3: Agregar PostgreSQL

1. En el proyecto, click en **"+ New"**
2. Seleccionar **"Database" > "Add PostgreSQL"**
3. Railway creará automáticamente la base de datos

### Paso 4: Configurar Variables de Entorno

En el servicio de la API, agregar las siguientes variables:

```env
# Connection String (usar la variable de Railway)
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}

# O formato manual:
ConnectionStrings__DefaultConnection=Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}

# API Settings
ApiSettings__ApiKey=tu-api-key-segura-aqui
ApiSettings__RunMigrations=true

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:${{PORT}}
```

### Paso 5: Configurar el Build

Railway detectará automáticamente el Dockerfile. Si no:

1. Click en el servicio de la API
2. Ir a **Settings > Build**
3. Seleccionar **Dockerfile**
4. Asegurar que el path es: `Dockerfile`

### Paso 6: Deploy

1. Railway iniciará el build automáticamente
2. Esperar a que el deployment termine (2-5 minutos)
3. Click en **"Generate Domain"** para obtener la URL pública

### Verificar Deployment

```bash
# Health check
curl https://tu-app.railway.app/health

# Crear un pedido
curl -X POST https://tu-app.railway.app/api/orders \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: tu-api-key-segura-aqui" \
  -d '{"orderNumber":"TEST-001","customerCode":"CUST-001","items":[{"sku":"SKU-001","quantity":1,"unitPrice":10}]}'
```

### Railway: 502 Bad Gateway y base de datos sin tablas

**502 Bad Gateway**  
Railway asigna un puerto dinámico (`PORT`). La API ya está preparada para usarlo: en el arranque lee `PORT` y escucha en ese puerto. Si ya desplegaste antes del cambio, haz un **nuevo deploy** (push al repo o “Redeploy” en Railway) para que tome el código actualizado.

**Base de datos sin tablas**  
Las tablas se crean con las **migraciones de EF Core** al arrancar. Para que se ejecuten necesitas:

1. **Connection string correcta**  
   En el servicio de la **API** (no en Postgres), en Variables:
   - Nombre: `ConnectionStrings__DefaultConnection`
   - Valor: la URL de tu base PostgreSQL.

   Cómo obtenerla:
   - Entra al servicio **PostgreSQL** en tu proyecto.
   - Pestaña **Variables** o **Connect**.
   - Copia **DATABASE_URL** o **Postgres Connection URL** (empieza con `postgresql://`).

   Pega ese valor en `ConnectionStrings__DefaultConnection` del servicio de la API.

2. **Migraciones habilitadas**  
   En el servicio de la **API**, agrega:
   - Nombre: `ApiSettings__RunMigrations`
   - Valor: `true`

3. **Reiniciar / redesplegar**  
   Guarda las variables y haz **Redeploy** del servicio API. En el primer arranque se ejecutarán las migraciones y se crearán las tablas.

**Resumen de variables en el servicio API (Railway)**

| Variable | Valor | Obligatorio |
|----------|--------|-------------|
| `ConnectionStrings__DefaultConnection` | URL de Postgres (del servicio Postgres) | ✅ |
| `ApiSettings__RunMigrations` | `true` | ✅ (para crear tablas) |
| `ApiSettings__ApiKey` | Tu API Key (ej. `tu-palabra-secreta`) | ✅ |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Opcional |

No hace falta definir `PORT` ni `ASPNETCORE_URLS`; Railway inyecta `PORT` y la API lo usa automáticamente.

---

## Render

Render es otra excelente opción con PostgreSQL gratuito.

### Paso 1: Crear Base de Datos

1. Ir a [render.com](https://render.com)
2. Click en **"New +" > "PostgreSQL"**
3. Configurar:
   - **Name**: `orderintegration-db`
   - **Region**: Elegir la más cercana
   - **Plan**: Free
4. Click en **"Create Database"**
5. Copiar el **Internal Database URL**

### Paso 2: Crear Web Service

1. Click en **"New +" > "Web Service"**
2. Conectar repositorio de GitHub
3. Configurar:
   - **Name**: `orderintegration-api`
   - **Region**: Misma que la base de datos
   - **Runtime**: Docker
   - **Plan**: Free

### Paso 3: Configurar Variables de Entorno

En **Environment**, agregar:

```env
ConnectionStrings__DefaultConnection=<Internal Database URL de paso 1>
ApiSettings__ApiKey=tu-api-key-segura-aqui
ApiSettings__RunMigrations=true
ASPNETCORE_ENVIRONMENT=Production
```

### Paso 4: Configurar Dockerfile

En **Settings**:
- **Dockerfile Path**: `./Dockerfile`
- **Docker Build Context**: `.`

### Paso 5: Deploy

1. Click en **"Create Web Service"**
2. Render iniciará el build
3. Una vez completado, la URL estará disponible en el dashboard

### Notas de Render

- El plan gratuito "duerme" después de 15 minutos de inactividad
- El primer request después de dormir tarda ~30 segundos
- Para evitar esto, configurar un health check externo (UptimeRobot)

---

## Fly.io

Fly.io ofrece más control y mejor rendimiento en el tier gratuito.

### Paso 1: Instalar CLI

```bash
# Windows (PowerShell)
powershell -Command "iwr https://fly.io/install.ps1 -useb | iex"

# macOS/Linux
curl -L https://fly.io/install.sh | sh
```

### Paso 2: Login y Setup

```bash
fly auth login
fly launch
```

Cuando pregunte, configurar:
- **App name**: `orderintegration-api`
- **Region**: Elegir la más cercana
- **PostgreSQL**: Yes (Fly Postgres)

### Paso 3: Configurar Secrets

```bash
# API Key
fly secrets set ApiSettings__ApiKey=tu-api-key-segura-aqui

# La connection string se configura automáticamente como DATABASE_URL
```

### Paso 4: Actualizar fly.toml

```toml
app = "orderintegration-api"
primary_region = "iad"

[env]
  ASPNETCORE_ENVIRONMENT = "Production"
  ApiSettings__RunMigrations = "true"

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = true
  auto_start_machines = true

[[services]]
  protocol = "tcp"
  internal_port = 8080

  [[services.ports]]
    port = 80
    handlers = ["http"]

  [[services.ports]]
    port = 443
    handlers = ["tls", "http"]

  [[services.http_checks]]
    interval = "10s"
    timeout = "2s"
    path = "/health"
```

### Paso 5: Deploy

```bash
fly deploy
```

### Paso 6: Verificar

```bash
fly status
fly logs
```

---

## Configuración de Dominio Personalizado

### Railway

1. Ir a **Settings > Domains**
2. Click en **"+ Custom Domain"**
3. Agregar tu dominio
4. Configurar DNS con los valores proporcionados

### Render

1. Ir a **Settings > Custom Domains**
2. Click en **"Add Custom Domain"**
3. Seguir instrucciones de DNS

### Fly.io

```bash
fly certs create tu-dominio.com
```

---

## Monitoreo y Logs

### Railway

```bash
# Ver logs en tiempo real
railway logs
```

O desde el dashboard web.

### Render

Los logs están disponibles en el dashboard del servicio.

### Fly.io

```bash
# Logs en tiempo real
fly logs

# Logs históricos
fly logs --app orderintegration-api
```

---

## Troubleshooting

### La aplicación no inicia

1. Verificar logs para errores
2. Asegurar que `ConnectionStrings__DefaultConnection` está correctamente configurada
3. Verificar que el puerto es correcto (usar `$PORT` o variable de entorno)

### Error de conexión a base de datos

1. Verificar que la base de datos está corriendo
2. Usar **Internal URL** (no External) para conexión
3. Verificar credenciales

### Migraciones no se ejecutan

1. Asegurar `ApiSettings__RunMigrations=true`
2. Verificar logs para errores de migración
3. Conectar manualmente y ejecutar: `dotnet ef database update`

### Health check falla

1. Verificar que la ruta `/health` responde localmente
2. Asegurar que el puerto interno coincide con la configuración
3. Revisar si la base de datos está accesible

---

## Costos Estimados

| Plataforma | Free Tier | Uso Ligero | Producción |
|------------|-----------|------------|------------|
| Railway | $5/mes créditos | ~$5-10/mes | ~$20+/mes |
| Render | Limitado | ~$7/mes | ~$25+/mes |
| Fly.io | Generoso | ~$5/mes | ~$15+/mes |

---

## Checklist de Deployment

- [ ] Código en repositorio Git (GitHub/GitLab)
- [ ] Dockerfile funciona localmente
- [ ] Variables de entorno configuradas
- [ ] Base de datos PostgreSQL creada
- [ ] Connection string configurada
- [ ] API Key segura generada
- [ ] Health check responde correctamente
- [ ] Swagger accesible en la URL pública
- [ ] Crear primer pedido de prueba
- [ ] Configurar monitoring/alertas (opcional)
