using DashboardApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOcrService, OcrService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("[INFO] Dashboard API starting on http://localhost:5000");
Console.WriteLine("[INFO] Available endpoints:");
Console.WriteLine("  - GET /api/scraper - Web scraping results");
Console.WriteLine("  - GET /api/ocr - OCR processing results");
Console.WriteLine("  - POST /api/ocr/upload - Upload images for OCR processing");

await app.RunAsync();

// Make Program class accessible for integration tests
public partial class Program 
{ 
    protected Program() { }
}
