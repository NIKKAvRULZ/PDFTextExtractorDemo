// using System;
// using System.Data.SqlClient;
// using System.Text.RegularExpressions;
// using Tesseract;

// class OCRProcessor
// {
//     static void Main()
//     {
//         string imagePath = "voucher.png";  // Path to your invoice image

//         if (!System.IO.File.Exists(imagePath))
//         {
//             Console.WriteLine("❌ Image file not found!");
//             return;
//         }

//         try
//         {
//             // Extract text from the image
//             Console.WriteLine("🔍 Extracting text from image...");
//             string extractedText = ExtractTextFromImage(imagePath);

//             // Parse extracted data
//             Console.WriteLine("🔎 Parsing extracted data...");
//             var invoiceData = ParseInvoiceData(extractedText);

//             // Display extracted data
//             DisplayTable(invoiceData);

//             // Save data to SQL Server
//             Console.WriteLine("💾 Saving data to SQL Server...");
//             SaveToDatabase(invoiceData);

//             Console.WriteLine("\n✅ Data stored successfully in SQL Server!");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("❌ Error: " + ex.Message);
//         }
//     }

//     static string ExtractTextFromImage(string imagePath)
//     {
//         try
//         {
//             using (var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default))
//             {
//                 using (var img = Pix.LoadFromFile(imagePath))
//                 {
//                     // Convert image to grayscale to enhance text recognition
//                     var grayscaleImg = img.ConvertRGBToGray();

//                     // Apply binarization (thresholding) for better contrast
//                     grayscaleImg = grayscaleImg.BinarizeOtsuAdaptiveThreshold(200, 200, 10, 10, 0.1f);

//                     using (var page = engine.Process(grayscaleImg))
//                     {
//                         return page.GetText();
//                     }
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"❌ Error during OCR extraction: {ex.Message}");
//             throw;
//         }
//     }

//     static (string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount) ParseInvoiceData(string text)
//     {
//         // Regex patterns to extract required fields
//         string paidToPattern = @"Paid To\s*(\w+\s\w+)";
//         string datePattern = @"Date\s*(\d{2}/\d{2}/\d{4})";
//         string voucherNoPattern = @"Voucher No\s*(\d+)";
//         string emailPattern = @"Email\s*([\w\.-]+@[\w\.-]+)";
//         string contactPattern = @"Contact No\s*(\d+)";
//         string totalAmountPattern = @"Final Amount\s*(\d+\.\d{2})";

//         try
//         {
//             string paidTo = Regex.Match(text, paidToPattern).Groups[1].Value;
//             string date = Regex.Match(text, datePattern).Groups[1].Value;
//             string voucherNo = Regex.Match(text, voucherNoPattern).Groups[1].Value;
//             string email = Regex.Match(text, emailPattern).Groups[1].Value;
//             string contactNo = Regex.Match(text, contactPattern).Groups[1].Value;
//             decimal totalAmount = decimal.TryParse(Regex.Match(text, totalAmountPattern).Groups[1].Value, out decimal parsedAmount) ? parsedAmount : 0;

//             return (paidTo, date, voucherNo, email, contactNo, totalAmount);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"❌ Error parsing invoice data: {ex.Message}");
//             throw;
//         }
//     }

//     static void DisplayTable((string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount) data)
//     {
//         Console.WriteLine("\n🧾 Extracted Petty Cash Voucher Data:");
//         Console.WriteLine("+------------+------------+------------+----------------------+---------------+------------+");
//         Console.WriteLine("| Paid To    | Date       | Voucher No | Email                | Contact No    | Amount     |");
//         Console.WriteLine("+------------+------------+------------+----------------------+---------------+------------+");
//         Console.WriteLine($"| {data.PaidTo,-10} | {data.Date,-10} | {data.VoucherNo,-10} | {data.Email,-20} | {data.ContactNo,-13} | {data.TotalAmount,-10} |");
//         Console.WriteLine("+------------+------------+------------+----------------------+---------------+------------+");
//     }

//     static void SaveToDatabase((string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount) data)
//     {
//         string connectionString = "Server=NIKKA;Database=PettyCashDB;Integrated Security=True;";

//         try
//         {
//             // Log the connection string being used
//             Console.WriteLine("🔗 Connecting to SQL Server...");

//             using (SqlConnection connection = new SqlConnection(connectionString))
//             {
//                 connection.Open();
//                 Console.WriteLine("✅ Connection established successfully.");

//                 string insertCommand = "INSERT INTO PettyCashVouchers1 (PaidTo, Date, VoucherNo, Email, ContactNo, TotalAmount) VALUES (@PaidTo, @Date, @VoucherNo, @Email, @ContactNo, @TotalAmount)";

