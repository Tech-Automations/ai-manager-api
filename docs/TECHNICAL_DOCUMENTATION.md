# AI Project Manager - Technical Documentation (SOP)

**Version:** 1.0  
**Last Updated:** 2024  
**Project:** AI Project Manager SaaS - .NET 8 Web API

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Solution Structure](#solution-structure)
4. [Domain Layer](#domain-layer)
5. [Application Layer](#application-layer)
6. [Infrastructure Layer](#infrastructure-layer)
7. [API Layer](#api-layer)
8. [Database Schema](#database-schema)
9. [Authentication & Authorization](#authentication--authorization)
10. [Multi-Tenancy](#multi-tenancy)
11. [API Endpoints](#api-endpoints)
12. [Configuration](#configuration)
13. [Development Workflow](#development-workflow)
14. [Database Migrations](#database-migrations)
15. [Common Tasks](#common-tasks)
16. [Testing](#testing)
17. [Deployment](#deployment)
18. [Troubleshooting](#troubleshooting)

---

## 1. Project Overview

### 1.1 Purpose
AI Project Manager is a production-ready .NET 8 Web API SaaS application built with Clean Architecture principles. It provides project management capabilities with AI-powered assistance.

### 1.2 Technology Stack
- **.NET 8** - Web API Framework
- **PostgreSQL 16+** - Database
- **Entity Framework Core 8.0** - ORM
- **JWT Bearer** - Authentication
- **AutoMapper** - Object Mapping
- **FluentValidation** - Request Validation
- **Serilog** - Logging
- **Swagger/OpenAPI** - API Documentation
- **BCrypt.Net** - Password Hashing

### 1.3 Key Features
- Multi-tenant SaaS architecture
- JWT-based authentication
- Role-based authorization (Admin, User)
- RESTful API with OpenAPI documentation
- Entity Framework Core with PostgreSQL
- Repository pattern for data access
- Clean Architecture separation of concerns
- Comprehensive error handling
- Structured logging with Serilog

---

## 2. Architecture

### 2.1 Clean Architecture Principles

The solution follows Clean Architecture with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         API Layer (Controllers)         │  ← Entry Point
├─────────────────────────────────────────┤
│      Application Layer (Services)       │  ← Business Logic
├─────────────────────────────────────────┤
│       Domain Layer (Entities)           │  ← Core Business
├─────────────────────────────────────────┤
│  Infrastructure Layer (Data/External)   │  ← Technical Details
└─────────────────────────────────────────┘
```

### 2.2 Layer Dependencies

- **API** depends on: Application, Infrastructure
- **Application** depends on: Domain
- **Infrastructure** depends on: Domain, Application (interfaces)
- **Domain** has no dependencies (pure business logic)

### 2.3 Design Patterns

- **Repository Pattern**: Abstract data access
- **Dependency Injection**: Loose coupling
- **DTO Pattern**: Data transfer objects
- **Unit of Work**: EF Core DbContext
- **CQRS-like**: Separate read/write models via DTOs

---

## 3. Solution Structure

```
ai-manager-dotnetcore/
├── src/
│   ├── AIProjectManager.API/              # Presentation Layer
│   │   ├── Controllers/                   # API Controllers
│   │   ├── Middleware/                    # Custom Middleware
│   │   ├── Program.cs                     # Application Entry Point
│   │   └── appsettings.json              # Configuration
│   │
│   ├── AIProjectManager.Application/      # Business Logic Layer
│   │   ├── DTOs/                         # Data Transfer Objects
│   │   ├── Interfaces/                   # Service Contracts
│   │   ├── Services/                     # Business Logic Implementation
│   │   ├── Mappings/                     # AutoMapper Profiles
│   │   ├── Validators/                   # FluentValidation Rules
│   │   └── DependencyInjection.cs        # DI Registration
│   │
│   ├── AIProjectManager.Domain/          # Core Domain Layer
│   │   ├── Entities/                     # Domain Entities
│   │   ├── Common/                       # Base Classes
│   │   └── Interfaces/                   # Domain Interfaces
│   │
│   └── AIProjectManager.Infrastructure/  # Data & External Services
│       ├── Data/                         # EF Core DbContext
│       ├── Repositories/                 # Repository Implementations
│       ├── Services/                     # Infrastructure Services
│       ├── Migrations/                   # Database Migrations
│       └── DependencyInjection.cs        # DI Registration
│
├── database-setup.sql                    # Database Setup Script
├── TECHNICAL_DOCUMENTATION.md            # This File
└── README.md                             # User Documentation
```

---

## 4. Domain Layer

### 4.1 Entities

#### BaseEntity
Base class for all entities with common audit fields:
- `Id` (Guid) - Primary key
- `TenantId` (Guid) - Multi-tenant isolation
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime?) - Last update timestamp
- `CreatedBy` (Guid?) - Creator user ID
- `UpdatedBy` (Guid?) - Last updater user ID

**Location:** `src/AIProjectManager.Domain/Common/BaseEntity.cs`

#### Tenant
Root entity for multi-tenancy:
- `Id` (Guid) - Primary key
- `Name` (string, max 200) - Tenant name
- `Subdomain` (string, max 50, unique) - Subdomain identifier
- `IsActive` (bool) - Active status
- `SubscriptionExpiresAt` (DateTime?) - Subscription expiration
- Navigation: `Users`, `Projects`

**Location:** `src/AIProjectManager.Domain/Entities/Tenant.cs`

#### User
Application user account:
- `Email` (string, max 255, unique per tenant) - User email
- `PasswordHash` (string) - BCrypt hashed password
- `FirstName` (string, max 100) - First name
- `LastName` (string, max 100) - Last name
- `Role` (string, max 50) - Role (Admin/User)
- `IsActive` (bool) - Active status
- Navigation: `Tenant`, `Projects`, `Tasks`

**Location:** `src/AIProjectManager.Domain/Entities/User.cs`

#### Project
Project entity:
- `Name` (string, max 200) - Project name
- `Description` (string, max 2000, nullable) - Project description
- `Status` (string, max 50) - Status: Active, Completed, Archived
- `OwnerId` (Guid) - Owner user ID
- `StartDate` (DateTime?) - Project start date
- `EndDate` (DateTime?) - Project end date
- Navigation: `Tenant`, `Owner`, `Tasks`

**Location:** `src/AIProjectManager.Domain/Entities/Project.cs`

#### TaskItem
Task entity:
- `Title` (string, max 200) - Task title
- `Description` (string, max 2000, nullable) - Task description
- `Status` (string, max 50) - Status: Todo, InProgress, Done, Blocked
- `Priority` (string, max 50) - Priority: Low, Medium, High, Critical
- `ProjectId` (Guid) - Parent project ID
- `AssignedToId` (Guid?) - Assigned user ID
- `DueDate` (DateTime?) - Due date
- `CompletedAt` (DateTime?) - Completion timestamp
- Navigation: `Tenant`, `Project`, `AssignedTo`

**Location:** `src/AIProjectManager.Domain/Entities/TaskItem.cs`

#### AIInteractionLog
AI interaction audit log:
- `ProjectId` (Guid?) - Related project
- `TaskItemId` (Guid?) - Related task
- `UserPrompt` (string, max 4000) - User's prompt
- `AIResponse` (string, max 8000, nullable) - AI response
- `Model` (string, max 100) - AI model used
- `TokenCount` (int?) - Token count
- `Cost` (decimal?) - Cost in dollars
- `ResponseTimeMs` (int?) - Response time in milliseconds
- `Status` (string, max 50) - Status: Success, Error
- `ErrorMessage` (string, max 1000, nullable) - Error message if failed
- Navigation: `Tenant`, `Project`, `TaskItem`

**Location:** `src/AIProjectManager.Domain/Entities/AIInteractionLog.cs`

### 4.2 Domain Interfaces

#### IRepository<T>
Generic repository interface for data access:
- `GetByIdAsync(Guid id, Guid tenantId)` - Get by ID
- `GetAllAsync(Guid tenantId)` - Get all for tenant
- `FindAsync(Expression<Func<T, bool>> predicate, Guid tenantId)` - Find with predicate
- `AddAsync(T entity)` - Add new entity
- `UpdateAsync(T entity)` - Update entity
- `DeleteAsync(T entity)` - Delete entity
- `ExistsAsync(Guid id, Guid tenantId)` - Check existence

**Location:** `src/AIProjectManager.Domain/Interfaces/IRepository.cs`

---

## 5. Application Layer

### 5.1 DTOs (Data Transfer Objects)

#### Authentication DTOs
- **RegisterRequestDto**: Email, Password, FirstName, LastName, TenantName, Subdomain
- **LoginRequestDto**: Email, Password
- **AuthResponseDto**: Token, User
- **UserDto**: Id, Email, FirstName, LastName, Role, TenantId

**Location:** `src/AIProjectManager.Application/DTOs/Auth/`

#### Project DTOs
- **CreateProjectDto**: Name, Description, StartDate, EndDate
- **UpdateProjectDto**: Name, Description, Status, StartDate, EndDate
- **ProjectDto**: Id, Name, Description, Status, OwnerId, OwnerName, StartDate, EndDate, CreatedAt

**Location:** `src/AIProjectManager.Application/DTOs/`

#### Task DTOs
- **CreateTaskItemDto**: Title, Description, Priority, ProjectId, AssignedToId, DueDate
- **UpdateTaskItemDto**: Title, Description, Status, Priority, AssignedToId, DueDate
- **TaskItemDto**: Id, Title, Description, Status, Priority, ProjectId, ProjectName, AssignedToId, AssignedToName, DueDate, CompletedAt, CreatedAt

**Location:** `src/AIProjectManager.Application/DTOs/`

#### AI Chat DTOs
- **AIChatRequestDto**: Message, ProjectId (optional), TaskItemId (optional)
- **AIChatResponseDto**: Response, InteractionLogId

**Location:** `src/AIProjectManager.Application/DTOs/`

### 5.2 Services

#### IUserService / UserService
**Location:** `src/AIProjectManager.Application/Services/UserService.cs`

**Methods:**
- `RegisterAsync(RegisterRequestDto)` - Register new user and tenant
- `LoginAsync(LoginRequestDto)` - Authenticate user
- `GetCurrentUserAsync(Guid userId, Guid tenantId)` - Get current user info

**Business Logic:**
- Creates tenant if subdomain doesn't exist
- Hashes passwords with BCrypt
- First user in tenant is assigned Admin role
- Validates tenant and user active status

#### IProjectService / ProjectService
**Location:** `src/AIProjectManager.Application/Services/ProjectService.cs`

**Methods:**
- `CreateAsync(CreateProjectDto, Guid userId, Guid tenantId)` - Create project
- `GetByIdAsync(Guid id, Guid tenantId)` - Get project by ID
- `GetAllAsync(Guid tenantId)` - Get all projects for tenant
- `UpdateAsync(Guid id, UpdateProjectDto, Guid tenantId)` - Update project
- `DeleteAsync(Guid id, Guid tenantId)` - Delete project

**Business Logic:**
- Sets OwnerId to current user on creation
- Enforces tenant isolation
- Validates user exists before creating project

#### ITaskItemService / TaskItemService
**Location:** `src/AIProjectManager.Application/Services/TaskItemService.cs`

**Methods:**
- `CreateAsync(CreateTaskItemDto, Guid userId, Guid tenantId)` - Create task
- `GetByIdAsync(Guid id, Guid tenantId)` - Get task by ID
- `GetAllAsync(Guid tenantId)` - Get all tasks for tenant
- `GetByProjectIdAsync(Guid projectId, Guid tenantId)` - Get tasks by project
- `UpdateAsync(Guid id, UpdateTaskItemDto, Guid tenantId)` - Update task
- `DeleteAsync(Guid id, Guid tenantId)` - Delete task

**Business Logic:**
- Validates project exists before creating task
- Validates assigned user exists
- Sets CompletedAt when status changes to Done
- Clears CompletedAt when status changes from Done

#### IAIChatService / AIChatService
**Location:** `src/AIProjectManager.Application/Services/AIChatService.cs`

**Methods:**
- `SendMessageAsync(AIChatRequestDto, Guid userId, Guid tenantId)` - Send AI chat message

**Business Logic:**
- Validates project/task exists if provided
- Creates AIInteractionLog entry
- Returns mock response (ready for OpenAI integration)
- Logs interaction details

### 5.3 Validators (FluentValidation)

All validators inherit from `AbstractValidator<T>`:

1. **RegisterRequestValidator**: Email format, password length (min 8), name lengths, subdomain format
2. **LoginRequestValidator**: Email format, password required
3. **CreateProjectValidator**: Name required (max 200), description max 2000, end date after start date
4. **UpdateProjectValidator**: Same as CreateProjectValidator
5. **CreateTaskItemValidator**: Title required (max 200), priority enum, project ID required
6. **UpdateTaskItemValidator**: Same as CreateTaskItemValidator
7. **AIChatRequestValidator**: Message required (max 4000)

**Location:** `src/AIProjectManager.Application/Validators/`

### 5.4 AutoMapper Profiles

**MappingProfile** maps:
- `User` → `UserDto`
- `Project` → `ProjectDto` (includes OwnerName)
- `TaskItem` → `TaskItemDto` (includes ProjectName, AssignedToName)

**Location:** `src/AIProjectManager.Application/Mappings/MappingProfile.cs`

### 5.5 Dependency Injection

**AddApplication()** registers:
- AutoMapper (MappingProfile)
- FluentValidation validators
- Application services (IUserService, IProjectService, ITaskItemService, IAIChatService)

**Location:** `src/AIProjectManager.Application/DependencyInjection.cs`

---

## 6. Infrastructure Layer

### 6.1 DbContext

**ApplicationDbContext** configures:
- All entity relationships
- Foreign key constraints
- Index definitions
- Property constraints (max lengths, required fields)
- Delete behaviors (Restrict, Cascade, SetNull)

**Key Configurations:**
- Tenant.Subdomain: Unique index
- User.Email + TenantId: Unique composite index
- User → Tenant: Restrict delete
- Project → Tenant: Restrict delete
- Project → User (Owner): Restrict delete
- TaskItem → Project: Cascade delete
- TaskItem → User (AssignedTo): SetNull on delete

**Location:** `src/AIProjectManager.Infrastructure/Data/ApplicationDbContext.cs`

### 6.2 Repositories

#### Repository<T> (Base)
Generic repository with tenant isolation:
- All queries filtered by TenantId
- Implements IRepository<T>

**Location:** `src/AIProjectManager.Infrastructure/Repositories/Repository.cs`

#### TenantRepository
Special handling for Tenant entity:
- GetByIdAsync: No tenant filter (Tenant is root)
- GetAllAsync: Returns all tenants
- FindAsync: No tenant filter

**Location:** `src/AIProjectManager.Infrastructure/Repositories/TenantRepository.cs`

#### ProjectRepository
Includes navigation properties:
- Includes Owner in GetByIdAsync and GetAllAsync

**Location:** `src/AIProjectManager.Infrastructure/Repositories/ProjectRepository.cs`

#### TaskItemRepository
Includes navigation properties:
- Includes Project and AssignedTo in all queries

**Location:** `src/AIProjectManager.Infrastructure/Repositories/TaskItemRepository.cs`

### 6.3 Infrastructure Services

#### JwtService
Generates JWT tokens with claims:
- Claims: sub (userId), email, tenantId, userId, role, jti
- Configurable expiry (default 1440 minutes = 24 hours)
- Reads from appsettings.json JwtSettings section

**Location:** `src/AIProjectManager.Infrastructure/Services/JwtService.cs`

### 6.4 Dependency Injection

**AddInfrastructure()** registers:
- DbContext with PostgreSQL connection
- All repositories (IRepository<T>)
- Infrastructure services (IJwtService)

**Location:** `src/AIProjectManager.Infrastructure/DependencyInjection.cs`

---

## 7. API Layer

### 7.1 Controllers

All controllers use:
- `[ApiController]` attribute
- `[Route("api/[controller]")]` routing
- Dependency injection for services
- FluentValidation for request validation
- Exception handling via middleware

#### AuthController
**Route:** `/api/auth`
- `POST /register` - Public, creates tenant and user
- `POST /login` - Public, authenticates user
- `GET /me` - Requires authentication

**Location:** `src/AIProjectManager.API/Controllers/AuthController.cs`

#### ProjectsController
**Route:** `/api/projects`
- `GET /` - Get all projects (tenant-scoped)
- `GET /{id}` - Get project by ID
- `POST /` - Create project
- `PUT /{id}` - Update project
- `DELETE /{id}` - Delete project

**All endpoints require authentication.**

**Location:** `src/AIProjectManager.API/Controllers/ProjectsController.cs`

#### TasksController
**Route:** `/api/tasks`
- `GET /?projectId={guid}` - Get all tasks (optionally filtered by projectId)
- `GET /{id}` - Get task by ID
- `POST /` - Create task
- `PUT /{id}` - Update task
- `DELETE /{id}` - Delete task

**All endpoints require authentication.**

**Location:** `src/AIProjectManager.API/Controllers/TasksController.cs`

#### AIChatController
**Route:** `/api/aichat`
- `POST /chat` - Send AI chat message

**Requires authentication.**

**Location:** `src/AIProjectManager.API/Controllers/AIChatController.cs`

### 7.2 Middleware

#### ExceptionHandlingMiddleware
Global exception handler:
- Logs all exceptions
- Returns appropriate HTTP status codes
- Hides sensitive error details in production
- Handles: UnauthorizedAccessException (401), KeyNotFoundException/InvalidOperationException (400), Others (500)

**Location:** `src/AIProjectManager.API/Middleware/ExceptionHandlingMiddleware.cs`

### 7.3 Program.cs Configuration

**Services Registered:**
1. Controllers
2. Swagger/OpenAPI with JWT auth
3. JWT Bearer authentication
4. Authorization
5. CORS (AllowAll policy for development)
6. Application layer services
7. Infrastructure layer services

**Middleware Pipeline:**
1. Swagger (development only)
2. HTTPS Redirection
3. CORS
4. Exception Handling Middleware
5. Authentication
6. Authorization
7. Controllers

**Database Migrations:**
- Automatically runs migrations on startup (development only)
- Should be removed or handled separately in production

**Location:** `src/AIProjectManager.API/Program.cs`

---

## 8. Database Schema

### 8.1 Tables

1. **Tenants**
   - Primary Key: Id (UUID)
   - Unique Index: Subdomain
   - Fields: Name, Subdomain, IsActive, SubscriptionExpiresAt, BaseEntity fields

2. **Users**
   - Primary Key: Id (UUID)
   - Foreign Key: TenantId → Tenants.Id (Restrict)
   - Unique Index: (Email, TenantId)
   - Fields: Email, PasswordHash, FirstName, LastName, Role, IsActive, BaseEntity fields

3. **Projects**
   - Primary Key: Id (UUID)
   - Foreign Keys: TenantId → Tenants.Id (Restrict), OwnerId → Users.Id (Restrict)
   - Fields: Name, Description, Status, OwnerId, StartDate, EndDate, BaseEntity fields

4. **Tasks**
   - Primary Key: Id (UUID)
   - Foreign Keys: TenantId → Tenants.Id (Restrict), ProjectId → Projects.Id (Cascade), AssignedToId → Users.Id (SetNull)
   - Fields: Title, Description, Status, Priority, ProjectId, AssignedToId, DueDate, CompletedAt, BaseEntity fields

5. **AIInteractionLogs**
   - Primary Key: Id (UUID)
   - Foreign Keys: TenantId → Tenants.Id (Restrict), ProjectId → Projects.Id (SetNull), TaskItemId → Tasks.Id (SetNull)
   - Fields: ProjectId, TaskItemId, UserPrompt, AIResponse, Model, TokenCount, Cost, ResponseTimeMs, Status, ErrorMessage, BaseEntity fields

### 8.2 Relationships

```
Tenants (1) ──< (N) Users
Tenants (1) ──< (N) Projects
Users (1) ──< (N) Projects (Owner)
Projects (1) ──< (N) Tasks
Users (1) ──< (N) Tasks (AssignedTo)
Projects (1) ──< (N) AIInteractionLogs
Tasks (1) ──< (N) AIInteractionLogs
Tenants (1) ──< (N) AIInteractionLogs
```

### 8.3 Database Setup Script

**Location:** `database-setup.sql`

Use this script to recreate the database structure if migrations are lost:
```bash
psql -d AIProjectManagerDb -f database-setup.sql
```

---

## 9. Authentication & Authorization

### 9.1 JWT Authentication

**Configuration (appsettings.json):**
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "AIProjectManager",
    "Audience": "AIProjectManager",
    "ExpiryMinutes": "1440"
  }
}
```

**Token Claims:**
- `sub`: User ID
- `email`: User email
- `tenantId`: Tenant ID
- `userId`: User ID (duplicate for convenience)
- `role`: User role (Admin/User)
- `jti`: JWT ID (unique token identifier)

**Token Validation:**
- Issuer validation enabled
- Audience validation enabled
- Lifetime validation enabled
- Signing key validation enabled
- Clock skew: Zero

### 9.2 Authorization

**Current Implementation:**
- `[Authorize]` attribute on protected endpoints
- Role-based authorization ready (claims included)

**Extending Authorization:**
To add role-based authorization, use:
```csharp
[Authorize(Roles = "Admin")]
```

### 9.3 Password Hashing

- Uses BCrypt.Net-Next
- Secure default cost factor
- Passwords never stored in plain text

---

## 10. Multi-Tenancy

### 10.1 Tenant Isolation Strategy

**Row-Level Security:**
- Every entity (except Tenant) has `TenantId` field
- Repository pattern enforces tenant filtering
- JWT token includes `tenantId` claim
- Controllers extract `tenantId` from token claims
- All queries automatically filtered by tenant

### 10.2 Tenant Repository Behavior

- `TenantRepository` does NOT filter by TenantId (Tenant is root entity)
- All other repositories filter by TenantId

### 10.3 Tenant Registration

- Tenant created during user registration if subdomain doesn't exist
- First user in tenant is assigned Admin role
- Tenant's `TenantId` field equals its own `Id`

---

## 11. API Endpoints

### 11.1 Authentication Endpoints

#### POST /api/auth/register
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "tenantName": "Example Company",
  "subdomain": "example-company"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Admin",
    "tenantId": "guid"
  }
}
```

#### POST /api/auth/login
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** Same as register

#### GET /api/auth/me
**Headers:** `Authorization: Bearer {token}`

**Response (200 OK):**
```json
{
  "id": "guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Admin",
  "tenantId": "guid"
}
```

### 11.2 Project Endpoints

All require authentication.

#### GET /api/projects
**Response:** Array of ProjectDto

#### GET /api/projects/{id}
**Response:** ProjectDto

#### POST /api/projects
**Request Body:**
```json
{
  "name": "My Project",
  "description": "Project description",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:** ProjectDto (201 Created)

#### PUT /api/projects/{id}
**Request Body:**
```json
{
  "name": "Updated Project",
  "description": "Updated description",
  "status": "Active",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:** ProjectDto

#### DELETE /api/projects/{id}
**Response:** 204 No Content

### 11.3 Task Endpoints

All require authentication.

#### GET /api/tasks?projectId={guid}
**Query Parameters:** projectId (optional)
**Response:** Array of TaskItemDto

#### GET /api/tasks/{id}
**Response:** TaskItemDto

#### POST /api/tasks
**Request Body:**
```json
{
  "title": "Complete task",
  "description": "Task description",
  "priority": "High",
  "projectId": "guid",
  "assignedToId": "guid",
  "dueDate": "2024-12-31T23:59:59Z"
}
```

**Response:** TaskItemDto (201 Created)

#### PUT /api/tasks/{id}
**Request Body:**
```json
{
  "title": "Updated task",
  "description": "Updated description",
  "status": "InProgress",
  "priority": "High",
  "assignedToId": "guid",
  "dueDate": "2024-12-31T23:59:59Z"
}
```

**Response:** TaskItemDto

#### DELETE /api/tasks/{id}
**Response:** 204 No Content

### 11.4 AI Chat Endpoints

Requires authentication.

#### POST /api/aichat/chat
**Request Body:**
```json
{
  "message": "Help me organize my project tasks",
  "projectId": "guid",
  "taskItemId": "guid"
}
```

**Response:**
```json
{
  "response": "Mock AI response...",
  "interactionLogId": "guid"
}
```

---

## 12. Configuration

### 12.1 appsettings.json

**Connection String:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AIProjectManagerDb;Username=rees"
  }
}
```

**JWT Settings:**
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "AIProjectManager",
    "Audience": "AIProjectManager",
    "ExpiryMinutes": "1440"
  }
}
```

**Serilog Configuration:**
- Console sink
- File sink (logs/log-.txt, daily rolling)
- Environment, MachineName, ThreadId enrichers

### 12.2 Environment Variables

For production, use environment variables:
- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__ExpiryMinutes`

---

## 13. Development Workflow

### 13.1 Initial Setup

1. Install .NET 8 SDK
2. Install PostgreSQL 16+
3. Install EF Core tools: `dotnet tool install --global dotnet-ef`
4. Clone repository
5. Restore packages: `dotnet restore`
6. Configure connection string in appsettings.json
7. Create database: `createdb AIProjectManagerDb`
8. Run migrations: `dotnet ef database update --project ../AIProjectManager.Infrastructure --startup-project .`
9. Run application: `dotnet run`

### 13.2 Adding New Features

1. **New Entity:**
   - Add entity to Domain/Entities
   - Add DbSet to ApplicationDbContext
   - Configure in OnModelCreating
   - Create migration: `dotnet ef migrations add AddNewEntity`
   - Update database-setup.sql

2. **New Service:**
   - Add interface to Application/Interfaces
   - Implement service in Application/Services
   - Register in Application/DependencyInjection.cs
   - Add controller in API/Controllers

3. **New Endpoint:**
   - Add method to appropriate service
   - Add controller action
   - Add DTOs if needed
   - Add validators if needed
   - Update this documentation

### 13.3 Code Style Guidelines

- Use nullable reference types
- Use async/await for all I/O operations
- Use CancellationToken for async methods
- Follow Clean Architecture principles
- Keep controllers thin (delegate to services)
- Use DTOs for all API interactions
- Validate all inputs with FluentValidation

---

## 14. Database Migrations

### 14.1 Creating Migrations

```bash
cd src/AIProjectManager.API
dotnet ef migrations add MigrationName --project ../AIProjectManager.Infrastructure --startup-project .
```

### 14.2 Applying Migrations

**Development (Automatic):**
- Migrations run automatically on application startup (Program.cs)

**Manual:**
```bash
cd src/AIProjectManager.API
dotnet ef database update --project ../AIProjectManager.Infrastructure --startup-project .
```

### 14.3 Removing Migrations

```bash
cd src/AIProjectManager.API
dotnet ef migrations remove --project ../AIProjectManager.Infrastructure --startup-project .
```

### 14.4 Database Setup Script

If migrations are lost, use `database-setup.sql` to recreate the schema:

```bash
psql -d AIProjectManagerDb -f database-setup.sql
```

**Location:** `database-setup.sql`

---

## 15. Common Tasks

### 15.1 Adding a New Field to an Entity

1. Add property to entity class
2. Update DbContext configuration if needed
3. Create migration: `dotnet ef migrations add AddFieldToEntity`
4. Update DTOs if exposing to API
5. Update AutoMapper profile if needed
6. Update validators if needed
7. Update database-setup.sql

### 15.2 Changing a Relationship

1. Update entity navigation properties
2. Update DbContext OnModelCreating configuration
3. Create migration
4. Update database-setup.sql

### 15.3 Adding Validation Rule

1. Locate appropriate validator
2. Add FluentValidation rule
3. Test with invalid data

### 15.4 Adding New API Endpoint

1. Add method to service interface
2. Implement in service
3. Add controller action
4. Add/update DTOs
5. Add/update validators
6. Update this documentation

### 15.5 Viewing Database Schema

```bash
psql -d AIProjectManagerDb
\dt              # List tables
\d+ TableName    # Describe table
\di              # List indexes
```

---

## 16. Testing

### 16.1 Manual Testing

**Swagger UI:**
- Navigate to `/swagger` when running in development
- Authorize with JWT token
- Test all endpoints

**Postman/Insomnia:**
- Import OpenAPI spec from Swagger
- Set up environment variables for base URL and token

### 16.2 Integration Testing (Future)

- Create test project
- Use TestServer for in-memory testing
- Test controllers with actual DbContext (InMemory provider)

### 16.3 Unit Testing (Future)

- Test services with mocked repositories
- Test validators
- Test DTOs

---

## 17. Deployment

### 17.1 Production Checklist

- [ ] Change JWT SecretKey to strong random value
- [ ] Store secrets in secure vault (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Remove automatic migration from Program.cs
- [ ] Configure CORS properly (remove AllowAll)
- [ ] Set up proper logging (Application Insights, CloudWatch, etc.)
- [ ] Configure HTTPS only
- [ ] Set up health checks
- [ ] Configure rate limiting
- [ ] Set up monitoring and alerting
- [ ] Review and update connection strings
- [ ] Set up backup strategy for PostgreSQL
- [ ] Configure production PostgreSQL (managed service recommended)

### 17.2 Deployment Steps

1. Build application: `dotnet publish -c Release`
2. Deploy to hosting platform (Azure App Service, AWS, Docker, etc.)
3. Run migrations: `dotnet ef database update --project ../AIProjectManager.Infrastructure`
4. Configure environment variables
5. Verify health checks
6. Monitor logs

---

## 18. Troubleshooting

### 18.1 Common Issues

#### Migration Errors
**Problem:** Migration fails with "relation already exists"
**Solution:** Drop and recreate database, or remove problematic migration

#### Connection String Issues
**Problem:** Cannot connect to database
**Solution:** 
- Verify PostgreSQL is running: `brew services list`
- Check connection string format
- Verify database exists: `psql -l`

#### JWT Token Invalid
**Problem:** Token validation fails
**Solution:**
- Check SecretKey matches between token generation and validation
- Verify token hasn't expired
- Check Issuer and Audience match

#### Tenant Isolation Issues
**Problem:** Users seeing other tenants' data
**Solution:**
- Verify tenantId claim in JWT token
- Check repository queries filter by TenantId
- Verify TenantRepository is NOT filtering (for Tenant queries)

### 18.2 Debugging Tips

**Enable Detailed Logging:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Check Database State:**
```sql
SELECT * FROM "Tenants";
SELECT * FROM "Users";
SELECT * FROM "Projects";
SELECT * FROM "Tasks";
```

**Verify JWT Token Claims:**
Use https://jwt.io to decode token and verify claims

---

## Appendix A: File Locations Reference

| Component | Location |
|-----------|----------|
| Entities | `src/AIProjectManager.Domain/Entities/` |
| BaseEntity | `src/AIProjectManager.Domain/Common/BaseEntity.cs` |
| DbContext | `src/AIProjectManager.Infrastructure/Data/ApplicationDbContext.cs` |
| Repositories | `src/AIProjectManager.Infrastructure/Repositories/` |
| Services | `src/AIProjectManager.Application/Services/` |
| Controllers | `src/AIProjectManager.API/Controllers/` |
| DTOs | `src/AIProjectManager.Application/DTOs/` |
| Validators | `src/AIProjectManager.Application/Validators/` |
| AutoMapper | `src/AIProjectManager.Application/Mappings/MappingProfile.cs` |
| Middleware | `src/AIProjectManager.API/Middleware/` |
| Configuration | `src/AIProjectManager.API/appsettings.json` |
| Database Script | `database-setup.sql` |

---

## Appendix B: Entity Field Reference

### BaseEntity Fields
- `Id` (Guid) - Primary key
- `TenantId` (Guid) - Tenant isolation
- `CreatedAt` (DateTime) - Creation timestamp (UTC)
- `UpdatedAt` (DateTime?) - Update timestamp (UTC)
- `CreatedBy` (Guid?) - Creator user ID
- `UpdatedBy` (Guid?) - Updater user ID

### Status Values
- **Project Status:** Active, Completed, Archived
- **Task Status:** Todo, InProgress, Done, Blocked
- **Task Priority:** Low, Medium, High, Critical
- **User Role:** Admin, User
- **AI Log Status:** Success, Error

---

## Document Maintenance

**When to Update This Document:**
- Adding new entities, services, or endpoints
- Changing database schema
- Modifying architecture patterns
- Adding new features or capabilities
- Changing configuration requirements

**Version History:**
- v1.0 - Initial comprehensive documentation
- v2.0 - Added comprehensive planning section for AI Manager features

---

## 19. Product Planning & Roadmap

### 19.1 Product Vision

**AI Manager** - An AI-first SaaS platform that enables project managers to manage multiple projects efficiently by reducing cognitive load and automating routine tasks.

**Core Promise:** "Manage 5 projects like 1"

**Key Principles:**
- AI-first design: AI orchestrates and drives most interactions
- Minimal UI: Focus on chat-first interaction
- Integration-first: Seamlessly connects with existing tools (Azure DevOps, Jira, Slack, Git, Calendar, Meetings)
- Manager-centric: Designed to reduce cognitive load and enable managers to handle more projects

**Epic Feature (Final Stage):** Virtual meeting presence - AI-driven video/facial expressions allowing managers to "join" meetings remotely while appearing present via AI-generated video.

---

### 19.2 Planned API Features & Architecture

#### Phase 1: Core AI Manager Features (MVP)

##### 1. Meeting Integration & Action Items

**APIs:**
- `POST /api/meetings/transcript` - Upload meeting transcript (Zoom/Teams/Google Meet)
  - Accepts: Transcript text, meeting metadata (date, participants, duration)
  - Returns: Meeting ID, extracted action items
  
- `GET /api/meetings/{id}` - Get meeting details
  - Returns: Meeting info, transcript, action items, participants
  
- `GET /api/meetings/{id}/action-items` - Get AI-extracted action items
  - Returns: List of action items with assignees, deadlines, priorities
  
- `POST /api/meetings/{id}/action-items/sync-to-ado` - Sync action items to Azure DevOps
  - Accepts: Mapping configuration (project, work item type)
  - Returns: Created work items in ADO
  
- `GET /api/meetings` - List all meetings (with filters)
  - Query params: projectId, dateFrom, dateTo
  - Returns: Paginated list of meetings

**Entities Needed:**
```csharp
public class Meeting : BaseEntity
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Transcript { get; set; }
    public string Source { get; set; } // Zoom, Teams, GoogleMeet
    public string? ExternalMeetingId { get; set; }
    public string? RecordingUrl { get; set; }
    public Guid? ProjectId { get; set; }
    public virtual ICollection<MeetingParticipant> Participants { get; set; }
    public virtual ICollection<ActionItem> ActionItems { get; set; }
}

public class MeetingParticipant : BaseEntity
{
    public Guid MeetingId { get; set; }
    public string Email { get; set; }
    public string? Name { get; set; }
    public bool IsOrganizer { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

public class ActionItem : BaseEntity
{
    public Guid MeetingId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } // Low, Medium, High
    public string Status { get; set; } // Open, InProgress, Completed, Cancelled
    public string? ExternalWorkItemId { get; set; } // ADO/Jira ID
    public string? ExternalSystem { get; set; } // AzureDevOps, Jira
}
```

##### 2. AI Standup Generator

**APIs:**
- `POST /api/standups/generate` - Generate standup report
  - Accepts: ProjectId (optional), dateRange, format (markdown, slack, teams)
  - Returns: Generated standup text, metadata
  
- `GET /api/standups/{id}` - Get generated standup
  - Returns: Standup content, generation metadata
  
- `POST /api/standups/{id}/send` - Send standup to Slack/Teams
  - Accepts: Channel/webhook URL
  - Returns: Send status
  
- `GET /api/standups` - List standup history
  - Query params: projectId, dateFrom, dateTo
  - Returns: Paginated list of standups

**Entities Needed:**
```csharp
public class Standup : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public DateTime StandupDate { get; set; }
    public string GeneratedContent { get; set; }
    public string Format { get; set; } // Markdown, Slack, Teams
    public string Status { get; set; } // Draft, Sent, Archived
    public DateTime? SentAt { get; set; }
    public string? SentTo { get; set; } // Channel/webhook
}
```

##### 3. Risk Radar (AI-Powered Risk Detection)

**APIs:**
- `GET /api/risks` - Get all risks
  - Query params: projectId, severity, status, dateFrom
  - Returns: Paginated list of risks
  
- `GET /api/risks/project/{projectId}` - Get risks for a project
  - Returns: List of risks with details
  
- `POST /api/risks/analyze` - Trigger AI risk analysis
  - Accepts: ProjectId (optional, analyzes all if not provided)
  - Returns: Analysis results, detected risks
  
- `PUT /api/risks/{id}` - Update risk status
  - Accepts: Status, notes, mitigation plan
  - Returns: Updated risk
  
- `GET /api/risks/{id}` - Get risk details
  - Returns: Risk details, history, related items

**Entities Needed:**
```csharp
public class Risk : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } // Delay, Silence, ScopeCreep, Dependency, Resource
    public string Severity { get; set; } // Low, Medium, High, Critical
    public string Status { get; set; } // Open, Mitigating, Resolved, Closed
    public DateTime? DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? MitigationPlan { get; set; }
    public string? Notes { get; set; }
    public decimal? Confidence { get; set; } // AI confidence score (0-1)
}
```

##### 4. Manager Chat (AI Q&A System)

**APIs:**
- `POST /api/chat/query` - Ask questions about projects
  - Accepts: Question text, context (projectId, includeHistory)
  - Returns: AI response, sources, confidence
  
- `GET /api/chat/history` - Get chat history
  - Query params: projectId, dateFrom, limit
  - Returns: Paginated chat history
  
- `POST /api/chat/follow-up` - Follow-up question in context
  - Accepts: Question, previousChatId
  - Returns: AI response with context
  
- `DELETE /api/chat/history/{id}` - Delete chat entry
  - Returns: Success status

**Entities Needed:**
```csharp
public class ChatSession : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public string Question { get; set; }
    public string? Response { get; set; }
    public string? Sources { get; set; } // JSON array of source references
    public decimal? Confidence { get; set; }
    public Guid? ParentSessionId { get; set; } // For follow-up questions
    public virtual ICollection<ChatSession> FollowUps { get; set; }
}
```

##### 5. Auto Reports (Weekly Leadership Reports)

**APIs:**
- `POST /api/reports/weekly/generate` - Generate weekly report
  - Accepts: Date range, projectIds (optional), format
  - Returns: Generated report ID
  
- `GET /api/reports/{id}` - Get report details
  - Returns: Report content, metadata, sections
  
- `POST /api/reports/{id}/send` - Send report to recipients
  - Accepts: Recipients (emails), delivery method
  - Returns: Send status
  
- `GET /api/reports` - List all reports
  - Query params: dateFrom, dateTo, status
  - Returns: Paginated list of reports
  
- `POST /api/reports/schedule` - Schedule automatic reports
  - Accepts: Schedule config (weekly, bi-weekly, monthly)
  - Returns: Schedule ID

**Entities Needed:**
```csharp
public class Report : BaseEntity
{
    public string Type { get; set; } // Weekly, Monthly, Custom
    public DateTime ReportDate { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string GeneratedContent { get; set; }
    public string Format { get; set; } // PDF, HTML, Markdown
    public string Status { get; set; } // Draft, Generated, Sent
    public DateTime? SentAt { get; set; }
    public string? Recipients { get; set; } // JSON array of emails
}

public class ReportSchedule : BaseEntity
{
    public string Type { get; set; } // Weekly, Monthly
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public TimeSpan Time { get; set; }
    public string Recipients { get; set; } // JSON array
    public bool IsActive { get; set; }
    public DateTime? LastGeneratedAt { get; set; }
}
```

#### Phase 2: Integration APIs

##### 6. Azure DevOps Integration

**APIs:**
- `POST /api/integrations/azure-devops/connect` - Connect ADO account
  - Accepts: Organization URL, Personal Access Token
  - Returns: Connection status, synced projects
  
- `GET /api/integrations/azure-devops/status` - Get connection status
  - Returns: Connection info, last sync time
  
- `POST /api/integrations/azure-devops/sync` - Sync work items
  - Accepts: ProjectId, teamId (optional)
  - Returns: Sync results (count of synced items)
  
- `GET /api/integrations/azure-devops/work-items` - Get synced work items
  - Query params: projectId, status, assignedTo
  - Returns: List of work items
  
- `POST /api/integrations/azure-devops/work-items/{id}/link` - Link ADO work item to task
  - Accepts: TaskItemId
  - Returns: Linked item info
  
- `PUT /api/integrations/azure-devops/disconnect` - Disconnect ADO
  - Returns: Success status

**Entities Needed:**
```csharp
public class Integration : BaseEntity
{
    public string Type { get; set; } // AzureDevOps, Jira, Slack, Git
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string? Configuration { get; set; } // JSON config (encrypted credentials)
    public DateTime? LastSyncedAt { get; set; }
    public string? Status { get; set; } // Connected, Disconnected, Error
    public string? ErrorMessage { get; set; }
}

public class ExternalWorkItem : BaseEntity
{
    public Guid IntegrationId { get; set; }
    public Guid? TaskItemId { get; set; }
    public string ExternalId { get; set; } // ADO/Jira work item ID
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? ExternalUrl { get; set; }
}
```

##### 7. Git Integration

**APIs:**
- `POST /api/integrations/git/webhook` - Git webhook handler
  - Accepts: Webhook payload (GitHub/GitLab/Bitbucket)
  - Returns: Processing status
  
- `POST /api/integrations/git/connect` - Connect Git repository
  - Accepts: Repository URL, access token
  - Returns: Connection status
  
- `GET /api/integrations/git/commits` - Get commits for project
  - Query params: projectId, branch, dateFrom
  - Returns: List of commits
  
- `GET /api/integrations/git/pull-requests` - Get PRs for project
  - Query params: projectId, status
  - Returns: List of pull requests

**Entities Needed:**
```csharp
public class GitRepository : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public string RepositoryUrl { get; set; }
    public string Provider { get; set; } // GitHub, GitLab, Bitbucket
    public string? AccessToken { get; set; } // Encrypted
    public bool IsActive { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

public class GitCommit : BaseEntity
{
    public Guid RepositoryId { get; set; }
    public Guid? ProjectId { get; set; }
    public string CommitHash { get; set; }
    public string Message { get; set; }
    public string Author { get; set; }
    public string AuthorEmail { get; set; }
    public DateTime CommittedAt { get; set; }
    public string Branch { get; set; }
}
```

##### 8. Slack/Teams Integration

**APIs:**
- `POST /api/integrations/slack/webhook` - Slack webhook handler
  - Accepts: Slack event payload
  - Returns: Processing status
  
- `POST /api/integrations/slack/connect` - Connect Slack workspace
  - Accepts: OAuth token, workspace info
  - Returns: Connection status
  
- `POST /api/integrations/slack/send` - Send message to Slack
  - Accepts: Channel, message, format
  - Returns: Send status
  
- `GET /api/integrations/slack/channels` - Get available channels
  - Returns: List of channels

**Similar APIs for Microsoft Teams**

**Entities Needed:**
```csharp
public class SlackWorkspace : BaseEntity
{
    public string WorkspaceId { get; set; }
    public string WorkspaceName { get; set; }
    public string? AccessToken { get; set; } // Encrypted
    public string? BotToken { get; set; } // Encrypted
    public bool IsActive { get; set; }
}
```

#### Phase 3: Advanced Features

##### 9. Calendar & Meeting Scheduling

**APIs:**
- `GET /api/calendar/events` - Get calendar events
  - Query params: dateFrom, dateTo
  - Returns: List of events
  
- `POST /api/calendar/events` - Create calendar event
  - Accepts: Event details
  - Returns: Created event
  
- `POST /api/calendar/sync` - Sync with external calendar
  - Accepts: Calendar provider (Google, Outlook)
  - Returns: Sync status

##### 10. Notification System

**APIs:**
- `GET /api/notifications` - Get user notifications
  - Query params: unreadOnly, limit
  - Returns: List of notifications
  
- `PUT /api/notifications/{id}/read` - Mark as read
  - Returns: Success status
  
- `POST /api/notifications/preferences` - Update notification preferences
  - Accepts: Preferences config
  - Returns: Updated preferences

**Entities Needed:**
```csharp
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } // Risk, Task, Meeting, Report
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public Guid? RelatedEntityId { get; set; } // Project, Task, etc.
}
```

---

### 19.3 AI/LLM Orchestration Architecture

#### LLM Provider Strategy

**Primary:** OpenAI GPT-4 (or GPT-4 Turbo)
**Fallback:** Azure OpenAI Service
**Future:** Support for Anthropic Claude, local models

#### AI Service Layer

```csharp
public interface ILLMService
{
    Task<string> GenerateResponseAsync(string prompt, LLMContext context);
    Task<T> GenerateStructuredResponseAsync<T>(string prompt, LLMContext context);
    Task<IEnumerable<ExtractedItem>> ExtractEntitiesAsync(string text, EntityType type);
}

public class LLMContext
{
    public Guid? ProjectId { get; set; }
    public Guid? TenantId { get; set; }
    public Dictionary<string, object> AdditionalContext { get; set; }
    public string Model { get; set; } // gpt-4, gpt-4-turbo
    public int? MaxTokens { get; set; }
    public decimal? Temperature { get; set; }
}
```

#### Prompt Engineering Strategy

1. **System Prompts:** Define role and constraints
2. **Context Injection:** Include relevant project/task data
3. **Few-shot Examples:** Provide examples for structured outputs
4. **Output Validation:** Validate LLM responses before saving

#### AI Features Implementation

**Meeting Transcript Processing:**
- Extract action items using LLM
- Identify assignees from transcript
- Estimate deadlines based on context
- Link to existing tasks/projects

**Standup Generation:**
- Aggregate recent activity (commits, PRs, task updates)
- Generate narrative summary using LLM
- Format for different platforms (Slack, Teams, Email)

**Risk Detection:**
- Analyze project metrics (velocity, task completion rates)
- Detect patterns indicating delays, scope creep
- Use LLM to contextualize and prioritize risks

**Manager Chat:**
- RAG (Retrieval Augmented Generation) approach
- Index project data, task history, meeting notes
- Retrieve relevant context for questions
- Generate accurate, contextualized responses

**Auto Reports:**
- Aggregate data from multiple sources
- Use LLM to generate narrative insights
- Format into professional reports

---

### 19.4 Database Schema Additions

#### New Tables Required

1. **Meetings**
   - Meeting metadata, transcript, source
   - Foreign keys: TenantId, ProjectId

2. **MeetingParticipants**
   - Participant info per meeting
   - Foreign keys: TenantId, MeetingId

3. **ActionItems**
   - Extracted action items from meetings
   - Foreign keys: TenantId, MeetingId, ProjectId, AssignedToId

4. **Standups**
   - Generated standup reports
   - Foreign keys: TenantId, ProjectId

5. **Risks**
   - Risk detection and tracking
   - Foreign keys: TenantId, ProjectId

6. **ChatSessions**
   - Manager chat history
   - Foreign keys: TenantId, ProjectId, ParentSessionId

7. **Reports**
   - Generated reports
   - Foreign keys: TenantId

8. **ReportSchedules**
   - Automated report schedules
   - Foreign keys: TenantId

9. **Integrations**
   - Integration connections
   - Foreign keys: TenantId

10. **ExternalWorkItems**
    - Synced work items from ADO/Jira
    - Foreign keys: TenantId, IntegrationId, TaskItemId

11. **GitRepositories**
    - Connected Git repositories
    - Foreign keys: TenantId, ProjectId

12. **GitCommits**
    - Synced commits
    - Foreign keys: TenantId, RepositoryId, ProjectId

13. **SlackWorkspaces / TeamsWorkspaces**
    - Connected messaging platforms
    - Foreign keys: TenantId

14. **Notifications**
    - User notifications
    - Foreign keys: TenantId, UserId

#### Indexes Needed

- `Meetings.ProjectId` + `StartTime`
- `ActionItems.MeetingId`
- `ActionItems.AssignedToId` + `Status`
- `Risks.ProjectId` + `Severity` + `Status`
- `ChatSessions.ProjectId` + `CreatedAt`
- `GitCommits.ProjectId` + `CommittedAt`
- `Notifications.UserId` + `IsRead` + `CreatedAt`

---

### 19.5 Integration Architecture

#### Integration Pattern

**Base Integration Service:**
```csharp
public interface IIntegrationService
{
    Task<bool> ConnectAsync(string configuration);
    Task<bool> DisconnectAsync();
    Task<IntegrationStatus> GetStatusAsync();
    Task SyncAsync();
}
```

**Specific Implementations:**
- `AzureDevOpsIntegrationService`
- `JiraIntegrationService`
- `GitHubIntegrationService`
- `SlackIntegrationService`
- `TeamsIntegrationService`
- `GoogleCalendarIntegrationService`
- `OutlookCalendarIntegrationService`

#### Webhook Handling

- Centralized webhook controller: `/api/webhooks/{provider}`
- Provider-specific handlers
- Queue-based processing for async operations
- Retry mechanism for failed webhooks

#### Data Synchronization

- Background jobs (Hangfire/Quartz) for periodic syncs
- Webhook-based real-time updates
- Conflict resolution strategy
- Last-sync timestamp tracking

---

### 19.6 Implementation Roadmap

#### Phase 1: Foundation (MVP) - Weeks 1-4

**Week 1-2: Database & Entities**
- Create migration for new entities
- Implement repositories
- Add DTOs and validators

**Week 2-3: Core AI Features**
- Meeting transcript processing
- Action item extraction
- Basic LLM integration (OpenAI)

**Week 3-4: Risk Radar**
- Risk detection logic
- Risk tracking APIs
- Basic risk analysis

#### Phase 2: Integrations - Weeks 5-8

**Week 5-6: Azure DevOps**
- ADO connection & authentication
- Work item syncing
- Action item → ADO work item creation

**Week 6-7: Git Integration**
- Repository connection
- Commit tracking
- PR monitoring

**Week 7-8: Slack/Teams**
- Workspace connection
- Message sending
- Webhook handling

#### Phase 3: Advanced Features - Weeks 9-12

**Week 9-10: Standup Generator**
- Activity aggregation
- LLM-based generation
- Multi-platform formatting

**Week 10-11: Manager Chat**
- RAG implementation
- Chat history
- Context management

**Week 11-12: Auto Reports**
- Report generation
- Scheduling system
- Multi-format support

#### Phase 4: Polish & Optimization - Weeks 13-16

- Performance optimization
- Error handling improvements
- UI/UX enhancements
- Documentation
- Testing

#### Phase 5: Epic Feature (Future) - TBD

- Virtual meeting presence
- AI-driven video generation
- Real-time facial expressions
- Audio/text to video conversion

---

### 19.7 MVP Scope vs Future Features

#### MVP Must-Haves (Phase 1)

✅ Meeting transcript processing  
✅ Action item extraction  
✅ Basic risk detection  
✅ Manager chat (Q&A)  
✅ Azure DevOps integration (basic)  
✅ Git integration (basic)  

#### Nice-to-Haves (Phase 2-3)

⭐ Standup generator  
⭐ Auto reports  
⭐ Slack/Teams integration  
⭐ Calendar sync  
⭐ Advanced risk analytics  
⭐ Notification system  

#### Future Features (Post-MVP)

🔮 Virtual meeting presence  
🔮 Advanced AI analytics  
🔮 Custom AI model training  
🔮 Mobile app  
🔮 Advanced reporting & dashboards  
🔮 Multi-language support  

---

### 19.8 Technical Decisions & Considerations

#### LLM Provider Selection

**OpenAI GPT-4:**
- Pros: Best quality, reliable API, good documentation
- Cons: Cost, rate limits, data privacy concerns
- Decision: Start with OpenAI, add Azure OpenAI for enterprise customers

#### Background Job Processing

**Options:** Hangfire, Quartz.NET, Azure Functions
**Decision:** Start with Hangfire (in-process), migrate to Azure Functions for scale

#### Caching Strategy

**Redis** for:
- LLM responses (where appropriate)
- Integration status
- Frequently accessed data

#### Message Queue

**Azure Service Bus** or **RabbitMQ** for:
- Webhook processing
- Background sync jobs
- Notification delivery

#### File Storage

**Azure Blob Storage** or **AWS S3** for:
- Meeting recordings
- Generated reports
- Attachment storage

---

### 19.9 API Rate Limits & Cost Considerations

#### LLM Usage Optimization

- Cache common queries
- Batch requests where possible
- Use cheaper models for simple tasks
- Implement request throttling per tenant
- Monitor token usage and costs

#### Integration Rate Limits

- Respect API rate limits (ADO, GitHub, Slack)
- Implement exponential backoff
- Queue requests when limits hit
- Track usage per integration

---

### 19.10 Security Considerations

#### Integration Credentials

- Encrypt all stored tokens/passwords
- Use Azure Key Vault / AWS Secrets Manager
- Rotate credentials periodically
- Audit credential access

#### LLM Data Privacy

- Don't send PII to LLM without consent
- Implement data anonymization where needed
- Review LLM provider privacy policies
- Consider on-premise LLM for sensitive data

#### Webhook Security

- Verify webhook signatures
- Use HTTPS only
- Implement IP whitelisting where possible
- Rate limit webhook endpoints

---

**End of Planning Section**

---

**End of Technical Documentation**

