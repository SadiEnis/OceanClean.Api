namespace OceanClean.Api.DTOs.Admin.Dashboard;

public class AdminDashboardActivityQueryRequest
{
    public string Range { get; set; } = "weekly";

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}