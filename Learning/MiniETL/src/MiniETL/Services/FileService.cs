using System.Text;
using MiniETL.Models;

namespace MiniETL.Services;

public interface IFileService
{
    Task SaveToCsvAsync(List<User> users, string filePath);
}

public class FileService : IFileService
{
    public async Task SaveToCsvAsync(List<User> users, string filePath)
    {
        try
        {
            Console.WriteLine($"[INFO] Saving {users.Count} users to {filePath}...");

            var csvContent = new StringBuilder();
            csvContent.AppendLine("Id,Name,Email,City,Company");

            foreach (var user in users)
            {
                var city = user.Address?.City ?? "Unknown";
                var company = user.Company?.Name ?? "Freelance";
                
                // Escape commas to avoid breaking CSV format
                var cleanName = user.Name.Replace(",", " ");
                var cleanCompany = company.Replace(",", " ");

                csvContent.AppendLine($"{user.Id},{cleanName},{user.Email},{city},{cleanCompany}");
            }

            await File.WriteAllTextAsync(filePath, csvContent.ToString());
            Console.WriteLine("[SUCCESS] Data saved successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error saving file: {ex.Message}");
        }
    }
}
