namespace Backend.Models;

public class PrintHistory
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string ProdutoIds { get; set; } = string.Empty;
    public int NumeroFolhas { get; set; }
    public int QuantidadeProdutos { get; set; }
}
