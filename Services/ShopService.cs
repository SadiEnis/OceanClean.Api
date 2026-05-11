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
    
    public async Task<PurchaseItemResponse> PurchaseItemAsync(PurchaseItemRequest request)
    {
        var validationError = ValidatePurchaseRequest(request);

        if (validationError != null)
        {
            return new PurchaseItemResponse
            {
                Success = false,
                Message = validationError,
                UserId = request.UserId,
                ShopItemId = request.ShopItemId
            };
        }

        return await _shopRepository.PurchaseItemAsync(request);
    }

    private static string? ValidatePurchaseRequest(PurchaseItemRequest request)
    {
        if (request.UserId == 0)
            return "UserId must be greater than zero.";

        if (request.ShopItemId == 0)
            return "ShopItemId must be greater than zero.";

        if (request.Quantity == 0)
            return "Quantity must be greater than zero.";

        if (request.Quantity > 3)
            return "Quantity cannot be greater than 3 per purchase.";

        return null;
    }
}