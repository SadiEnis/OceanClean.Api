using OceanClean.Api.DTOs.Inventory;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class InventoryService
{
    private readonly InventoryRepository _inventoryRepository;

    public InventoryService(InventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryResponse> GetInventoryAsync(ulong userId)
    {
        var records = await _inventoryRepository.GetInventoryByUserIdAsync(userId);

        return new InventoryResponse
        {
            Success = true,
            Message = "Inventory retrieved successfully.",
            UserId = userId,
            Items = records.Select(item => new InventoryItemResponse
            {
                InventoryId = item.InventoryId,
                ShopItemId = item.ShopItemId,

                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                ItemType = item.ItemType,

                Quantity = item.Quantity,
                MaxQuantity = item.MaxQuantity
            }).ToList()
        };
    }
}