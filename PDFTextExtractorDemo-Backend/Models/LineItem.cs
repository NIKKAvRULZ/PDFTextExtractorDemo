namespace PDFTextExtractorDemo_Backend.Models
{
    public class LineItem
    {
        public int Id { get; set; }
        public int VoucherDataId { get; set; }
        public required string ItemDate { get; set; }
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public required VoucherData VoucherData { get; set; }
    }
}