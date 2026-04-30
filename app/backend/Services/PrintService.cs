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
        var layoutConfig = LoadLayoutConfig();
        var document = new LayoutBasedDocument(produtos, layoutConfig);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
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
            Cards = new[]
            {
                new Card
                {
                    Id = "default",
                    X = 10,
                    Y = 10,
                    W = 92.5f,
                    H = 136,
                    Content = new CardContent
                    {
                        Title = new Element { X = 5, Y = 15, Text = "", FontSize = 16, Bold = true, Alignment = "left", Color = "#000", Visible = true },
                        Subtitle = new Element { X = 5, Y = 30, Text = "", FontSize = 12, Bold = false, Alignment = "left", Color = "#666", Visible = true },
                        Price = new Element { X = 5, Y = 60, Text = "", FontSize = 20, Bold = true, Alignment = "left", Color = "#000", Visible = true },
                        Unit = new Element { X = 70, Y = 63, Text = "KG", FontSize = 10, Bold = false, Alignment = "left", Color = "#666", Visible = true },
                        Footer = new Element { X = 5, Y = 115, Text = "", FontSize = 8, Bold = false, Alignment = "left", Color = "#999", Visible = true }
                    }
                }
            }
        };
    }
}

public class LayoutConfig
{
    public Card[] Cards { get; set; } = Array.Empty<Card>();
}

public class Card
{
    public string Id { get; set; } = "";
    public float X { get; set; }
    public float Y { get; set; }
    public float W { get; set; }
    public float H { get; set; }
    public CardContent Content { get; set; } = new();
}

public class CardContent
{
    public Element Title { get; set; } = new();
    public Element Subtitle { get; set; } = new();
    public Element Price { get; set; } = new();
    public Element Unit { get; set; } = new();
    public Element Footer { get; set; } = new();
}

public class Element
{
    public float X { get; set; }
    public float Y { get; set; }
    public string Text { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public int FontSize { get; set; }
    public bool Bold { get; set; }
    public string Alignment { get; set; } = "left";
    public string Color { get; set; } = "#000";
    public bool Visible { get; set; }
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
                                        var cardTemplate = _config.Cards[itemIndex % _config.Cards.Length];

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

    private void RenderProductCard(ColumnDescriptor card, Produto produto, CardContent template)
    {
        if (template.Title.Visible)
        {
            RenderElement(card, produto.Codigo, template.Title, 0);
        }

        if (template.Subtitle.Visible)
        {
            RenderElement(card, produto.Descricao, template.Subtitle, template.Subtitle.Y);
        }

        if (template.Price.Visible)
        {
            RenderElement(card, $"R$ {produto.Valor:N2}", template.Price, template.Price.Y);
        }

        if (template.Unit.Visible && !string.IsNullOrWhiteSpace(template.Unit.Text))
        {
            RenderElement(card, template.Unit.Text, template.Unit, template.Unit.Y);
        }

        if (template.Footer.Visible)
        {
            if (!string.IsNullOrWhiteSpace(produto.Yield))
            {
                RenderElement(card, $"Yield: {produto.Yield}", template.Footer, template.Footer.Y);
            }

            RenderElement(card, $"Impressões: {produto.QuantidadeImpresa}", template.Footer, template.Footer.Y + 10);
        }
    }

    private void RenderElement(ColumnDescriptor card, string text, Element element, float previousY)
    {
        if (!element.Visible) return;

        var spacing = element.Y - previousY;
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
            var textElement = card.Item().PaddingLeft(element.X, Unit.Millimetre).Text(text)
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
            card.Item().PaddingLeft(element.X, Unit.Millimetre).MaxWidth(50, Unit.Millimetre).Image(fullPath);
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
