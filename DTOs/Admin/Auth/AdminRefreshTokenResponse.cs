namespace OceanClean.Api.DTOs.Admin.Auth;

public class AdminRefreshTokenResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public string AccessToken { get; set; } = string.Empty;
    public AdminUserDto? Admin { get; set; }
}