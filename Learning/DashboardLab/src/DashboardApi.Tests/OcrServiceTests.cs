using DashboardApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DashboardApi.Tests;

/// <summary>
/// Tests for OcrService - demonstrates unit testing with mocking
/// </summary>
public class OcrServiceTests
{
    /// <summary>
    /// Test: ProcessImageAsync returns error result when configuration is missing
    /// Pattern: Arrange-Act-Assert
    /// </summary>
    [Fact]
    public async Task ProcessImageAsync_WhenTessDataPathNotConfigured_ReturnsErrorResult()
    {
        // Arrange - Preparar los objetos y dependencias necesarias
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["Ocr:TessDataPath"]).Returns((string?)null);
        mockConfiguration.Setup(c => c["Ocr:Language"]).Returns("eng");

        var ocrService = new OcrService(mockConfiguration.Object);
        
        using var imageStream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]); // PNG header bytes
        var fileName = "test.png";

        // Act - Ejecutar el método que queremos probar
        var result = await ocrService.ProcessImageAsync(imageStream, fileName);

        // Assert - Verificar que el resultado es el esperado
        Assert.False(result.Success);
        Assert.Equal(fileName, result.FileName);
        Assert.Contains("Tesseract data path not configured", result.ExtractedText);
        Assert.Equal(0, result.Confidence);
    }

    /// <summary>
    /// Test: ProcessImageAsync returns error when image stream is null
    /// Demonstrates error handling testing
    /// </summary>
    [Fact]
    public async Task ProcessImageAsync_WhenImageStreamIsNull_ReturnsErrorResult()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["Ocr:TessDataPath"]).Returns("tessdata");
        mockConfiguration.Setup(c => c["Ocr:Language"]).Returns("eng");

        var ocrService = new OcrService(mockConfiguration.Object);

        // Act
        var result = await ocrService.ProcessImageAsync(null!, "test.png");

        // Assert - Verify error result is returned
        Assert.False(result.Success);
        Assert.Contains("Error:", result.ExtractedText);
        Assert.Equal(0, result.Confidence);
    }

    /// <summary>
    /// Test: Verify configuration is accessed correctly
    /// Theory with InlineData allows testing multiple scenarios
    /// </summary>
    [Theory]
    [InlineData("eng", "tessdata")]
    [InlineData("spa", "C:\\tessdata")]
    [InlineData("fra", "/usr/share/tessdata")]
    public async Task ProcessImageAsync_ReadsConfigurationCorrectly(string language, string tessDataPath)
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["Ocr:TessDataPath"]).Returns(tessDataPath);
        mockConfiguration.Setup(c => c["Ocr:Language"]).Returns(language);

        var ocrService = new OcrService(mockConfiguration.Object);
        
        using var imageStream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

        // Act
        var result = await ocrService.ProcessImageAsync(imageStream, "test.png");

        // Assert - Verify configuration was accessed
        mockConfiguration.Verify(c => c["Ocr:TessDataPath"], Times.Once);
        mockConfiguration.Verify(c => c["Ocr:Language"], Times.Once);
    }
}
