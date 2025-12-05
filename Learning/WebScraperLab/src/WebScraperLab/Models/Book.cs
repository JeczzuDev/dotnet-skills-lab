namespace WebScraperLab.Models;

public class Book
{
  public string Title { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public int Rating { get; set; }
  public string Availability { get; set; } = string.Empty;
}
