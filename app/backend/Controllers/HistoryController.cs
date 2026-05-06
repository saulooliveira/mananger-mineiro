using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly PrintHistoryService _historyService;

    public HistoryController(PrintHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] int limite = 100)
    {
        try
        {
            var history = await _historyService.GetHistoryAsync(limite);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
