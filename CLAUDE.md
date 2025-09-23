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

### Backend Structure (Clean Architecture + Services Pattern)

-   **SDIA.API**: REST API controllers, authentication (cookie-based)
-   **SDIA.Application**: Services pattern implementation (NO MediatR, NO FluentValidation)
    -   Services organized by feature (Users, Registrations, FormTemplates)
    -   Each operation has its own folder with Service, Validator, and Model
    -   Uses Result pattern (Ardalis.Result) for consistent error handling
    -   Separate Validator classes for input validation (NO FluentValidation)
    -   Validation extensions in _Extensions for common validation patterns
    -   Manual mapping methods for DTOs (NO AutoMapper)
-   **SDIA.Core**: Domain entities, repository interfaces
-   **SDIA.Infrastructure**: EF Core, repository implementations, external services
-   **SDIA.SharedKernel**: Shared code, enums, base entities

### Services Structure in Application Layer

```
SDIA.Application/
├── _Abstractions/     # IValidator interface
├── _Extensions/       # ValidationExtensions with common validation helpers
├── Auth/
│   ├── ForgotPassword/    # AuthForgotPasswordService, AuthForgotPasswordValidator, AuthForgotPasswordModel
│   ├── Login/             # AuthLoginService, AuthLoginValidator, AuthLoginModel
│   ├── Register/          # AuthRegisterService, AuthRegisterValidator, AuthRegisterModel
│   ├── ResetPassword/     # AuthResetPasswordService, AuthResetPasswordValidator, AuthResetPasswordModel
│   └── ValidateResetToken/ # AuthValidateResetTokenService, AuthValidateResetTokenModel
├── PublicAccess/
│   ├── CheckStatus/       # PublicAccessCheckStatusService, PublicAccessCheckStatusModel
│   ├── ResendVerification/ # PublicAccessResendVerificationService, PublicAccessResendVerificationModel
│   ├── VerifyEmail/       # PublicAccessVerifyEmailService, PublicAccessVerifyEmailModel
│   └── VerifyPhone/       # PublicAccessVerifyPhoneService, PublicAccessVerifyPhoneModel
├── Users/
│   ├── Management/
│   │   ├── Delete/        # UserManagementDeleteService
│   │   ├── Export/        # UserManagementExportService, UserManagementExportModel
│   │   ├── GetById/       # UserManagementGetByIdService, UserManagementGetByIdModel
│   │   ├── Grid/          # UserManagementGridService, UserManagementGridModel, UserManagementGridQuery
│   │   ├── ResetPassword/ # UserManagementResetPasswordService, UserManagementResetPasswordModel
│   │   ├── Statistics/    # UserManagementStatisticsService, UserManagementStatisticsModel
│   │   ├── ToggleStatus/  # UserManagementToggleStatusService, UserManagementToggleStatusModel
│   │   └── Upsert/        # UserManagementUpsertService, UserManagementUpsertValidator, UserManagementUpsertModel
│   └── Me/
│       ├── Get/           # UserMeGetService, UserMeGetModel
│       └── UpdateLanguage/ # UserMeUpdateLanguageService, UserMeUpdateLanguageModel
├── Registrations/
│   ├── Management/
│   │   ├── AddComment/    # RegistrationManagementAddCommentService, RegistrationManagementAddCommentValidator, RegistrationManagementAddCommentModel
│   │   ├── Assign/        # RegistrationManagementAssignService, RegistrationManagementAssignValidator, RegistrationManagementAssignModel
│   │   ├── Delete/        # RegistrationManagementDeleteService
│   │   ├── GetAll/        # RegistrationManagementGetAllService, RegistrationManagementGetAllModel, RegistrationManagementGetAllQuery
│   │   ├── GetById/       # RegistrationManagementGetByIdService, RegistrationManagementGetByIdModel
│   │   ├── Grid/          # RegistrationManagementGridService, RegistrationManagementGridModel, RegistrationManagementGridQuery
│   │   ├── Search/        # RegistrationManagementSearchService, RegistrationManagementSearchModel
│   │   ├── SendVerification/ # RegistrationManagementSendVerificationService, RegistrationManagementSendVerificationModel
│   │   ├── Submit/        # RegistrationManagementSubmitService, RegistrationManagementSubmitModel
│   │   ├── Upsert/        # RegistrationManagementUpsertService, RegistrationManagementUpsertValidator, RegistrationManagementUpsertModel
│   │   └── Validate/      # RegistrationManagementValidateService, RegistrationManagementValidateValidator, RegistrationManagementValidateModel
│   └── Public/
│       ├── GetByIdentifier/ # RegistrationPublicGetByIdentifierService, RegistrationPublicGetByIdentifierModel
│       ├── Submit/        # RegistrationPublicSubmitService, RegistrationPublicSubmitValidator, RegistrationPublicSubmitModel
│       └── VerifyCode/    # RegistrationPublicVerifyCodeService, RegistrationPublicVerifyCodeValidator, RegistrationPublicVerifyCodeModel
└── FormTemplates/
    └── Management/
        ├── Delete/        # FormTemplateManagementDeleteService
        ├── GetAll/        # FormTemplateManagementGetAllService, FormTemplateManagementGetAllModel, FormTemplateManagementGetAllQuery
        ├── GetById/       # FormTemplateManagementGetByIdService, FormTemplateManagementGetByIdModel
        ├── Grid/          # FormTemplateManagementGridService, FormTemplateManagementGridModel, FormTemplateManagementGridQuery
        └── Upsert/        # FormTemplateManagementUpsertService, FormTemplateManagementUpsertValidator, FormTemplateManagementUpsertModel
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
-   **NO MediatR** - Use services directly with dependency injection
-   **NO FluentValidation** - Use separate Validator classes with manual validation
-   **NO AutoMapper** - Use manual mapping methods
-   **Services Pattern** - One service per operation for maximum separation of concerns
-   **[FromServices] Attribute** - Controllers use services via method injection, not constructor injection
-   **Auto-registration** - Services are automatically registered via convention-based DI

## Services Usage Examples

### Using Services in Controllers with [FromServices]

```csharp
[HttpGet("{id:Guid}")]
public async Task<IActionResult> GetById(
    Guid id,
    [FromServices] UserManagementGetByIdService service,
    CancellationToken cancellationToken)
{
    var result = await service.ExecuteAsync(id, cancellationToken);

    if (!result.IsSuccess)
    {
        if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            return NotFound(new { message = result.Errors.FirstOrDefault() });
        return BadRequest(result.Errors);
    }

    return Ok(result.Value);
}
```

### Creating/Updating with Services

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(
    [FromBody] UserManagementUpsertModel model,
    [FromServices] UserManagementUpsertService service,
    CancellationToken cancellationToken)
{
    var result = await service.ExecuteAsync(model, cancellationToken);

    if (!result.IsSuccess)
    {
        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
            return BadRequest(result.ValidationErrors);
        return BadRequest(result.Errors);
    }

    return Ok(result.Value);
}
```

