namespace OceanClean.Api.DTOs.Matches;

public class UsedItemRequest
{
    public ulong UserId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public uint Quantity { get; set; }
}