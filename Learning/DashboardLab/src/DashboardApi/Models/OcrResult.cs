namespace DashboardApi.Models;

public record OcrResult(
    string FileName,
    string ExtractedText,
    double Confidence,
    bool Success
);
