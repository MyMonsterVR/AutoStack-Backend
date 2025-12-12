# AutoStack

A stack-sharing platform built with .NET 10 and Clean Architecture. Allows developers to discover, share, and manage technology stacks for web development projects.

## Features

- **Stack Management**: Create, browse, download, and share technology stacks
- **Package Discovery**: Organize and discover verified packages
- **User Authentication**: JWT-based authentication with refresh tokens
- **Two-Factor Authentication**: TOTP-based 2FA with recovery codes
- **Email Verification**: Secure email verification for new users
- **Password Reset**: Secure password recovery flow
- **Avatar Uploads**: Local file storage for user avatars
- **Cookie & Bearer Token Support**: Flexible authentication for web and API clients
- **Audit Logging**: Comprehensive logging with automatic cleanup (GDPR compliant)
- **Rate Limiting**: Protection against abuse
- **Filter and Sort**: Stacks by popularity, type, and date
- **Pagination Support**: Efficient data loading

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

- **.NET 10**: Latest .NET framework
- **Entity Framework Core 10**: ORM for database access
- **MediatR**: CQRS pattern implementation
- **FluentValidation**: Request validation
- **PostgreSQL**: Primary database
- **JWT Authentication**: Secure token-based authentication
- **Scalar**: Interactive API documentation
- **xUnit, Moq, FluentAssertions**: Testing framework

## Testing

Run all tests using:

```bash
dotnet test
```

Run tests with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) (version 12 or higher)

### Setup

#### 1. Clone the repository

```bash
git clone https://github.com/MyMonsterVR/AutoStack-Backend.git
cd AutoStack
```

#### 2. Database Setup

**Option A: Using Docker (Recommended)**

```bash
docker run --name autostack-postgres \
  -e POSTGRES_PASSWORD=yourpassword \
  -e POSTGRES_DB=autostack \
  -p 5432:5432 \
  -d postgres:latest
```

**Option B: Local PostgreSQL Installation**

1. Install PostgreSQL
2. Create a new database:

```sql
CREATE DATABASE autostack;
```

#### 3. Configure Application Settings

Update `AutoStack.Presentation/appsettings.Development.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=autostack;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters-long-for-security",
    "Issuer": "AutoStack",
    "Audience": "AutoStack",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 30
  },
  "TwoFactorSettings": {
    "EncryptionKey": "your-encryption-key-32-chars",
  },
  "CookieSettings": {
    "AccessTokenCookieName": "access_token",
    "RefreshTokenCookieName": "refresh_token",
    "UseHttpOnlyCookies": true,
    "UseSecureCookies": false,
    "SameSite": "Lax"
  },
  "FileStorage": {
    "BaseUrl": "https://localhost:7135",
    "AvatarPath": "uploads/avatars",
    "MaxFileSizeBytes": 5242880,
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".webp", ".gif" ]
  },
  "ResendClient": {
    "ApiToken": "YOUR_RESEND_API_TOKEN",
  },
  "FrontendUrl": "https://localhost:3000"
}
```

**Alternatively, use User Secrets for sensitive data:**

```bash
cd AutoStack.Presentation
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=autostack;Username=postgres;Password=yourpassword"
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-at-least-32-characters"
dotnet user-secrets set "TwoFactorSettings:EncryptionKey" "your-encryption-key-32-chars"
dotnet user-secrets set "ResendClient:ApiToken" "YOUR_RESEND_API_TOKEN"
```

**Important Security Notes:**
- Generate a strong secret key for `JwtSettings.SecretKey` (minimum 32 characters)
- Generate a strong encryption key for `TwoFactorSettings.EncryptionKey` (exactly 32 characters)
- Never commit production credentials to source control
- Set `UseSecureCookies: true` in production

#### 4. Apply Database Migrations

```bash
dotnet ef database update --project AutoStack.Infrastructure
```

This will create all necessary tables in your PostgreSQL database.

#### 5. Run the Application

```bash
dotnet run --project AutoStack.Presentation
```

Or from the Presentation directory:

```bash
cd AutoStack.Presentation
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7135`
- HTTP: `http://localhost:5211`

#### 6. Access API Documentation

Navigate to:
- **Scalar UI**: `https://localhost:7135/scalar/v1`
- **OpenAPI JSON**: `https://localhost:7135/openapi/v1.json`

## Patterns Used

- CQRS (Command Query Responsibility Segregation)
- Repository Pattern with Unit of Work
- Factory Pattern for entity creation
- Result Pattern for error handling
- Mediator Pattern via MediatR

## API Usage Examples

### Register a New User

```bash
curl -X POST https://localhost:7135/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "username",
    "password": "SecurePassword123!"
  }'
```

### Login

```bash
curl -X POST https://localhost:7135/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "username",
    "password": "SecurePassword123!"
  }'
```

### Create a Stack (Authenticated)

```bash
curl -X POST https://localhost:7135/api/stacks \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Modern Web Stack",
    "description": "A modern full-stack web development setup",
    "type": "FULLSTACK",
    "stackInfo": [
      {
        "packageName": "React",
        "packageLink": "https://react.dev"
      },
      {
        "packageName": "Node.js",
        "packageLink": "https://nodejs.org"
      }
    ]
  }'
```

### Get All Stacks (paged)

```bash
curl https://localhost:7135/api/stacks?pageNumber=1&pageSize=10
```

## Development

### Adding a Migration

```bash
dotnet ef migrations add MigrationName
dotnet ef database update --project AutoStack.Infrastructure
```

### Code Style

This project follows the standard .NET coding conventions and Clean Architecture principles:
- Domain layer has no dependencies
- Application layer depends only on Domain
- Infrastructure and Presentation depends on Application
- Use MediatR for CQRS pattern
- Use FluentValidation for request validation

## Security Features

- **JWT Access Tokens**: Short-lived tokens for API authentication
- **Refresh Token Rotation**: Secure token refresh mechanism
- **Argon2id Password Hashing**: Industry-standard password hashing with salt
- **Two-Factor Authentication**: TOTP-based with QR code generation
- **Recovery Codes**: Backup authentication method
- **Email Verification**: Prevents fake accounts
- **Request Validation**: FluentValidation ensures data integrity
- **Security Headers**: XSS, clickjacking, and MIME-sniffing protection
- **Rate Limiting**: Prevents abuse and DDoS attacks
- **GDPR Compliance**: Automatic log cleanup and data sanitization
- **Protected Endpoints**: Authentication required for sensitive operations

## Background Service

### Log Cleanup Service

Automatically runs daily at 2 AM UTC to delete audit logs older than 30 days (GDPR compliance).