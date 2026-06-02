namespace OceanClean.Api.Models.Admin;

public class AdminPlayerListItemRecord
{
    public ulong UserId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PlayerStatus { get; set; } = string.Empty;

    public uint TotalPoints { get; set; }
    public uint TotalMatches { get; set; }
    public uint SoftCurrency { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}