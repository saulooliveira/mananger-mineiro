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
    private const string UnifiedLayoutFileName = "layout.json";
    private const string LegacyBuilderLayoutFileName = "layout-builder-config.json";

    private readonly BarcodeService _barcodeService;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public PrintService(BarcodeService barcodeService)
    {
        _barcodeService = barcodeService;
    }

    public byte[] GeneratePreviewPdf(List<Produto> produtos, object? layoutData = null)
    {
        var layout = ResolveLayout(layoutData);
        var document = new BuilderBasedDocument(produtos, layout, _barcodeService);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    public byte[] GenerateBuilderPdf(List<Produto> produtos, object? layoutData = null)
    {
        return GeneratePreviewPdf(produtos, layoutData);
    }

    private LayoutBuilderData ResolveLayout(object? layoutData)
    {
        if (layoutData == null)
        {
            return LoadLayoutFromDisk();
        }

        if (layoutData is LayoutBuilderData layoutBuilder)
        {
            return SanitizeLayout(layoutBuilder);
        }

        if (layoutData is LayoutConfig legacyLayoutConfig)
        {
            return ConvertLegacyLayout(legacyLayoutConfig);
        }

        if (layoutData is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return LoadLayoutFromDisk();
            }

            if (jsonElement.TryGetProperty("layoutConfig", out var legacyLayoutConfigElement))
            {
                var legacy = legacyLayoutConfigElement.Deserialize<LayoutConfig>(_jsonOptions);
                if (legacy != null)
                {
                    return ConvertLegacyLayout(legacy);
                }
            }

            var parsedLayout = jsonElement.Deserialize<LayoutBuilderData>(_jsonOptions);
            if (parsedLayout != null)
            {
                return SanitizeLayout(parsedLayout);
            }
        }

        return LoadLayoutFromDisk();
    }

    private LayoutBuilderData LoadLayoutFromDisk()
    {
        var unifiedPath = Path.Combine(AppContext.BaseDirectory, UnifiedLayoutFileName);
        if (TryReadLayout(unifiedPath, out var unifiedLayout))
        {
            return unifiedLayout;
        }

        var legacyBuilderPath = Path.Combine(AppContext.BaseDirectory, LegacyBuilderLayoutFileName);
        if (TryReadLayout(legacyBuilderPath, out var legacyBuilderLayout))
        {
            TryWriteLayout(unifiedPath, legacyBuilderLayout);
            return legacyBuilderLayout;
        }

        var fallback = GetDefaultBuilderLayout();
        TryWriteLayout(unifiedPath, fallback);
        return fallback;
    }

    private bool TryReadLayout(string path, out LayoutBuilderData layout)
    {
        layout = GetDefaultBuilderLayout();

        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var json = File.ReadAllText(path);
            var parsed = JsonSerializer.Deserialize<LayoutBuilderData>(json, _jsonOptions);
            if (parsed == null)
            {
                return false;
            }

            layout = SanitizeLayout(parsed);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void TryWriteLayout(string path, LayoutBuilderData layout)
    {
        try
        {
            var json = JsonSerializer.Serialize(layout, _jsonOptions);
            File.WriteAllText(path, json);
        }
        catch
        {
            // Best-effort write. Failing to persist should not block PDF generation.
        }
    }

    private static LayoutBuilderData SanitizeLayout(LayoutBuilderData layout)
    {
        layout.Page ??= new PageConfig();
        layout.Elements ??= new List<LayoutBuilderElement>();

        layout.Page.MarginMm = Math.Max(0, layout.Page.MarginMm);
        layout.Page.Columns = Math.Max(1, layout.Page.Columns);
        layout.Page.Rows = Math.Max(1, layout.Page.Rows);
        layout.Page.GapMm = Math.Max(0, layout.Page.GapMm);
        layout.Page.CardWidthMm = Math.Max(1, layout.Page.CardWidthMm);
        layout.Page.CardHeightMm = Math.Max(1, layout.Page.CardHeightMm);

        return layout;
    }

    private static LayoutBuilderData GetDefaultBuilderLayout()
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

    private static LayoutBuilderData ConvertLegacyLayout(LayoutConfig legacyConfig)
    {
        var defaultLayout = GetDefaultBuilderLayout();
        if (legacyConfig.Cards == null || legacyConfig.Cards.Count == 0)
        {
            return defaultLayout;
        }

        var firstCard = legacyConfig.Cards[0];
        var cardWidth = firstCard.W > 0 ? firstCard.W : defaultLayout.Page.CardWidthMm;
        var cardHeight = firstCard.H > 0 ? firstCard.H : defaultLayout.Page.CardHeightMm;

        var converted = new LayoutBuilderData
        {
            Page = new PageConfig
            {
                MarginMm = Math.Max(0, legacyConfig.PageMargin),
                Columns = Math.Max(1, legacyConfig.GridColumns),
                Rows = Math.Max(1, legacyConfig.GridRows),
                GapMm = Math.Max(0, legacyConfig.GridGapMm),
                CardWidthMm = cardWidth,
                CardHeightMm = cardHeight,
            },
            Elements = new List<LayoutBuilderElement>(),
        };

        var content = firstCard.Content ?? new Dictionary<string, Element>();

        if (content.TryGetValue("title", out var title) && title.Visible)
            converted.Elements.Add(ToTextElement("title", title, cardWidth, "field", "codigo"));

        if (content.TryGetValue("subtitle", out var subtitle) && subtitle.Visible)
            converted.Elements.Add(ToTextElement("subtitle", subtitle, cardWidth, "field", "descricao"));

        if (content.TryGetValue("price", out var price) && price.Visible)
            converted.Elements.Add(ToTextElement("price", price, cardWidth, "field", "valor"));

        if (content.TryGetValue("unit", out var unit) && unit.Visible)
            converted.Elements.Add(ToTextElement("unit", unit, cardWidth, "fixed", null));

        if (content.TryGetValue("footer", out var footer) && footer.Visible)
            converted.Elements.Add(ToTextElement("footer", footer, cardWidth, "field", "yield"));

        return SanitizeLayout(converted);
    }

    private static LayoutBuilderElement ToTextElement(string id, Element source, double cardWidth, string sourceType, string? fieldName)
    {
        var width = Math.Max(10, cardWidth - source.X - 2);
        var estimatedHeight = Math.Max(6, source.FontSize * 0.7);

        return new LayoutBuilderElement
        {
            Id = id,
            Type = "text",
            Source = sourceType,
            FieldName = fieldName,
            PreviewText = source.Text,
            XMm = source.X,
            YMm = source.Y,
            WidthMm = width,
            HeightMm = estimatedHeight,
            FontSize = source.FontSize,
            Bold = source.Bold,
            Align = source.Alignment,
            Color = source.Color,
        };
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

        var cardWidthMm = (float)_layout.Page.CardWidthMm;
        var cardHeightMm = (float)_layout.Page.CardHeightMm;
        var gapMm = (float)_layout.Page.GapMm;

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin((float)_layout.Page.MarginMm, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

            page.Content().Column(pageColumn =>
            {
                for (var groupIndex = 0; groupIndex < productGroups.Count; groupIndex++)
                {
                    var pageGroup = productGroups[groupIndex].ToList();

                    pageColumn.Item().Column(gridColumn =>
                    {
                        for (var row = 0; row < _layout.Page.Rows; row++)
                        {
                            gridColumn.Item().Height(cardHeightMm, Unit.Millimetre).Row(gridRow =>
                            {
                                for (var col = 0; col < _layout.Page.Columns; col++)
                                {
                                    if (col > 0 && gapMm > 0)
                                    {
                                        gridRow.ConstantItem(gapMm, Unit.Millimetre);
                                    }

                                    var itemIndex = row * _layout.Page.Columns + col;
                                    if (itemIndex < pageGroup.Count)
                                    {
                                        var produto = pageGroup[itemIndex].produto;
                                        gridRow.ConstantItem(cardWidthMm, Unit.Millimetre)
                                            .Height(cardHeightMm, Unit.Millimetre)
                                            .Element(card => RenderCard(card, produto));
                                    }
                                    else
                                    {
                                        gridRow.ConstantItem(cardWidthMm, Unit.Millimetre);
                                    }
                                }
                            });

                            if (row < _layout.Page.Rows - 1 && gapMm > 0)
                            {
                                gridColumn.Item().Height(gapMm, Unit.Millimetre);
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

    private void RenderCard(IContainer card, Produto produto)
    {
        card.Layers(layers =>
        {
            layers.PrimaryLayer().Extend();

            foreach (var element in _layout.Elements)
            {
                layers.Layer()
                    .TranslateX((float)element.XMm, Unit.Millimetre)
                    .TranslateY((float)element.YMm, Unit.Millimetre)
                    .Width((float)element.WidthMm, Unit.Millimetre)
                    .Height((float)element.HeightMm, Unit.Millimetre)
                    .Element(container => RenderElement(container, element, produto));
            }
        });
    }

    private void RenderElement(IContainer container, LayoutBuilderElement element, Produto produto)
    {
        if (element.Type == "qrcode")
        {
            var value = GetElementText(element, produto);
            if (string.IsNullOrWhiteSpace(value))
                return;

            var imagePath = _barcodeService.GenerateQRCode(value);
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                container.Image(imagePath).FitArea();
            }

            return;
        }

        if (element.Type == "barcode")
        {
            var value = GetElementText(element, produto);
            if (string.IsNullOrWhiteSpace(value))
                return;

            var barcodeType = element.BarcodeType ?? "ean13";
            var imagePath = _barcodeService.GenerateBarcode(value, barcodeType);
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                container.Image(imagePath).FitArea();
            }

            return;
        }

        if (element.Type == "image" && !string.IsNullOrWhiteSpace(element.ImagePath))
        {
            var fullPath = Path.Combine(AppContext.BaseDirectory, element.ImagePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                container.Image(fullPath).FitArea();
            }

            return;
        }

        var text = GetElementText(element, produto);
        if (string.IsNullOrWhiteSpace(text))
            return;

        var textElement = container.Text(text)
            .FontSize(element.FontSize)
            .FontColor(ParseColor(element.Color ?? "#000000"));

        if (element.Align == "center")
            textElement.AlignCenter();
        else if (element.Align == "right")
            textElement.AlignRight();

        if (element.Bold)
            textElement.SemiBold();
    }

    private string GetElementText(LayoutBuilderElement element, Produto produto)
    {
        return element.Type switch
        {
            "text" => element.Source switch
            {
                "fixed" => element.PreviewText ?? "",
                "field" => GetFieldValue(element.FieldName, produto),
                "formula" => EvaluateFormula(element.Formula ?? "", produto),
                _ => "",
            },
            "formula" => EvaluateFormula(element.Formula ?? "", produto),
            "qrcode" => element.Source switch
            {
                "fixed" => element.Value ?? "",
                "field" => GetFieldValue(element.FieldName, produto),
                "formula" => EvaluateFormula(element.Formula ?? "", produto),
                _ => "",
            },
            "barcode" => element.Source switch
            {
                "fixed" => element.Value ?? "",
                "field" => GetFieldValue(element.FieldName, produto),
                "formula" => EvaluateFormula(element.Formula ?? "", produto),
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
            "codigo_barras" => produto.Codigo,
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
                .Replace("yield", $"\"{produto.Yield}\"")
                .Replace("quantidadeimpresa", $"\"{produto.QuantidadeImpresa}\"");

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
