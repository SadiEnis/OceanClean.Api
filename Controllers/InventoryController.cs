using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.Services;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetInventory(long userId)
    {
        if (userId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid user id."
            });
        }

        var response = await _inventoryService.GetInventoryAsync((ulong)userId);
        return Ok(response);
    }
}