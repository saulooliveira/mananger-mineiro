using System.IO;
using System.Text.Json;
using Backend.Data;
using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services;

public class PrintService
{
    private readonly BarcodeService _barcodeService;

    public PrintService(BarcodeService barcodeService)
    {
        _barcodeService = barcodeService;
    }

    public byte[] GeneratePreviewPdf(List<Produto> produtos, LayoutConfig? requestConfig = null)
    {
        var layoutConfig = requestConfig ?? LoadLayoutConfig();
        var document = new LayoutBasedDocument(produtos, layoutConfig);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    public byte[] GenerateBuilderPdf(List<Produto> produtos, object? layoutData = null)
    {
        LayoutBuilderData layout;

        if (layoutData == null)
        {
            layout = LoadBuilderLayout();
            System.Diagnostics.Debug.WriteLine("[PrintService] Using default layout (no data received)");
        }
        else if (layoutData is LayoutBuilderData lbd)
        {
            layout = lbd;
            System.Diagnostics.Debug.WriteLine($"[PrintService] Using LayoutBuilderData directly. Elements: {lbd.Elements.Count}");
        }
        else if (layoutData is System.Text.Json.JsonElement je)
        {
            var json = je.GetRawText();
            System.Diagnostics.Debug.WriteLine($"[PrintService] Received JsonElement: {json}");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            layout = JsonSerializer.Deserialize<LayoutBuilderData>(json, options) ?? LoadBuilderLayout();
            System.Diagnostics.Debug.WriteLine($"[PrintService] Deserialized layout. Elements: {layout.Elements.Count}. Page: {layout.Page.Columns}x{layout.Page.Rows}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[PrintService] Unknown layout data type: {layoutData?.GetType().Name}. Using default.");
            layout = LoadBuilderLayout();
        }

        var document = new BuilderBasedDocument(produtos, layout, _barcodeService);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private LayoutBuilderData LoadBuilderLayout()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "layout-builder-config.json");

        if (!File.Exists(configPath))
        {
            return GetDefaultBuilderLayout();
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<LayoutBuilderData>(json, options);
            return config ?? GetDefaultBuilderLayout();
        }
        catch
        {
            return GetDefaultBuilderLayout();
        }
    }

    private LayoutBuilderData GetDefaultBuilderLayout()
    {
        return new LayoutBuilderData
        {
            Page = new PageConfig
            {
                MarginMm = 10,
                Columns = 2,
                Rows = 2,
                GapMm = 5,
                CardWidthMm = 80,
                CardHeightMm = 120,
            },
            Elements = new List<LayoutBuilderElement>(),
        };
    }

    private LayoutConfig LoadLayoutConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "layout-config.json");

        if (!File.Exists(configPath))
        {
            return GetDefaultConfig();
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<LayoutConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return config ?? GetDefaultConfig();
        }
        catch
        {
            return GetDefaultConfig();
        }
    }

    private LayoutConfig GetDefaultConfig()
    {
        return new LayoutConfig
        {
            Cards = new List<Card>
            {
                new Card
                {
                    Id = "default",
                    X = 10,
                    Y = 10,
                    W = 92.5,
                    H = 136,
                    Content = new Dictionary<string, Element>
                    {
                        { "title", new Element { X = 5, Y = 15, Text = "", FontSize = 16, Bold = true, Alignment = "left", Color = "#000", Visible = true } },
                        { "subtitle", new Element { X = 5, Y = 30, Text = "", FontSize = 12, Bold = false, Alignment = "left", Color = "#666", Visible = true } },
                        { "price", new Element { X = 5, Y = 60, Text = "", FontSize = 20, Bold = true, Alignment = "left", Color = "#000", Visible = true } },
                        { "unit", new Element { X = 70, Y = 63, Text = "KG", FontSize = 10, Bold = false, Alignment = "left", Color = "#666", Visible = true } },
                        { "footer", new Element { X = 5, Y = 115, Text = "", FontSize = 8, Bold = false, Alignment = "left", Color = "#999", Visible = true } }
                    }
                }
            }
        };
    }
}

internal class LayoutBasedDocument : IDocument
{
    private readonly List<Produto> _produtos;
    private readonly LayoutConfig _config;

    public LayoutBasedDocument(List<Produto> produtos, LayoutConfig config)
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
        var itemsPerPage = 4;
        var productGroups = _produtos
            .Select((produto, index) => new { produto, index })
            .GroupBy(x => x.index / itemsPerPage)
            .ToList();

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

