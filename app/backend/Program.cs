using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

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

builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<PrintService>();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseCors("FrontendDev");
app.UseAuthorization();
app.MapControllers();

// Seed data (Debug only, for testing UI with large dataset)
async Task SeedData()
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            if (db.Produtos.Any()) return; // Already seeded

#if DEBUG
            var random = new Random(1234);
            var categorias = new[]
            {
                "Bebidas",
                "Alimentos",
                "Padaria",
                "Hortifruti",
                "Laticínios",
                "Congelados",
                "Carnes",
                "Doces",
                "Higiene",
                "Mercearia"
            };

            var descricoes = new[]
            {
                "Água Mineral 1L",
                "Suco Natural 1L",
                "Leite Integral 1L",
                "Pão Francês kg",
                "Banana Nanica kg",
                "Maçã Fuji kg",
                "Tomate Italiano kg",
                "Queijo Minas 300g",
                "Iogurte Natural 500g",
                "Café Torrado 250g",
                "Arroz Branco 5kg",
                "Feijão Carioca 1kg",
                "Carne Bovina kg",
                "Frango Resfriado kg",
                "Manteiga 200g",
                "Chocolate ao Leite 100g",
                "Biscoito Recheado 200g",
                "Cerveja Pilsen 350ml",
                "Água de Coco 500ml",
                "Azeite Extra Virgem 500ml"
            };

            var produtos = Enumerable.Range(1, 1000).Select(index =>
            {
                var categoria = categorias[random.Next(categorias.Length)];
                var descricao = $"{descricoes[random.Next(descricoes.Length)]} {index:D4}";
                var codigo = index.ToString("D4");
                var valor = Math.Round((decimal)(random.NextDouble() * 40.0 + 1.0), 2);
                var yieldText = random.Next(2) == 0 ? null : $"{random.Next(1, 5)}kg = {random.Next(1, 20)} unidades";

                return new Backend.Models.Produto
                {
                    Codigo = codigo,
                    Descricao = descricao,
                    Categoria = categoria,
                    Valor = valor,
                    Yield = yieldText
                };
            }).ToArray();

            await db.Produtos.AddRangeAsync(produtos);
            await db.SaveChangesAsync();
            Console.WriteLine($"Seed data added: {produtos.Length} produtos (Debug only)");
#else
            Console.WriteLine("Seed data skipped outside DEBUG.");
#endif
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to seed database");
        throw;
    }
}

#if DEBUG
await SeedData();
#endif

app.Run();
