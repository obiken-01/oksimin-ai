using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OksiMin.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for creating DbContext during migrations.
    /// This bypasses Program.cs and avoids startup issues during migration commands.
    /// </summary>
    public class OksiMinDbContextFactory : IDesignTimeDbContextFactory<OksiMinDbContext>
    {
        public OksiMinDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OksiMinDbContext>();

            // Connection string for design-time only (migrations)
            // Adjust to match your SQL Server setup
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=OksiMinDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
                sqlOptions => sqlOptions.EnableRetryOnFailure());

            return new OksiMinDbContext(optionsBuilder.Options);
        }
    }
}