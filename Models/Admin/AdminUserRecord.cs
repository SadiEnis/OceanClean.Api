namespace OceanClean.Api.Models.Admin;

public class AdminUserRecord
{
    public ulong AdminUserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string AdminRole { get; set; } = string.Empty;
    public string AdminStatus { get; set; } = string.Empty;
}