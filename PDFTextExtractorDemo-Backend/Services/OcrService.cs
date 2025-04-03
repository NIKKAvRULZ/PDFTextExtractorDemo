using PDFTextExtractorDemo_Backend.Data;
using PDFTextExtractorDemo_Backend.Models;
using PDFTextExtractorDemo_Backend.Services.Interfaces;
using Tesseract;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace PDFTextExtractorDemo_Backend.Services
{
    public class OcrService : IOcrService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _tessdataPath;

        public OcrService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _tessdataPath = Path.Combine(env.ContentRootPath, "tessdata");
        }

        public async Task<VoucherData> ProcessImage(IFormFile file)
        {
            string tempImagePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempImagePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string extractedText = ExtractTextFromImage(tempImagePath);
                var (paidTo, date, voucherNo, email, contactNo, totalAmount, lineItems) = ParseInvoiceData(extractedText);

                var voucherData = new VoucherData
                {
                    PaidTo = paidTo,
                    Date = date,
                    VoucherNo = voucherNo,
                    Email = email,
                    ContactNo = contactNo,
                    TotalAmount = totalAmount,
                    LineItems = new List<LineItem>()
                };

                // Create LineItems with proper reference to VoucherData
                foreach (var item in lineItems)
                {
                    voucherData.LineItems.Add(new LineItem
                    {
                        ItemDate = item.ItemDate,
                        Description = item.Description,
                        Amount = item.Amount,
                        VoucherData = voucherData
                    });
                }

                _context.VoucherData.Add(voucherData);
                await _context.SaveChangesAsync();

                return voucherData;
            }
            finally
            {
                if (File.Exists(tempImagePath))
                {
                    File.Delete(tempImagePath);
                }
            }
        }

        private string ExtractTextFromImage(string imagePath)
        {
            using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }

        private (string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, 
                decimal TotalAmount, List<(string ItemDate, string Description, decimal Amount)> LineItems) 
        ParseInvoiceData(string text)
        {
            string paidTo = ExtractValue(text, @"Paid To\s*:\s*([^\n]+)") ?? "Unknown";
            string date = ExtractValue(text, @"Date\s*:\s*([^\n]+)") ?? DateTime.Now.ToString("yyyy-MM-dd");
            string voucherNo = ExtractValue(text, @"Voucher No\s*:\s*([^\n]+)") ?? "Unknown";
            string email = ExtractValue(text, @"Email\s*:\s*([^\n]+)") ?? "Unknown";
            string contactNo = ExtractValue(text, @"Contact No\s*:\s*([^\n]+)") ?? "Unknown";
            
            decimal.TryParse(ExtractValue(text, @"Total Amount\s*:\s*(\d+\.?\d*)"), out decimal totalAmount);
            
            var lineItems = ParseLineItems(text);
            
            return (paidTo, date, voucherNo, email, contactNo, totalAmount, lineItems);
        }

        private string? ExtractValue(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private List<(string ItemDate, string Description, decimal Amount)> ParseLineItems(string text)
        {
            var items = new List<(string ItemDate, string Description, decimal Amount)>();
            var itemPattern = @"(\d{2}/\d{2}/\d{4})\s+([^\d]+)\s+(\d+\.?\d*)";
            
            var matches = Regex.Matches(text, itemPattern);
            foreach (Match match in matches)
            {
                if (match.Groups.Count == 4)
                {
                    decimal.TryParse(match.Groups[3].Value, out decimal amount);
                    items.Add((
                        match.Groups[1].Value.Trim(),
                        match.Groups[2].Value.Trim(),
                        amount
                    ));
                }
            }
            
            return items;
        }
    }
}