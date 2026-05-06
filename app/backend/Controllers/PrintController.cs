using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrintController : ControllerBase
{
    private readonly ProdutoService _produtoService;
    private readonly PrintService _printService;
    private readonly PrintHistoryService _historyService;

    public PrintController(ProdutoService produtoService, PrintService printService, PrintHistoryService historyService)
    {
        _produtoService = produtoService;
        _printService = printService;
        _historyService = historyService;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] PrintPreviewRequest request)
    {
        if (request?.ProdutoIds == null || !request.ProdutoIds.Any())
        {
            return BadRequest(new ErrorResponse { Error = "É necessário enviar pelo menos um produto." });
        }

        var produtos = await _produtoService.GetByIdsAsync(request.ProdutoIds);
        if (!produtos.Any())
        {
            return NotFound(new ErrorResponse { Error = "Nenhum produto encontrado para os IDs informados." });
        }

        var pdfBytes = _printService.GeneratePreviewPdf(produtos, request.LayoutConfig);
        return File(pdfBytes, "application/pdf", "preview.pdf");
    }

    [HttpPost("builder-preview")]
    public async Task<IActionResult> BuilderPreview([FromBody] object rawRequest)
    {
        var request = (rawRequest as System.Text.Json.JsonElement?);
        if (!request.HasValue)
        {
            return BadRequest(new ErrorResponse { Error = "Request inválido" });
        }

        var req = request.Value;
        System.Diagnostics.Debug.WriteLine($"[PrintController] BuilderPreview request: {req.GetRawText()}");

        var produtoIds = req.GetProperty("produtoIds").EnumerateArray()
            .Select(x => x.GetInt32())
            .ToList();

        if (!produtoIds.Any())
        {
            return BadRequest(new ErrorResponse { Error = "É necessário enviar pelo menos um produto." });
        }

        var produtos = await _produtoService.GetByIdsAsync(produtoIds);
        if (!produtos.Any())
        {
            return NotFound(new ErrorResponse { Error = "Nenhum produto encontrado para os IDs informados." });
        }

        object? layout = null;
        if (req.TryGetProperty("layout", out var layoutProp))
        {
            layout = layoutProp;
            System.Diagnostics.Debug.WriteLine($"[PrintController] Layout property found: {layoutProp.GetRawText()}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[PrintController] Layout property NOT found in request");
        }

        var pdfBytes = _printService.GenerateBuilderPdf(produtos, layout);
        return File(pdfBytes, "application/pdf", "builder-preview.pdf");
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] PrintPreviewRequest request)
    {
        if (request?.ProdutoIds == null || !request.ProdutoIds.Any())
        {
            return BadRequest(new ErrorResponse { Error = "É necessário enviar pelo menos um produto." });
        }

        var produtos = await _produtoService.GetByIdsAsync(request.ProdutoIds);
        if (!produtos.Any())
        {
            return NotFound(new ErrorResponse { Error = "Nenhum produto encontrado para os IDs informados." });
        }

        await _produtoService.IncrementQuantidadeImpressaAsync(request.ProdutoIds);
        await _historyService.AddPrintAsync(request.ProdutoIds);
        return Ok(new SuccessResponse { Success = true });
    }
}

public class PrintPreviewRequest
{
    public List<int> ProdutoIds { get; set; } = new();
    public Dictionary<string, decimal> EditedPrices { get; set; } = new();
    public Backend.Data.LayoutConfig? LayoutConfig { get; set; }
}

