namespace OCRLab.Models;

public class OcrResult
{
    public string FileName { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public bool Success { get; set; }
}
