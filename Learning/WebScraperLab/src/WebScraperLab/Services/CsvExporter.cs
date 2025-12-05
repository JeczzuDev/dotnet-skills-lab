using System.Text;
using WebScraperLab.Models;

namespace WebScraperLab.Services;

public interface ICsvExporter
{
  Task ExportToCsvAsync(List<Book> books, string filePath);
}

public class CsvExporter : ICsvExporter
{
  public async Task ExportToCsvAsync(List<Book> books, string filePath)
  {
    try
    {
      Console.WriteLine($"[INFO] Exporting {books.Count} books to {filePath}...");

      var csv = new StringBuilder();
      csv.AppendLine("Title,Price,Rating,Availability");

      foreach (var book in books)
      {
        var cleanTitle = book.Title.Replace(",", " ").Replace("\"", "'");
        csv.AppendLine($"\"{cleanTitle}\",{book.Price},{book.Rating},\"{book.Availability}\"");
      }

      await File.WriteAllTextAsync(filePath, csv.ToString());
      Console.WriteLine("[SUCCESS] Export completed successfully!");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[ERROR] Export failed: {ex.Message}");
    }
  }
}
