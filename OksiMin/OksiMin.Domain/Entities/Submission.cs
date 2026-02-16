using OksiMin.Domain.Enums;

namespace OksiMin.Domain.Entities
{
    public class Submission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? LandmarkDirections { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Tags { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
        public string? ReviewNotes { get; set; }
        public int? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? SubmitterEmail { get; set; }
        public string? SubmitterIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;

        public virtual User? ReviewedBy { get; set; }
    }
}