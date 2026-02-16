using OksiMin.Application.Common;
using OksiMin.Application.DTOs;

namespace OksiMin.Application.Interfaces
{
    public interface IPlaceService
    {
        /// <summary>
        /// Get all approved places
        /// </summary>
        Task<Result<List<PlaceListResponse>>> GetAllPlacesAsync();

        /// <summary>
        /// Get place by ID
        /// </summary>
        Task<Result<PlaceResponse>> GetPlaceByIdAsync(int id);

        /// <summary>
        /// Get places by municipality
        /// </summary>
        Task<Result<List<PlaceListResponse>>> GetPlacesByMunicipalityAsync(string municipality);

        /// <summary>
        /// Get places by category
        /// </summary>
        Task<Result<List<PlaceListResponse>>> GetPlacesByCategoryAsync(int categoryId);

        /// <summary>
        /// Search places by name or description
        /// </summary>
        Task<Result<List<PlaceListResponse>>> SearchPlacesAsync(string searchTerm);
    }
}