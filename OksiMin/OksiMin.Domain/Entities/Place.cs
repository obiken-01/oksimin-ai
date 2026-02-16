using OksiMin.Domain.Enums;

namespace OksiMin.Domain.Entities
{
    public class Place
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
        public byte[]? Embedding { get; set; }  // Vector embeddings stored as binary
        public PlaceStatus Status { get; set; } = PlaceStatus.Approved;
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;

        public virtual User? CreatedBy { get; set; }
    }
}