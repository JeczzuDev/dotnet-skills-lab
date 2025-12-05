using HtmlAgilityPack;
using System.Text.RegularExpressions;
using WebScraperLab.Models;

namespace WebScraperLab.Services;

public class ScraperService(HttpClient httpClient) : IScraperService
{
  private readonly HttpClient _httpClient = httpClient;
  private const string BaseUrl = "https://books.toscrape.com/catalogue/page-{0}.html";

  public async Task<List<Book>> ScrapePageAsync(int pageNumber = 1)
  {
    var books = new List<Book>();

    try
    {
      var url = string.Format(BaseUrl, pageNumber);
      Console.WriteLine($"[INFO] Scraping page {pageNumber}: {url}");

      var html = await _httpClient.GetStringAsync(url);
      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(html);

      var bookNodes = htmlDoc.DocumentNode.SelectNodes("//article[@class='product_pod']");

      if (bookNodes == null)
      {
        Console.WriteLine("[WARNING] No books found on this page");
        return books;
      }

      foreach (var bookNode in bookNodes)
      {
        var book = ExtractBookData(bookNode);
        if (book != null)
        {
          books.Add(book);
        }
      }

      Console.WriteLine($"[SUCCESS] Extracted {books.Count} books from page {pageNumber}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[ERROR] Failed to scrape page {pageNumber}: {ex.Message}");
    }

    return books;
  }

  private Book? ExtractBookData(HtmlNode bookNode)
  {
    try
    {
      var titleNode = bookNode.SelectSingleNode(".//h3/a");
      var priceNode = bookNode.SelectSingleNode(".//p[@class='price_color']");
      var ratingNode = bookNode.SelectSingleNode(".//p[contains(@class, 'star-rating')]");
      var availabilityNode = bookNode.SelectSingleNode(".//p[@class='instock availability']");

      if (titleNode == null || priceNode == null)
      {
        return null;
      }

      var title = titleNode.GetAttributeValue("title", string.Empty);
      var priceText = priceNode.InnerText.Trim();
      var price = ParsePrice(priceText);
      var rating = ParseRating(ratingNode?.GetAttributeValue("class", string.Empty) ?? string.Empty);
      var availability = availabilityNode?.InnerText.Trim() ?? "Unknown";

      return new Book
      {
        Title = title,
        Price = price,
        Rating = rating,
        Availability = availability
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[WARNING] Failed to extract book data: {ex.Message}");
      return null;
    }
  }

  private decimal ParsePrice(string priceText)
  {
    var match = Regex.Match(priceText, @"[\d.]+");
    if (match.Success && decimal.TryParse(match.Value, out var price))
    {
      return price;
    }
    return 0m;
  }

  private int ParseRating(string ratingClass)
  {
    var ratings = new Dictionary<string, int>
        {
            { "One", 1 },
            { "Two", 2 },
            { "Three", 3 },
            { "Four", 4 },
            { "Five", 5 }
        };

    foreach (var (key, value) in ratings)
    {
      if (ratingClass.Contains(key))
      {
        return value;
      }
    }

    return 0;
  }
}
