using System.Drawing;
using Microsoft.Extensions.Logging;
using QRCoder;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace Backend.Services;

// Custom Bitmap renderer for ZXing
internal class BitmapRenderer : IBarcodeRenderer<Bitmap>
{
    public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
    {
        return RenderBitmap(matrix);
    }

    public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
    {
        return RenderBitmap(matrix);
    }

    private Bitmap RenderBitmap(BitMatrix matrix)
    {
        int width = matrix.Width;
        int height = matrix.Height;
        var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
            }
        }

        return bitmap;
    }
}

public class BarcodeService
{
    private readonly string _tempDir = Path.Combine(AppContext.BaseDirectory, "temp-barcodes");
    private readonly ILogger<BarcodeService> _logger;

    public BarcodeService(ILogger<BarcodeService> logger)
    {
        _logger = logger;
        try
        {
            if (!Directory.Exists(_tempDir))
            {
                Directory.CreateDirectory(_tempDir);
                _logger.LogInformation("[BarcodeService] Temp directory created: {TempDir}", _tempDir);
            }
            else
            {
                _logger.LogInformation("[BarcodeService] Temp directory already exists: {TempDir}", _tempDir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BarcodeService] Error creating temp directory: {TempDir}", _tempDir);
        }
    }

    public string GenerateQRCode(string value)
    {
        try
        {
            _logger.LogInformation("[BarcodeService] GenerateQRCode: value={Value}", value);
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
                        _logger.LogInformation("[BarcodeService] QR code saved: {FilePath}", filepath);
                        return filepath;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BarcodeService] Error generating QR code: {Message}", ex.Message);
            return "";
        }
    }

    public string GenerateBarcode(string value, string type = "ean13")
    {
        try
        {
            _logger.LogInformation("[BarcodeService] GenerateBarcode: value={Value}, type={Type}", value, type);

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
                },
                Renderer = new BitmapRenderer()
            };
            _logger.LogInformation("[BarcodeService] BarcodeWriter created with custom BitmapRenderer");

            Bitmap? bitmap = null;
            try
            {
                bitmap = writer.Write(value);
                _logger.LogInformation("[BarcodeService] Bitmap generated successfully");
            }
            catch (Exception wex)
            {
                _logger.LogError(wex, "[BarcodeService] Error in writer.Write: {Message}", wex.Message);
                throw;
            }

            using (bitmap)
            {
                if (bitmap == null)
                {
                    _logger.LogWarning("[BarcodeService] Bitmap is null");
                    return "";
                }

                if (!Directory.Exists(_tempDir))
                {
                    _logger.LogWarning("[BarcodeService] Temp dir doesn't exist, creating: {TempDir}", _tempDir);
                    Directory.CreateDirectory(_tempDir);
                }

                var filename = $"barcode_{Guid.NewGuid()}.png";
                var filepath = Path.Combine(_tempDir, filename);
                _logger.LogInformation("[BarcodeService] About to save to: {FilePath}", filepath);

                try
                {
                    bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                    _logger.LogInformation("[BarcodeService] Barcode saved successfully: {FilePath}", filepath);
                    return filepath;
                }
                catch (Exception sex)
                {
                    _logger.LogError(sex, "[BarcodeService] Error during bitmap.Save: {Message}", sex.Message);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BarcodeService] FATAL ERROR generating barcode: {Message}", ex.Message);
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
