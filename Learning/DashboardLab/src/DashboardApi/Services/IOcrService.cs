using DashboardApi.Models;

namespace DashboardApi.Services;

public interface IOcrService
{
  Task<OcrResult> ProcessImageAsync(Stream imageStream, string fileName);
}
