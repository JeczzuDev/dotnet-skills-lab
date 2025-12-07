using DashboardApi.Controllers;
using DashboardApi.Models;
using DashboardApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DashboardApi.Tests;

/// <summary>
/// Tests for OcrController - demonstrates controller testing with dependency injection
/// </summary>
public class OcrControllerTests
{
    private readonly Mock<ILogger<OcrController>> _mockLogger;
    private readonly Mock<IOcrService> _mockOcrService;
    private readonly OcrController _controller;

    /// <summary>
    /// Constructor - Setup common test dependencies
    /// This runs before each test method
    /// </summary>
    public OcrControllerTests()
    {
        _mockLogger = new Mock<ILogger<OcrController>>();
        _mockOcrService = new Mock<IOcrService>();
        _controller = new OcrController(_mockLogger.Object, _mockOcrService.Object);
    }

    #region UploadImage Tests

    /// <summary>
    /// Test: UploadImage returns BadRequest when file is null
    /// </summary>
    [Fact]
    public async Task UploadImage_WhenFileIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UploadImage(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    /// <summary>
    /// Test: UploadImage returns BadRequest when file is empty
    /// </summary>
    [Fact]
    public async Task UploadImage_WhenFileIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);
        mockFile.Setup(f => f.FileName).Returns("test.png");

        // Act
        var result = await _controller.UploadImage(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    /// <summary>
    /// Test: UploadImage returns BadRequest for invalid file extensions
    /// Theory with InlineData tests multiple invalid scenarios
    /// </summary>
    [Theory]
    [InlineData("document.pdf")]
    [InlineData("script.js")]
    [InlineData("data.txt")]
    [InlineData("executable.exe")]
    public async Task UploadImage_WhenFileExtensionInvalid_ReturnsBadRequest(string fileName)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);

        // Act
        var result = await _controller.UploadImage(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    /// <summary>
    /// Test: UploadImage processes valid image files successfully
    /// </summary>
    [Theory]
    [InlineData("image.png")]
    [InlineData("photo.jpg")]
    [InlineData("picture.jpeg")]
    [InlineData("scan.bmp")]
    [InlineData("document.tiff")]
    public async Task UploadImage_WhenFileIsValid_ReturnsOkWithResult(string fileName)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([0x89, 0x50, 0x4E, 0x47]));

        var expectedResult = new OcrResult(
            FileName: fileName,
            ExtractedText: "Sample text",
            Confidence: 0.95f,
            Success: true
        );

        _mockOcrService.Setup(s => s.ProcessImageAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.UploadImage(mockFile.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var ocrResult = Assert.IsType<OcrResult>(okResult.Value);
        Assert.Equal(fileName, ocrResult.FileName);
        Assert.Equal("Sample text", ocrResult.ExtractedText);
        Assert.True(ocrResult.Success);
        Assert.Equal(0.95f, ocrResult.Confidence);
    }

    /// <summary>
    /// Test: Verify that OcrService is called when processing valid file
    /// </summary>
    [Fact]
    public async Task UploadImage_CallsOcrService_WhenFileIsValid()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("test.png");
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([0x89, 0x50, 0x4E, 0x47]));

        var expectedResult = new OcrResult("test.png", "text", 0.95f, true);
        _mockOcrService.Setup(s => s.ProcessImageAsync(It.IsAny<Stream>(), "test.png"))
            .ReturnsAsync(expectedResult);

        // Act
        await _controller.UploadImage(mockFile.Object);

        // Assert - Verify the service was called exactly once
        _mockOcrService.Verify(
            s => s.ProcessImageAsync(It.IsAny<Stream>(), "test.png"),
            Times.Once
        );
    }

    #endregion

    #region GetOcrResults Tests

    /// <summary>
    /// Test: GetOcrResults returns empty list when file doesn't exist
    /// Note: This test depends on file system state
    /// </summary>
    [Fact]
    public void GetOcrResults_WhenFileNotFound_ReturnsEmptyList()
    {
        // Act
        var result = _controller.GetOcrResults();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var results = Assert.IsAssignableFrom<IEnumerable<OcrResult>>(okResult.Value);
        Assert.Empty(results);
    }

    #endregion
}
