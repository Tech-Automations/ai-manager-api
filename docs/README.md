# Documentation (SOP)

This folder contains all project documentation, setup instructions, and Standard Operating Procedures (SOP).

## Documentation Files

### Core Documentation
- **TECHNICAL_DOCUMENTATION.md** - Complete technical documentation and SOP for the entire project
- **README.md** (in project root) - Quick start guide and user documentation

### Setup & Installation
- **INSTALL_DOTNET8.md** - Instructions for installing .NET 8 SDK
- **INSTALL_DOTNET8_RUNTIME.md** - Instructions for installing .NET 8 runtime
- **INSTALL_RUNTIME_COMMAND.md** - Quick command reference for runtime installation

### Database
- **database-setup.sql** - Complete SQL script to recreate database schema
- **DATABASE_STATUS.md** - Current database status and maintenance guide
- **MIGRATION_INSTRUCTIONS.md** - EF Core migration commands and procedures

### Project Management
- **BUILD_STATUS.md** - Current build and runtime status
- **RUN_PROJECT.md** - Instructions for running the project
- **setup-runtime.sh** - Script to setup .NET 8 runtime

## Quick Links

### For New Developers
1. Start with **TECHNICAL_DOCUMENTATION.md** for complete project understanding
2. Follow **RUN_PROJECT.md** to get started
3. Refer to **DATABASE_STATUS.md** for database setup

### For Setup Issues
- .NET 8 SDK not installed? → See **INSTALL_DOTNET8.md**
- .NET 8 Runtime missing? → See **INSTALL_DOTNET8_RUNTIME.md** or run **setup-runtime.sh**
- Database issues? → See **DATABASE_STATUS.md** and **database-setup.sql**

### For Database Maintenance
- Need to recreate database? → Run **database-setup.sql**
- Creating migrations? → See **MIGRATION_INSTRUCTIONS.md**

---

**Note:** All documentation files are maintained as part of the project. Update them when making significant changes to the codebase.

