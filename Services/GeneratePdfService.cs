using Banko.Models;
using System.IO;
using System.Reflection;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.Text;

namespace Banko.Services
{
  public static class GeneratePdfService
  {
    private sealed class CustomFontResolver : IFontResolver
    {
      // Static constructor to initialize Unicode encoding support
      static CustomFontResolver()
      {
        // Ensures PdfSharp can handle Unicode characters
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      }

      public byte[]? GetFont(string faceName)
      {
        // We'll embed a simple font resource as a fallback
        if (faceName.StartsWith("Arial#") || faceName.StartsWith("Helvetica#"))
        {
          // Load embedded font resource (this is the actual font file bytes)
          var assembly = Assembly.GetExecutingAssembly();
          using var stream = assembly.GetManifestResourceStream("Banko.Fonts.arial.ttf");

          if (stream == null)
          {
            // If no embedded font, create a simple font substitute (not ideal but prevents crashes)
            return File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.Fonts), "arial.ttf"));
          }

          using var ms = new MemoryStream();
          stream.CopyTo(ms);
          return ms.ToArray();
        }

        return null;
      }

      public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
      {
        // Create a font key that's used by PdfSharp's font cache
        string fontName;
        if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase) ||
            familyName.Equals("Helvetica", StringComparison.OrdinalIgnoreCase))
        {
          fontName = "Arial#";
          if (isBold && isItalic) fontName += "BoldItalic";
          else if (isBold) fontName += "Bold";
          else if (isItalic) fontName += "Italic";
          else fontName += "Regular";
        }
        else
        {
          // Default to Arial for other fonts
          fontName = "Arial#Regular";
        }

        return new FontResolverInfo(fontName);
      }
    }

    static GeneratePdfService()
    {
      // Configure the font resolver once when the class is first used
      if (GlobalFontSettings.FontResolver == null)
      {
        GlobalFontSettings.FontResolver = new CustomFontResolver();
      }
    }

    private const string DefaultFontName = "Arial";

    public static byte[] GenerateStatementPdf(IEnumerable<Transactions> transactions, DateTime startDate, DateTime endDate)
    {
      // Create a new PDF document
      var document = new PdfDocument();
      var page = document.AddPage();
      var gfx = XGraphics.FromPdfPage(page);

      // Define fonts
      var titleFont = new XFont(DefaultFontName, 18, XFontStyleEx.Bold);
      var headerFont = new XFont(DefaultFontName, 12, XFontStyleEx.Bold);
      var normalFont = new XFont(DefaultFontName, 10, XFontStyleEx.Regular);
      var smallFont = new XFont(DefaultFontName, 8, XFontStyleEx.Regular);

      // Set up drawing positions
      double yPosition = 50;
      double leftMargin = 50;
      double rightMargin = page.Width.Point - 50;

      // Header - Bank Title
      gfx.DrawString("BANKO - Bank Statement", titleFont, XBrushes.DarkBlue,
          new XRect(leftMargin, yPosition, page.Width.Point - 100, 30), XStringFormats.TopCenter);
      yPosition += 40;

      // Statement Period
      gfx.DrawString($"Statement Period: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
          normalFont, XBrushes.Black, leftMargin, yPosition);
      yPosition += 20;

      // Generated Date
      gfx.DrawString($"Generated: {DateTime.UtcNow:dd/MM/yyyy HH:mm}",
          normalFont, XBrushes.Black, leftMargin, yPosition);
      yPosition += 40;

      // Transaction Summary
      var totalCredits = transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);
      var totalDebits = transactions.Where(t => t.Type == TransactionType.Withdrawal || t.Type == TransactionType.Transfer).Sum(t => t.Amount);

      gfx.DrawString("TRANSACTION SUMMARY", headerFont, XBrushes.Black, leftMargin, yPosition);
      yPosition += 25;

      gfx.DrawString($"Total Credits: ${totalCredits:N2}", normalFont, XBrushes.Green, leftMargin, yPosition);
      yPosition += 15;

      gfx.DrawString($"Total Debits: ${totalDebits:N2}", normalFont, XBrushes.Red, leftMargin, yPosition);
      yPosition += 30;

      // Table Header
      gfx.DrawString("TRANSACTION HISTORY", headerFont, XBrushes.Black, leftMargin, yPosition);
      yPosition += 25;

      // Draw table headers
      var colWidths = new double[] { 80, 70, 200, 80, 100 }; // Date, Type, Description, Amount, Reference
      var colPositions = new double[5];
      colPositions[0] = leftMargin;
      for (int i = 1; i < colPositions.Length; i++)
      {
        colPositions[i] = colPositions[i - 1] + colWidths[i - 1];
      }

      // Header background
      gfx.DrawRectangle(XBrushes.LightGray, leftMargin, yPosition - 5, rightMargin - leftMargin, 20);

      gfx.DrawString("Date", headerFont, XBrushes.Black, colPositions[0], yPosition);
      gfx.DrawString("Type", headerFont, XBrushes.Black, colPositions[1], yPosition);
      gfx.DrawString("Description", headerFont, XBrushes.Black, colPositions[2], yPosition);
      gfx.DrawString("Amount", headerFont, XBrushes.Black, colPositions[3], yPosition);
      gfx.DrawString("Reference", headerFont, XBrushes.Black, colPositions[4], yPosition);

      yPosition += 25;

      // Draw transaction rows
      var transactionsList = transactions.OrderByDescending(t => t.CreatedAt).ToList();

      foreach (var transaction in transactionsList)
      {
        // Check if we need a new page
        if (yPosition > page.Height.Point - 100)
        {
          page = document.AddPage();
          gfx = XGraphics.FromPdfPage(page);
          yPosition = 50;

          // Redraw header on new page
          gfx.DrawString("BANKO - Bank Statement (Continued)", titleFont, XBrushes.DarkBlue,
              new XRect(leftMargin, yPosition, page.Width.Point - 100, 30), XStringFormats.TopCenter);
          yPosition += 40;
        }

        // Alternate row background
        if ((transactionsList.IndexOf(transaction) % 2) == 0)
        {
          gfx.DrawRectangle(XBrushes.AliceBlue, leftMargin, yPosition - 3, rightMargin - leftMargin, 18);
        }

        // Draw transaction data
        gfx.DrawString(transaction.CreatedAt.ToString("dd/MM/yyyy"), normalFont, XBrushes.Black, colPositions[0], yPosition);
        gfx.DrawString(transaction.Type.ToString(), normalFont, XBrushes.Black, colPositions[1], yPosition);

        // Truncate description if too long
        var description = transaction.Description ?? "";
        if (description.Length > 30)
          description = description.Substring(0, 27) + "...";
        gfx.DrawString(description, normalFont, XBrushes.Black, colPositions[2], yPosition);

        // Amount with color coding
        var amountBrush = transaction.Type == TransactionType.Withdrawal || transaction.Type == TransactionType.Transfer
            ? XBrushes.Red : XBrushes.Green;
        gfx.DrawString($"${transaction.Amount:N2}", normalFont, amountBrush, colPositions[3], yPosition);

        gfx.DrawString(transaction.ReferenceNumber ?? "", smallFont, XBrushes.Black, colPositions[4], yPosition);

        yPosition += 18;
      }

      // Footer
      yPosition = page.Height.Point - 50;
      gfx.DrawString("This is a computer-generated statement and does not require a signature.",
          smallFont, XBrushes.Gray,
          new XRect(leftMargin, yPosition, page.Width.Point - 100, 20), XStringFormats.TopCenter);

      // Convert to byte array
      using var stream = new MemoryStream();
      document.Save(stream);
      document.Close();

      return stream.ToArray();
    }
  }
}