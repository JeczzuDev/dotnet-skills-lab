using DashboardApi.Models;
using DashboardApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DashboardApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController(ILogger<OcrController> logger, IOcrService ocrService) : ControllerBase
{
  private readonly ILogger<OcrController> _logger = logger;
  private readonly IOcrService _ocrService = ocrService;

  [HttpGet]
  public ActionResult<IEnumerable<OcrResult>> GetOcrResults()
  {
    _logger.LogInformation("[INFO] Fetching OCR results");

    try
    {
      // Read JSON from OCRLab project
      var jsonPath = Path.Combine(
          Directory.GetCurrentDirectory(),
          "..", "..", "..", "OCRLab", "src", "OCRLab", "ocr_results.json"
      );

      if (!System.IO.File.Exists(jsonPath))
      {
        _logger.LogWarning("[WARNING] JSON file not found at {Path}", jsonPath);
        return Ok(new List<OcrResult>());
      }

      var jsonContent = System.IO.File.ReadAllText(jsonPath);
      var results = JsonSerializer.Deserialize<List<OcrResult>>(jsonContent);

      _logger.LogInformation("[SUCCESS] Loaded {Count} OCR results from JSON", results?.Count ?? 0);
      return Ok(results ?? new List<OcrResult>());
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[ERROR] Failed to load OCR results");
      return Ok(new List<OcrResult>());
    }
  }

  [HttpPost("upload")]
  public async Task<ActionResult<OcrResult>> UploadImage(IFormFile file)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest(new { error = "No file uploaded" });
    }

    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

    if (!allowedExtensions.Contains(extension))
    {
      return BadRequest(new { error = "Invalid file type. Only image files are allowed." });
    }

    try
    {
      _logger.LogInformation("[INFO] Processing uploaded image: {FileName}", file.FileName);

      using var stream = file.OpenReadStream();
      var result = await _ocrService.ProcessImageAsync(stream, file.FileName);

      _logger.LogInformation("[SUCCESS] OCR completed for {FileName}", file.FileName);
      return Ok(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[ERROR] Failed to process image");
      return StatusCode(500, new { error = $"Failed to process image: {ex.Message}" });
    }
  }
}
