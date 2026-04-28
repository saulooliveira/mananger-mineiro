using System.IO;
using Backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services;

public class PrintService
{
    public byte[] GeneratePreviewPdf(List<Produto> produtos)
    {
        var document = new ProductPreviewDocument(produtos);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}

internal class ProductPreviewDocument : IDocument
{
    private readonly List<Produto> _produtos;

    public ProductPreviewDocument(List<Produto> produtos)
    {
        _produtos = produtos;
    }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        var productGroups = _produtos
            .Select((produto, index) => new { produto, index })
            .GroupBy(x => x.index / 4)
            .ToList();

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

            page.Content().Column(column =>
            {
                column.Spacing(10);
                column.Item().Text("Preview de Ofertas").FontSize(18).SemiBold();
                column.Item().Text($"Total de produtos: {_produtos.Count}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);

                for (var groupIndex = 0; groupIndex < productGroups.Count; groupIndex++)
                {
                    var pageGroup = productGroups[groupIndex];

                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                    column.Item().PaddingTop(10).Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.Spacing(10);

                        foreach (var item in pageGroup)
                        {
                            grid.Item().Padding(8).Background(Colors.White).Border(1).BorderColor(Colors.Grey.Lighten2).Column(card =>
                            {
                                card.Spacing(6);
                                card.Item().Text(item.produto.Codigo).FontSize(12).SemiBold();
                                card.Item().Text(item.produto.Descricao).FontSize(14).Bold();
                                card.Item().Text(item.produto.Categoria ?? "Categoria não definida").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                card.Item().Text($"Valor: R$ {item.produto.Valor:N2}").FontSize(12).Bold();

                                if (!string.IsNullOrWhiteSpace(item.produto.Yield))
                                {
                                    card.Item().Text($"Yield: {item.produto.Yield}").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);
                                }

                                card.Item().Text($"Impressões: {item.produto.QuantidadeImpresa}").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                            });
                        }

                        if (pageGroup.Count() == 1)
                        {
                            grid.Item().Text(string.Empty);
                            grid.Item().Text(string.Empty);
                        }
                        else if (pageGroup.Count() == 3)
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
}
