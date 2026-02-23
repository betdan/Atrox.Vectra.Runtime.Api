# Atrox.Vectra.Runtime.Api

Atrox.Vectra.Runtime.Api is a transport-agnostic runtime service that executes configurable stored procedures against a selected database engine (SQL Server or PostgreSQL) and returns normalized execution results.

It exposes the same execution capability through multiple transports:

- REST (HTTP)
- AMQP (RabbitMQ / MassTransit)
- gRPC
- WebSocket

All transports use a shared execution contract (`IExecutionService`) and the same application service implementation, so business logic is centralized and reusable.

## Table of contents

- [Key capabilities](#key-capabilities)
- [Architecture](#architecture)
- [Project structure](#project-structure)
- [Execution contract](#execution-contract)
- [Transport endpoints](#transport-endpoints)
- [Configuration](#configuration)
- [Run locally](#run-locally)
- [Build and test](#build-and-test)
- [Docker](#docker)
- [Observability](#observability)
- [Security notes](#security-notes)
- [Troubleshooting](#troubleshooting)

## Key capabilities

- Centralized procedure execution service.
- Configurable database engine at runtime:
  - `SqlServer`
  - `PostgreSql`
- Multi-transport access with no duplicated execution logic.
- Health checks and metrics.
- Global exception handling and required header validation middleware.

## Architecture

The solution follows layered architecture:

- **Transport layer**: REST, AMQP, gRPC, WebSocket adapters.
- **Application layer**: orchestration and execution service abstraction.
- **Data access layer**: DB provider implementations and stored procedure execution.
- **Cross-cutting layer**: middleware, crypto, health check helpers, configuration support.

### Core principle

Transports do not execute database logic directly.  
They map incoming messages to internal request DTOs and call:

- `IExecutionService.ExecuteServiceAsync(...)`

This keeps the runtime core independent of transport protocol.

## Project structure

```text
Atrox.Vectra.Runtime.Api/
├─ Atrox.Vectra.Runtime.Api                    # Web host (Program, middleware pipeline, REST, gRPC, WebSocket transport)
│  └─ Transports
│     ├─ Rest
│     ├─ Grpc
│     └─ WebSocket
├─ Atrox.Vectra.Runtime.Api.Application        # Execution service implementation + AMQP transport adapter
│  └─ Transports
│     └─ Amqp
├─ Atrox.Vectra.Runtime.Api.Application.Contracts
│  └─ Services                                 # IExecutionService and service contracts
├─ Atrox.Vectra.Runtime.Api.Business           # Request/response domain models
├─ Atrox.Vectra.Runtime.Api.DataAccess         # SQL Server/PostgreSQL providers and DB execution
├─ Atrox.Vectra.Runtime.Api.DataAccess.Contracts
└─ Atrox.Vectra.Runtime.Api.CrossCutting       # Middleware, health checks, crypto, misc helpers
```

## Execution contract

### Internal request model

The core execution request contains:

- `databaseName` (string)
- `procedureName` (string)
- `inputParameters` (name/value list)

### Internal response model

The execution response returns:

- `returns` (int)
- `prints` (list of strings)
- `raisError` (list of error objects from DB)
- `outputParameters` (dictionary)
- `resultSets` (list of tabular sets)

Wrapped in:

- `ServiceResult`:
  - `Data`
  - `Error` (normalized errors list)

## Transport endpoints

### REST

- Base route:
  - `POST /api/v1/AtroxVectraRuntimeApi`
  - `POST /api/v1/AtroxVectraRuntimeApi/{extraValue}`

### AMQP (RabbitMQ)

- Consumer class:
  - `AtroxVectraRuntimeApiConsumer`
- Queue name is configured in:
  - `RabbitMqQueueName:Atrox.Vectra.Runtime.Api`

### gRPC

- Proto file:
  - `Transports/Grpc/Protos/runtime_execution.proto`
- Service:
  - `runtimeexecution.RuntimeExecution`
- Method:
  - `Execute`

### WebSocket

- Endpoint path (configurable):
  - `/ws/runtime`
- Accepts JSON equivalent to REST payload.
- Returns standardized JSON execution response.

## Configuration

Main settings file:

- `Atrox.Vectra.Runtime.Api/appsettings.json`

### Required sections

```json
{
  "Database": {
    "Engine": "SqlServer"
  },
  "ConnectionStrings": {
    "SqlServer": {
      "ConnectionStringFormat": "Server={server},{port};Initial Catalog={database};User ID={userId};Password={decryptedPassword};TrustServerCertificate=true;",
      "Server": "localhost",
      "Port": 1433,
      "UserId": "sa",
      "Password": "",
      "Database": "master"
    },
    "PostgreSql": {
      "ConnectionStringFormat": "Host={server};Port={port};Database={database};Username={userId};Password={decryptedPassword};Include Error Detail=true;",
      "Server": "localhost",
      "Port": 5432,
      "UserId": "postgres",
      "Password": "",
      "Database": "postgres"
    }
  },
  "Transports": {
    "Grpc": {
      "Enabled": true,
      "Port": 5001
    },
    "WebSocket": {
      "Enabled": true,
      "Path": "/ws/runtime",
      "KeepAliveSeconds": 120
    }
  }
}
```

### Engine selection behavior

- Allowed values:
  - `SqlServer`
  - `PostgreSql`
- Any other value throws startup exception.
- Only one engine is active at runtime.

### gRPC behavior

- If `Transports:Grpc:Enabled = true`, gRPC services are registered and mapped.
- If disabled, gRPC is not registered.

### WebSocket behavior

- If `Transports:WebSocket:Enabled = true`, middleware is enabled.
- If disabled, `/ws/...` endpoint is not handled.

## Run locally

From solution root:

```powershell
dotnet run --project .\Atrox.Vectra.Runtime.Api\Atrox.Vectra.Runtime.Api.csproj
```

## Build and test

Build solution:

```powershell
dotnet build .\Atrox.Vectra.Runtime.Api.sln
```

Run tests:

```powershell
dotnet test .\Atrox.Vectra.Runtime.Api.Tests\Atrox.Vectra.Runtime.Api.Tests.csproj
```

## Docker

A `Dockerfile` is included in the solution root. Typical flow:

1. Build image.
2. Push to registry.
3. Deploy with your orchestrator (for example Helm/Rancher).

## Observability

- Health endpoint:
  - `GET /health`
- Prometheus metrics:
  - `GET /metrics`
- Serilog configuration is driven from `appsettings.json`.

## Security notes

- Request header middleware validates required headers:
  - `x-TransactionId`
  - `x-SessionId`
  - `x-ChannelId`
  - `x-I18n`
- Secrets should not be committed in plaintext.
- Prefer secret stores / environment variable overrides in production.

## Troubleshooting

### Startup fails with invalid engine

Check:

- `Database:Engine` is either `SqlServer` or `PostgreSql`.

### WebSocket DI error with `RequestDelegate`

Do not register WebSocket middleware as singleton/transient manually.  
Use:

- `app.UseMiddleware<RuntimeWebSocketMiddleware>()`

### gRPC not available

Check:

- `Transports:Grpc:Enabled = true`
- Port is free and reachable.

---

If you extend the solution with new transports, keep adapters in `Transports/*` and call only `IExecutionService` from transport handlers.
