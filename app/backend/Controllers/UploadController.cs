using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly string _uploadPath = Path.Combine(AppContext.BaseDirectory, "uploads");

    public UploadController()
    {
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "Arquivo não enviado" });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { error = "Formato de imagem não permitido" });
        }

        try
        {
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new {
                success = true,
                filename = fileName,
                path = $"/uploads/{fileName}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
