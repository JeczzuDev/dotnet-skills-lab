using WebScraperLab.Models;

namespace WebScraperLab.Services;

public interface IScraperService
{
  Task<List<Book>> ScrapePageAsync(int pageNumber = 1);
}
