# AppStarterKit - Template de Structure de Projet

Ce document sert de référence pour Claude afin de recréer automatiquement cette architecture sur un nouveau projet.

## 🎯 Vue d'ensemble de l'Architecture

**Stack Technique :**

-   Backend : .NET 9 avec Clean Architecture
-   Frontend : React 19 + Vite + TypeScript
-   Base de données : SQL Server avec EF Core 8
-   Tests : xUnit + Selenium + Integration Tests avec TestContainers
-   CI/CD : GitLab CI + Docker

## 📁 Structure Complète du Projet

```
app-starter-kit/
├── client/                          # Application Frontend React
│   ├── public/
│   ├── scripts/
│   │   └── routes-generator.ts      # Génération automatique des routes API
│   ├── src/
│   │   ├── api-routes.gen.ts        # Routes API générées
│   │   ├── assets/
│   │   ├── components/
│   │   │   ├── common/               # Composants réutilisables
│   │   │   └── features/             # Composants par domaine métier
│   │   ├── configs/                  # Configuration app (API, theme, etc.)
│   │   ├── constants/
│   │   ├── contexts/                 # React Contexts (Auth, Theme, etc.)
│   │   ├── domains/                  # Logique métier par domaine
│   │   │   └── [domain]/
│   │   │       ├── types.ts         # Schemas Zod + Types TypeScript
│   │   │       ├── queries.ts       # React Query hooks (GET)
│   │   │       ├── mutations.ts     # React Query hooks (POST/PUT/DELETE)
│   │   │       └── templates.ts     # URL templates
│   │   ├── enums/
│   │   ├── formatters/
│   │   ├── helpers/
│   │   ├── types/
│   │   └── Entrypoint.tsx
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
│
└── server/
    └── AppStarterKit/
        ├── AppStarterKit.sln
        │
        ├── AppStarterKit.SharedKernel/     # Code partagé entre projets
        │   ├── Attributes/
        │   ├── Constants/
        │   ├── Extensions/
        │   ├── Helpers/
        │   ├── Interfaces/
        │   └── Models/
        │
        ├── AppStarterKit.Core/             # Domaine métier (Entities)
        │   ├── _Abstractions/
        │   ├── _Attributes/
        │   ├── _Enums/
        │   ├── _Exceptions/
        │   ├── _Services/
        │   └── [DomainName]/               # Par domaine métier
        │       ├── Entity.cs
        │       └── IEntityRepository.cs
        │
        ├── AppStarterKit.Application/      # Logique applicative (CQRS)
        │   ├── _Abstractions/
        │   ├── _Constants/
        │   ├── _Extensions/
        │   ├── _Factories/
        │   ├── _Settings/
        │   └── [DomainName]/
        │       ├── Management/
        │       │   ├── Grid/
        │       │   │   ├── EntityManagementGridService.cs
        │       │   │   └── EntityManagementGridModel.cs
        │       │   ├── Upsert/
        │       │   │   ├── EntityManagementUpsertService.cs
        │       │   │   ├── EntityManagementUpsertValidator.cs
        │       │   │   └── EntityManagementUpsertModel.cs
        │       │   └── Delete/
        │       │       └── EntityManagementDeleteService.cs
        │       └── [UseCase]/
        │
        ├── AppStarterKit.Infrastructure/   # Implémentation technique
        │   ├── AppDbContext.cs
        │   ├── Configurations/              # EF Core configurations
        │   ├── Migrations/
        │   ├── Repositories/                # Implémentation des repos
        │   ├── Seeds/
        │   ├── UnitOfWork/
        │   ├── Mails/
        │   └── SMS/
        │
        ├── AppStarterKit.API/               # API REST
        │   ├── ActionFilters/
        │   ├── Controllers/
        │   │   └── EntityController.cs      # Controllers partiels par feature
        │   ├── ExceptionHandlers/
        │   ├── Extensions/
        │   ├── Helpers/
        │   ├── ModelBinders/
        │   ├── Models/
        │   ├── Program.cs
        │   └── appsettings.json
        │
        ├── AppStarterKit.IntegrationTests/
        │   ├── Builders/
        │   ├── Constants/
        │   └── Services/
        │
        └── AppStarterKit.UnitTests/
            └── Extensions/
```

