# Order Integration API

API REST en .NET 8 para gestión de pedidos e integración WMS-ERP.

## 📋 Descripción

Este proyecto simula un escenario real de integración entre un **Sistema de Gestión de Almacén (WMS)** y un **Sistema de Planificación de Recursos Empresariales (ERP)**. Implementa:

- **Gestión de Pedidos**: CRUD completo con workflow de estados
- **Integración ERP Simulada**: Envío de pedidos y webhooks de confirmación
- **Auditoría**: Registro completo de eventos y cambios
- **Buenas Prácticas**: Validación, errores consistentes (ProblemDetails), logs estructurados

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                      API REST (.NET 8)                       │
├─────────────────────────────────────────────────────────────┤
│  Controllers    │  Contracts (DTOs)  │  Middleware          │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                         │
│  Services: OrderService, IntegrationService, AuditService   │
├─────────────────────────────────────────────────────────────┤
│                    Domain Layer                              │
│  Entities: Order, OrderItem, AuditEvent, IntegrationAttempt │
│  Enums: OrderStatus, EventType, IntegrationStatus           │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│  Persistence: EF Core + PostgreSQL                          │
│  Integrations: ERP Simulator                                │
└─────────────────────────────────────────────────────────────┘
```

## 🔄 Workflow de Estados

```
┌─────────┐    ┌──────────┐    ┌────────────┐    ┌───────────┐
│ Created │───▶│ Prepared │───▶│ Dispatched │───▶│ Delivered │
└────┬────┘    └────┬─────┘    └─────┬──────┘    └───────────┘
     │              │                │                 
     ▼              ▼                ▼                 
┌─────────────────────────────────────┐
│            Cancelled                │
└─────────────────────────────────────┘
```

**Reglas de transición:**
- `Created` → `Prepared` | `Cancelled`
- `Prepared` → `Dispatched` | `Cancelled`
- `Dispatched` → `Delivered` | `Cancelled`
- `Delivered` → *(estado final)*
- `Cancelled` → *(estado final)*

## 🛠️ Tech Stack

- **.NET 8** - Framework
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core 8** - ORM
- **PostgreSQL 16** - Base de datos
- **Serilog** - Logging estructurado
- **Swagger/OpenAPI** - Documentación
- **Docker** - Contenedores
- **xUnit** - Testing

## 🚀 Inicio Rápido

### Opción 1: Docker Compose (Recomendado)

```bash
# Clonar el repositorio
git clone https://github.com/tu-usuario/OrdersAPI.git
cd OrdersAPI

# Levantar con Docker Compose
cd deploy
docker-compose up -d

# Ver logs
docker-compose logs -f api
```

**Acceso:**
- Swagger UI: http://localhost:8080
- Health Check: http://localhost:8080/health
- API Key: `dev-api-key-12345`

### Opción 2: Desarrollo Local

**Requisitos:**
- .NET 8 SDK
- PostgreSQL (o Docker para PostgreSQL)

```bash
# Levantar PostgreSQL con Docker
docker run -d --name postgres-orders \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=orderintegration \
  -p 5432:5432 \
  postgres:16-alpine

# Restaurar dependencias
dotnet restore

# Ejecutar migraciones
cd src/OrderIntegration.Api
dotnet ef database update

