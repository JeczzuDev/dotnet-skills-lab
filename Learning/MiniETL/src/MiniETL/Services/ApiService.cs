using System.Text.Json;
using MiniETL.Models;

namespace MiniETL.Services;

public interface IApiService
{
    Task<List<User>> GetUsersAsync();
}

public class ApiService(HttpClient httpClient) : IApiService
{
    private readonly HttpClient _httpClient = httpClient;
    private const string Url = "https://jsonplaceholder.typicode.com/users";

    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            Console.WriteLine($"[INFO] Fetching data from {Url}...");
            
            var response = await _httpClient.GetAsync(Url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json);
            
            return users ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error fetching data: {ex.Message}");
            return new List<User>();
        }
    }
}