## 🔧 Configuration Backend (.NET 9)

### Dépendances Principales (NuGet)

```xml
<!-- AppStarterKit.API -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.*" />
<PackageReference Include="Scalar.AspNetCore" Version="2.*" />
<PackageReference Include="Serilog" Version="3.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.*" />

<!-- AppStarterKit.Application -->
<PackageReference Include="Ardalis.Result" Version="*" />

<!-- AppStarterKit.Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />

<!-- Tests -->
<PackageReference Include="xunit" Version="*" />
<PackageReference Include="Testcontainers.MsSql" Version="*" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="*" />
```

### Configuration Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// CORS pour développement local
if (isLocal)
{
    builder.Services.AddCors(options => options.AddPolicy("AllowLocalhostPolicy",
        policy => policy.WithOrigins("https://localhost:5173")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()));
}

// Base de données
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Authentication Cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.HttpOnly = true;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opt.Cookie.SameSite = SameSiteMode.None;
        opt.ExpireTimeSpan = TimeSpan.FromHours(18);
        opt.SlidingExpiration = true;
    });

// Services par couche
builder.Services
    .AddApplication()    // Services Application
    .AddInfrastructure() // Services Infrastructure
    .AddSettings();      // Configuration Settings

// OpenAPI/Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware et configuration...
```

## 🎨 Configuration Frontend (React + Vite)

### Dépendances Principales (package.json)

```json
{
    "dependencies": {
        "@emotion/react": "^11",
        "@emotion/styled": "^11",
        "@hookform/resolvers": "^5",
        "@mui/material": "^7",
        "@mui/x-data-grid": "^8",
        "@tanstack/react-query": "^5",
        "axios": "^1",
        "i18next": "^25",
        "react": "^19",
        "react-dom": "^19",
        "react-hook-form": "^7",
        "react-i18next": "^15",
        "react-router-dom": "^7",
        "zod": "^3"
    },
    "devDependencies": {
        "@types/react": "^19",
        "@typescript-eslint/eslint-plugin": "^8",
        "@vitejs/plugin-react": "^4",
        "eslint": "^9",
        "typescript": "^5",
        "vite": "^6"
    }
}
```

### Configuration Vite (vite.config.ts)

```typescript
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            "@": "/src",
        },
    },
    server: {
        port: 5173,
        https: true,
    },
});
```

## 🏗️ Patterns et Conventions de Code

### Backend - Clean Architecture

#### 1. Entité du Domaine (Core)

```csharp
[Entity]
public class User : EntityWithId
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public virtual Role Role { get; set; }
    public virtual ICollection<UserNotice> UserNotices { get; set; }

    // Méthodes métier
    public void UpdatePassword(string password) { }
    public bool IsPasswordCorrect(string givenPassword) { }
}
```

#### 2. Repository Interface (Core)

```csharp
public interface IUserRepository : ICRUDEntityWithIdRepository<User>
{
    Task<User> GetOrDefaultAsync(string email, CancellationToken cancellationToken);
    IQueryable<User> GetWithSupervisorFilter(bool isSupervisor, bool asNoTracking);
}
```

#### 3. Service Application (Application)

```csharp
public class UserManagementGridService(IUserRepository userRepository)
{
    public async Task<Result<PaginatedEnumerable<UserManagementGridModel>>> ExecuteAsync(
        QueryOptionsModel queryOptions,
        CancellationToken cancellationToken)
    {
        return await userRepository
            .GetAll()
            .Select(UserManagementGridModel.FromUser)
            .Apply(queryOptions.Filters)
            .Apply(queryOptions.Search)
            .Apply(queryOptions.Sort)
            .PaginateAsync(queryOptions.Pagination);
    }
}
```

#### 4. Controller API

```csharp
[Route("api/users")]
[ApiController]
public partial class UsersController : FTELBaseController
{
    private readonly UserManagementGridService _gridService;

