using DashboardApi.Controllers;
using DashboardApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DashboardApi.Tests;

/// <summary>
/// Tests for ScraperController - demonstrates testing file I/O and CSV parsing
/// Phase 2: Advanced Testing - Mocking & Dependency Injection
/// </summary>
public class ScraperControllerTests
{
    private readonly Mock<ILogger<ScraperController>> _mockLogger;
    private readonly ScraperController _controller;

    /// <summary>
    /// Constructor - Setup common test dependencies
    /// </summary>
    public ScraperControllerTests()
    {
        _mockLogger = new Mock<ILogger<ScraperController>>();
        _controller = new ScraperController(_mockLogger.Object);
    }

    #region GetBooks Tests

    /// <summary>
    /// Test: GetBooks returns empty list when CSV file doesn't exist
    /// This tests the file-not-found scenario
    /// </summary>
    [Fact]
    public void GetBooks_WhenFileNotFound_ReturnsEmptyList()
    {
        // Act
        var result = _controller.GetBooks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var books = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
        Assert.Empty(books);
    }

    /// <summary>
    /// Test: Verify logger is called when fetching books
    /// Demonstrates mock verification for logging
    /// </summary>
    [Fact]
    public void GetBooks_LogsInformation_WhenCalled()
    {
        // Act
        _controller.GetBooks();

        // Assert - Verify logging occurred
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching scraper results")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test: GetBooks returns OK result with proper type
    /// Validates controller returns correct HTTP status
    /// </summary>
    [Fact]
    public void GetBooks_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetBooks();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    /// <summary>
    /// Test: GetBooks returns enumerable collection
    /// Ensures return type is iterable
    /// </summary>
    [Fact]
    public void GetBooks_ReturnsEnumerableOfBooks()
    {
        // Act
        var result = _controller.GetBooks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
    }

    #endregion

    #region Book Model Tests

    /// <summary>
    /// Test: Book record properties are correctly initialized
    /// Theory with InlineData tests different book scenarios
    /// </summary>
    [Theory]
    [InlineData("The Great Gatsby", 15.99, 5, "In stock")]
    [InlineData("1984", 12.50, 4, "Out of stock")]
    [InlineData("To Kill a Mockingbird", 18.75, 5, "In stock (3 available)")]
    public void Book_PropertiesAreSetCorrectly(string title, decimal price, int rating, string availability)
    {
        // Act
        var book = new Book(title, price, rating, availability);

        // Assert
        Assert.Equal(title, book.Title);
        Assert.Equal(price, book.Price);
        Assert.Equal(rating, book.Rating);
        Assert.Equal(availability, book.Availability);
    }

    /// <summary>
    /// Test: Book record equality comparison
    /// Records in C# have value-based equality
    /// </summary>
    [Fact]
    public void Book_RecordsWithSameValues_AreEqual()
    {
        // Arrange
        var book1 = new Book("Test Book", 10.99m, 5, "In stock");
        var book2 = new Book("Test Book", 10.99m, 5, "In stock");

        // Assert
        Assert.Equal(book1, book2);
    }

    /// <summary>
    /// Test: Book record inequality comparison
    /// </summary>
    [Fact]
    public void Book_RecordsWithDifferentValues_AreNotEqual()
    {
        // Arrange
        var book1 = new Book("Book A", 10.99m, 5, "In stock");
        var book2 = new Book("Book B", 15.99m, 4, "Out of stock");

        // Assert
        Assert.NotEqual(book1, book2);
    }

    /// <summary>
    /// Test: Book price validation - should accept valid decimal values
    /// </summary>
    [Theory]
    [InlineData(0.01)]
    [InlineData(9.99)]
    [InlineData(100.00)]
    [InlineData(999.99)]
    public void Book_AcceptsValidPrices(decimal price)
    {
        // Act
        var book = new Book("Test", price, 5, "In stock");

        // Assert
        Assert.Equal(price, book.Price);
    }

    /// <summary>
    /// Test: Book rating validation - should accept ratings 1-5
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Book_AcceptsValidRatings(int rating)
    {
        // Act
        var book = new Book("Test", 10.99m, rating, "In stock");

        // Assert
        Assert.Equal(rating, book.Rating);
    }

    #endregion
}
