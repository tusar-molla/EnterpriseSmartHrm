# Enterprise Smart HRM Database

The API uses SQL Server with Dapper. Database changes are stored as ordered,
versioned SQL scripts so they can be reviewed and applied consistently.

## First-time setup

Run the scripts in this order using SQL Server Management Studio (SSMS),
Azure Data Studio, or `sqlcmd`:

1. `bootstrap/001_CreateDatabase.sql`
2. `migrations/001_CreateAuthSchema.sql`
3. `migrations/002_SeedRolesAndPermissions.sql`
4. `migrations/003_SeedRolePermissions.sql`

The migration scripts are idempotent. Applied migrations are recorded in
`dbo.DatabaseMigrations`.

## Authentication schema

The first migration creates:

- `auth.Users`
- `auth.Roles`
- `auth.Permissions`
- `auth.UserRoles`
- `auth.RolePermissions`
- `auth.RefreshTokens`
- `auth.LoginHistories`

`auth.Users.EmployeeId` is intentionally nullable and does not yet have a
foreign key. The foreign key will be added after the Employee module tables
are created.

## Initial administrator

The database scripts do not contain a default password. A secure initial-admin
bootstrap flow will be added after the password hashing service is implemented.
This avoids storing a reusable password or an incompatible password hash in Git.

## Connection

The API currently expects:

```text
Server=.\SQLEXPRESS;Database=EnterpriseSmartHrmDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

Change the `DefaultConnection` value in the API configuration when using a
different SQL Server instance.
