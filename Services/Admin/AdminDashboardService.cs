using OceanClean.Api.DTOs.Admin.Dashboard;
using OceanClean.Api.Repositories.Admin;

namespace OceanClean.Api.Services.Admin;

public class AdminDashboardService
{
    private readonly AdminDashboardRepository _adminDashboardRepository;

    public AdminDashboardService(AdminDashboardRepository adminDashboardRepository)
    {
        _adminDashboardRepository = adminDashboardRepository;
    }

    public async Task<AdminDashboardSummaryResponse> GetSummaryAsync()
    {
        var summary = await _adminDashboardRepository.GetSummaryAsync();

        return new AdminDashboardSummaryResponse
        {
            Success = true,
            Message = "Dashboard summary retrieved successfully.",
            Summary = summary
        };
    }
}