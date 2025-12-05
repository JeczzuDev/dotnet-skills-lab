using Tesseract;
using Microsoft.Extensions.Configuration;
using OCRLab.Models;

namespace OCRLab.Services;

public class OcrService(IConfiguration configuration) : IOcrService
{
    private readonly IConfiguration _configuration = configuration;
    
    public async Task<OcrResult> ProcessImageAsync(string imagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                Console.WriteLine($"[INFO] Processing image: {imagePath}");

                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"[ERROR] File not found: {imagePath}");
                    return new OcrResult 
                    { 
                        FileName = Path.GetFileName(imagePath),
                        Success = false 
                    };
                }

                var tessDataPath = _configuration["Ocr:TessDataPath"] 
                    ?? @"C:\Program Files\Tesseract-OCR\tessdata";
                var language = _configuration["Ocr:Language"] ?? "eng";

                using var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                
                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                Console.WriteLine($"[SUCCESS] OCR completed with {confidence:P} confidence");

                return new OcrResult
                {
                    FileName = Path.GetFileName(imagePath),
                    ExtractedText = text,
                    Confidence = confidence,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OCR failed: {ex.Message}");
                return new OcrResult 
                { 
                    FileName = Path.GetFileName(imagePath),
                    Success = false 
                };
            }
        });
    }

    public async Task<List<OcrResult>> ProcessBatchAsync(string folderPath)
    {
        var results = new List<OcrResult>();

        try
        {
            var imageFiles = Directory.GetFiles(folderPath, "*.png")
                .Concat(Directory.GetFiles(folderPath, "*.jpg"))
                .Concat(Directory.GetFiles(folderPath, "*.jpeg"))
                .ToList();

            Console.WriteLine($"[INFO] Found {imageFiles.Count} images in {folderPath}");

            foreach (var imagePath in imageFiles)
            {
                var result = await ProcessImageAsync(imagePath);
                results.Add(result);
            }

            Console.WriteLine($"[SUCCESS] Batch processing completed: {results.Count(r => r.Success)}/{results.Count} succeeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Batch processing failed: {ex.Message}");
        }

        return results;
    }
}
