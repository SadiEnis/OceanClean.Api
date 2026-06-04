namespace OceanClean.Api.DTOs.Admin.Events;

public class AdminEventTopActorDto
{
    public ulong UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public long EventCount { get; set; }
}