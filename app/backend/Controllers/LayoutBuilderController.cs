using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/layout-builder")]
public class LayoutBuilderController : ControllerBase
{
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "layout-builder-config.json");
    private readonly object _configLock = new();

    [HttpGet]
    public IActionResult GetLayout()
    {
        try
        {
            lock (_configLock)
            {
                if (!System.IO.File.Exists(_configPath))
                {
                    return Ok(GetDefaultLayout());
                }

                var json = System.IO.File.ReadAllText(_configPath);
                var layout = JsonSerializer.Deserialize<object>(json);
                return Ok(layout);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult SaveLayout([FromBody] object layout)
    {
        try
        {
            lock (_configLock)
            {
                var json = JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_configPath, json);
            }

            return Ok(new { message = "Layout salvo com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static object GetDefaultLayout()
    {
        return new
        {
            page = new
            {
                marginMm = 10,
                columns = 2,
                rows = 2,
                gapMm = 5,
                cardWidthMm = 80,
                cardHeightMm = 120,
            },
            elements = new object[] { },
        };
    }
}
