# Features — Vertical Slices

Every use case lives in **one folder**. You should never have to jump across
`Commands/`, `Queries/`, `Validators/`, `Handlers/` to read a single use case.

## The rule

```
Features/
  <Module>/                     e.g. Employees, Attendance, Leave, Payroll
    Interfaces/                 interfaces for the whole module
      I<Entity>Repository.cs    data access contract  (implemented in Infrastructure)
      I<Thing>Service.cs        service contract, plus any small return types it needs
    <UseCase>/                  e.g. CreateEmployee, GetEmployeeById, ApproveLeave
      <UseCase>Command.cs       the request + its response DTO   (or <UseCase>Query.cs)
      <UseCase>Handler.cs       IRequestHandler — business logic
      <UseCase>Validator.cs     FluentValidation rules (only if the request takes input)
```

- **Commands** change state (Create/Update/Approve/Finalize). **Queries** only read.
- Handlers return `Result` / `Result<T>` — see `Common/Models/Result.cs`. Throw only
  for unexpected system failures, never for validation or business rejections.
- **Handlers never touch Dapper, SQL, or a connection.** They depend on the interfaces
  in `Interfaces/`. All SQL lives in `Infrastructure/Repositories/`. This keeps the
  Application project free of infrastructure and keeps handlers unit-testable with a mock.
- Request/response DTOs live **inside the slice** that owns them. Only genuinely shared
  shapes (`ApiResponse<T>`, `PagedResponse<T>`, `PaginationQuery`) live in `Common/Models/`.
- Controllers stay tiny: `FromResult(await _sender.Send(command, ct))`.
- Validation, logging and exception handling are already wired as MediatR pipeline
  behaviors (`Common/Pipeline/`), so handlers contain business logic only.

**`Features/Authentication/` is the reference implementation.** Copy its shape.

## Adding a module — the 6 files

| # | File | Project |
|---|------|---------|
| 1 | `<Module>/<Entity>.cs` | Domain |
| 2 | `Features/<Module>/Interfaces/I<Entity>Repository.cs` | Application |
| 3 | `Features/<Module>/<UseCase>/<UseCase>Command.cs` | Application |
| 4 | `Features/<Module>/<UseCase>/<UseCase>Handler.cs` (+ `Validator.cs`) | Application |
| 5 | `Repositories/<Entity>Repository.cs` — **the only place SQL lives** | Infrastructure |
| 6 | `Controllers/<Module>Controller.cs` | Api |

Then register the repository in `Infrastructure/DependencyInjection.cs`.
MediatR handlers and FluentValidation validators are picked up by assembly scan, so
**the slice itself needs no DI wiring** — only the repository does.

## Minimal example (Create Employee)

`Features/Employees/Interfaces/IEmployeeRepository.cs`
```csharp
namespace EnterpriseSmartHrm.Application.Features.Employees.Interfaces;

public interface IEmployeeRepository
{
    Task<int> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
```

`Features/Employees/CreateEmployee/CreateEmployeeCommand.cs`
```csharp
public sealed record CreateEmployeeCommand(string FirstName, string LastName, string Email)
    : IRequest<Result<int>>;
```

`Features/Employees/CreateEmployee/CreateEmployeeHandler.cs`
```csharp
public sealed class CreateEmployeeHandler(IEmployeeRepository employees)
    : IRequestHandler<CreateEmployeeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        if (await employees.EmailExistsAsync(request.Email, ct))
        {
            return Result<int>.Failure("An employee with this email already exists.");
        }

        var id = await employees.CreateAsync(new Employee { /* map */ }, ct);

        return Result<int>.Success(id, "Employee created.");
    }
}
```

`Features/Employees/CreateEmployee/CreateEmployeeValidator.cs`
```csharp
public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

That's the whole pattern. Every other feature is the same shape.

## Build order

1. **Employees** — CreateEmployee, GetEmployeeById, ListEmployees, UpdateEmployee, UpdateStatus
2. **Organization** — Departments, Designations, Locations, Shifts, Holidays
3. **Attendance** — CheckIn, CheckOut, ManualAttendance, MonthlySummary, CorrectionRequest
4. **Leave** — LeaveTypes, ApplyLeave, ApproveLeave, LeaveBalance, Calendar
5. **Payroll** — SalaryComponents, SalaryStructure, GeneratePayroll, Finalize, Payslip
6. **Reports & Dashboard**

Build **one full slice end-to-end** (`Employees/CreateEmployee`) before starting anything
else — once that works, the rest is repetition.
