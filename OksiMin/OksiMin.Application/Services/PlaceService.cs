using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OksiMin.Application.Common;
using OksiMin.Application.DTOs;
using OksiMin.Application.Interfaces;
using OksiMin.Domain.Enums;

namespace OksiMin.Application.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<PlaceService> _logger;

        public PlaceService(
            IApplicationDbContext context,
            ILogger<PlaceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<PlaceListResponse>>> GetAllPlacesAsync()
        {
            try
            {
                _logger.LogDebug("Fetching all approved places");

                var places = await _context.Places
                    .Include(p => p.Category)
                    .Where(p => p.Status == PlaceStatus.Approved)
                    .OrderBy(p => p.Name)
                    .Select(p => new PlaceListResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Municipality = p.Municipality,
                        CategoryName = p.Category.Name,
                        HasEmbedding = p.Embedding != null
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} approved places", places.Count);

                return Result<List<PlaceListResponse>>.Success(places);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving places");
                return Result<List<PlaceListResponse>>.Failure("An error occurred while retrieving places");
            }
        }

        public async Task<Result<PlaceResponse>> GetPlaceByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("Fetching place {PlaceId}", id);

                var place = await _context.Places
                    .Include(p => p.Category)
                    .Where(p => p.Id == id && p.Status == PlaceStatus.Approved)
                    .Select(p => new PlaceResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Municipality = p.Municipality,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        Address = p.Address,
                        Description = p.Description,
                        LandmarkDirections = p.LandmarkDirections,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        Tags = p.Tags,
                        CreatedAt = p.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (place == null)
                {
                    _logger.LogWarning("Place not found or not approved: {PlaceId}", id);
                    return Result<PlaceResponse>.Failure($"Place with ID {id} not found");
                }

                return Result<PlaceResponse>.Success(place);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving place {PlaceId}", id);
                return Result<PlaceResponse>.Failure("An error occurred while retrieving the place");
            }
        }

        public async Task<Result<List<PlaceListResponse>>> GetPlacesByMunicipalityAsync(string municipality)
        {
            try
            {
                _logger.LogDebug("Fetching places in municipality: {Municipality}", municipality);

                var places = await _context.Places
                    .Include(p => p.Category)
                    .Where(p => p.Municipality.ToLower() == municipality.ToLower() &&
                               p.Status == PlaceStatus.Approved)
                    .OrderBy(p => p.Name)
                    .Select(p => new PlaceListResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Municipality = p.Municipality,
                        CategoryName = p.Category.Name,
                        HasEmbedding = p.Embedding != null
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} places in {Municipality}",
                    places.Count,
                    municipality);

                return Result<List<PlaceListResponse>>.Success(places);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving places for municipality: {Municipality}", municipality);
                return Result<List<PlaceListResponse>>.Failure("An error occurred while retrieving places");
            }
        }

        public async Task<Result<List<PlaceListResponse>>> GetPlacesByCategoryAsync(int categoryId)
        {
            try
            {
                _logger.LogDebug("Fetching places in category: {CategoryId}", categoryId);

                // Verify category exists
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
                if (!categoryExists)
                {
                    return Result<List<PlaceListResponse>>.Failure($"Category with ID {categoryId} not found");
                }

                var places = await _context.Places
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId && p.Status == PlaceStatus.Approved)
                    .OrderBy(p => p.Name)
                    .Select(p => new PlaceListResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Municipality = p.Municipality,
                        CategoryName = p.Category.Name,
                        HasEmbedding = p.Embedding != null
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} places in category {CategoryId}",
                    places.Count,
                    categoryId);

                return Result<List<PlaceListResponse>>.Success(places);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving places for category: {CategoryId}", categoryId);
                return Result<List<PlaceListResponse>>.Failure("An error occurred while retrieving places");
            }
        }

        public async Task<Result<List<PlaceListResponse>>> SearchPlacesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Result<List<PlaceListResponse>>.Failure("Search term cannot be empty");
                }

                _logger.LogDebug("Searching places with term: {SearchTerm}", searchTerm);

                var term = searchTerm.ToLower();

                var places = await _context.Places
                    .Include(p => p.Category)
                    .Where(p => p.Status == PlaceStatus.Approved &&
                               (p.Name.ToLower().Contains(term) ||
                                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                                (p.Tags != null && p.Tags.ToLower().Contains(term))))
                    .OrderBy(p => p.Name)
                    .Select(p => new PlaceListResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Municipality = p.Municipality,
                        CategoryName = p.Category.Name,
                        HasEmbedding = p.Embedding != null
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Found {Count} places matching search term: {SearchTerm}",
                    places.Count,
                    searchTerm);

                return Result<List<PlaceListResponse>>.Success(places);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching places with term: {SearchTerm}", searchTerm);
                return Result<List<PlaceListResponse>>.Failure("An error occurred while searching places");
            }
        }
    }
}