    [HttpGet("management")]
    public async Task<IActionResult> GetUsersGrid(
        [FromQuery] QueryOptionsModel queryOptions)
    {
        var result = await _gridService.ExecuteAsync(queryOptions, CancellationToken);
        return FtelResult(result);
    }
}
```

### Frontend - React avec TypeScript

#### 1. Types avec Zod (domains/users/types.ts)

```typescript
export const UserSchema = () =>
    z.object({
        id: z.string().uuid(),
        firstName: z.string().min(3),
        lastName: z.string().min(3),
        email: z.string().email(),
        role: RoleSchema().nullable(),
    });

export type User = z.infer<ReturnType<typeof UserSchema>>;
```

#### 2. Queries React Query (domains/users/queries.ts)

```typescript
export function useUsersQuery(
    params?: object,
    options?: UseQueryOptions,
    filtersPaginationAndSort?: FilterPaginationAndSortType
) {
    return useQuery({
        queryKey: ["users", params, filtersPaginationAndSort],
        queryFn: () => api.get("/api/users", { params }),
        ...options,
    });
}
```

#### 3. Mutations (domains/users/mutations.ts)

```typescript
export function useCreateUserMutation(options?: UseMutationOptions) {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: User) => api.post("/api/users", data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["users"] });
        },
        ...options,
    });
}
```

#### 4. Composant Feature (components/features/Users/index.tsx)

```typescript
export function UsersManager() {
    const { data, isLoading } = useUsersQuery();
    const createMutation = useCreateUserMutation();

    return (
        <DataGrid
            id="users-grid"
            rows={data?.items ?? []}
            columns={columns}
            loading={isLoading}
            onRowClick={handleRowClick}
        />
    );
}
```

## 🧪 Structure des Tests

### Tests d'Intégration avec Containers

```csharp
[Collection("UserServiceTests")]
public class UserServiceTests : BaseIntegrationTest<UserServiceTests>
{
    public UserServiceTests(DatabaseFixture<UserServiceTests> databaseFixture)
        : base(databaseFixture) { }

    [Fact]
    public async Task CreateUser_Should_ReturnSuccess()
    {
        // Arrange
        var user = new UserBuilder().Build();

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users", user);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }
}
```

## 🚀 Commandes de Génération Automatique

Pour que Claude puisse recréer cette structure automatiquement :

### Backend

```bash
# Créer la solution
dotnet new sln -n AppStarterKit

# Créer les projets
dotnet new classlib -n AppStarterKit.SharedKernel
dotnet new classlib -n AppStarterKit.Core
dotnet new classlib -n AppStarterKit.Application
dotnet new classlib -n AppStarterKit.Infrastructure
dotnet new webapi -n AppStarterKit.API
dotnet new xunit -n AppStarterKit.UnitTests
dotnet new xunit -n AppStarterKit.IntegrationTests

# Ajouter les projets à la solution
dotnet sln add **/*.csproj

# Ajouter les références entre projets
dotnet add AppStarterKit.Core reference AppStarterKit.SharedKernel
dotnet add AppStarterKit.Application reference AppStarterKit.Core
dotnet add AppStarterKit.Infrastructure reference AppStarterKit.Core
dotnet add AppStarterKit.API reference AppStarterKit.Application AppStarterKit.Infrastructure
```

### Frontend

```bash
# Créer le projet Vite
npm create vite@latest client -- --template react-ts

# Installer les dépendances
cd client
npm install @mui/material @emotion/react @emotion/styled
npm install @tanstack/react-query react-hook-form zod
npm install axios react-router-dom i18next
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

## 🔐 Sécurité et Bonnes Pratiques

1. **Authentification** : Cookie-based avec expiration sliding
2. **Autorisation** : Basée sur les rôles et permissions
3. **Validation** : Double validation (Frontend avec Zod, Backend avec Ardalis.Result)
4. **Logging** : Serilog avec différents niveaux et sinks
5. **Error Handling** : GlobalExceptionHandler + React Error Boundaries
6. **CORS** : Configuration stricte par environnement
7. **Secrets** : Utilisation d'appsettings par environnement
8. **SQL Injection** : Protection via EF Core et paramètres
9. **XSS** : Protection via React et Content Security Policy
10. **Rate Limiting** : À implémenter selon les besoins

Ce template contient toutes les informations nécessaires pour que Claude puisse recréer automatiquement cette architecture sur un nouveau projet.
