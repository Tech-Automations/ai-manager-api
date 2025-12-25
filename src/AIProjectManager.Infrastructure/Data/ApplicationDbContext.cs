using AIProjectManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIProjectManager.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<AIInteractionLog> AIInteractionLogs { get; set; } = null!;
    public DbSet<ManagerStyleProfile> ManagerStyleProfiles { get; set; } = null!;
    public DbSet<ChatSession> ChatSessions { get; set; } = null!;
    public DbSet<Integration> Integrations { get; set; } = null!;
    public DbSet<ExternalWorkItem> ExternalWorkItems { get; set; } = null!;
    public DbSet<GitRepository> GitRepositories { get; set; } = null!;
    public DbSet<GitCommit> GitCommits { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(50);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Projects)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Projects)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TaskItem configuration
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedTo)
                .WithMany(u => u.Tasks)
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AIInteractionLog configuration
        modelBuilder.Entity<AIInteractionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserPrompt).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.AIResponse).HasMaxLength(8000);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TaskItem)
                .WithMany()
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ManagerStyleProfile configuration
        modelBuilder.Entity<ManagerStyleProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Tone).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.User)
                .WithOne(u => u.StyleProfile)
                .HasForeignKey<ManagerStyleProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatSession configuration
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Response).HasMaxLength(10000);
            entity.Property(e => e.Sources).HasMaxLength(2000);
            entity.Property(e => e.Model).HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ChatSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentSession)
                .WithMany(p => p.FollowUps)
                .HasForeignKey(e => e.ParentSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // Integration configuration
        modelBuilder.Entity<Integration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Configuration).HasMaxLength(5000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            
            entity.HasIndex(e => new { e.TenantId, e.Type });
        });

        // ExternalWorkItem configuration
        modelBuilder.Entity<ExternalWorkItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AssignedTo).HasMaxLength(255);
            entity.Property(e => e.ExternalUrl).HasMaxLength(1000);

            entity.HasOne(e => e.Integration)
                .WithMany()
                .HasForeignKey(e => e.IntegrationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TaskItem)
                .WithMany()
                .HasForeignKey(e => e.TaskItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.IntegrationId, e.ExternalId });
            entity.HasIndex(e => e.TaskItemId);
        });

        // GitRepository configuration
        modelBuilder.Entity<GitRepository>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RepositoryUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AccessToken).HasMaxLength(500); // Encrypted

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.ProjectId);
        });

        // GitCommit configuration
        modelBuilder.Entity<GitCommit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommitHash).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AuthorEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Branch).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Repository)
                .WithMany(r => r.Commits)
                .HasForeignKey(e => e.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.RepositoryId, e.CommittedAt });
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.CommitHash);
        });
    }
}

