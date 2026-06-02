namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchDetailResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public AdminMatchDetailDto? Match { get; set; }
}