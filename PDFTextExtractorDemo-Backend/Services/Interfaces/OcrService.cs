using Microsoft.AspNetCore.Http;
using PDFTextExtractorDemo_Backend.Models;

namespace PDFTextExtractorDemo_Backend.Services.Interfaces
{
    public interface IOcrService
    {
        Task<VoucherData> ProcessImage(IFormFile file);
    }
}