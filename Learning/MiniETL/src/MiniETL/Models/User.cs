using System.Text.Json.Serialization;

namespace MiniETL.Models;

public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public Address? Address { get; set; }

    [JsonPropertyName("company")]
    public Company? Company { get; set; }
}

public class Address
{
    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;
}

public class Company
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
