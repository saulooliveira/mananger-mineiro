using System.Drawing;
using QRCoder;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Backend.Services;

public class BarcodeService
{
    private readonly string _tempDir = Path.Combine(AppContext.BaseDirectory, "temp-barcodes");

    public BarcodeService()
    {
        if (!Directory.Exists(_tempDir))
        {
            Directory.CreateDirectory(_tempDir);
        }
    }

    public string GenerateQRCode(string value)
    {
        try
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                    {
                        var filename = $"qr_{Guid.NewGuid()}.png";
                        var filepath = Path.Combine(_tempDir, filename);
                        qrCodeImage.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                        return filepath;
                    }
                }
            }
        }
        catch
        {
            return "";
        }
    }

    public string GenerateBarcode(string value, string type = "ean13")
    {
        try
        {
            var barcodeFormat = type.ToLower() switch
            {
                "code128" => BarcodeFormat.CODE_128,
                "ean13" => BarcodeFormat.EAN_13,
                _ => BarcodeFormat.EAN_13,
            };

            var writer = new BarcodeWriter<Bitmap>
            {
                Format = barcodeFormat,
                Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 150,
                    Margin = 10
                }
            };

            using (var bitmap = writer.Write(value))
            {
                var filename = $"barcode_{Guid.NewGuid()}.png";
                var filepath = Path.Combine(_tempDir, filename);
                bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"[BarcodeService] Barcode salvo: {filepath}");
                return filepath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BarcodeService] Erro ao gerar barcode: {ex.Message}");
            Console.WriteLine($"[BarcodeService] Stack: {ex.StackTrace}");
            return "";
        }
    }

    public void CleanupTempFiles()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                foreach (var file in Directory.GetFiles(_tempDir, "*.png"))
                {
                    var fileInfo = new FileInfo(file);
                    if (DateTime.Now - fileInfo.CreationTime > TimeSpan.FromHours(1))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
        catch { }
    }
}
