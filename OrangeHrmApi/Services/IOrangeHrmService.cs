using OrangeHrmApi.Models.DTOs;

namespace OrangeHrmApi.Services
{
    public interface IOrangeHrmService
    {
        Task<(bool success, string? result, string? error)> AddEmployeeAsync(AddEmployeeRequest request);
        Task<(bool success, string? result, string? error)> CreateClaimAsync(CreateClaimRequest request);
    }
}
