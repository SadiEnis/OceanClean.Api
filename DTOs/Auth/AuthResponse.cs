namespace OceanClean.Api.DTOs.Auth;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } =  string.Empty;
    
    public ulong? UserId { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
}