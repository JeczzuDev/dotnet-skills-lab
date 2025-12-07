namespace DashboardApi.Models;

public record Book(
    string Title,
    decimal Price,
    int Rating,
    string Availability
);
