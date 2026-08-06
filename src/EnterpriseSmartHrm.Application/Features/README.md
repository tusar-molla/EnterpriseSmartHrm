# Features — Vertical Slices

Every feature lives in **one folder**. You should never have to jump across
`Commands/`, `Queries/`, `Validators/`, `Handlers/` to build or read a single use case.

## The rule

```
Features/
  <Module>/            e.g. Employees, Attendance, Leave, Payroll
    <UseCase>/         e.g. CreateEmployee, GetEmployeeById
      <UseCase>Command.cs   (or <UseCase>Query.cs)  -> the request + its response DTO
      <UseCase>Handler.cs   -> IRequestHandler, talks to the DB via IDbConnectionFactory (Dapper)
      <UseCase>Validator.cs -> FluentValidation rules (only if the request needs validation)
```

- **Commands** change state (Create/Update/Delete/Approve). **Queries** only read.
- Handlers return `Result` / `Result<T>` (see `Common/Models/Result.cs`).
- The controller stays tiny — it just does `FromResult(await mediator.Send(cmd))`
  (see `Api/Controllers/Common/BaseApiController.cs`).
- Validation, logging, and exception handling are already wired as MediatR pipeline
  behaviors (`Common/Behaviors/`), so handlers only contain business logic.

## Minimal example (Create Employee)

`Features/Employees/CreateEmployee/CreateEmployeeCommand.cs`
```csharp
using MediatR;
using EnterpriseSmartHrm.Application.Common.Models;

namespace EnterpriseSmartHrm.Application.Features.Employees.CreateEmployee;

public record CreateEmployeeCommand(string FirstName, string LastName, string Email)
    : IRequest<Result<int>>;
```

`Features/Employees/CreateEmployee/CreateEmployeeHandler.cs`
```csharp
using Dapper;
using MediatR;
using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Application.Common.Models;

namespace EnterpriseSmartHrm.Application.Features.Employees.CreateEmployee;

public class CreateEmployeeHandler(IDbConnectionFactory db)
    : IRequestHandler<CreateEmployeeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        using var connection = await db.CreateOpenConnectionAsync(ct);

        const string sql = """
            INSERT INTO Employees (FirstName, LastName, Email)
            OUTPUT INSERTED.Id
            VALUES (@FirstName, @LastName, @Email);
            """;

        var id = await connection.ExecuteScalarAsync<int>(sql, request);
        return Result<int>.Success(id, "Employee created.");
    }
}
```

`Features/Employees/CreateEmployee/CreateEmployeeValidator.cs`
```csharp
using FluentValidation;

namespace EnterpriseSmartHrm.Application.Features.Employees.CreateEmployee;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

That's the whole pattern. MediatR and FluentValidation are auto-registered by assembly
scan (`DependencyInjection/ApplicationServiceRegistration.cs`), so **new slices need no
DI wiring** — just drop the folder in and add a 3-line controller action.

## Build order (from the project doc, Section 12)

1. **Employees** — CreateEmployee, GetEmployeeById, ListEmployees, UpdateEmployee
2. **Organization** — Departments, Designations, Shifts, Locations, Holidays
3. **Attendance** — CheckIn, CheckOut, ManualAttendance, MonthlySummary
4. **Leave** — LeaveTypes, ApplyLeave, ApproveLeave, LeaveBalance
5. **Payroll** — SalaryComponents, SalaryStructure, GeneratePayroll, Payslip
6. **Reports & Dashboard**

Build **one full slice end-to-end** (Employee CreateEmployee) before anything else —
once it works, every other feature is the same shape.
