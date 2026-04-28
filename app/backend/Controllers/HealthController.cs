using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    private readonly DatabaseContext _db;

    public HealthController(DatabaseContext db)
    {
        _db = db;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            // Test database connection
            await _db.Database.ExecuteSqlRawAsync("SELECT 1");
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { status = "unhealthy", error = ex.Message });
        }
    }
}
