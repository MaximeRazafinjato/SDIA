# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Application Overview

SDIA (Système de suivi D'Inscription avec IA) is a full-stack application for managing registration workflows with AI assistance. It uses:
- **Backend**: .NET 9 with Clean Architecture (API at http://localhost:5206)
- **Frontend**: React 19 + TypeScript + Vite (UI at http://localhost:5174)
- **Database**: InMemory for development (SQL Server ready)

## Commands

### Running the Application

```bash
# Backend (.NET API)
cd SDIA
dotnet run --project SDIA.API

# Frontend (React)
cd sdia-client
npm run dev
```

### Build and Test

```bash
# Backend
cd SDIA
dotnet build
dotnet test

# Frontend
cd sdia-client
npm run build
npm run lint
```

## Architecture

### Backend Structure (Clean Architecture + CQRS)
- **SDIA.API**: REST API controllers, authentication (cookie-based)
- **SDIA.Application**: CQRS implementation with MediatR
  - Commands/Queries organized by feature (Users, Registrations, FormTemplates)
  - Each operation has its own folder with Command/Query, Handler, Validator, and DTOs
  - Uses Result pattern (Ardalis.Result) for consistent error handling
  - FluentValidation for input validation
  - Manual mapping methods for DTOs (no AutoMapper)
- **SDIA.Core**: Domain entities, repository interfaces
- **SDIA.Infrastructure**: EF Core, repository implementations, external services
- **SDIA.SharedKernel**: Shared code, enums, base entities

### CQRS Structure in Application Layer
```
SDIA.Application/
├── Common/
│   ├── Base/          # BaseCommand, BaseQuery, GridQuery, GridResult
│   └── Extensions/    # QueryExtensions, ServiceCollectionExtensions
├── Users/
│   ├── Management/
│   │   ├── Delete/    # DeleteUserCommand, Handler, Validator
│   │   ├── GetById/   # GetUserByIdQuery, Handler, UserDto
│   │   ├── Grid/      # GetUsersGridQuery, Handler, UserGridDto
│   │   └── Upsert/    # UpsertUserCommand, Handler, Validator
│   └── Me/
│       ├── Get/       # GetCurrentUserQuery, Handler, CurrentUserDto
│       └── UpdateLanguage/ # UpdateLanguageCommand, Handler, Validator
├── Registrations/
│   └── Management/    # Similar structure as Users
└── FormTemplates/
    └── Management/    # Similar structure as Users
```

### Frontend Structure
- **src/api**: Axios configuration, API client
- **src/components**: Reusable UI components
- **src/contexts**: React contexts (Auth, Theme)
- **src/pages**: Application pages/routes
- **src/types**: TypeScript type definitions

## Key Features & Workflows

### Authentication Flow
- Cookie-based authentication with 8-hour sliding expiration
- Test users (InMemory DB):
  - Admin: admin@sdia.com / Admin123!
  - Manager: manager@sdia.com / Manager123!
  - User: user@sdia.com / User123!

### Registration Workflow
States: Draft → Pending → Validated/Rejected
- Email/SMS verification required
- Document upload support
- Dynamic form templates

## API Documentation

Access interactive API documentation at http://localhost:5206/scalar/v1 when backend is running.

Key endpoints:
- `/api/auth/*` - Authentication
- `/api/registrations/*` - Registration management
- `/api/form-templates/*` - Form template CRUD
- `/api/users/*` - User management

## Database Configuration

Currently using InMemory database. To switch to SQL Server:
1. Update connection string in `appsettings.json`
2. Modify `Program.cs` line 22-23 to use `UseSqlServer`
3. Run migrations: `dotnet ef database update`

## Development Notes

- CORS configured for local development (ports 5173/5174)
- Serilog logging to console and files (`logs/sdia-*.txt`)
- Global query filters for soft delete on entities
- Cookie authentication with SameSite=None for cross-origin requests
- CQRS pattern with MediatR for clean separation of concerns
- Result pattern for consistent error handling across all operations
- FluentValidation for comprehensive input validation
- Manual mapping methods for DTO transformations (type-safe, no reflection)

## CQRS Usage Examples

### Using Commands in Controllers
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] UpsertUserCommand command)
{
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
}
```

### Using Queries in Controllers
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id)
{
    var result = await _mediator.Send(new GetUserByIdQuery { Id = id });
    return result.IsSuccess ? Ok(result.Value) : NotFound();
}
```

### Grid Queries with Pagination
```csharp
[HttpGet]
public async Task<IActionResult> GetUsers([FromQuery] GetUsersGridQuery query)
{
    var result = await _mediator.Send(query);
    return Ok(result.Value); // Returns GridResult<UserGridDto>
}
```