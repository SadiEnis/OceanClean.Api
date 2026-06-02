namespace OceanClean.Api.DTOs.Admin.Players;

public class AdminUpdatePlayerStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public ulong UserId { get; set; }
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
}