# Ejecutar la API
dotnet run
```

La API estará disponible en: https://localhost:5001 o http://localhost:5000

## 📡 Endpoints

### Autenticación

Todas las peticiones a `/api/*` requieren el header:

```
X-API-KEY: dev-api-key-12345
```

### Orders

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/orders` | Crear pedido |
| `GET` | `/api/orders` | Listar pedidos (con filtros y paginación) |
| `GET` | `/api/orders/{id}` | Obtener pedido por ID |
| `POST` | `/api/orders/{id}/status` | Cambiar estado del pedido |
| `POST` | `/api/orders/{id}/send-to-erp` | Enviar pedido al ERP (simulado) |

### Webhooks

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/webhooks/erp/order-ack` | Recibir ACK del ERP |

### Auditoría

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/audit` | Consultar eventos de auditoría |
| `GET` | `/api/audit/recent` | Últimos eventos |

### Health

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/health` | Health check básico |
| `GET` | `/health/detailed` | Health check detallado |

## 📝 Ejemplos de Uso

### Crear un Pedido

```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: dev-api-key-12345" \
  -d '{
    "orderNumber": "ORD-2026-0100",
    "customerCode": "CUST-001",
    "items": [
      {
        "sku": "SKU-LAPTOP-001",
        "description": "Laptop HP 15.6",
        "quantity": 2,
        "unitPrice": 899.99
      },
      {
        "sku": "SKU-MOUSE-001",
        "description": "Mouse inalámbrico",
        "quantity": 2,
        "unitPrice": 29.99
      }
    ]
  }'
```

**Respuesta (201 Created):**

```json
{
  "id": 1,
  "orderNumber": "ORD-2026-0100",
  "customerCode": "CUST-001",
  "status": "Created",
  "createdAt": "2026-01-28T10:00:00Z",
  "updatedAt": "2026-01-28T10:00:00Z",
  "totalAmount": 1859.96,
  "items": [...],
  "allowedTransitions": ["Prepared", "Cancelled"]
}
```

### Cambiar Estado

```bash
curl -X POST http://localhost:8080/api/orders/1/status \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: dev-api-key-12345" \
  -d '{"newStatus": "Prepared"}'
```

### Listar Pedidos con Filtros

```bash
# Filtrar por estado
curl "http://localhost:8080/api/orders?status=Created&page=1&pageSize=10" \
  -H "X-API-KEY: dev-api-key-12345"

# Filtrar por cliente y fechas
curl "http://localhost:8080/api/orders?customerCode=CUST-001&fromDate=2026-01-01" \
  -H "X-API-KEY: dev-api-key-12345"
```

### Enviar a ERP

```bash
curl -X POST http://localhost:8080/api/orders/1/send-to-erp \
  -H "X-API-KEY: dev-api-key-12345"
```

**Respuesta:**

```json
{
  "success": true,
  "message": "Pedido enviado al ERP correctamente",
  "attemptId": 1,
  "correlationId": "abc123def456"
}
```

### Webhook ERP (Simular ACK)

```bash
curl -X POST http://localhost:8080/api/webhooks/erp/order-ack \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: dev-api-key-12345" \
  -d '{
    "orderNumber": "ORD-2026-0100",
    "success": true,
    "message": "Pedido procesado correctamente",
    "correlationId": "abc123def456"
  }'
```

## ⚙️ Configuración

### Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Connection string PostgreSQL | - |
| `ApiSettings__ApiKey` | API Key para autenticación | - |
| `ApiSettings__RunMigrations` | Ejecutar migraciones al iniciar | `true` |
| `ASPNETCORE_ENVIRONMENT` | Entorno (Development/Production) | `Production` |

### Simulación ERP

El simulador ERP es configurable:

```json
{
  "ErpIntegration": {
    "SimulationMode": "Random",        // Random, AlwaysSucceed, AlwaysFail
    "SimulatedFailureRate": 0.1,       // 10% de fallos en modo Random
    "MinLatencyMs": 100,               // Latencia mínima simulada
    "MaxLatencyMs": 500,               // Latencia máxima simulada
    "ForceFailOrderNumbers": ["FAIL"], // Pedidos que siempre fallan
    "ForceSuccessOrderNumbers": ["OK"] // Pedidos que siempre tienen éxito
  }
}
```

## 🧪 Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests específicos
dotnet test --filter "OrderStatusTransitionsTests"
```

## 📁 Estructura del Proyecto

```
OrdersAPI/
├── src/
│   └── OrderIntegration.Api/
│       ├── Controllers/          # Endpoints REST
│       ├── Contracts/            # DTOs (Request/Response)
│       ├── Domain/
│       │   ├── Entities/         # Entidades de dominio
│       │   ├── Enums/            # Enumeraciones
│       │   └── Exceptions/       # Excepciones de negocio
│       ├── Application/
│       │   ├── Services/         # Lógica de negocio
│       │   └── Interfaces/       # Contratos de servicios
│       ├── Infrastructure/
│       │   ├── Persistence/      # EF Core, DbContext
│       │   └── Integrations/     # Simulador ERP
│       └── Middleware/           # Exception handling, Auth, CorrelationId
├── tests/
│   └── OrderIntegration.Tests/   # Unit & Integration tests
├── deploy/
│   ├── docker-compose.yml        # Orquestación local
│   └── .env.example              # Variables de entorno
├── Dockerfile                    # Build de la API
└── README.md
```

## 🔒 Seguridad

- **API Key**: Autenticación simple mediante header `X-API-KEY`
- **Rutas públicas**: `/health`, `/swagger`
- **ProblemDetails**: Respuestas de error consistentes (RFC 7807)
- **Usuario no-root**: El contenedor Docker ejecuta como usuario sin privilegios

## 📊 Observabilidad

- **Logs estructurados** con Serilog
- **CorrelationId** en todas las peticiones
- **Request timing** automático
- **Health checks** con verificación de base de datos

## 🚧 Roadmap

- [ ] Rate limiting
- [ ] Retry automático de integración
- [ ] Outbox pattern para mensajes
- [ ] OpenTelemetry
- [ ] Background worker para reintentos

## 📄 Licencia

MIT License - ver [LICENSE](LICENSE) para más detalles.
