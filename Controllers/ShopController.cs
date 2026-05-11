using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.DTOs.Shop;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopController : ControllerBase
{
    private readonly ShopService  _shopService;
    public ShopController(ShopService shopService)
    {
        _shopService = shopService;
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetItems()
    {
        var response = await _shopService.GetActiveShopItemsAsync();
        return Ok(response);
    }
    
    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase(PurchaseItemRequest request)
    {
        var response = await _shopService.PurchaseItemAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}