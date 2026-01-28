# Sample Payloads

Ejemplos de request/response para la Order Integration API.

## Orders

### Create Order

**Request:**

```json
POST /api/orders
Content-Type: application/json
X-API-KEY: dev-api-key-12345

{
  "orderNumber": "ORD-2026-0100",
  "customerCode": "CUST-001",
  "items": [
    {
      "sku": "SKU-LAPTOP-001",
      "description": "Laptop HP 15.6 pulgadas",
      "quantity": 2,
      "unitPrice": 899.99
    },
    {
      "sku": "SKU-MOUSE-001",
      "description": "Mouse inalámbrico Logitech",
      "quantity": 2,
      "unitPrice": 29.99
    }
  ]
}
```

**Response (201 Created):**

```json
{
  "id": 1,
  "orderNumber": "ORD-2026-0100",
  "customerCode": "CUST-001",
  "status": "Created",
  "createdAt": "2026-01-28T10:30:00Z",
  "updatedAt": "2026-01-28T10:30:00Z",
  "totalAmount": 1859.96,
  "items": [
    {
      "id": 1,
      "sku": "SKU-LAPTOP-001",
      "description": "Laptop HP 15.6 pulgadas",
      "quantity": 2,
      "unitPrice": 899.99,
      "lineTotal": 1799.98
    },
    {
      "id": 2,
      "sku": "SKU-MOUSE-001",
      "description": "Mouse inalámbrico Logitech",
      "quantity": 2,
      "unitPrice": 29.99,
      "lineTotal": 59.98
    }
  ],
  "allowedTransitions": ["Prepared", "Cancelled"]
}
```

### List Orders (Paginated)

**Request:**

```
GET /api/orders?status=Created&customerCode=CUST-001&page=1&pageSize=10
X-API-KEY: dev-api-key-12345
```

**Response (200 OK):**

```json
{
  "items": [
    {
      "id": 1,
      "orderNumber": "ORD-2026-0100",
      "customerCode": "CUST-001",
      "status": "Created",
      "createdAt": "2026-01-28T10:30:00Z",
      "updatedAt": "2026-01-28T10:30:00Z",
      "totalAmount": 1859.96,
      "items": [...],
      "allowedTransitions": ["Prepared", "Cancelled"]
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Change Status

**Request:**

```json
POST /api/orders/1/status
Content-Type: application/json
X-API-KEY: dev-api-key-12345

{
  "newStatus": "Prepared"
}
```

**Response (200 OK):**

```json
{
  "id": 1,
  "orderNumber": "ORD-2026-0100",
  "customerCode": "CUST-001",
  "status": "Prepared",
  "createdAt": "2026-01-28T10:30:00Z",
  "updatedAt": "2026-01-28T10:35:00Z",
  "totalAmount": 1859.96,
  "items": [...],
  "allowedTransitions": ["Dispatched", "Cancelled"]
}
```

**Error Response (409 Conflict - Invalid Transition):**

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Conflicto",
  "status": 409,
  "detail": "Transición de estado inválida: de Created a Delivered. Transiciones permitidas: Prepared, Cancelled",
  "instance": "/api/orders/1/status"
}
```

---

## ERP Integration

### Send to ERP

**Request:**

```
POST /api/orders/1/send-to-erp
X-API-KEY: dev-api-key-12345
```

**Response - Success (200 OK):**

```json
{
  "success": true,
  "message": "Pedido enviado al ERP correctamente",
  "attemptId": 1,
  "correlationId": "7f3d8a2b1c4e"
}
```

**Response - Failure (200 OK):**

```json
{
  "success": false,
  "message": "Error al enviar al ERP: Timeout de conexión",
  "attemptId": 2,
  "correlationId": "9a1b2c3d4e5f"
}
```

### ERP Webhook (Order ACK)

**Request - Success:**

```json
POST /api/webhooks/erp/order-ack
Content-Type: application/json
X-API-KEY: dev-api-key-12345

{
  "orderNumber": "ORD-2026-0100",
  "success": true,
  "message": "Pedido procesado correctamente en SAP",
  "correlationId": "7f3d8a2b1c4e"
}
```

