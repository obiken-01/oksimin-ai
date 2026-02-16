using Microsoft.EntityFrameworkCore;
using OksiMin.Application.Interfaces;
using OksiMin.Domain.Entities;

namespace OksiMin.Infrastructure.Data
{
    public class OksiMinDbContext : DbContext, IApplicationDbContext
    {
        public OksiMinDbContext(DbContextOptions<OksiMinDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Place> Places => Set<Place>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<User> Users => Set<User>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // Category Configuration
            // ============================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            // ============================================
            // Place Configuration
            // ============================================
            modelBuilder.Entity<Place>(entity =>
            {
                entity.ToTable("Places");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Municipality)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Address)
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.Property(e => e.LandmarkDirections)
                    .HasMaxLength(1000);

                entity.Property(e => e.Latitude)
                    .HasPrecision(10, 8);

                entity.Property(e => e.Longitude)
                    .HasPrecision(11, 8);

                entity.Property(e => e.Tags)
                    .HasMaxLength(500);

                // Vector embedding as binary (SQL Server 2025 will have native vector type)
                entity.Property(e => e.Embedding)
                    .HasColumnType("varbinary(max)");

                // Relationships
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Places)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.CreatedPlaces)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for performance
                entity.HasIndex(e => e.Municipality);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Status);
            });

            // ============================================
            // Submission Configuration
            // ============================================
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.ToTable("Submissions");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Municipality)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Address)
                    .HasMaxLength(300);

                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.Property(e => e.LandmarkDirections)
                    .HasMaxLength(1000);

                entity.Property(e => e.Latitude)
                    .HasPrecision(10, 8);

                entity.Property(e => e.Longitude)
                    .HasPrecision(11, 8);

                entity.Property(e => e.Tags)
                    .HasMaxLength(500);

                entity.Property(e => e.ReviewNotes)
                    .HasMaxLength(1000);

                entity.Property(e => e.SubmitterEmail)
                    .HasMaxLength(100);

                entity.Property(e => e.SubmitterIpAddress)
                    .HasMaxLength(45);

                // Relationships
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Submissions)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReviewedBy)
                    .WithMany(u => u.ReviewedSubmissions)
                    .HasForeignKey(e => e.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // ============================================
            // User Configuration
            // ============================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasIndex(e => e.Username)
                    .IsUnique();
            });

            // ============================================
            // AuditLog Configuration
            // ============================================
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EntityType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Details)
                    .HasMaxLength(2000);

                entity.HasOne(e => e.PerformedBy)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.PerformedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
            });

            // ============================================
            // Seed Data
            // ============================================
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Restaurant",
                    Description = "Food and dining establishments",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 2,
                    Name = "Hotel",
                    Description = "Accommodation and lodging",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 3,
                    Name = "Tourist Spot",
                    Description = "Tourist attractions and destinations",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 4,
                    Name = "Government Office",
                    Description = "Government offices and services",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 5,
                    Name = "Healthcare",
                    Description = "Hospitals and clinics",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 6,
                    Name = "Shopping",
                    Description = "Stores and markets",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Category
                {
                    Id = 7,
                    Name = "Transportation",
                    Description = "Bus terminals, ports, airports",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }
    }
}