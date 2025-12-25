# Migration Instructions

## Prerequisites

1. **Install .NET 8 SDK:**
   ```bash
   # Option 1: Using Homebrew (recommended)
   brew install --cask dotnet-sdk
   
   # Option 2: Download from Microsoft
   # Visit: https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. **Verify .NET 8 SDK installation:**
   ```bash
   dotnet --version
   # Should show: 8.x.x (not 6.x.x)
   ```

3. **Install EF Core Tools:**
   ```bash
   dotnet tool install --global dotnet-ef
   # Or update if already installed:
   dotnet tool update --global dotnet-ef
   ```

## Creating and Running Migrations

### Step 1: Navigate to API Project
```bash
cd src/AIProjectManager.API
```

### Step 2: Create Initial Migration
```bash
dotnet ef migrations add InitialCreate --project ../AIProjectManager.Infrastructure --startup-project .
```

This will create migration files in: `src/AIProjectManager.Infrastructure/Migrations/`

### Step 3: Apply Migration to Database
```bash
dotnet ef database update --project ../AIProjectManager.Infrastructure --startup-project .
```

### Step 4: Verify Tables Created
```bash
psql -d AIProjectManagerDb -c "\dt"
```

You should see:
- Tenants
- Users
- Projects
- Tasks
- AIInteractionLogs
- __EFMigrationsHistory

## Alternative: Using Database Setup Script

If migrations are lost or you need to recreate the database structure:

```bash
# Drop existing database (CAUTION: This deletes all data)
dropdb AIProjectManagerDb

# Recreate database
createdb AIProjectManagerDb

# Run setup script
psql -d AIProjectManagerDb -f database-setup.sql
```

The `database-setup.sql` script creates all tables, indexes, and constraints without needing migrations.

## Creating Additional Migrations

When you modify entities or DbContext:

```bash
cd src/AIProjectManager.API
dotnet ef migrations add MigrationName --project ../AIProjectManager.Infrastructure --startup-project .
dotnet ef database update --project ../AIProjectManager.Infrastructure --startup-project .
```

## Removing Migrations

If you need to remove the last migration:

```bash
cd src/AIProjectManager.API
dotnet ef migrations remove --project ../AIProjectManager.Infrastructure --startup-project .
```

## Troubleshooting

### Error: "The current .NET SDK does not support targeting .NET 8.0"
**Solution:** Install .NET 8 SDK (see Prerequisites above)

### Error: "dotnet-ef command not found"
**Solution:** Install EF Core tools (see Prerequisites above)

### Error: "Could not connect to database"
**Solution:** 
- Verify PostgreSQL is running: `brew services list | grep postgresql`
- Check connection string in appsettings.json
- Verify database exists: `psql -l | grep AIProjectManagerDb`

### Error: "relation already exists"
**Solution:** 
- If starting fresh, drop and recreate database
- If updating, ensure you're running the correct migration

## Current Status

✅ Database setup script created: `database-setup.sql`  
✅ Database structure applied manually via SQL script  
⏳ EF Core migrations pending (requires .NET 8 SDK)

**Note:** The database schema is already set up using the SQL script. EF Core migrations can be created later when .NET 8 SDK is installed, or you can continue using the SQL script approach for schema changes.

