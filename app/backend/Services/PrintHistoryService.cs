using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class PrintHistoryService
{
    private readonly DatabaseContext _db;

    public PrintHistoryService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task AddPrintAsync(List<int> produtoIds)
    {
        var quantidadeFolhas = (int)Math.Ceiling(produtoIds.Count / 4.0);

        var history = new PrintHistory
        {
            DataHora = DateTime.Now,
            ProdutoIds = string.Join(",", produtoIds),
            NumeroFolhas = quantidadeFolhas,
            QuantidadeProdutos = produtoIds.Count
        };

        _db.PrintHistories.Add(history);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PrintHistory>> GetHistoryAsync(int limite = 100)
    {
        return await Task.FromResult(
            _db.PrintHistories
                .OrderByDescending(h => h.DataHora)
                .Take(limite)
                .ToList()
        );
    }
}
