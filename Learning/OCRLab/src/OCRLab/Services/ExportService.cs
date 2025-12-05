using System.Text;
using System.Text.Json;
using OCRLab.Models;

namespace OCRLab.Services;

public interface IExportService
{
    Task ExportToCsvAsync(List<OcrResult> results, string filePath);
    Task ExportToJsonAsync(List<OcrResult> results, string filePath);
}

public class ExportService : IExportService
{
    public async Task ExportToCsvAsync(List<OcrResult> results, string filePath)
    {
        try
        {
            Console.WriteLine($"[INFO] Exporting {results.Count} results to CSV...");

            var csv = new StringBuilder();
            csv.AppendLine("FileName,Success,Confidence,ExtractedText");

            foreach (var result in results)
            {
                var cleanText = result.ExtractedText
                    .Replace("\n", " ")
                    .Replace("\r", "")
                    .Replace("\"", "'")
                    .Trim();
                
                csv.AppendLine($"\"{result.FileName}\",{result.Success},{result.Confidence:F2},\"{cleanText}\"");
            }

            await File.WriteAllTextAsync(filePath, csv.ToString());
            Console.WriteLine("[SUCCESS] CSV export completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CSV export failed: {ex.Message}");
        }
    }

    public async Task ExportToJsonAsync(List<OcrResult> results, string filePath)
    {
        try
        {
            Console.WriteLine($"[INFO] Exporting {results.Count} results to JSON...");

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(results, options);

            await File.WriteAllTextAsync(filePath, json);
            Console.WriteLine("[SUCCESS] JSON export completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] JSON export failed: {ex.Message}");
        }
    }
}
