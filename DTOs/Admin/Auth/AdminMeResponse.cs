namespace OceanClean.Api.DTOs.Admin.Auth;

public class AdminMeResponse
{
    public ulong AdminUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public string AdminStatus { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}