using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OCRLab.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddScoped<IOcrService, OcrService>()
    .AddScoped<IExportService, ExportService>()
    .BuildServiceProvider();

Console.WriteLine("[INFO] OCRLab Starting...\n");
Console.WriteLine("[WARNING] This project requires Tesseract OCR to be installed on your system.");
Console.WriteLine("[INFO] Install with: choco install tesseract\n");

try
{
    var ocrService = serviceProvider.GetRequiredService<IOcrService>();
    var exportService = serviceProvider.GetRequiredService<IExportService>();

    Console.WriteLine("Enter path to image or folder: ");
    var input = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("[ERROR] No path provided");
        return;
    }

    var results = new List<OCRLab.Models.OcrResult>();

    if (Directory.Exists(input))
    {
        results = await ocrService.ProcessBatchAsync(input);
    }
    else if (File.Exists(input))
    {
        var result = await ocrService.ProcessImageAsync(input);
        results.Add(result);
    }
    else
    {
        Console.WriteLine($"[ERROR] Path not found: {input}");
        return;
    }

    if (results.Count == 0)
    {
        Console.WriteLine("[WARNING] No results to export");
        return;
    }

    var outputCsv = Path.Combine(Directory.GetCurrentDirectory(), "ocr_results.csv");
    var outputJson = Path.Combine(Directory.GetCurrentDirectory(), "ocr_results.json");

    await exportService.ExportToCsvAsync(results, outputCsv);
    await exportService.ExportToJsonAsync(results, outputJson);

    Console.WriteLine($"\n[INFO] CSV output: {outputCsv}");
    Console.WriteLine($"[INFO] JSON output: {outputJson}");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] OCR processing failed: {ex.Message}");
    Console.WriteLine($"[ERROR] Make sure Tesseract is installed and tessdata files are available");
}

await serviceProvider.DisposeAsync();
