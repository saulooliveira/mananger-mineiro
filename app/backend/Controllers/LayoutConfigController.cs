using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/layout-config")]
public class LayoutConfigController : ControllerBase
{
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "layout-config.json");
    private readonly object _configLock = new();

    [HttpGet]
    public IActionResult GetConfig()
    {
        try
        {
            lock (_configLock)
            {
                if (!System.IO.File.Exists(_configPath))
                {
                    return Ok(GetDefaultConfig());
                }

                var json = System.IO.File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<object>(json);
                return Ok(config);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult SaveConfig([FromBody] object config)
    {
        try
        {
            lock (_configLock)
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_configPath, json);
            }

            return Ok(new { message = "Layout configurado com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static object GetDefaultConfig()
    {
        return new
        {
            cards = new[]
            {
                new
                {
                    id = "cebola",
                    x = 10,
                    y = 10,
                    w = 92.5,
                    h = 136,
                    content = new
                    {
                        title = new { x = 5, y = 15, text = "CEBOLA", fontSize = 16, bold = true, alignment = "left", color = "#000", visible = true },
                        subtitle = new { x = 5, y = 30, text = "Nacional", fontSize = 12, bold = false, alignment = "left", color = "#666", visible = true },
                        price = new { x = 5, y = 60, text = "2,99", fontSize = 20, bold = true, alignment = "left", color = "#000", visible = true },
                        unit = new { x = 70, y = 63, text = "KG", fontSize = 10, bold = false, alignment = "left", color = "#666", visible = true },
                        footer = new { x = 5, y = 115, text = "Oferta válida enquanto durarem os estoques", fontSize = 8, bold = false, alignment = "left", color = "#999", visible = true }
                    }
                }
            }
        };
    }
}
