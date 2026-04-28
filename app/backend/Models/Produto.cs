namespace Backend.Models;

public class Produto
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public string? Yield { get; set; }
    public required string Descricao { get; set; }
    public string? Categoria { get; set; }
    public decimal Valor { get; set; } = 0m;
    public int QuantidadeImpresa { get; set; } = 0;
}
