using Microsoft.AspNetCore.Mvc;
using OceanClean.Api.Data;

namespace OceanClean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly MySqlConnectionFactory _connectionFactory;

    public DatabaseController(MySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Ping()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1;";
        
        var result = await command.ExecuteScalarAsync();
        return Ok(new
        {
            connected = true,
            result,
            timestamp = DateTime.UtcNow
        });
    }
}