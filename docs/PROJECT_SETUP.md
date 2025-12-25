# Project Setup Guide - First Time Setup

Quick setup guide for Mac and Windows.

---

## Mac Setup

### Prerequisites
- macOS with Homebrew installed

### Steps

#### 1. Install .NET 8 SDK
```bash
brew install --cask dotnet-sdk
```

#### 2. Install PostgreSQL
```bash
brew install postgresql@16
brew services start postgresql@16
```

#### 3. Create Database
```bash
createdb AIProjectManagerDb
```

#### 4. Clone and Setup
```bash
git clone https://github.com/Tech-Automations/ai-manager-api.git
cd ai-manager-api
dotnet restore
```

#### 5. Configure Connection String
Edit `src/AIProjectManager.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AIProjectManagerDb;Username=$(whoami)"
  }
}
```

#### 6. Setup Database Schema
```bash
psql -d AIProjectManagerDb -f docs/database-setup.sql
```

#### 7. Run
```bash
cd src/AIProjectManager.API
dotnet run
```

**Access:** https://localhost:5001/swagger

---

## Windows Setup

### Prerequisites
- Windows 10/11
- PowerShell or Command Prompt

### Steps

#### 1. Install .NET 8 SDK
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- Run installer
- Verify: `dotnet --version`

#### 2. Install PostgreSQL
- Download from: https://www.postgresql.org/download/windows/
- Run installer
- Remember your postgres user password
- Start PostgreSQL service (usually auto-starts)

#### 3. Create Database
Open **pgAdmin** or **psql**:
```sql
CREATE DATABASE "AIProjectManagerDb";
```

Or via Command Prompt (if psql is in PATH):
```cmd
psql -U postgres -c "CREATE DATABASE AIProjectManagerDb;"
```

#### 4. Clone and Setup
```cmd
git clone https://github.com/Tech-Automations/ai-manager-api.git
cd ai-manager-api
dotnet restore
```

#### 5. Configure Connection String
Edit `src/AIProjectManager.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AIProjectManagerDb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

#### 6. Setup Database Schema
```cmd
psql -U postgres -d AIProjectManagerDb -f docs/database-setup.sql
```

#### 7. Run
```cmd
cd src/AIProjectManager.API
dotnet run
```

**Access:** https://localhost:5001/swagger

---

## Quick Verification

After setup, verify everything works:

1. **Check .NET:**
   ```bash
   dotnet --version
   # Should show: 8.x.x
   ```

2. **Check PostgreSQL:**
   ```bash
   psql --version
   # Mac: psql (PostgreSQL) 16.x
   # Windows: Check via pgAdmin or psql command
   ```

3. **Check Database:**
   ```bash
   psql -d AIProjectManagerDb -c "\dt"
   # Should show: Tenants, Users, Projects, Tasks, AIInteractionLogs
   ```

4. **Check API:**
   - Open https://localhost:5001/swagger
   - Should see API documentation

---

## Troubleshooting

### Mac: PostgreSQL not starting
```bash
brew services restart postgresql@16
```

### Windows: PostgreSQL connection failed
- Check if PostgreSQL service is running (Services app)
- Verify password in connection string
- Check if port 5432 is available

### .NET SDK not found
- Verify installation: `dotnet --list-sdks`
- Restart terminal/command prompt
- Add to PATH if needed

---

**Setup time:** ~10-15 minutes

