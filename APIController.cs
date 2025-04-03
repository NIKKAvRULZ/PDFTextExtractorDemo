using Microsoft.AspNetCore.Mvc;
using System.IO;
using Tesseract;

[ApiController]
[Route("[controller]")]
public class OCRController : ControllerBase
{
    [HttpPost("upload")]
    public IActionResult UploadInvoice([FromForm] IFormFile invoice)
    {
        if (invoice == null || invoice.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        try
        {
            // Save the file temporarily
            var tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                invoice.CopyTo(stream);
            }

            // Process the file with Tesseract
            string extractedText = ExtractTextFromImage(tempFilePath);

            // Return the extracted text
            return Ok(new { extractedText });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private string ExtractTextFromImage(string imagePath)
    {
        using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
        {
            using (var img = Pix.LoadFromFile(imagePath))
            {
                using (var page = engine.Process(img))
                {
                    return page.GetText();
                }
            }
        }
    }
}