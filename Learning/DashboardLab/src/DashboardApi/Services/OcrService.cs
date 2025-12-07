using Tesseract;
using DashboardApi.Models;

namespace DashboardApi.Services;

public class OcrService(IConfiguration configuration) : IOcrService
{
  private readonly IConfiguration _configuration = configuration;

  public async Task<OcrResult> ProcessImageAsync(Stream imageStream, string fileName)
  {
    try
    {
      var tempPath = Path.Combine(Path.GetTempPath(), fileName);

      await using (var fileStream = File.Create(tempPath))
      {
        await imageStream.CopyToAsync(fileStream);
      }

      var tessDataPath = _configuration["Ocr:TessDataPath"]
          ?? throw new InvalidOperationException("Tesseract data path not configured in appsettings.json");
      var language = _configuration["Ocr:Language"] ?? "eng";

      return await Task.Run(() =>
      {
        using var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
        using var img = Pix.LoadFromFile(tempPath);
        using var page = engine.Process(img);

        var text = page.GetText().Trim();
        var confidence = page.GetMeanConfidence();

        if (System.IO.File.Exists(tempPath))
        {
          System.IO.File.Delete(tempPath);
        }

        return new OcrResult(
                  FileName: fileName,
                  ExtractedText: text,
                  Confidence: confidence,
                  Success: true
              );
      });
    }
    catch (Exception ex)
    {
      return new OcrResult(
          FileName: fileName,
          ExtractedText: $"Error: {ex.Message}",
          Confidence: 0,
          Success: false
      );
    }
  }
}