### Validation Pattern with Separate Validator Classes

```csharp
// Validator class
public class UserManagementUpsertValidator : IValidator
{
    private readonly IUserRepository _userRepository;

    public UserManagementUpsertValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> ValidateAsync(UserManagementUpsertModel model, CancellationToken cancellationToken, Guid? id = null)
    {
        var validationErrors = new List<ValidationError>();

        // Using validation extensions
        validationErrors.AddErrorIfNullOrWhiteSpace(model.Email, nameof(model.Email), "Email is required");
        validationErrors.AddErrorIfNotEmail(model.Email, nameof(model.Email), "Invalid email format");
        validationErrors.AddErrorIfExceedsLength(model.FirstName, 100, nameof(model.FirstName), "First name must not exceed 100 characters");

        // Custom validation logic
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            var existingUser = await _userRepository.GetByEmailAsync(model.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != id)
            {
                validationErrors.Add(new ValidationError {
                    Identifier = nameof(model.Email),
                    ErrorMessage = "Email already exists"
                });
            }
        }

        return validationErrors.Any() ? Result.Invalid(validationErrors) : Result.Success();
    }
}

// Service class using the validator
public class UserManagementUpsertService
{
    private readonly UserManagementUpsertValidator _validator;
    private readonly IUserRepository _userRepository;

    public UserManagementUpsertService(
        UserManagementUpsertValidator validator,
        IUserRepository userRepository)
    {
        _validator = validator;
        _userRepository = userRepository;
    }

    public async Task<Result<UserManagementUpsertResult>> ExecuteAsync(UserManagementUpsertModel model, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(model, cancellationToken, model.Id);
        if (!validationResult.IsSuccess)
        {
            return Result<UserManagementUpsertResult>.Invalid(validationResult.ValidationErrors);
        }

        // Business logic here...
    }
}
```

## 📝 Checklist de Création

Pour créer un nouveau module métier :

### Backend

-   [ ] Créer l'entité dans Core/[Domain]/Entity.cs
-   [ ] Créer l'interface repository dans Core/[Domain]/IEntityRepository.cs
-   [ ] Créer les services et validators dans Application/[Domain]/
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

### Important Instructions for Claude

-   **ALWAYS USE pnpm** instead of npm or yarn for all frontend operations
-   If you can, **ALWAYS** implement features on the backend so that computations are handled by the server rather than the clien

