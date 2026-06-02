namespace OceanClean.Api.DTOs.Admin.Matches;

public class AdminMatchUsedItemDto
{
    public ulong LogId { get; set; }

    public ulong UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public ulong? ShopItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? ItemType { get; set; }

    public int? Quantity { get; set; }

    public DateTime CreatedAt { get; set; }
}