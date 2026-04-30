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

    public PrintController(ProdutoService produtoService, PrintService printService)
    {
        _produtoService = produtoService;
        _printService = printService;
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

        var pdfBytes = _printService.GeneratePreviewPdf(produtos);
        return File(pdfBytes, "application/pdf", "preview.pdf");
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
        return Ok(new SuccessResponse { Success = true });
    }
}

public class PrintPreviewRequest
{
    public List<int> ProdutoIds { get; set; } = new();
    public Dictionary<string, decimal> EditedPrices { get; set; } = new();
}
