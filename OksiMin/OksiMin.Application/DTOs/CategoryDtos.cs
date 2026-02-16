namespace OksiMin.Application.DTOs
{
    /// <summary>
    /// Response DTO for category
    /// </summary>
    public record CategoryResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }

    /// <summary>
    /// Detailed category response with statistics
    /// </summary>
    public record CategoryDetailResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int ApprovedPlacesCount { get; init; }
        public int PendingSubmissionsCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}