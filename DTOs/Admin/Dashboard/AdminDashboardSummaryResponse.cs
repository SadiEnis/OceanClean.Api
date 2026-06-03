namespace OceanClean.Api.DTOs.Admin.Dashboard;

public class AdminDashboardSummaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public AdminDashboardSummaryDto Summary { get; set; } = new();
}