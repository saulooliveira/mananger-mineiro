using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Controllers;

[ApiController]
[Route("api/pdf-config")]
public class PdfConfigController : ControllerBase
{
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "pdf-config.json");

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

            return Ok(new { message = "Configuração salva com sucesso." });
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
            pageMargin = 10,
            gridColumns = 2,
            gridRows = 2,
            gridGapMm = 5,
            elements = new
            {
                title = new { fontSize = 16, fontFamily = "Arial" },
                description = new { fontSize = 12, fontFamily = "Arial" },
                price = new { fontSize = 20, fontFamily = "Arial Bold" },
                unit = new { fontSize = 10, fontFamily = "Arial" },
                footer = new { fontSize = 8, fontFamily = "Arial" }
            }
        };
    }
}
