using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OksiMin.Application.Common;
using OksiMin.Application.DTOs;
using OksiMin.Application.Interfaces;
using OksiMin.Domain.Enums;

namespace OksiMin.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IApplicationDbContext context,
            ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<CategoryResponse>>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogDebug("Fetching all categories");

                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} categories", categories.Count);

                return Result<List<CategoryResponse>>.Success(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return Result<List<CategoryResponse>>.Failure("An error occurred while retrieving categories");
            }
        }

        public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("Fetching category {CategoryId}", id);

                var category = await _context.Categories
                    .Where(c => c.Id == id)
                    .Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    _logger.LogWarning("Category not found: {CategoryId}", id);
                    return Result<CategoryResponse>.Failure($"Category with ID {id} not found");
                }

                return Result<CategoryResponse>.Success(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return Result<CategoryResponse>.Failure("An error occurred while retrieving the category");
            }
        }

        public async Task<Result<CategoryDetailResponse>> GetCategoryDetailAsync(int id)
        {
            try
            {
                _logger.LogDebug("Fetching category details for {CategoryId}", id);

                var category = await _context.Categories
                    .Where(c => c.Id == id)
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    _logger.LogWarning("Category not found: {CategoryId}", id);
                    return Result<CategoryDetailResponse>.Failure($"Category with ID {id} not found");
                }

                // Get counts
                var approvedPlacesCount = await _context.Places
                    .CountAsync(p => p.CategoryId == id && p.Status == PlaceStatus.Approved);

                var pendingSubmissionsCount = await _context.Submissions
                    .CountAsync(s => s.CategoryId == id && s.Status == SubmissionStatus.Pending);

                var response = new CategoryDetailResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ApprovedPlacesCount = approvedPlacesCount,
                    PendingSubmissionsCount = pendingSubmissionsCount,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                return Result<CategoryDetailResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category details {CategoryId}", id);
                return Result<CategoryDetailResponse>.Failure("An error occurred while retrieving category details");
            }
        }
    }
}