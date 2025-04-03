using Microsoft.AspNetCore.Mvc;
using PDFTextExtractorDemo_Backend.Services.Interfaces;

namespace PDFTextExtractorDemo_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public OcrController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                var result = await _ocrService.ProcessImage(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}