//                 using (SqlCommand command = new SqlCommand(insertCommand, connection))
//                 {
//                     command.Parameters.AddWithValue("@PaidTo", data.PaidTo);
//                     command.Parameters.AddWithValue("@Date", data.Date);
//                     command.Parameters.AddWithValue("@VoucherNo", data.VoucherNo);
//                     command.Parameters.AddWithValue("@Email", data.Email);
//                     command.Parameters.AddWithValue("@ContactNo", data.ContactNo);
//                     command.Parameters.AddWithValue("@TotalAmount", data.TotalAmount);

//                     Console.WriteLine("📝 Executing insert query...");
//                     int rowsAffected = command.ExecuteNonQuery();

//                     if (rowsAffected > 0)
//                     {
//                         Console.WriteLine("✅ Data successfully saved to SQL Server.");
//                     }
//                     else
//                     {
//                         Console.WriteLine("❌ No rows affected. Data may not have been inserted.");
//                     }
//                 }
//             }
//         }
//         catch (SqlException ex)
//         {
//             Console.WriteLine("❌ SQL Error: " + ex.Message);
//             Console.WriteLine($"Stack Trace: {ex.StackTrace}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("❌ General Error: " + ex.Message);
//             Console.WriteLine($"Stack Trace: {ex.StackTrace}");
//         }
//     }
// }
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using Tesseract;

class OCRProcessor
{
    static void Main()
    {
        string imagePath = "voucher2.png";  // Path to your invoice image

        if (!System.IO.File.Exists(imagePath))
        {
            Console.WriteLine("❌ Image file not found!");
            return;
        }

        try
        {
            // Extract text from the image
            Console.WriteLine("🔍 Extracting text from image...");
            string extractedText = ExtractTextFromImage(imagePath);

            // Debug: Show raw extracted text
            Console.WriteLine("=== RAW EXTRACTED TEXT ===");
            Console.WriteLine(extractedText);
            Console.WriteLine("==========================");

            // Parse extracted data
            Console.WriteLine("🔎 Parsing extracted data...");
            var (voucherData, lineItems) = ParseInvoiceData(extractedText);

            // Debug: Show parsed data before display
            Console.WriteLine("\n=== PARSED DATA DEBUG ===");
            Console.WriteLine($"PaidTo: {voucherData.PaidTo}");
            Console.WriteLine($"Date: {voucherData.Date}");
            Console.WriteLine($"VoucherNo: {voucherData.VoucherNo}");
            Console.WriteLine($"Email: {voucherData.Email}");
            Console.WriteLine($"ContactNo: {voucherData.ContactNo}");
            Console.WriteLine($"TotalAmount: {voucherData.TotalAmount}");
            Console.WriteLine($"Line Items Count: {lineItems.Count}");
            foreach (var item in lineItems)
            {
                Console.WriteLine($"- {item.ItemDate} | {item.Description} | {item.Amount}");
            }
            Console.WriteLine("========================");

            // Display extracted data
            DisplayTable(voucherData, lineItems);

            // Save data to SQL Server
            Console.WriteLine("💾 Saving data to SQL Server...");
            SaveToDatabase(voucherData, lineItems);

            Console.WriteLine("\n✅ Data stored successfully in SQL Server!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error: " + ex.Message);
            Console.WriteLine("Stack Trace: " + ex.StackTrace);
        }
    }

    static string ExtractTextFromImage(string imagePath)
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

    static (
        (string PaidTo, string Date, string VoucherNo, string Email, string ContactNo, decimal TotalAmount),
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
            string paidTo = SafeRegexExtract(text, paidToPattern, 1, "Paid To")?.Trim();
            string date = SafeRegexExtract(text, datePattern, 1, "Date")?.Trim();
            string voucherNo = SafeRegexExtract(text, voucherNoPattern, 1, "Voucher No")?.Trim();
            string email = SafeRegexExtract(text, emailPattern, 1, "Email")?.Trim();
            string contactNo = SafeRegexExtract(text, contactPattern, 1, "Contact No")?.Trim();
            
            decimal totalAmount = 0;
            string amountStr = SafeRegexExtract(text, totalAmountPattern, 1, "Total Amount");
            if (!string.IsNullOrEmpty(amountStr))
            {
                decimal.TryParse(amountStr.Replace(",", ""), out totalAmount);
            }

            // Parse line items
            var lineItems = ParseLineItems(text);

            return ((paidTo, date, voucherNo, email, contactNo, totalAmount), lineItems);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing invoice data: {ex.Message}");
            throw;
        }
    }

    static string SafeRegexExtract(string text, string pattern, int group, string fieldName)
    {
        try
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > group)
            {
                return match.Groups[group].Value;
            }
            Console.WriteLine($"⚠️ Could not extract {fieldName} using pattern: {pattern}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error extracting {fieldName}: {ex.Message}");
            return null;
        }
    }

    static List<(string ItemDate, string Description, decimal Amount)> ParseLineItems(string text)
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