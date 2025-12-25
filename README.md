# AI Project Manager - .NET 8 Web API

A production-ready .NET 8 Web API built with Clean Architecture principles for an AI Project Manager SaaS application.

## Architecture

This solution follows Clean Architecture with the following layers:

- **Domain**: Core business entities and interfaces
- **Application**: Business logic, DTOs, use cases, and service interfaces
- **Infrastructure**: Data access, repositories, external services (JWT, EF Core)
- **API**: Controllers, middleware, and application configuration

## Features

- ✅ .NET 8 Web API
- ✅ Entity Framework Core with PostgreSQL
- ✅ JWT Authentication
- ✅ Multi-tenant architecture (TenantId on every entity)
- ✅ Clean Architecture (separation of concerns)
- ✅ Repository pattern
- ✅ AutoMapper for DTO mapping
- ✅ FluentValidation for request validation
- ✅ Proper error handling middleware
- ✅ Swagger/OpenAPI documentation
- ✅ Serilog for structured logging
- ✅ Role-based authorization ready

## Core Entities

- **Tenant**: Multi-tenant isolation
- **User**: User accounts with roles (Admin, User)
- **Project**: Project management
- **TaskItem**: Task items linked to projects
- **AIInteractionLog**: Logs of AI interactions (ready for OpenAI integration)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user and tenant
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/me` - Get current user info (requires authentication)

### Projects
- `GET /api/projects` - Get all projects for current tenant
- `GET /api/projects/{id}` - Get project by ID
- `POST /api/projects` - Create a new project
- `PUT /api/projects/{id}` - Update a project
- `DELETE /api/projects/{id}` - Delete a project

### Tasks
- `GET /api/tasks` - Get all tasks (optionally filter by projectId query parameter)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create a new task
- `PUT /api/tasks/{id}` - Update a task
- `DELETE /api/tasks/{id}` - Delete a task

### AI Chat
- `POST /api/aichat/chat` - Send a message to AI (currently returns mock response, ready for OpenAI integration)

All endpoints (except auth/register and auth/login) require JWT authentication.

## Prerequisites

- .NET 8 SDK
- PostgreSQL 12+ (or Docker for running PostgreSQL)
- Visual Studio 2022, VS Code, or Rider

## Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ai-manager-dotnetcore
```

### 2. Database Setup

#### Option A: Using Docker (Recommended for Development)

```bash
docker run --name postgres-ai-manager \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=AIProjectManagerDb \
  -p 5432:5432 \
  -d postgres:15
```

#### Option B: Local PostgreSQL Installation

1. Install PostgreSQL
2. Create a database:
```sql
CREATE DATABASE "AIProjectManagerDb";
```

### 3. Configure Connection String

Update `appsettings.json` or `appsettings.Development.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AIProjectManagerDb;Username=postgres;Password=postgres"
  }
}
```

### 4. Configure JWT Secret Key

**Important**: In production, use a strong, randomly generated secret key stored securely (e.g., environment variables, Azure Key Vault, etc.).

Update `appsettings.json`:

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

### 5. Restore Dependencies

```bash
dotnet restore
```

### 6. Install EF Core Tools (if not already installed)

If you haven't already installed the EF Core tools, install them globally:

```bash
dotnet tool install --global dotnet-ef
```

### 7. Run Database Migrations

The application will automatically run migrations on startup. Alternatively, you can run them manually:

```bash
cd src/AIProjectManager.API
dotnet ef database update --project ../AIProjectManager.Infrastructure --startup-project .
```

If you need to create a new migration:

```bash
cd src/AIProjectManager.API
dotnet ef migrations add InitialCreate --project ../AIProjectManager.Infrastructure --startup-project .
```

### 8. Run the Application

```bash
cd src/AIProjectManager.API
dotnet run
```

Or use your IDE's run command. The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Testing the API

### 1. Register a New User

```bash
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "tenantName": "Example Company",
  "subdomain": "example-company"
}
```

Response will include a JWT token. Save this token for subsequent requests.

### 2. Login

```bash
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "SecurePassword123!"
}
```

### 3. Use the Token

Include the token in the Authorization header for authenticated requests:

```
Authorization: Bearer <your-jwt-token>
```

### 4. Create a Project

```bash
POST https://localhost:5001/api/projects
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "name": "My First Project",
  "description": "Project description",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

### 5. Create a Task

```bash
POST https://localhost:5001/api/tasks
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "title": "Complete project setup",
  "description": "Set up development environment",
  "priority": "High",
  "projectId": "<project-id-from-previous-step>",
  "dueDate": "2024-01-15T00:00:00Z"
}
```

### 6. Chat with AI (Mock)

```bash
POST https://localhost:5001/api/aichat/chat
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "message": "Help me organize my project tasks",
  "projectId": "<optional-project-id>"
}
```

## Project Structure

```
ai-manager-dotnetcore/
├── docs/                              # Documentation and SOP files
│   ├── TECHNICAL_DOCUMENTATION.md     # Complete technical documentation (SOP)
│   ├── database-setup.sql             # Database schema script
│   ├── setup-runtime.sh               # Runtime setup script
│   └── ...                            # Other documentation files
├── src/
│   ├── AIProjectManager.API/          # API layer (controllers, middleware, Program.cs)
│   ├── AIProjectManager.Application/  # Application layer (services, DTOs, validators)
│   ├── AIProjectManager.Domain/       # Domain layer (entities, interfaces)
│   └── AIProjectManager.Infrastructure/ # Infrastructure layer (EF Core, repositories, JWT)
├── AIProjectManager.sln               # Solution file
└── README.md                          # This file
```

## Documentation

All project documentation, setup guides, and Standard Operating Procedures (SOP) are located in the `docs/` folder:

- **Complete Technical Documentation:** `docs/TECHNICAL_DOCUMENTATION.md` - Full project SOP
- **Database Setup:** `docs/database-setup.sql` - SQL script to recreate database
- **Setup Guides:** See `docs/` folder for installation and setup instructions

For detailed information, see the [Documentation Index](docs/README.md).

## Multi-Tenancy

Every entity (except Tenant itself) includes a `TenantId` field. The repository pattern automatically filters all queries by `TenantId`, ensuring complete tenant isolation. The JWT token includes the `tenantId` claim, which is extracted in controllers to ensure users can only access their tenant's data.

## Security Considerations

1. **JWT Secret Key**: Must be at least 32 characters and stored securely in production
2. **Password Hashing**: Uses BCrypt with secure defaults
3. **Tenant Isolation**: Enforced at the repository level
4. **Input Validation**: All requests validated using FluentValidation
5. **Error Handling**: Sensitive errors are not exposed to clients

## Future Enhancements

- [ ] OpenAI integration for AI chat endpoint
- [ ] Unit and integration tests
- [ ] API rate limiting
- [ ] Email verification
- [ ] Password reset functionality
- [ ] File upload support
- [ ] Real-time notifications (SignalR)
- [ ] Caching layer (Redis)
- [ ] Background job processing (Hangfire/Quartz)

## Contributing

1. Follow Clean Architecture principles
2. Maintain separation of concerns
3. Write meaningful commit messages
4. Ensure all validations pass
5. Update documentation as needed

## License

[Your License Here]

