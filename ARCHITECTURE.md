# Enterprise Smart HRM — Architecture

.NET 10 Web API, Clean Architecture, CQRS via MediatR, Dapper on SQL Server, JWT auth.
The full delivery roadmap lives in `EnterpriseSmartHRM_API_CleanArchitecture_ImplementationPlan.docx`.
This file records only what the code actually does, so it stays true.

## Dependency direction

```
Api  ->  Application  ->  Domain
  \          ^
   \         |
    ->  Infrastructure
```

| Project | Contains | Must never reference |
|---|---|---|
| **Domain** | Entities, enums, domain rules | Anything. No project or package references at all. |
| **Application** | CQRS slices, MediatR handlers, validators, repository *interfaces*, `Result`, permission constants | Infrastructure, Api, Dapper, SQL, ASP.NET Core |
| **Infrastructure** | Dapper repositories, SQL, JWT, hashing, audit writer, connection factory | Api |
| **Api** | Controllers, middleware, DI composition root, authorization policies | — (but holds **no business logic**) |

Enforced at review time by the checklist at the bottom of this file.

## Folder map

```
EnterpriseSmartHrm/
  Directory.Build.props          TargetFramework/Nullable/ImplicitUsings for every project
  Directory.Packages.props       central package versions + transitive pins
  database/
    bootstrap/                   create-database script
    migrations/                  numbered, forward-only SQL
  src/
    EnterpriseSmartHrm.Domain/
      Common/                    BaseEntity, AuditableEntity, SoftDeletableEntity
      Authentication/            User, Role, Permission, RolePermission, RefreshToken, LoginHistory
    EnterpriseSmartHrm.Application/
      Common/
        Interfaces/              ICurrentUserService, IDbConnectionFactory, IDateTimeProvider, IAuditLogService
        Pipeline/                MediatR validation / logging / unhandled-exception behaviors
        Exceptions/              AppException, NotFoundException
        Models/                  Result, ApiResponse, PagedResponse, PaginationQuery, AuditLogEntry
        Security/                PermissionConstants, RoleConstants, JwtSettings, ClaimConstants
      Features/                  <- all use cases live here, see Features/README.md
        Authentication/
          Interfaces/            IUserRepository, ITokenService, IPasswordHasher, ...
          Login/                 LoginCommand, LoginHandler, LoginValidator
      DependencyInjection.cs     AddApplicationServices()
    EnterpriseSmartHrm.Infrastructure/
      Database/                  SqlServerConnectionFactory
      Repositories/              ALL SQL lives here. One file per entity.
      Services/                  JwtTokenService, Pbkdf2PasswordHasher, AuditLogService, SystemDateTimeProvider
      DependencyInjection.cs     AddInfrastructureServices()
    EnterpriseSmartHrm.Api/
      Controllers/               one controller per module + BaseApiController
      Authorization/             HasPermissionAttribute, PermissionAuthorizationHandler, PermissionRequirement
      Extensions/                AddJwtAuthentication()
      Middleware/                ExceptionHandlingMiddleware
      Services/                  CurrentUserService (reads the HTTP context)
      Program.cs
```

**Rule of thumb: no folder exists until it holds a file.** Create `Features/Payroll/`
on the day you write the first payroll handler, not before.

## Request flow

```
Controller  ->  ISender.Send(command)  ->  MediatR pipeline (validate, log)
            ->  Handler in Application ->  I<Entity>Repository
            ->  Repository in Infrastructure  ->  Dapper  ->  SQL Server
```

Controllers call MediatR only. The single exception is `/health`, mapped directly in
`Program.cs`.

## Conventions

| Thing | Pattern | Example |
|---|---|---|
| Command | `<Verb><Entity>Command` | `CreateEmployeeCommand` |
| Query | `Get<Thing>Query` | `GetEmployeeByIdQuery` |
| Handler | `<Request>Handler` | `CreateEmployeeHandler` |
| Validator | `<Request>Validator` | `CreateEmployeeValidator` |
| Repository | `I<Entity>Repository` / `<Entity>Repository` | `IEmployeeRepository` |
| Controller | `<Module>Controller` | `EmployeesController` |
| Permission | `Module.Action` | `Payroll.Finalize` |
| Route | `api/v{version:apiVersion}/[controller]` | `api/v1/employees` |

- Every response is wrapped in `ApiResponse<T>`; lists use `PagedResponse<T>`.
- Handlers return `Result` / `Result<T>`. `BaseApiController.FromResult` maps
  `ResultStatus` to the HTTP status code — do not hand-write status codes in controllers.
- Master and transaction tables carry `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/IsDeleted/IsActive`.
  Attendance, leave, payroll and audit rows are **never** physically deleted.
- Audit critical actions through `IAuditLogService`: login, employee create/update/status,
  manual attendance, leave approve/reject, salary change, payroll generate/finalize/pay,
  role/permission change, setting change, document upload/delete.

## Deliberately not here yet

Add these when there is something to put in them, not before:

- **`EnterpriseSmartHrm.Contracts` project.** The plan doc calls for one. Shared DTOs
  currently live in `Application/Common/Models/` and inside each slice. A separate
  assembly only pays off if a **.NET** client (Blazor/MAUI) shares it — a JS/React
  frontend consumes JSON through Swagger and gains nothing.
- **`tests/`.** The plan calls for `Application.Tests` and `Api.IntegrationTests`.
  Add `Application.Tests` alongside the first Employees slice — the handler-with-mock-repository
  pattern is what makes it worth having.
- **Background jobs, email, file storage.** Phase 7. `IAuditLogService` is the model to
  follow: interface in `Application/Common/Interfaces/`, implementation in `Infrastructure/Services/`.
- **Interfaces with no implementation.** An interface that nothing implements and nothing
  registers is a footgun: injecting it compiles fine, then fails at runtime with "no service
  registered for IFoo". A missing interface fails at compile time instead. Write
  `I<Entity>Repository` in the same commit as its Dapper implementation and its DI line,
  never ahead of them. `IRoleRepository`/`IPermissionRepository` were deleted for this reason
  and get rewritten when the Roles module is actually built.

## Definition of done, per module

- [ ] Endpoints implemented and documented in Swagger
- [ ] FluentValidation on every command that takes user input
- [ ] Permission constant added and enforced with `[HasPermission]`
- [ ] Audit log written for critical actions
- [ ] No business logic in the controller
- [ ] No Application class referencing Infrastructure
- [ ] No Domain class referencing Application, Infrastructure, or Api
- [ ] `dotnet build` clean — 0 warnings
