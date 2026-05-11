using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.Models.Shop;
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
}