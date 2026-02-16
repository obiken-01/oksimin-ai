using OksiMin.Domain.Enums;

namespace OksiMin.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Moderator;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Place> CreatedPlaces { get; set; } = new List<Place>();

        public virtual ICollection<Submission> ReviewedSubmissions { get; set; } = new List<Submission>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}