**Request - Failure:**

```json
POST /api/webhooks/erp/order-ack
Content-Type: application/json
X-API-KEY: dev-api-key-12345

{
  "orderNumber": "ORD-2026-0100",
  "success": false,
  "message": "Error: SKU-LAPTOP-001 no encontrado en catálogo ERP",
  "correlationId": "7f3d8a2b1c4e"
}
```

**Response (200 OK):**

```json
{
  "processed": true,
  "orderNumber": "ORD-2026-0100",
  "newStatus": "Acked",
  "message": "Webhook procesado correctamente"
}
```

---

## Audit

### Get Audit Events

**Request:**

```
GET /api/audit?entityType=Order&entityId=1&limit=50
X-API-KEY: dev-api-key-12345
```

**Response (200 OK):**

```json
[
  {
    "id": 1,
    "entityType": "Order",
    "entityId": "1",
    "eventType": "OrderCreated",
    "timestampUtc": "2026-01-28T10:30:00Z",
    "userOrClient": "API",
    "data": "{\"orderNumber\":\"ORD-2026-0100\",\"customerCode\":\"CUST-001\",\"itemCount\":2}",
    "correlationId": "abc123def456"
  },
  {
    "id": 2,
    "entityType": "Order",
    "entityId": "1",
    "eventType": "StatusChanged",
    "timestampUtc": "2026-01-28T10:35:00Z",
    "userOrClient": "API",
    "data": "{\"fromStatus\":\"Created\",\"toStatus\":\"Prepared\"}",
    "correlationId": "def456ghi789"
  },
  {
    "id": 3,
    "entityType": "Order",
    "entityId": "1",
    "eventType": "ErpSent",
    "timestampUtc": "2026-01-28T10:40:00Z",
    "userOrClient": "API",
    "data": "{\"attemptId\":1,\"success\":true}",
    "correlationId": "7f3d8a2b1c4e"
  }
]
```

---

## Health

### Basic Health Check

**Request:**

```
GET /health
```

**Response - Healthy (200 OK):**

```json
{
  "status": "Healthy",
  "database": "Healthy",
  "version": "1.0.0.0",
  "timestamp": "2026-01-28T10:45:00Z"
}
```

**Response - Unhealthy (503 Service Unavailable):**

```json
{
  "status": "Unhealthy",
  "database": "Error",
  "version": "1.0.0.0",
  "timestamp": "2026-01-28T10:45:00Z"
}
```

### Detailed Health Check

**Request:**

```
GET /health/detailed
```

**Response (200 OK):**

```json
{
  "status": "Healthy",
  "database": "Healthy",
  "version": "1.0.0.0",
  "timestamp": "2026-01-28T10:45:00Z",
  "environment": "Development",
  "machineName": "orderintegration-api",
  "orderCount": 15
}
```

---

## Error Responses

### 400 Bad Request (Validation Error)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "OrderNumber": ["The OrderNumber field is required."],
    "Items": ["The Items field requires at least 1 item."]
  }
}
```

### 401 Unauthorized

```json
{
  "type": "https://httpstatuses.com/401",
  "title": "No autorizado",
  "status": 401,
  "detail": "API Key no proporcionada. Incluya el header 'X-API-KEY'.",
  "instance": "/api/orders"
}
```

### 404 Not Found

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "No encontrado",
  "status": 404,
  "detail": "Pedido con ID 999 no encontrado.",
  "instance": "/api/orders/999"
}
```

### 409 Conflict

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Conflicto",
  "status": 409,
  "detail": "Ya existe un pedido con el número ORD-2026-0100",
  "instance": "/api/orders"
}
```

### 500 Internal Server Error

```json
{
  "type": "https://httpstatuses.com/500",
  "title": "Error interno del servidor",
  "status": 500,
  "detail": "Ha ocurrido un error inesperado. Por favor, intente de nuevo más tarde.",
  "instance": "/api/orders",
  "traceId": "00-abc123def456-789ghi-00"
}
```
