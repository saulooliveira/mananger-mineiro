using System.Globalization;
using System.Text;
using System.Text.Json;
using Backend.Models;
using DbfReader = DbfDataReader.DbfDataReader;

namespace Backend.Services;

public class DbfImportService
{
    private readonly ProdutoService _produtoService;
    private readonly object _configLock = new();

    public DbfImportService(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    public DbfConfig LoadConfig()
    {
        lock (_configLock)
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "dbf-config.json");
            if (!File.Exists(configPath))
                return new DbfConfig();

            try
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<DbfConfig>(json) ?? new DbfConfig();
            }
            catch
            {
                return new DbfConfig();
            }
        }
    }

    public void SaveConfig(DbfConfig config)
    {
        lock (_configLock)
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "dbf-config.json");
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }
    }

    public async Task<List<string>> GetFieldsAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine($"[DbfImport] Arquivo não existe: {filePath}");
            return new List<string>();
        }

        var fields = new List<string>();

        try
        {
            var tempPath = Path.Combine(AppContext.BaseDirectory, "import.dbf");
            Console.WriteLine($"[DbfImport] Copiando de {filePath} para {tempPath}");
            File.Copy(filePath, tempPath, overwrite: true);
            Console.WriteLine($"[DbfImport] Arquivo copiado com sucesso");

            using var reader = new DbfReader(tempPath, Encoding.GetEncoding("ISO-8859-1"));
            Console.WriteLine($"[DbfImport] DBF aberto, FieldCount: {reader.FieldCount}");

            for (int i = 0; i < reader.FieldCount; i++)
            {
                fields.Add(reader.GetName(i));
            }

            Console.WriteLine($"[DbfImport] {fields.Count} campos lidos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbfImport] Erro ao ler campos: {ex.Message}");
            Console.WriteLine($"[DbfImport] Stack: {ex.StackTrace}");
            return new List<string>();
        }

        return await Task.FromResult(fields);
    }

    public async Task<(int Inserted, int Updated, string? Error)> ImportAsync(DbfConfig config)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(config.FilePath))
                return (0, 0, "Caminho do arquivo DBF não configurado");

            if (string.IsNullOrWhiteSpace(config.CodigoColumn))
                return (0, 0, "Coluna 'Código' não mapeada");

            if (string.IsNullOrWhiteSpace(config.DescricaoColumn))
                return (0, 0, "Coluna 'Descrição' não mapeada");

            if (!File.Exists(config.FilePath))
                return (0, 0, $"Arquivo DBF não encontrado: {config.FilePath}");

            var tempPath = Path.Combine(AppContext.BaseDirectory, "import.dbf");
            File.Copy(config.FilePath, tempPath, overwrite: true);

            var produtos = new List<Produto>();

            using (var reader = new DbfReader(tempPath, Encoding.GetEncoding("ISO-8859-1")))
            {
                while (reader.Read())
                {

                    var codigo = GetFieldValue(reader, config.CodigoColumn, "");
                    if (string.IsNullOrWhiteSpace(codigo))
                        continue;

                    var descricao = GetFieldValue(reader, config.DescricaoColumn, codigo);
                    var categoria = GetFieldValue(reader, config.CategoriaColumn, "");
                    var valorStr = GetFieldValue(reader, config.ValorColumn, "0");
                    var yield = GetFieldValue(reader, config.YieldColumn, "");

                    decimal valor = ParseValor(valorStr);

                    var produto = new Produto
                    {
                        Codigo = codigo,
                        Descricao = descricao,
                        Categoria = categoria,
                        Valor = valor,
                        Yield = yield,
                        QuantidadeImpresa = 0
                    };

                    produtos.Add(produto);
                }
            }

            var (inserted, updated) = await _produtoService.UpsertAsync(produtos);
            return (inserted, updated, null);
        }
        catch (Exception ex)
        {
            return (0, 0, ex.Message);
        }
    }

    private string GetFieldValue(DbfReader reader, string columnName, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return defaultValue;

        try
        {
            var value = reader[columnName];
            return value?.ToString()?.Trim() ?? defaultValue;
        }
        catch
        {
            return defaultValue ?? string.Empty;
        }
    }

    private decimal ParseValor(string valorStr)
    {
        if (string.IsNullOrWhiteSpace(valorStr))
            return 0m;

        valorStr = valorStr.Trim();

        if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out var resultBr))
            return resultBr;

        return 0m;
    }
}
