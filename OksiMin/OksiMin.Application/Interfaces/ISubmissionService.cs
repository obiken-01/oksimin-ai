using OksiMin.Application.Common;
using OksiMin.Application.DTOs;
using OksiMin.Domain.Enums;

namespace OksiMin.Application.Interfaces
{
    public interface ISubmissionService
    {
        /// <summary>
        /// Create a new submission (anonymous users)
        /// </summary>
        Task<Result<SubmissionResponse>> CreateSubmissionAsync(
            CreateSubmissionRequest request,
            string? ipAddress);

        /// <summary>
        /// Get submission by ID (admin only)
        /// </summary>
        Task<Result<SubmissionDetailResponse>> GetSubmissionByIdAsync(int id);

        /// <summary>
        /// Get submissions by status (admin only)
        /// </summary>
        Task<Result<List<SubmissionListResponse>>> GetSubmissionsByStatusAsync(
            SubmissionStatus? status = null);

        /// <summary>
        /// Get pending submissions count
        /// </summary>
        Task<Result<int>> GetPendingCountAsync();
    }
}