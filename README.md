# AmazonClone Microservices

A production-oriented microservices e-commerce platform built with .NET 9, PostgreSQL, Redis, RabbitMQ, Ocelot, and a React frontend. The solution is designed to showcase clean architecture, service decomposition, API gateway patterns, authentication, and event-driven communication in a realistic marketplace scenario.

This repository combines:

- A .NET 9 backend built as independent services
- An API Gateway for centralized routing
- JWT-based authentication and authorization
- PostgreSQL for relational persistence
- Redis for cart/session-like data
- RabbitMQ for asynchronous messaging
- A Vite + React storefront UI
- Docker support for local infrastructure orchestration

## Architecture Overview

The system is organized into multiple bounded services connected through a single entry point:

```text
                  ┌──────────────────────┐
                  │   React Frontend     │
                  │   (Vite / React)     │
                  └───────────┬──────────┘
                              │ HTTP
                              ▼
                  ┌──────────────────────┐
                  │   API Gateway        │
                  │   Ocelot / ASP.NET   │
                  └───────────┬──────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌────────────────┐   ┌──────────────────┐   ┌──────────────────┐
│ Catalog        │   │ Identity         │   │ Orders           │
│ Service        │   │ Service          │   │ Service          │
│ - Products     │   │ - Auth           │   │ - Orders         │
│ - Categories   │   │ - JWT            │   │ - Payments       │
└──────┬─────────┘   └──────┬───────────┘   └──────┬───────────┘
       │                     │                      │
       └─────────────────────┼──────────────────────┘
                             │
                             ▼
                   ┌──────────────────────┐
                   │ Shared Dependencies  │
                   │ PostgreSQL / Redis   │
                   │ RabbitMQ / Storage   │
                   └──────────────────────┘
```

## Core Features

- Product catalog management
- Category-based product organization
- User registration and login
- JWT authentication and role-aware access
- Basket and cart operations
- Order placement and processing
- Messaging-based integration between services
- API aggregation through Ocelot
- Dockerized database infrastructure
- Modern React frontend for user interaction

## Tech Stack

### Backend
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- RabbitMQ (MassTransit)
- Ocelot API Gateway
- FluentValidation
- JWT / Bearer authentication

### Frontend
- React 19
- Vite
- React Router
- Axios
- SignalR client support for chat

### DevOps / Local Setup
- Docker Compose
- .NET CLI
- npm / Node.js

## Repository Structure

```text
.
├── README.md
├── docker-compose.yml
├── global.json
├── docker/
│   ├── init-db.sh
│   └── init-multiple-databases.sh
├── frontend/
│   ├── src/
│   ├── public/
│   ├── package.json
│   └── vite.config.js
├── src/
│   ├── ApiGateway/
│   │   └── Gateway.Api/
│   ├── BuildingBlocks/
│   │   └── Messaging/
│   └── Services/
│       ├── Basket/
│       ├── Catalog/
│       ├── Chat/
│       ├── Identity/
│       ├── Orders/
│       ├── Reviews/
│       └── Storage/
└── AmazonClone.sln
```

## Prerequisites

Before running the project locally, make sure you have:

- .NET 9 SDK
- Node.js 18+ or 20+
- npm
- Docker + Docker Compose
- RabbitMQ installed and running locally on port 5672

> Note: In this repository, PostgreSQL and Redis are started with Docker Compose. RabbitMQ is expected to be available on localhost:5672 (either as a local installation or a Docker container).

## Local Infrastructure Setup

Start the required infrastructure services:

```bash
docker compose up -d
```

This starts:

- PostgreSQL on `localhost:5433`
- Redis on `localhost:6379`

The Docker setup creates databases for the application:

- `catalog_db`
- `identity_db`
- `orders_db`
- `notification_db`

## Running the Backend Services

From the repository root, restore and build the solution:

```bash
dotnet restore
-dotnet build
```

Run the gateway and the individual service APIs:

```bash
dotnet run --project src/ApiGateway/Gateway.Api
```

