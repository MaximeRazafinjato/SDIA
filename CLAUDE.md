# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Application Overview

SDIA (Système de suivi D'Inscription avec IA) is a full-stack application for managing registration workflows with AI assistance. It uses:

-   **Backend**: .NET 9 with Clean Architecture (API at http://localhost:5206)
-   **Frontend**: React 19 + TypeScript + Vite (UI at http://localhost:5173)
-   **Database**: SQL Server

## Commands

### Running the Application

```bash
# Backend (.NET API)
cd SDIA
dotnet run --project SDIA.API

# Frontend (React)
cd sdia-client
pnpm run dev
```

### Build and Test

```bash
# Backend
cd SDIA
dotnet build
dotnet test

# Frontend
cd sdia-client
pnpm run build
pnpm run lint
pnpm run format
```

### Important Instructions for Claude

-   **ALWAYS USE pnpm** instead of npm or yarn for all frontend operations
-   **ALWAYS** run `pnpm run lint` AND `pnpm run format` in the sdia-client directory after making any changes to TypeScript/React files
-   If linting errors are found, fix them immediately before proceeding
-   The commands to run after any frontend modification:
    ```bash
    cd sdia-client && pnpm run lint && pnpm run format
    ```
-   **ALWAYS** run puppeteer-navigate with the argument "args: ["--start-maximized"]" and with the resolution 1920x1080

## Architecture

### Backend Structure (Clean Architecture + CQRS)

-   **SDIA.API**: REST API controllers, authentication (cookie-based)
-   **SDIA.Application**: CQRS implementation
    -   Services organized by feature (Users, Registrations, FormTemplates)
    -   Each operation has its own folder with Service, Model if needed, Validator if needed
    -   Uses Result pattern (Ardalis.Result) for consistent error handling
    -   Manual input validation (no Fluentvalidation)
    -   Manual mapping methods for DTOs (no AutoMapper)
-   **SDIA.Core**: Domain entities, repository interfaces
-   **SDIA.Infrastructure**: EF Core, repository implementations, external services
-   **SDIA.SharedKernel**: Shared code, enums, base entities

### CQRS Structure in Application Layer

```
SDIA.Application/
├── Users/
│   ├── Management/
│   │   ├── Delete/    # UserManagementDeleteService, UserManagementDeleteValidator
│   │   ├── GetById/   # UserManagementGetByIdService, UserManagementGetByIdValidator, UserManagementGetByIdModel
│   │   ├── Grid/      # UserManagementGridService, UserManagementGridModel
│   │   └── Upsert/    # UserManagementUpsertService, UserManagementUpsertValidator, UserManagementUpsertModel
│   └── Me/
│       ├── Get/       # UserMeGetService, UserMeGetValidator, UserMeGetModel
│       └── UpdateLanguage/ # UserMeUpdateLanguageGetService, UserMeUpdateLanguageGetValidator, UserMeUpdateLanguageGetModel
├── Registrations/
│   └── Management/    # Similar structure as Users
└── FormTemplates/
    └── Management/    # Similar structure as Users
```

### Frontend Structure

-   **src/api**: Axios configuration, API client
-   **src/components**: Reusable UI components
-   **src/contexts**: React contexts (Auth, Theme)
-   **src/pages**: Application pages/routes
-   **src/types**: TypeScript type definitions

## Key Features & Workflows

### Authentication Flow

-   Cookie-based authentication with 8-hour sliding expiration
-   Test users :
    -   Admin: admin@sdia.com / Admin123!
    -   Manager: manager@sdia.com / Manager123!
    -   User: user@sdia.com / User123!

### Registration Workflow

States: Draft → Pending → Validated/Rejected

-   Email/SMS verification required
-   Document upload support
-   Dynamic form templates

## API Documentation

Access interactive API documentation at http://localhost:5206/scalar/v1 when backend is running.

Key endpoints:

-   `/api/auth/*` - Authentication
-   `/api/registrations/*` - Registration management
-   `/api/form-templates/*` - Form template CRUD
-   `/api/users/*` - User management

## Development Notes

-   CORS configured for local development (ports 5173/5174)
-   Serilog logging to console and files (`logs/sdia-*.txt`)
-   Global query filters for soft delete on entities
-   Cookie authentication with SameSite=None for cross-origin requests
-   Result pattern for consistent error handling across all operations
-   Manual mapping methods for DTO transformations (type-safe, no reflection)

## CQRS Usage Examples

### Using Queries in Controllers

```csharp

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetById(Guid id,
        [FromServices] UserManagementGetByIdService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(id, cancellationToken);

        return FtelResult(result);
    }
```

### Grid Queries with Pagination

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] UserManagementUpsertModel model,
  [FromService] UserManagementUpsertService service,
  CancellationToken cancellationToken)
{
    var result = await service.ExecuteAsync(model, cancellationToken);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Errors);
}
```

## 📝 Checklist de Création

Pour créer un nouveau module métier :

### Backend

-   [ ] Créer l'entité dans Core/[Domain]/Entity.cs
-   [ ] Créer l'interface repository dans Core/[Domain]/IEntityRepository.cs
-   [ ] Créer les services dans Application/[Domain]/
-   [ ] Implémenter le repository dans Infrastructure/Repositories/
-   [ ] Créer la configuration EF dans Infrastructure/Configurations/
-   [ ] Créer le controller dans API/Controllers/
-   [ ] Ajouter les migrations EF Core
-   [ ] Créer les tests d'intégration

### Frontend

-   [ ] Créer les types Zod dans domains/[domain]/types.ts
-   [ ] Créer les queries dans domains/[domain]/queries.ts
-   [ ] Créer les mutations dans domains/[domain]/mutations.ts
-   [ ] Créer le composant manager dans components/features/[Domain]/
-   [ ] Ajouter les routes dans le router
-   [ ] Ajouter les traductions i18n
-   [ ] Créer les tests unitaires

## Configuration du Puppeteer MCP

-   Lance toujours le navigateur avec l'argument "args: ["--start-maximized"]" et en 1920x1080
