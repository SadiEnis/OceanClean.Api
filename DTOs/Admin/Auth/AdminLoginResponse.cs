namespace OceanClean.Api.DTOs.Admin.Auth;

public class AdminLoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }

    public AdminUserDto? Admin { get; set; }
}