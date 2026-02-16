using Microsoft.EntityFrameworkCore;
using OksiMin.Domain.Entities;

namespace OksiMin.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Category> Categories { get; }
        DbSet<Place> Places { get; }
        DbSet<Submission> Submissions { get; }
        DbSet<User> Users { get; }
        DbSet<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}