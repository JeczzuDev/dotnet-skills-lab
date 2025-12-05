using Microsoft.Extensions.DependencyInjection;
using WebScraperLab.Services;

var serviceProvider = new ServiceCollection()
    .AddHttpClient<IScraperService, ScraperService>()
    .Services
    .AddScoped<ICsvExporter, CsvExporter>()
    .BuildServiceProvider();

Console.WriteLine("[INFO] WebScraperLab Starting...\n");

try
{
    var scraper = serviceProvider.GetRequiredService<IScraperService>();
    var exporter = serviceProvider.GetRequiredService<ICsvExporter>();

    Console.WriteLine("How many pages do you want to scrape? (1-50): ");
    var input = Console.ReadLine();
    var pagesToScrape = int.TryParse(input, out var pages) ? pages : 1;

    var allBooks = new List<WebScraperLab.Models.Book>();

    for (int i = 1; i <= pagesToScrape; i++)
    {
        var books = await scraper.ScrapePageAsync(i);
        allBooks.AddRange(books);
    }

    Console.WriteLine($"\n[SUCCESS] Total books scraped: {allBooks.Count}");

    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "books_output.csv");
    await exporter.ExportToCsvAsync(allBooks, outputPath);

    Console.WriteLine($"[INFO] Output file: {outputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Scraping failed: {ex.Message}");
}

await serviceProvider.DisposeAsync();
