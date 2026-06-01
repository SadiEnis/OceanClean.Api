namespace OceanClean.Api.DTOs.Admin.Auth;

public class AdminLogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}