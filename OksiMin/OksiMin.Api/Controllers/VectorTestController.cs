using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OksiMin.Domain.Entities;
using OksiMin.Domain.Enums;
using OksiMin.Infrastructure.Data;
using OksiMin.Infrastructure.Helpers;

namespace OksiMin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VectorTestController : ControllerBase
    {
        private readonly OksiMinDbContext _context;
        private readonly ILogger<VectorTestController> _logger;

        public VectorTestController(
            OksiMinDbContext context,
            ILogger<VectorTestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Test 1: Create a test place with mock embedding
        /// </summary>
        [HttpPost("create-test-place")]
        [ProducesResponseType(typeof(VectorTestResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTestPlace([FromQuery] int dimensions = 768)
        {
            try
            {
                _logger.LogInformation("Creating test place with {Dimensions}-dimensional vector", dimensions);

                // Generate random vector
                var vector = VectorHelper.GenerateRandomVector(dimensions, seed: 42);
                var embedding = VectorHelper.FloatArrayToBytes(vector);

                // Create test place
                var place = new Place
                {
                    Name = $"Test Place - Vector {dimensions}D",
                    Municipality = "Mamburao",
                    CategoryId = 3, // Tourist Spot
                    Description = $"Test place for vector storage with {dimensions} dimensions",
                    Address = "Test Address",
                    Embedding = embedding,
                    Status = PlaceStatus.Approved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Places.Add(place);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Test place created successfully. PlaceId: {PlaceId}, EmbeddingSize: {Size} bytes",
                    place.Id,
                    embedding.Length);

                return CreatedAtAction(nameof(GetTestPlace), new { id = place.Id }, new VectorTestResponse
                {
                    PlaceId = place.Id,
                    PlaceName = place.Name,
                    Dimensions = dimensions,
                    EmbeddingSizeBytes = embedding.Length,
                    Message = $"Test place created with {dimensions}-dimensional vector"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create test place");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test 2: Retrieve and verify embedding
        /// </summary>
        [HttpGet("test-place/{id}")]
        [ProducesResponseType(typeof(VectorRetrievalResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTestPlace(int id)
        {
            try
            {
                var place = await _context.Places
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();

                if (place == null)
                    return NotFound(new { message = $"Place with ID {id} not found" });

                if (place.Embedding == null)
                    return Ok(new { message = "Place found but has no embedding" });

                // Convert bytes back to vector
                var vector = VectorHelper.BytesToFloatArray(place.Embedding);

                _logger.LogInformation(
                    "Retrieved place {PlaceId} with embedding. Dimensions: {Dimensions}",
                    place.Id,
                    vector.Length);

                return Ok(new VectorRetrievalResponse
                {
                    PlaceId = place.Id,
                    PlaceName = place.Name,
                    Municipality = place.Municipality,
                    HasEmbedding = true,
                    EmbeddingSizeBytes = place.Embedding.Length,
                    Dimensions = vector.Length,
                    FirstFiveValues = vector.Take(5).ToArray(),
                    LastFiveValues = vector.TakeLast(5).ToArray(),
                    VectorMagnitude = CalculateMagnitude(vector)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve test place");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test 3: Test similarity search between vectors
        /// </summary>
        [HttpPost("test-similarity")]
        [ProducesResponseType(typeof(SimilarityTestResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> TestSimilarity([FromQuery] int dimensions = 768)
        {
            try
            {
                _logger.LogInformation("Testing vector similarity with {Dimensions} dimensions", dimensions);

                // Create two similar vectors
                var vector1 = VectorHelper.GenerateRandomVector(dimensions, seed: 100);
                var vector2 = VectorHelper.GenerateRandomVector(dimensions, seed: 100); // Same seed = identical
                var vector3 = VectorHelper.GenerateRandomVector(dimensions, seed: 200); // Different seed = different

                // Store them
                var place1 = new Place
                {
                    Name = "Similarity Test Place 1",
                    Municipality = "Mamburao",
                    CategoryId = 3,
                    Description = "First test place for similarity",
                    Embedding = VectorHelper.FloatArrayToBytes(vector1),
                    Status = PlaceStatus.Approved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var place2 = new Place
                {
                    Name = "Similarity Test Place 2",
                    Municipality = "Sablayan",
                    CategoryId = 3,
                    Description = "Second test place for similarity",
                    Embedding = VectorHelper.FloatArrayToBytes(vector3),
                    Status = PlaceStatus.Approved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Places.AddRange(place1, place2);
                await _context.SaveChangesAsync();

                // Calculate similarities
                var similarity1to2 = VectorHelper.CosineSimilarity(vector1, vector2);
                var similarity1to3 = VectorHelper.CosineSimilarity(vector1, vector3);

                _logger.LogInformation(
                    "Similarity test completed. Same vectors: {Sim1}, Different vectors: {Sim2}",
                    similarity1to2,
                    similarity1to3);

                return Ok(new SimilarityTestResponse
                {
                    Place1Id = place1.Id,
                    Place2Id = place2.Id,
                    Dimensions = dimensions,
                    SimilarityIdenticalVectors = similarity1to2,
                    SimilarityDifferentVectors = similarity1to3,
                    Message = $"Identical vectors should have similarity ~1.0 (actual: {similarity1to2:F4}), " +
                             $"Different vectors should have lower similarity (actual: {similarity1to3:F4})"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test similarity");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test 4: Find similar places (mock similarity search)
        /// </summary>
        [HttpGet("find-similar/{placeId}")]
        [ProducesResponseType(typeof(List<SimilarPlaceResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindSimilarPlaces(int placeId, [FromQuery] int topK = 5)
        {
            try
            {
                // Get the query place
                var queryPlace = await _context.Places
                    .Where(p => p.Id == placeId && p.Embedding != null)
                    .FirstOrDefaultAsync();

                if (queryPlace == null)
                    return NotFound(new { message = "Place not found or has no embedding" });

                var queryVector = VectorHelper.BytesToFloatArray(queryPlace.Embedding!);

                // Get all places with embeddings (in production, you'd use vector search in SQL)
                var allPlaces = await _context.Places
                    .Where(p => p.Id != placeId && p.Embedding != null)
                    .ToListAsync();

                _logger.LogInformation(
                    "Finding similar places to {PlaceId}. Comparing against {Count} places",
                    placeId,
                    allPlaces.Count);

                // Calculate similarities (in-memory, for testing only)
                var similarities = allPlaces.Select(place =>
                {
                    var placeVector = VectorHelper.BytesToFloatArray(place.Embedding!);
                    var similarity = VectorHelper.CosineSimilarity(queryVector, placeVector);

                    return new SimilarPlaceResult
                    {
                        PlaceId = place.Id,
                        PlaceName = place.Name,
                        Municipality = place.Municipality,
                        Description = place.Description,
                        SimilarityScore = similarity
                    };
                })
                .OrderByDescending(x => x.SimilarityScore)
                .Take(topK)
                .ToList();

                return Ok(similarities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find similar places");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test 5: Get statistics about embeddings in database
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(EmbeddingStatistics), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var placesWithEmbeddings = await _context.Places
                    .Where(p => p.Embedding != null)
                    .Select(p => new { p.Id, EmbeddingSize = p.Embedding!.Length })
                    .ToListAsync();

                var totalPlaces = await _context.Places.CountAsync();

                var stats = new EmbeddingStatistics
                {
                    TotalPlaces = totalPlaces,
                    PlacesWithEmbeddings = placesWithEmbeddings.Count,
                    PlacesWithoutEmbeddings = totalPlaces - placesWithEmbeddings.Count,
                    AverageEmbeddingSizeBytes = placesWithEmbeddings.Any()
                        ? (int)placesWithEmbeddings.Average(p => p.EmbeddingSize)
                        : 0,
                    MinEmbeddingSizeBytes = placesWithEmbeddings.Any()
                        ? placesWithEmbeddings.Min(p => p.EmbeddingSize)
                        : 0,
                    MaxEmbeddingSizeBytes = placesWithEmbeddings.Any()
                        ? placesWithEmbeddings.Max(p => p.EmbeddingSize)
                        : 0
                };

                if (stats.AverageEmbeddingSizeBytes > 0)
                {
                    stats.EstimatedDimensions = stats.AverageEmbeddingSizeBytes / sizeof(float);
                }

                _logger.LogInformation(
                    "Embedding statistics: {Total} total places, {WithEmbeddings} with embeddings",
                    stats.TotalPlaces,
                    stats.PlacesWithEmbeddings);

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get embedding statistics");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cleanup: Delete all test places
        /// </summary>
        [HttpDelete("cleanup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CleanupTestData()
        {
            try
            {
                var testPlaces = await _context.Places
                    .Where(p => p.Name.StartsWith("Test Place") ||
                               p.Name.StartsWith("Similarity Test"))
                    .ToListAsync();

                if (!testPlaces.Any())
                    return Ok(new { message = "No test places found", deletedCount = 0 });

                _context.Places.RemoveRange(testPlaces);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} test places", testPlaces.Count);

                return Ok(new { message = $"Deleted {testPlaces.Count} test places", deletedCount = testPlaces.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup test data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private double CalculateMagnitude(float[] vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }
            return Math.Sqrt(sum);
        }
    }

    // Response DTOs
    public class VectorTestResponse
    {
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public int Dimensions { get; set; }
        public int EmbeddingSizeBytes { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class VectorRetrievalResponse
    {
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public bool HasEmbedding { get; set; }
        public int EmbeddingSizeBytes { get; set; }
        public int Dimensions { get; set; }
        public float[] FirstFiveValues { get; set; } = Array.Empty<float>();
        public float[] LastFiveValues { get; set; } = Array.Empty<float>();
        public double VectorMagnitude { get; set; }
    }

    public class SimilarityTestResponse
    {
        public int Place1Id { get; set; }
        public int Place2Id { get; set; }
        public int Dimensions { get; set; }
        public double SimilarityIdenticalVectors { get; set; }
        public double SimilarityDifferentVectors { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SimilarPlaceResult
    {
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double SimilarityScore { get; set; }
    }

    public class EmbeddingStatistics
    {
        public int TotalPlaces { get; set; }
        public int PlacesWithEmbeddings { get; set; }
        public int PlacesWithoutEmbeddings { get; set; }
        public int AverageEmbeddingSizeBytes { get; set; }
        public int MinEmbeddingSizeBytes { get; set; }
        public int MaxEmbeddingSizeBytes { get; set; }
        public int EstimatedDimensions { get; set; }
    }
}
