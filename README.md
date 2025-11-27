# AutoStack

A stack-sharing platform built with .NET 10 and Clean Architecture. Allows developers to discover, share, and manage technology stacks for web development projects.

## Features

- Stack management (create, browse, share)
- Package discovery and organization
- JWT-based authentication with refresh tokens
- Filter and sort stacks by popularity, type, and date
- Pagination support

## Architecture

Clean Architecture with four distinct layers:

```
AutoStack/
├── AutoStack.Domain/              # Business entities and rules
│   ├── Entities/                  # Stack, User, Package
│   └── Repositories/              # Repository interfaces
├── AutoStack.Application/         # Use cases
│   ├── Features/                  # CQRS commands & queries
│   ├── DTOs/                      # Data transfer objects
│   └── Common/                    # Shared logic
├── AutoStack.Infrastructure/      # Data access
│   ├── Persistence/               # EF Core configuration
│   └── Repositories/              # Repository implementations
└── AutoStack.Presentation/        # HTTP API
    └── Endpoints/                 # Minimal API endpoints
```

### Tech Stack

- .NET 10
- Entity Framework Core 10
- MediatR (CQRS)
- FluentValidation
- PostgreSQL
- JWT Authentication
- Scalar (API docs)

## Testing

using xUnit, Moq, and FluentAssertions.

```bash
dotnet test
```

## Getting Started

**Prerequisites**
- .NET 10 SDK
- PostgreSQL

**Setup**

1. Clone the repository
   ```bash
   git clone https://github.com/MyMonsterVR/AutoStack-Backend.git
   cd AutoStack
   ```

2. Configure database connection in a production or development file
EXAMPLE:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=autostack;Username=postgres;Password=yourpassword"
     }
   }
   ```
OR
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "DefaultConnection": "Host=localhost;Database=autostack;Username=postgres;Password=yourpassword"
```

3. Apply migrations
   ```bash
   dotnet ef database update --project AutoStack.Infrastructure
   ```

4. Start the application
   ```bash
   cd AutoStack.Presentation
   dotnet run
   ```

5. View API documentation at `https://localhost:7135/scalar`

## Patterns Used

- CQRS (Command Query Responsibility Segregation)
- Repository Pattern with Unit of Work
- Factory Pattern for entity creation
- Result Pattern for error handling
- Mediator Pattern via MediatR

## Security

- JWT access tokens
- Refresh token rotation
- Argon2id password hashing
- Request validation with FluentValidation
- Protected endpoints require authentication
