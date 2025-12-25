-- ============================================================================
-- AI Project Manager - Database Setup Script
-- ============================================================================
-- This script recreates the complete database structure from scratch.
-- Use this script if migrations are lost or you need to recreate the database.
--
-- Usage:
--   psql -d AIProjectManagerDb -f database-setup.sql
--
-- Or to recreate from scratch:
--   dropdb AIProjectManagerDb
--   createdb AIProjectManagerDb
--   psql -d AIProjectManagerDb -f database-setup.sql
-- ============================================================================

-- Enable UUID extension (if not already enabled)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================================
-- Table: Tenants
-- ============================================================================
CREATE TABLE "Tenants" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Subdomain" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "SubscriptionExpiresAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedBy" UUID NULL,
    "UpdatedBy" UUID NULL,
    CONSTRAINT "PK_Tenants" PRIMARY KEY ("Id")
);

-- Unique index on Subdomain
CREATE UNIQUE INDEX "IX_Tenants_Subdomain" ON "Tenants" ("Subdomain");

-- ============================================================================
-- Table: Users
-- ============================================================================
CREATE TABLE "Users" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Email" VARCHAR(255) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Role" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedBy" UUID NULL,
    "UpdatedBy" UUID NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Tenants_TenantId" FOREIGN KEY ("TenantId") 
        REFERENCES "Tenants" ("Id") ON DELETE RESTRICT
);

-- Unique index on Email and TenantId combination
CREATE UNIQUE INDEX "IX_Users_Email_TenantId" ON "Users" ("Email", "TenantId");

-- ============================================================================
-- Table: Projects
-- ============================================================================
CREATE TABLE "Projects" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000) NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Active',
    "OwnerId" UUID NOT NULL,
    "StartDate" TIMESTAMP WITHOUT TIME ZONE NULL,
    "EndDate" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedBy" UUID NULL,
    "UpdatedBy" UUID NULL,
    CONSTRAINT "PK_Projects" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Projects_Tenants_TenantId" FOREIGN KEY ("TenantId") 
        REFERENCES "Tenants" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Projects_Users_OwnerId" FOREIGN KEY ("OwnerId") 
        REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

-- ============================================================================
-- Table: Tasks (TaskItems)
-- ============================================================================
CREATE TABLE "Tasks" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000) NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Todo',
    "Priority" VARCHAR(50) NOT NULL DEFAULT 'Medium',
    "ProjectId" UUID NOT NULL,
    "AssignedToId" UUID NULL,
    "DueDate" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CompletedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedBy" UUID NULL,
    "UpdatedBy" UUID NULL,
    CONSTRAINT "PK_Tasks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Tasks_Tenants_TenantId" FOREIGN KEY ("TenantId") 
        REFERENCES "Tenants" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Tasks_Projects_ProjectId" FOREIGN KEY ("ProjectId") 
        REFERENCES "Projects" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Tasks_Users_AssignedToId" FOREIGN KEY ("AssignedToId") 
        REFERENCES "Users" ("Id") ON DELETE SET NULL
);

-- ============================================================================
-- Table: AIInteractionLogs
-- ============================================================================
CREATE TABLE "AIInteractionLogs" (
    "Id" UUID NOT NULL,
    "TenantId" UUID NOT NULL,
    "ProjectId" UUID NULL,
    "TaskItemId" UUID NULL,
    "UserPrompt" VARCHAR(4000) NOT NULL,
    "AIResponse" VARCHAR(8000) NULL,
    "Model" VARCHAR(100) NULL DEFAULT 'mock',
    "TokenCount" INTEGER NULL,
    "Cost" DECIMAL(18, 2) NULL,
    "ResponseTimeMs" INTEGER NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Success',
    "ErrorMessage" VARCHAR(1000) NULL,
    "CreatedAt" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "UpdatedAt" TIMESTAMP WITHOUT TIME ZONE NULL,
    "CreatedBy" UUID NULL,
    "UpdatedBy" UUID NULL,
    CONSTRAINT "PK_AIInteractionLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AIInteractionLogs_Tenants_TenantId" FOREIGN KEY ("TenantId") 
        REFERENCES "Tenants" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AIInteractionLogs_Projects_ProjectId" FOREIGN KEY ("ProjectId") 
        REFERENCES "Projects" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_AIInteractionLogs_Tasks_TaskItemId" FOREIGN KEY ("TaskItemId") 
        REFERENCES "Tasks" ("Id") ON DELETE SET NULL
);

-- ============================================================================
-- Indexes for better query performance
-- ============================================================================

-- Index on TenantId for all tenant-isolated tables (already covered by FKs, but explicit for clarity)
CREATE INDEX IF NOT EXISTS "IX_Users_TenantId" ON "Users" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Projects_TenantId" ON "Projects" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Projects_OwnerId" ON "Projects" ("OwnerId");
CREATE INDEX IF NOT EXISTS "IX_Tasks_TenantId" ON "Tasks" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Tasks_ProjectId" ON "Tasks" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_Tasks_AssignedToId" ON "Tasks" ("AssignedToId");
CREATE INDEX IF NOT EXISTS "IX_AIInteractionLogs_TenantId" ON "AIInteractionLogs" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_AIInteractionLogs_ProjectId" ON "AIInteractionLogs" ("ProjectId");
CREATE INDEX IF NOT EXISTS "IX_AIInteractionLogs_TaskItemId" ON "AIInteractionLogs" ("TaskItemId");

-- ============================================================================
-- Comments for documentation
-- ============================================================================
COMMENT ON TABLE "Tenants" IS 'Multi-tenant isolation root table. Each tenant represents an organization/customer.';
COMMENT ON TABLE "Users" IS 'Application users. Each user belongs to exactly one tenant.';
COMMENT ON TABLE "Projects" IS 'Projects belong to tenants and have an owner (user).';
COMMENT ON TABLE "Tasks" IS 'Task items within projects. Can be assigned to users.';
COMMENT ON TABLE "AIInteractionLogs" IS 'Logs of all AI chat interactions for auditing and analytics.';

COMMENT ON COLUMN "Tenants"."TenantId" IS 'For Tenants, TenantId equals Id (self-reference for consistency)';
COMMENT ON COLUMN "Tasks"."Status" IS 'Valid values: Todo, InProgress, Done, Blocked';
COMMENT ON COLUMN "Tasks"."Priority" IS 'Valid values: Low, Medium, High, Critical';
COMMENT ON COLUMN "Projects"."Status" IS 'Valid values: Active, Completed, Archived';
COMMENT ON COLUMN "Users"."Role" IS 'Valid values: Admin, User';
COMMENT ON COLUMN "AIInteractionLogs"."Status" IS 'Valid values: Success, Error';

-- ============================================================================
-- Script completed successfully
-- ============================================================================

