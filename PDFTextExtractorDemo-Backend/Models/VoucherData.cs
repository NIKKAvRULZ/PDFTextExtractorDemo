using System.Collections.Generic;

namespace PDFTextExtractorDemo_Backend.Models
{
    public class VoucherData
    {
        public int Id { get; set; }
        public required string PaidTo { get; set; }
        public required string Date { get; set; }
        public required string VoucherNo { get; set; }
        public required string Email { get; set; }
        public required string ContactNo { get; set; }
        public decimal TotalAmount { get; set; }
        public required List<LineItem> LineItems { get; set; } = new();
    }
}