```bash
dotnet run --project src/Services/Catalog/Catalog.Api
```

```bash
dotnet run --project src/Services/Identity/Identity.Api
```

```bash
dotnet run --project src/Services/Basket/Basket.Api
```

```bash
dotnet run --project src/Services/Orders/Orders.Api
```

```bash
dotnet run --project src/Services/Chat/Chat.Api
```

If the additional services exist in your local branch, run them similarly:

```bash
dotnet run --project src/Services/Reviews/Reviews.Api
dotnet run --project src/Services/Storage/Storage.Api
```

The API Gateway is configured at:

- `http://localhost:5000`

The service-specific ports are mapped in the gateway configuration and typically include:

- Catalog: `5001`
- Identity: `5002`
- Orders: `5003`
- Basket: `5004`
- Reviews: `5006`
- Chat: `5007`
- Storage: `5008`

## Running the Frontend

```bash
cd frontend
npm install
npm run dev
```

Then open:

- `http://localhost:5173`

## Authentication Flow

The Identity service secures requests with JWT tokens.

### Register user

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@test.com",
    "password": "Test123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@test.com",
    "password": "Test123"
  }'
```

### Use the token

```bash
curl http://localhost:5000/api/products \
  -H "Authorization: Bearer <your-token>"
```

## Example End-to-End Scenario

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"Test123"}' | jq -r .accessToken)

# 2. Create a category
CATEGORY_ID=$(curl -s -X POST http://localhost:5000/api/categories \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"Phones","slug":"phones"}' | jq -r .id)

# 3. Create a product
PRODUCT_ID=$(curl -s -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"categoryId":"'$CATEGORY_ID'","name":"iPhone 15","slug":"iphone-15","price":999.99,"currency":"USD"}' | jq -r .id)

# 4. Add to basket
curl -X POST http://localhost:5000/api/basket/items \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"productId":"'$PRODUCT_ID'","productName":"iPhone 15","unitPrice":999.99,"quantity":2}'
```

## Configuration Notes

### ASP.NET configuration
The application uses `appsettings.json` for runtime configuration. Example values include:

- PostgreSQL connection strings
- JWT issuer, audience, secret, and expiry
- Allowed hosts

### Example database settings
```json
{
  "ConnectionStrings": {
    "CatalogDatabase": "Host=localhost;Port=5433;Database=catalog_db;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Issuer": "AmazonClone",
    "Audience": "AmazonClone.Client",
    "Secret": "YOUR_LONG_SECRET_KEY_HERE",
    "ExpiryMinutes": 60
  }
}
```

## Gateway Routing

The Ocelot gateway exposes a unified API surface. Key routes include:

- `/api/categories`
- `/api/products`
- `/api/auth`
- `/api/profile`
- `/api/orders`
- `/api/payments`
- `/api/basket`
- `/api/reviews`
- `/api/chat`
- `/api/storage`

Base URL for all API requests through the gateway:

```text
http://localhost:5000
```

## Development Guidelines

This project follows a clean separation of concerns across application layers:

- Domain layer: entities and business rules
- Application layer: DTOs, validators, and use cases
- Infrastructure layer: EF Core, database access, external integrations
- API layer: HTTP controllers and entry points

This pattern keeps the services maintainable and testable as they grow.

## Troubleshooting

### PostgreSQL not starting
```bash
docker compose ps
docker logs amazonclone-postgres
```

### RabbitMQ connection issues
Ensure RabbitMQ is installed and listening on:

```text
amqp://localhost:5672
```

### JWT errors
Validate that the secret in the Identity service matches the expected JWT secret and that the token is correctly sent in the `Authorization` header.

### Frontend not connecting to API
Check that the gateway is running at `http://localhost:5000` and that the frontend is calling the correct base URL.

## Contributing

Contributions are welcome. If you want to improve the project:

1. Fork the repository
2. Create a feature branch
3. Implement your changes
4. Run validation and build checks
5. Submit a pull request with a clear description

## License

This project is distributed under the MIT license unless otherwise specified.

