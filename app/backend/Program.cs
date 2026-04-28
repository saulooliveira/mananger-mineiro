using Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "data", "produtos.db");
var dbDir = Path.GetDirectoryName(dbPath);

try
{
    if (dbDir != null && !Directory.Exists(dbDir))
    {
        Directory.CreateDirectory(dbDir);
    }
}
catch (Exception ex)
{
    throw new InvalidOperationException($"Failed to create database directory: {dbDir}", ex);
}

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);

builder.Services.AddControllers();

var app = builder.Build();

// Database initialization
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        db.Database.EnsureCreated();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to initialize database at startup");
    throw;
}

app.UseAuthorization();
app.MapControllers();

// Seed data (optional, for testing UI)
async Task SeedData()
{
    var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
    if (db.Produtos.Any()) return; // Already seeded

    var produtos = new[]
    {
        new Backend.Models.Produto
        {
            Codigo = "001",
            Descricao = "Café Premium 500g",
            Categoria = "Bebidas",
            Valor = 24.90m,
            Yield = "1kg renders 2 cups"
        },
        new Backend.Models.Produto
        {
            Codigo = "002",
            Descricao = "Chocolate 100g",
            Categoria = "Doces",
            Valor = 8.50m
        },
        new Backend.Models.Produto
        {
            Codigo = "003",
            Descricao = "Leite Integral 1L",
            Categoria = "Laticínios",
            Valor = 4.20m
        },
        new Backend.Models.Produto
        {
            Codigo = "004",
            Descricao = "Pão Francês kg",
            Categoria = "Padaria",
            Valor = 12.00m
        }
    };

    await db.Produtos.AddRangeAsync(produtos);
    await db.SaveChangesAsync();
    Console.WriteLine("Seed data added: 4 products");
}

await SeedData();

app.Run();
