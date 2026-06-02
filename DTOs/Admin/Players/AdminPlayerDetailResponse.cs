namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminPlayerDetailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public AdminPlayerDetailDto? Player { get; set; }
}