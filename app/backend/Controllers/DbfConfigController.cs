using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/dbf-config")]
public class DbfConfigController : ControllerBase
{
    private readonly DbfImportService _dbfService;

    public DbfConfigController(DbfImportService dbfService)
    {
        _dbfService = dbfService;
    }

    [HttpGet]
    public ActionResult<DbfConfig> GetConfig()
    {
        var config = _dbfService.LoadConfig();
        return Ok(config);
    }

    [HttpPost]
    public ActionResult<object> SaveConfig([FromBody] DbfConfig config)
    {
        if (config == null)
            return BadRequest(new { error = "Configuração inválida" });

        _dbfService.SaveConfig(config);
        return Ok(new { message = "Configuração salva com sucesso" });
    }

    [HttpGet("fields")]
    public async Task<ActionResult<List<string>>> GetFields([FromQuery] string? path)
    {
        var config = _dbfService.LoadConfig();
        var filePath = path ?? config.FilePath;

        Console.WriteLine($"[DBF] GetFields - filePath: {filePath}");

        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest(new { error = "Caminho do arquivo não especificado" });

        if (!System.IO.File.Exists(filePath))
        {
            Console.WriteLine($"[DBF] Arquivo não encontrado: {filePath}");
            return BadRequest(new { error = $"Arquivo não encontrado: {filePath}" });
        }

        Console.WriteLine($"[DBF] Carregando campos...");
        var fields = await _dbfService.GetFieldsAsync(filePath);
        Console.WriteLine($"[DBF] Campos carregados: {fields.Count}");

        if (fields.Count == 0)
            return BadRequest(new { error = "Não foi possível ler os campos do arquivo DBF" });

        return Ok(fields);
    }

    [HttpPost("import")]
    public async Task<ActionResult<object>> Import()
    {
        var config = _dbfService.LoadConfig();

        if (string.IsNullOrWhiteSpace(config.FilePath))
            return BadRequest(new { error = "Arquivo DBF não configurado" });

        var (inserted, updated, error) = await _dbfService.ImportAsync(config);

        if (error != null)
            return BadRequest(new { error });

        return Ok(new { inserted, updated });
    }
}
