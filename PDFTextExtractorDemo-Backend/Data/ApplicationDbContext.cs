using Microsoft.EntityFrameworkCore;
using PDFTextExtractorDemo_Backend.Models;

namespace PDFTextExtractorDemo_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<VoucherData> VoucherData { get; set; }
        public DbSet<LineItem> LineItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineItem>()
                .HasOne(l => l.VoucherData)
                .WithMany(v => v.LineItems)
                .HasForeignKey(l => l.VoucherDataId);
        }
    }
}