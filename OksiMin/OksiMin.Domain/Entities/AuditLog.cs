namespace OksiMin.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public int PerformedByUserId { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties
        public virtual User PerformedBy { get; set; } = null!;
    }
}