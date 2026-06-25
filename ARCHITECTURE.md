# LogiPulse Architecture

## Overview

LogiPulse is a microservice-based application with a separate frontend, message broker, database, and several backend services.

The current project structure contains:
- `CatalogService/` — product catalog and checkout API service
- `OrderAPIs/` — order lifecycle service, API and message consumer
- `FleetService/` — fleet worker consuming order completion events
- `Frontend/` — Angular single-page UI served by NGINX
- `SharedContracts/` — shared event contract definitions
- `docker-compose.yml` — multi-container runtime orchestration

## Technology stack

- .NET 10 Minimal APIs for backend services
- Angular 22 frontend application
- NGINX for frontend static hosting in Docker
- RabbitMQ for asynchronous message brokering
- MassTransit as the RabbitMQ transport and consumer/publisher library
- Microsoft SQL Server for order persistence
- Entity Framework Core for database access and migrations
- Docker Compose for local orchestration

## Docker Compose services

### rabbitmq (`logipulse-broker`)
- Image: `rabbitmq:3-management`
- Ports exposed: `5672`, `15672`
- Network: `logipulse-net`
- Healthcheck: `rabbitmq-diagnostics -q ping`
- Used as the message broker for all MassTransit event communication

### sqlserver (`logipulse-db`)
- Image: `mcr.microsoft.com/mssql/server:2022-latest`
- Ports exposed: `1433`
- Network: `logipulse-net`
- Environment:
  - `ACCEPT_EULA=Y`
  - `MSSQL_SA_PASSWORD=YourStrong@Password123`
- Healthcheck: TCP connect to `127.0.0.1:1433`
- Stores the `LogiPulseOrders` database for orders created by `OrderAPIs`

### catalog-service (`catalog-api`)
- Build context: root with `CatalogService/Dockerfile`
- Ports exposed: `5200`
- Network: `logipulse-net`
- Depends on: `rabbitmq` being healthy
- Environment:
  - `ASPNETCORE_HTTP_PORTS=5200`
  - `RABBITMQ_HOST=rabbitmq`
- Responsibilities:
  - Exposes product catalog API
  - Accepts checkout commands from UI
  - Publishes `CheckoutRequestedEvent` into RabbitMQ

### order-service (`order-api`)
- Build context: root with `OrderAPIs/Dockerfile`
- Ports exposed: `5257`
- Network: `logipulse-net`
- Depends on: `sqlserver` and `rabbitmq` being healthy
- Environment:
  - `ASPNETCORE_HTTP_PORTS=5257`
  - `RABBITMQ_HOST=rabbitmq`
  - `SQL_CONNECTIONSTRING=Server=sqlserver;Database=LogiPulseOrders;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;`
- Responsibilities:
  - Exposes order API endpoints at `/api/orders`
  - Persists orders to SQL Server using EF Core
  - Consumes `CheckoutRequestedEvent` from RabbitMQ
  - Publishes `OrderPlacedEvent` after order creation

### fleet-service (`fleet-worker`)
- Build context: root with `FleetService/Dockerfile`
- Network: `logipulse-net`
- Depends on: `rabbitmq` being healthy
- Environment:
  - `RABBITMQ_HOST=rabbitmq`
- Responsibilities:
  - Runs as a background worker
  - Consumes `OrderPlacedEvent`
  - Simulates fleet allocation / dispatch logging

### frontend-ui (`logipulse-ui`)
- Build context: root with `Frontend/Dockerfile`
- Ports exposed: `4200` on host mapped to `80` inside container
- Network: `logipulse-net`
- Depends on: `catalog-service`, `order-service`
- Responsibilities:
  - Serves the Angular UI from NGINX
  - Calls backend HTTP APIs on host ports

## Service communication

### HTTP communication
- Frontend UI → CatalogService
  - `GET http://localhost:5200/api/products`
  - `POST http://localhost:5200/api/cart/checkout`
- Frontend UI → OrderService
  - `GET http://localhost:5257/api/orders`

### Event communication via RabbitMQ / MassTransit

#### Checkout flow
1. User clicks checkout in the frontend
2. Frontend sends `POST /api/cart/checkout` to `CatalogService`
3. `CatalogService` publishes a `CheckoutRequestedEvent` to RabbitMQ
4. `OrderService` consumes `CheckoutRequestedEvent`
5. `OrderService` creates a new `Order` in SQL Server
6. `OrderService` publishes `OrderPlacedEvent` to RabbitMQ
7. `FleetService` consumes `OrderPlacedEvent`
8. `FleetService` logs or processes fleet allocation details

#### Order retrieval
- Frontend reads order history from `OrderService` using `GET /api/orders`

## Shared contracts and message types

### `SharedContracts/Events.cs`
- Defines `CheckoutRequestedEvent`:
  - `DeliveryAddress`
  - `TotalAmount`

### `OrderAPIs/OrderMessages.cs`
- Defines `OrderPlacedEvent`:
  - `OrderId`
  - `OrderNumber`
  - `DeliveryAddress`

### `FleetService/FleetMessages.cs`
- Defines the same `OrderPlacedEvent` contract as the fleet consumer

> Note: `CheckoutRequestedEvent` is shared in `SharedContracts`, while `OrderPlacedEvent` is currently defined in each service project separately.

## Database details

### SQL Server setup
- Uses the official SQL Server 2022 Docker image
- Exposes host port `1433`
- `OrderAPIs` uses EF Core and automatic migrations during startup

### Order entity
- Persisted in `OrderAPIs`
- Created when `CheckoutRequestedConsumer` receives a checkout event
- Fields include:
  - `Id`
  - `OrderNumber`
  - `DeliveryAddress`
  - `Status`

## Backend implementation details

### CatalogService
- Uses `MassTransit` with RabbitMQ transport
- Configures CORS to allow any origin, header, and method
- Exposes minimal API endpoints with `app.MapGet` and `app.MapPost`
- Publishes events via `IPublishEndpoint`

### OrderAPIs
- Uses `AddDbContext<OrderDbContext>` with SQL Server
- Registers both consumers:
  - `CheckoutRequestedConsumer`
  - `OrderPlacedConsumer`
- Has a dedicated `order-api` REST surface
- Calls `context.Database.MigrateAsync()` at startup to create/apply migrations
- `CheckoutRequestedConsumer` saves orders and publishes `OrderPlacedEvent`

### FleetService
- Registers `OrderPlacedConsumer`
- Connects to RabbitMQ and logs order dispatch activity
- No HTTP endpoints are exposed

## Frontend behavior

### UI endpoints
- `http://localhost:4200/` serves the Angular app
- The application calls backend services using hard-coded local URLs:
  - `catalogUrl = 'http://localhost:5200/api/products'`
  - `checkoutUrl = 'http://localhost:5200/api/cart/checkout'`
  - `ordersUrl = 'http://localhost:5257/api/orders'`

### Frontend flow
- Loads products from `CatalogService`
- Adds items to a cart in memory
- Submits checkout payload to `CatalogService`
- Reloads orders after checkout

## Run instructions

From the workspace root:

```bash
docker compose up -d
```

To stop and remove compose containers:

```bash
docker compose down
```

If you need to rebuild images after code changes:

```bash
docker compose build
```

## Notes

- The application uses Docker network `logipulse-net` for service-to-service communication.
- Backend services resolve RabbitMQ using the hostname `rabbitmq` inside Docker Compose.
- SQL Server is reachable inside the compose network as `sqlserver` and from the host as `localhost:1433`.
- Healthchecks are configured for RabbitMQ and SQL Server in `docker-compose.yml`.
