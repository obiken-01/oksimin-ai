using OksiMin.Application.Common;
using OksiMin.Application.DTOs;

namespace OksiMin.Application.Interfaces
{
    public interface ICategoryService
    {
        /// <summary>
        /// Get all categories
        /// </summary>
        Task<Result<List<CategoryResponse>>> GetAllCategoriesAsync();

        /// <summary>
        /// Get category by ID
        /// </summary>
        Task<Result<CategoryResponse>> GetCategoryByIdAsync(int id);

        /// <summary>
        /// Get category with statistics (admin view)
        /// </summary>
        Task<Result<CategoryDetailResponse>> GetCategoryDetailAsync(int id);
    }
}