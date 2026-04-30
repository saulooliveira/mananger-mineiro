using System.Text.Json.Serialization;
using Backend.Models;

namespace Backend.Data;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(Produto))]
[JsonSerializable(typeof(List<Produto>))]
[JsonSerializable(typeof(LayoutConfig))]
[JsonSerializable(typeof(UploadResponse))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}

public class LayoutConfig
{
    public List<Card> Cards { get; set; } = new();
    public int PageMargin { get; set; } = 10;
    public int GridColumns { get; set; } = 2;
    public int GridRows { get; set; } = 2;
    public int GridGapMm { get; set; } = 5;
}

public class Card
{
    public string Id { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    public Dictionary<string, Element> Content { get; set; } = new();
}

public class Element
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Text { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public int FontSize { get; set; } = 12;
    public bool Bold { get; set; }
    public string Alignment { get; set; } = "left";
    public string Color { get; set; } = "#000";
    public bool Visible { get; set; } = true;
}

public class UploadResponse
{
    public bool Success { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
