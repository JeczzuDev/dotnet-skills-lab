using OCRLab.Models;

namespace OCRLab.Services;

public interface IOcrService
{
    Task<OcrResult> ProcessImageAsync(string imagePath);
    Task<List<OcrResult>> ProcessBatchAsync(string folderPath);
}
