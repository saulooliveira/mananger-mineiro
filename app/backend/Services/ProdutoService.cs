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

    public async Task<List<Produto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _db.Produtos
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public async Task IncrementQuantidadeImpressaAsync(IEnumerable<int> ids)
    {
        var produtos = await _db.Produtos
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        foreach (var produto in produtos)
        {
            produto.QuantidadeImpresa += 1;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<(int Inserted, int Updated)> UpsertAsync(IEnumerable<Produto> incoming)
    {
        var existing = await _db.Produtos.ToDictionaryAsync(p => p.Codigo.ToUpperInvariant());
        int inserted = 0, updated = 0;

        foreach (var item in incoming)
        {
            var key = item.Codigo.ToUpperInvariant();
            if (existing.TryGetValue(key, out var found))
            {
                found.Descricao = item.Descricao;
                found.Categoria = item.Categoria;
                found.Valor = item.Valor;
                found.Yield = item.Yield;
                updated++;
            }
            else
            {
                _db.Produtos.Add(item);
                inserted++;
            }
        }

        await _db.SaveChangesAsync();
        return (inserted, updated);
    }
}
