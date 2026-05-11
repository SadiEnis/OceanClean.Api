namespace OceanClean.Api.DTOs.Shop;

public class ShopItemsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public List<ShopItemResponse> Items { get; set; } = new();
}