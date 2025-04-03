using Microsoft.AspNetCore.Mvc;
using PDFTextExtractorDemo_Backend.Services.Interfaces;

namespace PDFTextExtractorDemo_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtractController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public ExtractController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost]
        public async Task<IActionResult> Extract([FromForm] IFormFile file)
        {
            var result = await _ocrService.ProcessImage(file);
            return Ok(result);
        }
    }

    public class ExtractRequest
    {
        public string? FilePath { get; set; }
    }
}