            page.Content().Column(pageColumn =>
            {
                for (var groupIndex = 0; groupIndex < productGroups.Count; groupIndex++)
                {
                    var pageGroup = productGroups[groupIndex].ToList();

                    pageColumn.Item().Column(gridColumn =>
                    {
                        for (int row = 0; row < 2; row++)
                        {
                            gridColumn.Item().Height(148.5f, Unit.Millimetre).Row(gridRow =>
                            {
                                for (int col = 0; col < 2; col++)
                                {
                                    var itemIndex = row * 2 + col;
                                    if (itemIndex < pageGroup.Count)
                                    {
                                        var item = pageGroup[itemIndex];
                                        var cardTemplate = _config.Cards[itemIndex % _config.Cards.Count];

                                        gridRow.RelativeItem()
                                            .Column(cardContent =>
                                            {
                                                RenderProductCard(cardContent, item.produto, cardTemplate.Content);
                                            });
                                    }
                                    else
                                    {
                                        gridRow.RelativeItem();
                                    }
                                }
                            });
                        }
                    });

                    if (groupIndex < productGroups.Count - 1)
                    {
                        pageColumn.Item().PageBreak();
                    }
                }
            });
        });
    }

    private void RenderProductCard(ColumnDescriptor card, Produto produto, Dictionary<string, Element> template)
    {
        if (template.TryGetValue("title", out var title) && title.Visible)
        {
            RenderElement(card, produto.Codigo, title, 0);
        }

        if (template.TryGetValue("subtitle", out var subtitle) && subtitle.Visible)
        {
            RenderElement(card, produto.Descricao, subtitle, (float)subtitle.Y);
        }

        if (template.TryGetValue("price", out var price) && price.Visible)
        {
            RenderElement(card, $"R$ {produto.Valor:N2}", price, (float)price.Y);
        }

        if (template.TryGetValue("unit", out var unit) && unit.Visible && !string.IsNullOrWhiteSpace(unit.Text))
        {
            RenderElement(card, unit.Text, unit, (float)unit.Y);
        }

        if (template.TryGetValue("footer", out var footer) && footer.Visible)
        {
            if (!string.IsNullOrWhiteSpace(produto.Yield))
            {
                RenderElement(card, $"Yield: {produto.Yield}", footer, (float)footer.Y);
            }

            RenderElement(card, $"Impressões: {produto.QuantidadeImpresa}", footer, (float)(footer.Y + 10));
        }
    }

    private void RenderElement(ColumnDescriptor card, string text, Element element, float previousY)
    {
        if (!element.Visible) return;

        var spacing = (float)element.Y - previousY;
        if (spacing > 0)
        {
            card.Item().Height(spacing, Unit.Millimetre);
        }

        if (!string.IsNullOrWhiteSpace(element.ImagePath))
        {
            RenderImage(card, element);
        }
        else if (!string.IsNullOrWhiteSpace(text))
        {
            var textElement = card.Item().PaddingLeft((float)element.X, Unit.Millimetre).Text(text)
                .FontSize(element.FontSize)
                .FontColor(ParseColor(element.Color));

            if (element.Alignment == "center")
                textElement.AlignCenter();
            else if (element.Alignment == "right")
                textElement.AlignRight();

            if (element.Bold) textElement.SemiBold();
        }
    }

    private void RenderImage(ColumnDescriptor card, Element element)
    {
        var imagePath = element.ImagePath;
        if (!imagePath.StartsWith("/"))
        {
            imagePath = "/" + imagePath;
        }

        var fullPath = Path.Combine(AppContext.BaseDirectory, imagePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            card.Item().PaddingLeft((float)element.X, Unit.Millimetre).MaxWidth(50, Unit.Millimetre).Image(fullPath);
        }
    }

    private Color ParseColor(string colorHex)
    {
        try
        {
            return Color.FromHex(colorHex);
        }
        catch
        {
            return Colors.Black;
        }
    }
}

internal class BuilderBasedDocument : IDocument
{
    private readonly List<Produto> _produtos;
    private readonly LayoutBuilderData _layout;
    private readonly BarcodeService _barcodeService;

