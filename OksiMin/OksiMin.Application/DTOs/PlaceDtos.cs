using OksiMin.Domain.Enums;

namespace OksiMin.Application.DTOs
{
    /// <summary>
    /// Response DTO for place (public view)
    /// </summary>
    public record PlaceResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string? Description { get; init; }
        public string? LandmarkDirections { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public string? Tags { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    /// <summary>
    /// List response for places
    /// </summary>
    public record PlaceListResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public string CategoryName { get; init; } = string.Empty;
        public bool HasEmbedding { get; init; }
    }

    /// <summary>
    /// Request DTO for updating a place
    /// </summary>
    public record UpdatePlaceRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public int CategoryId { get; init; }
        public string? Address { get; init; }
        public string? Description { get; init; }
        public string? LandmarkDirections { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public string? Tags { get; init; }
        public PlaceStatus Status { get; init; }
    }
}