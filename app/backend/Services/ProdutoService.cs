using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ProdutoService
{
    private readonly DatabaseContext _db;

    public ProdutoService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<List<Produto>> GetAllAsync()
    {
        return await _db.Produtos.ToListAsync();
    }

    public async Task<List<Produto>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        var searchLower = query.ToLower();
        return await _db.Produtos
            .Where(p => p.Codigo.ToLower().Contains(searchLower) ||
                        p.Descricao.ToLower().Contains(searchLower) ||
                        (p.Categoria != null && p.Categoria.ToLower().Contains(searchLower)))
            .ToListAsync();
    }
}
