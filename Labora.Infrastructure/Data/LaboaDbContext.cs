using Labora.Domain.Entities;
using Labora.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Labora.Infrastructure.Data;

public class LaboaDbContext : DbContext
{
    public LaboaDbContext(DbContextOptions<LaboaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<PushToken> PushTokens { get; set; }
    public DbSet<WorkerPost> WorkerPosts => Set<WorkerPost>();
    public DbSet<WorkerPortfolioImage> WorkerPortfolioImages => Set<WorkerPortfolioImage>();
    public DbSet<SavedJob> SavedJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
        });

        // Job
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Employer)
                .WithMany(u => u.Jobs)
                .HasForeignKey(e => e.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Jobs)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // JobApplication
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Job)
                .WithMany(j => j.JobApplications)
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Worker)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(e => e.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category Seed Data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Name = "Daily", Description = "Kunlik ishlar", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = "Construction", Description = "Qurilish", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = "Driver", Description = "Haydovchi", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = "Chef", Description = "Oshpaz", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = "Medical", Description = "Tibbiyot", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111105"), Name = "Education", Description = "Ta'lim", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111106"), Name = "Finance", Description = "Moliya", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111107"), Name = "Security", Description = "Qorovul", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111108"), Name = "Cleaning", Description = "Tozalik", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111109"), Name = "Design", Description = "Dizayn", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111110"), Name = "Marketing", Description = "Marketing", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Warehouse", Description = "Ombor", IconUrl = "", JobType = JobType.FullTime, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }

        );

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Reviewee)
                .WithMany()
                .HasForeignKey(e => e.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.JobApplication)
                .WithMany()
                .HasForeignKey(e => e.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserPreference
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Preferences)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PreferredCategory)
                .WithMany()
                .HasForeignKey(e => e.PreferredCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PushToken
        modelBuilder.Entity<PushToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.PushTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SavedJob
        modelBuilder.Entity<SavedJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Job)
                .WithMany()
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.JobId }).IsUnique();
        });
    }
}