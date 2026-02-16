using OksiMin.Application.Common;
using OksiMin.Application.DTOs;
using OksiMin.Application.Interfaces;
using OksiMin.Domain.Entities;
using OksiMin.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace OksiMin.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(
            IApplicationDbContext context,
            ILogger<SubmissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<SubmissionResponse>> CreateSubmissionAsync(
            CreateSubmissionRequest request,
            string? ipAddress)
        {
            try
            {
                _logger.LogInformation(
                    "Creating submission: {Name} in {Municipality}, CategoryId: {CategoryId}",
                    request.Name,
                    request.Municipality,
                    request.CategoryId);

                // Verify category exists
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == request.CategoryId);

                if (!categoryExists)
                {
                    _logger.LogWarning(
                        "Invalid category ID: {CategoryId} for submission: {Name}",
                        request.CategoryId,
                        request.Name);
                    return Result<SubmissionResponse>.Failure("Invalid category");
                }

                // Create submission entity
                var submission = new Submission
                {
                    Name = request.Name.Trim(),
                    Municipality = request.Municipality.Trim(),
                    CategoryId = request.CategoryId,
                    Address = request.Address?.Trim(),
                    Description = request.Description?.Trim(),
                    LandmarkDirections = request.LandmarkDirections?.Trim(),
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Tags = request.Tags?.Trim(),
                    SubmitterEmail = request.SubmitterEmail?.Trim(),
                    SubmitterIpAddress = ipAddress,
                    Status = SubmissionStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Submission created successfully. SubmissionId: {SubmissionId}, Name: {Name}, Status: {Status}",
                    submission.Id,
                    submission.Name,
                    submission.Status);

                // Get category name for response
                var category = await _context.Categories
                    .FirstAsync(c => c.Id == request.CategoryId);

                var response = new SubmissionResponse
                {
                    Id = submission.Id,
                    Name = submission.Name,
                    Municipality = submission.Municipality,
                    CategoryId = submission.CategoryId,
                    CategoryName = category.Name,
                    Address = submission.Address,
                    Description = submission.Description,
                    LandmarkDirections = submission.LandmarkDirections,
                    Latitude = submission.Latitude,
                    Longitude = submission.Longitude,
                    Tags = submission.Tags,
                    Status = submission.Status,
                    ReviewNotes = null,
                    CreatedAt = submission.CreatedAt
                };

                return Result<SubmissionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating submission. Name: {Name}, Municipality: {Municipality}",
                    request.Name,
                    request.Municipality);
                return Result<SubmissionResponse>.Failure(
                    "An error occurred while creating the submission. Please try again later.");
            }
        }

        public async Task<Result<SubmissionDetailResponse>> GetSubmissionByIdAsync(int id)
        {
            try
            {
                _logger.LogDebug("Fetching submission {SubmissionId}", id);

                var submission = await _context.Submissions
                    .Include(s => s.Category)
                    .Include(s => s.ReviewedBy)
                    .Where(s => s.Id == id)
                    .Select(s => new SubmissionDetailResponse
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Municipality = s.Municipality,
                        CategoryId = s.CategoryId,
                        CategoryName = s.Category.Name,
                        Address = s.Address,
                        Description = s.Description,
                        LandmarkDirections = s.LandmarkDirections,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        Tags = s.Tags,
                        Status = s.Status,
                        ReviewNotes = s.ReviewNotes,
                        ReviewedByUserId = s.ReviewedByUserId,
                        ReviewedByUsername = s.ReviewedBy != null ? s.ReviewedBy.Username : null,
                        ReviewedAt = s.ReviewedAt,
                        SubmitterEmail = s.SubmitterEmail,
                        SubmitterIpAddress = s.SubmitterIpAddress,
                        CreatedAt = s.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (submission == null)
                {
                    _logger.LogWarning("Submission not found: {SubmissionId}", id);
                    return Result<SubmissionDetailResponse>.Failure($"Submission with ID {id} not found");
                }

                return Result<SubmissionDetailResponse>.Success(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submission {SubmissionId}", id);
                return Result<SubmissionDetailResponse>.Failure("An error occurred while retrieving the submission");
            }
        }

        public async Task<Result<List<SubmissionListResponse>>> GetSubmissionsByStatusAsync(
            SubmissionStatus? status = null)
        {
            try
            {
                _logger.LogDebug("Fetching submissions with status filter: {Status}", status?.ToString() ?? "All");

                var query = _context.Submissions
                    .Include(s => s.Category)
                    .AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(s => s.Status == status.Value);
                }

                var submissions = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new SubmissionListResponse
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Municipality = s.Municipality,
                        CategoryName = s.Category.Name,
                        Status = s.Status,
                        CreatedAt = s.CreatedAt,
                        SubmitterEmail = s.SubmitterEmail
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} submissions with status filter: {Status}",
                    submissions.Count,
                    status?.ToString() ?? "All");

                return Result<List<SubmissionListResponse>>.Success(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submissions");
                return Result<List<SubmissionListResponse>>.Failure("An error occurred while retrieving submissions");
            }
        }

        public async Task<Result<int>> GetPendingCountAsync()
        {
            try
            {
                var count = await _context.Submissions
                    .CountAsync(s => s.Status == SubmissionStatus.Pending);

                _logger.LogDebug("Pending submissions count: {Count}", count);

                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending submissions count");
                return Result<int>.Failure("An error occurred while counting pending submissions");
            }
        }
    }
}
