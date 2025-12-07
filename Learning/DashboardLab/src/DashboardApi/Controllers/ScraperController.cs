using DashboardApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScraperController : ControllerBase
{
  private readonly ILogger<ScraperController> _logger;

  public ScraperController(ILogger<ScraperController> logger)
  {
    _logger = logger;
  }

  [HttpGet]
  public ActionResult<IEnumerable<Book>> GetBooks()
  {
    _logger.LogInformation("[INFO] Fetching scraper results");

    try
    {
      // Read CSV from WebScraperLab project
      var csvPath = Path.Combine(
          Directory.GetCurrentDirectory(),
          "..", "..", "..", "WebScraperLab", "src", "WebScraperLab", "books_output.csv"
      );

      if (!System.IO.File.Exists(csvPath))
      {
        _logger.LogWarning("[WARNING] CSV file not found at {Path}", csvPath);
        return Ok(new List<Book>());
      }

      var books = new List<Book>();
      var lines = System.IO.File.ReadAllLines(csvPath).Skip(1); // Skip header

      foreach (var line in lines)
      {
        var parts = line.Split(',');
        if (parts.Length >= 4)
        {
          books.Add(new Book(
              Title: parts[0].Trim('"'),
              Price: decimal.Parse(parts[1].Trim('"').Replace("$", "")),
              Rating: int.Parse(parts[2]),
              Availability: parts[3].Trim('"')
          ));
        }
      }

      _logger.LogInformation("[SUCCESS] Loaded {Count} books from CSV", books.Count);
      return Ok(books);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[ERROR] Failed to load books");
      return Ok(new List<Book>());
    }
  }
}
