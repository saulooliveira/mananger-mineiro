using System.IO;
using System.Text.Json;
using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services;

public class PrintService
{
    public byte[] GeneratePreviewPdf(List<Produto> produtos)
    {
        var config = LoadPdfConfig();
        var document = new ProductPreviewDocument(produtos, config);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private PdfConfig LoadPdfConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "pdf-config.json");

        if (!File.Exists(configPath))
        {
            return GetDefaultConfig();
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<PdfConfig>(json);
            return config ?? GetDefaultConfig();
        }
        catch
        {
            return GetDefaultConfig();
        }
    }

    private PdfConfig GetDefaultConfig()
    {
        return new PdfConfig
        {
            PageMargin = 10,
            GridColumns = 2,
            GridRows = 2,
            GridGapMm = 5,
            Elements = new Dictionary<string, FontConfig>
            {
                { "title", new FontConfig { FontSize = 16, FontFamily = "Arial" } },
                { "description", new FontConfig { FontSize = 12, FontFamily = "Arial" } },
                { "price", new FontConfig { FontSize = 20, FontFamily = "Arial Bold" } },
                { "unit", new FontConfig { FontSize = 10, FontFamily = "Arial" } },
                { "footer", new FontConfig { FontSize = 8, FontFamily = "Arial" } }
            }
        };
    }
}

public class PdfConfig
{
    public int PageMargin { get; set; } = 10;
    public int GridColumns { get; set; } = 2;
    public int GridRows { get; set; } = 2;
    public int GridGapMm { get; set; } = 5;
    public Dictionary<string, FontConfig> Elements { get; set; } = new();
}

public class FontConfig
{
    public int FontSize { get; set; } = 12;
    public string FontFamily { get; set; } = "Arial";
}

internal class ProductPreviewDocument : IDocument
{
    private readonly List<Produto> _produtos;
    private readonly PdfConfig _config;

    public ProductPreviewDocument(List<Produto> produtos, PdfConfig config)
    {
        _produtos = produtos;
        _config = config;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        var itemsPerPage = _config.GridColumns * _config.GridRows;
        var productGroups = _produtos
            .Select((produto, index) => new { produto, index })
            .GroupBy(x => x.index / itemsPerPage)
            .ToList();

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(_config.PageMargin);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

            page.Content().Column(column =>
            {
                column.Spacing(10);
                column.Item().Text("Preview de Ofertas").FontSize(GetFontSize("title")).SemiBold();
                column.Item().Text($"Total de produtos: {_produtos.Count}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);

                for (var groupIndex = 0; groupIndex < productGroups.Count; groupIndex++)
                {
                    var pageGroup = productGroups[groupIndex];

                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                    column.Item().PaddingTop(10).Grid(grid =>
                    {
                        grid.Columns(_config.GridColumns);
                        grid.Spacing(_config.GridGapMm);

                        foreach (var item in pageGroup)
                        {
                            grid.Item().Padding(8).Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Column(card =>
                            {
                                card.Spacing(6);
                                card.Item().Text(item.produto.Codigo).FontSize(GetFontSize("title")).SemiBold();
                                card.Item().Text(item.produto.Descricao).FontSize(GetFontSize("description")).Bold();
                                card.Item().Text(item.produto.Categoria ?? "Categoria não definida").FontSize(GetFontSize("unit")).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                card.Item().Text($"Valor: R$ {item.produto.Valor:N2}").FontSize(GetFontSize("price")).Bold();

                                if (!string.IsNullOrWhiteSpace(item.produto.Yield))
                                {
                                    card.Item().Text($"Yield: {item.produto.Yield}").FontSize(GetFontSize("footer")).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                }

                                card.Item().Text($"Impressões: {item.produto.QuantidadeImpresa}").FontSize(GetFontSize("footer")).FontColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                            });
                        }

                        var emptyCount = itemsPerPage - pageGroup.Count();
                        for (int i = 0; i < emptyCount; i++)
                        {
                            grid.Item().Text(string.Empty);
                        }
                    });

                    if (groupIndex < productGroups.Count - 1)
                    {
                        column.Item().PageBreak();
                    }
                }
            });
        });
    }

    private int GetFontSize(string elementKey)
    {
        if (_config.Elements.TryGetValue(elementKey, out var fontConfig))
        {
            return fontConfig.FontSize;
        }

        return 12;
    }
}
