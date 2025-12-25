# Migrations

This folder contains Entity Framework Core migrations for the database schema.

## Creating Migrations

After installing .NET 8 SDK, run the following command from the `src/AIProjectManager.API` directory:

```bash
dotnet ef migrations add InitialCreate --project ../AIProjectManager.Infrastructure --startup-project .
```

## Applying Migrations

Migrations are automatically applied when the application starts (in development mode). 

To apply migrations manually:

```bash
dotnet ef database update --project ../AIProjectManager.Infrastructure --startup-project .
```

## Migration Naming Convention

- Use descriptive names: `AddUserTable`, `AddProjectStatusIndex`, etc.
- Use PascalCase for migration names
- Be specific about what the migration does