    public BuilderBasedDocument(List<Produto> produtos, LayoutBuilderData layout, BarcodeService barcodeService)
    {
        _produtos = produtos;
        _layout = layout;
        _barcodeService = barcodeService;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        var itemsPerPage = _layout.Page.Columns * _layout.Page.Rows;
        var productGroups = _produtos
            .Select((produto, index) => new { produto, index })
            .GroupBy(x => x.index / itemsPerPage)
            .ToList();

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin((float)_layout.Page.MarginMm, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

            page.Content().Column(pageColumn =>
            {
                for (int groupIndex = 0; groupIndex < productGroups.Count; groupIndex++)
                {
                    var pageGroup = productGroups[groupIndex].ToList();

                    pageColumn.Item().Column(gridColumn =>
                    {
                        for (int row = 0; row < _layout.Page.Rows; row++)
                        {
                            gridColumn.Item().Height((float)_layout.Page.CardHeightMm, Unit.Millimetre).Row(gridRow =>
                            {
                                for (int col = 0; col < _layout.Page.Columns; col++)
                                {
                                    var itemIndex = row * _layout.Page.Columns + col;
                                    if (itemIndex < pageGroup.Count)
                                    {
                                        var produto = pageGroup[itemIndex].produto;
                                        gridRow.RelativeItem()
                                            .Width((float)_layout.Page.CardWidthMm, Unit.Millimetre)
                                            .Height((float)_layout.Page.CardHeightMm, Unit.Millimetre)
                                            .Column(cardContent =>
                                            {
                                                RenderCard(cardContent, produto);
                                            });
                                    }
                                    else
                                    {
                                        gridRow.RelativeItem();
                                    }
                                }
                            });

                            if (row < _layout.Page.Rows - 1)
                            {
                                gridColumn.Item().Height((float)_layout.Page.GapMm, Unit.Millimetre);
                            }
                        }
                    });

                    if (groupIndex < productGroups.Count - 1)
                    {
                        pageColumn.Item().PageBreak();
                    }
                }
            });
        });
    }

    private void RenderCard(ColumnDescriptor card, Produto produto)
    {
        var sortedElements = _layout.Elements.OrderBy(e => e.YMm).ThenBy(e => e.XMm).ToList();
        var currentY = 0f;

        foreach (var element in sortedElements)
        {
            var x = (float)element.XMm;
            var y = (float)element.YMm;
            var w = (float)element.WidthMm;
            var h = (float)element.HeightMm;

            // Add spacing to reach absolute Y position
            if (y > currentY)
            {
                var spacing = y - currentY;
                card.Item().Height(spacing, Unit.Millimetre);
                currentY = y;
            }

            if (element.Type == "qrcode")
            {
                var value = GetElementText(element, produto);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var imagePath = _barcodeService.GenerateQRCode(value);
                    if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                    {
                        card.Item()
                            .PaddingLeft(x, Unit.Millimetre)
                            .Width(w, Unit.Millimetre)
                            .Height(h, Unit.Millimetre)
                            .Image(imagePath);
                    }
                }
            }
            else if (element.Type == "barcode")
            {
                var value = GetElementText(element, produto);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var barcodeType = element.BarcodeType ?? "ean13";
                    var imagePath = _barcodeService.GenerateBarcode(value, barcodeType);
                    if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                    {
                        card.Item()
                            .PaddingLeft(x, Unit.Millimetre)
                            .Width(w, Unit.Millimetre)
                            .Height(h, Unit.Millimetre)
                            .Image(imagePath);
                    }
                }
            }
            else if (element.Type == "text" || element.Type == "formula")
            {
                var text = GetElementText(element, produto);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var textElement = card.Item()
                        .PaddingLeft(x, Unit.Millimetre)
                        .Width(w, Unit.Millimetre)
                        .Height(h, Unit.Millimetre)
                        .Text(text)
                        .FontSize(element.FontSize)
                        .FontColor(ParseColor(element.Color ?? "#000000"));

                    if (element.Align == "center")
                        textElement.AlignCenter();
                    else if (element.Align == "right")
                        textElement.AlignRight();

                    if (element.Bold)
                        textElement.SemiBold();
                }
            }
            else if (element.Type == "image" && !string.IsNullOrWhiteSpace(element.ImagePath))
            {
                var fullPath = Path.Combine(AppContext.BaseDirectory, element.ImagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    card.Item()
                        .PaddingLeft(x, Unit.Millimetre)
                        .Width(w, Unit.Millimetre)
                        .Height(h, Unit.Millimetre)
                        .Image(fullPath);
                }
            }

            currentY = y + h;
        }
    }

    private string GetElementText(LayoutBuilderElement element, Produto produto)
    {
        return element.Type switch
        {
            "text" => element.Source switch
            {
                "fixed" => element.PreviewText ?? "",
                "field" => GetFieldValue(element.FieldName, produto),
                _ => "",
            },
            "formula" => EvaluateFormula(element.Formula ?? "", produto),
            "qrcode" => element.Source switch
            {
                "fixed" => element.Value ?? "",
                "field" => GetFieldValue(element.FieldName, produto),
                _ => "",
            },
            "barcode" => element.Source switch
            {
                "fixed" => element.Value ?? "",
                "field" => GetFieldValue(element.FieldName, produto),
                _ => "",
            },
            _ => "",
        };
    }

    private string GetFieldValue(string? fieldName, Produto produto)
    {
        return fieldName?.ToLower() switch
        {
            "descricao" => produto.Descricao,
            "codigo" => produto.Codigo,
            "categoria" => produto.Categoria ?? "",
            "valor" => $"R$ {produto.Valor:N2}",
            "yield" => produto.Yield ?? "",
            "quantidadeimpresa" => produto.QuantidadeImpresa.ToString(),
            _ => "",
        };
    }

    private string EvaluateFormula(string formula, Produto produto)
    {
        try
        {
            var result = formula
                .Replace("preco", $"\"{produto.Valor:N2}\"")
                .Replace("descricao", $"\"{produto.Descricao}\"")
                .Replace("codigo", $"\"{produto.Codigo}\"")
                .Replace("categoria", $"\"{produto.Categoria}\"")
                .Replace("yield", $"\"{produto.Yield}\"");

            return result.Replace("\"", "");
        }
        catch
        {
            return formula;
        }
    }

    private Color ParseColor(string colorHex)
    {
        try
        {
            return Color.FromHex(colorHex);
        }
        catch
        {
            return Colors.Black;
        }
    }
}
