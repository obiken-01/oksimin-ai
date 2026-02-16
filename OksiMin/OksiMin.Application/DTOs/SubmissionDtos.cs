using OksiMin.Domain.Enums;

namespace OksiMin.Application.DTOs
{
    /// <summary>
    /// Request DTO for creating a new submission
    /// </summary>
    public record CreateSubmissionRequest
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
        public string? SubmitterEmail { get; init; }
    }

    /// <summary>
    /// Response DTO for submission
    /// </summary>
    public record SubmissionResponse
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
        public SubmissionStatus Status { get; init; }
        public string? ReviewNotes { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    /// <summary>
    /// Response DTO for submission list (admin view)
    /// </summary>
    public record SubmissionListResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public string CategoryName { get; init; } = string.Empty;
        public SubmissionStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? SubmitterEmail { get; init; }
    }

    /// <summary>
    /// Detailed submission response (admin view)
    /// </summary>
    public record SubmissionDetailResponse
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
        public SubmissionStatus Status { get; init; }
        public string? ReviewNotes { get; init; }
        public int? ReviewedByUserId { get; init; }
        public string? ReviewedByUsername { get; init; }
        public DateTime? ReviewedAt { get; init; }
        public string? SubmitterEmail { get; init; }
        public string? SubmitterIpAddress { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}