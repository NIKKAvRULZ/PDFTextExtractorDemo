using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tesseract;

public class OCRProcessor
{
    public static async Task ProcessUploadedImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            Console.WriteLine("❌ No image file uploaded!");
            return;
        }

        try
        {
            // Create a temporary file to store the uploaded image
            string tempImagePath = Path.GetTempFileName();
            
            // Save the uploaded file to temp location
            using (var stream = new FileStream(tempImagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Extract text from the image
            Console.WriteLine("🔍 Extracting text from uploaded image...");
            string extractedText = ExtractTextFromImage(tempImagePath);

            // Clean up the temporary file
            File.Delete(tempImagePath);

            // Parse extracted data
            var (paidTo, date, voucherNo, email, contactNo, totalAmount, lineItems) = ParseInvoiceData(extractedText);
            var voucherData = (PaidTo: paidTo, Date: date, VoucherNo: voucherNo, Email: email, ContactNo: contactNo, TotalAmount: totalAmount);

            // Display and save data
            DisplayTable(voucherData, lineItems);
            SaveToDatabase(voucherData, lineItems);

            Console.WriteLine("\n✅ Data stored successfully in SQL Server!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
        }
    }

    public static string ExtractTextFromImage(string imagePath)
    {
        try
        {
            using (var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    var grayscaleImg = img.ConvertRGBToGray();
                    grayscaleImg = grayscaleImg.BinarizeOtsuAdaptiveThreshold(200, 200, 10, 10, 0.1f);

                    using (var page = engine.Process(grayscaleImg))
                    {
                        return page.GetText();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during OCR extraction: {ex.Message}");
            throw;
        }
    }

    public static string SafeRegexExtract(string input, string pattern, int groupIndex)
    {
        try
        {
            var match = Regex.Match(input, pattern);
            if (match.Success && match.Groups.Count > groupIndex)
            {
                return match.Groups[groupIndex].Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in SafeRegexExtract: {ex.Message}");
        }
        return null;
    }
            // Removed redundant line as 'paidTo' is already assigned in ParseInvoiceData
    public static (
        string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount,
        List<(string ItemDate, string Description, decimal Amount)>
    ) ParseInvoiceData(string text)
    {
        // Improved regex patterns with more flexible matching
        string paidToPattern = @"Paid To\s*(\w+\s\w+)";
        string datePattern = @"Date\s*(\d{2}/\d{2}/\d{4})";
        string voucherNoPattern = @"Voucher No\s*(\d+)";
        string emailPattern = @"Email\s*([\w\.-]+@[\w\.-]+)";
        string contactPattern = @"Contact No\s*(\d+)";
        string totalAmountPattern = @"Final Amount\s*(\d+\.\d{2})";

        try
        {
            // Parse main voucher data
            string paidTo = SafeRegexExtract(text, paidToPattern, 1)?.Trim();
            string date = SafeRegexExtract(text, datePattern, 1)?.Trim();
            string voucherNo = SafeRegexExtract(text, voucherNoPattern, 1)?.Trim();
            string email = SafeRegexExtract(text, emailPattern, 1)?.Trim();
            string contactNo = SafeRegexExtract(text, contactPattern, 1)?.Trim();
            
            decimal totalAmount = 0;
            string amountStr = SafeRegexExtract(text, totalAmountPattern, 1);
            if (!string.IsNullOrEmpty(amountStr))
            {
                decimal.TryParse(amountStr.Replace(",", ""), out totalAmount);
            }

            // Parse line items
            var lineItems = ParseLineItems(text);

            return (paidTo, date, voucherNo, email, contactNo, totalAmount, lineItems);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing invoice data: {ex.Message}");
            throw;
        }
    }

    public static List<(string ItemDate, string Description, decimal Amount)> ParseLineItems(string text)
    {
        var lineItems = new List<(string, string, decimal)>();
        var lines = text.Split('\n');

        foreach (var line in lines)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Try to parse line with number prefix (e.g., "1 22/12/2024 item/ Equipment No.1 3000.00")
            var match = Regex.Match(line.Trim(), 
                @"^\d+\s+(\d{2}/\d{2}/\d{4})\s+(.+?)\s+(\d+\.\d{2})$");
            if (match.Success)
            {
                string date = match.Groups[1].Value.Trim();
                string description = match.Groups[2].Value.Trim();
                string amountStr = match.Groups[3].Value.Replace(",", "").Trim();

                if (DateTime.TryParseExact(date, "dd/MM/yyyy", null, 
                    System.Globalization.DateTimeStyles.None, out _) &&
                    decimal.TryParse(amountStr, out decimal amount))
                {
                    lineItems.Add((date, description, amount));
                    continue;
                }
            }

            // Alternative parsing without line numbers if the above fails
            match = Regex.Match(line.Trim(),
                @"^(\d{2}/\d{2}/\d{4})\s+(.+?)\s+(\d+\.\d{2})$");
            if (match.Success)
            {
                string date = match.Groups[1].Value.Trim();
                string description = match.Groups[2].Value.Trim();
                string amountStr = match.Groups[3].Value.Replace(",", "").Trim();

                if (DateTime.TryParseExact(date, "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out _) &&
                    decimal.TryParse(amountStr, out decimal amount))
                {
                    lineItems.Add((date, description, amount));
                }
            }
        }

        return lineItems;
    }

    static void DisplayTable(
        (string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount) voucherData,
        List<(string ItemDate, string Description, decimal Amount)> lineItems)
    {
        try
        {
            Console.WriteLine("\n🧾 Extracted Petty Cash Voucher Data:");
            Console.WriteLine("+------------+------------+------------+----------------------+---------------+------------+");
            Console.WriteLine("| Paid To    | Date       | Voucher No | Email                | Contact No    | Amount     |");
            Console.WriteLine("+------------+------------+------------+----------------------+---------------+------------+");
            
            Console.WriteLine($"| {voucherData.PaidTo?.PadRight(10).Substring(0, 10)} | " +
                            $"{voucherData.Date?.PadRight(10).Substring(0, 10)} | " +
                            $"{voucherData.VoucherNo?.PadRight(10).Substring(0, 10)} | " +
                            $"{voucherData.Email?.PadRight(20).Substring(0, 20)} | " +
                            $"{voucherData.ContactNo?.PadRight(13).Substring(0, 13)} | " +
                            $"{voucherData.TotalAmount.ToString("N2").PadRight(10).Substring(0, 10)} |");
            
            Console.WriteLine("+------------+------------+------------+----------------------+---------------+------------+");

            Console.WriteLine("\n📋 Line Items:");
            Console.WriteLine("+------------+--------------------------------+------------+");
            Console.WriteLine("| Date       | Description                   | Amount     |");
            Console.WriteLine("+------------+--------------------------------+------------+");
            
            foreach (var item in lineItems)
            {
                Console.WriteLine($"| {item.ItemDate?.PadRight(10).Substring(0, 10)} | " +
                                $"{item.Description?.PadRight(30).Substring(0, 30)} | " +
                                $"{item.Amount.ToString("N2").PadRight(10).Substring(0, 10)} |");
            }
            
            Console.WriteLine("+------------+--------------------------------+------------+");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error displaying data: {ex.Message}");
        }
    }

    static void SaveToDatabase(
        (string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount) voucherData,
        List<(string ItemDate, string Description, decimal Amount)> lineItems)
    {
        string connectionString = "Server=NIKKA;Database=PettyCashDB;Integrated Security=True;";

        try
        {
            Console.WriteLine("🔗 Connecting to SQL Server...");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("✅ Connection established successfully.");

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert main voucher
                        string insertVoucherCommand = @"
                            INSERT INTO PettyCashVouchers 
                            (PaidTo, Date, VoucherNo, Email, ContactNo, TotalAmount) 
                            VALUES (@PaidTo, @Date, @VoucherNo, @Email, @ContactNo, @TotalAmount);
                            SELECT SCOPE_IDENTITY();";

                        int voucherId;
                        using (SqlCommand command = new SqlCommand(insertVoucherCommand, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@PaidTo", (object)voucherData.PaidTo ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Date", (object)voucherData.Date ?? DBNull.Value);
                            command.Parameters.AddWithValue("@VoucherNo", (object)voucherData.VoucherNo ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Email", (object)voucherData.Email ?? DBNull.Value);
                            command.Parameters.AddWithValue("@ContactNo", (object)voucherData.ContactNo ?? DBNull.Value);
                            command.Parameters.AddWithValue("@TotalAmount", voucherData.TotalAmount);

                            Console.WriteLine("📝 Inserting voucher record...");
                            voucherId = Convert.ToInt32(command.ExecuteScalar());
                            Console.WriteLine($"✅ Voucher record inserted with ID: {voucherId}");
                        }

                        // Insert line items
                        if (lineItems.Count > 0)
                        {
                            string insertLineItemCommand = @"
                                INSERT INTO VoucherLineItems 
                                (VoucherId, ItemDate, Description, Amount) 
                                VALUES (@VoucherId, @ItemDate, @Description, @Amount)";

                            Console.WriteLine("📝 Inserting line items...");
                            int itemsInserted = 0;
                            foreach (var item in lineItems)
                            {
                                using (SqlCommand command = new SqlCommand(insertLineItemCommand, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@VoucherId", voucherId);
                                    command.Parameters.AddWithValue("@ItemDate", 
                                        DateTime.ParseExact(item.ItemDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                                    command.Parameters.AddWithValue("@Description", item.Description);
                                    command.Parameters.AddWithValue("@Amount", item.Amount);

                                    itemsInserted += command.ExecuteNonQuery();
                                }
                            }
                            Console.WriteLine($"✅ Inserted {itemsInserted} line items for voucher {voucherId}");
                        }

                        transaction.Commit();
                        Console.WriteLine("💾 Transaction committed successfully.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("❌ Transaction rolled back due to error.");
                        Console.WriteLine($"Error details: {ex.Message}");
                        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                        throw;
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine("❌ SQL Error: " + ex.Message);
            Console.WriteLine($"SQL Error Number: {ex.Number}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ General Error: " + ex.Message);
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class OCRController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        try
        {
            await OCRProcessor.ProcessUploadedImage(file);
            return Ok("Image processed successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}