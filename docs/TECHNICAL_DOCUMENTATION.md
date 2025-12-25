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

---

**End of Technical Documentation**

