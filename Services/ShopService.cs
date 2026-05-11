using OceanClean.Api.DTOs.Shop;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class ShopService
{
    private readonly ShopRepository  _shopRepository;

    public ShopService(ShopRepository shopRepository)
    {
        _shopRepository = shopRepository;
    }
    
    public async Task<ShopItemsResponse> GetActiveShopItemsAsync()
    {
        var records = await _shopRepository.GetActiveShopItemsAsync();

        return new ShopItemsResponse
        {
            Success = true,
            Message = "Shop items retrieved successfully.",
            Items = records.Select(item => new ShopItemResponse
            {
                ShopItemId = item.ShopItemId,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemDescription = item.ItemDescription,
                ItemType = item.ItemType,
                PriceSoftCurrency = item.PriceSoftCurrency,
                MaxQuantity = item.MaxQuantity,
                IsActive = item.IsActive
            }).ToList()
        };
    }
}