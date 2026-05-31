namespace OceanClean.Api.DTOs.Admin.Auth;

public class AdminUserDto
{
    public ulong AdminUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}