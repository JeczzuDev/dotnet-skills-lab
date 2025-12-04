using Microsoft.Extensions.DependencyInjection;
using MiniETL.Services;

var serviceProvider = new ServiceCollection()
    .AddHttpClient<IApiService, ApiService>()
    .Services
    .AddScoped<IFileService, FileService>()
    .BuildServiceProvider();

Console.WriteLine("[INFO] Mini-ETL Pipeline Starting...\n");

try
{
    var apiService = serviceProvider.GetRequiredService<IApiService>();
    var users = await apiService.GetUsersAsync();
    
    Console.WriteLine($"[SUCCESS] Extracted {users.Count} users from API\n");
    
    var fileService = serviceProvider.GetRequiredService<IFileService>();
    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "users_output.csv");
    await fileService.SaveToCsvAsync(users, outputPath);

    Console.WriteLine($"\n[SUCCESS] ETL Pipeline completed successfully!");
    Console.WriteLine($"[INFO] Output file: {outputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n[ERROR] Pipeline failed: {ex.Message}");
    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
}

await serviceProvider.DisposeAsync();
