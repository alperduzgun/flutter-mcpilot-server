using FlutterMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// MCP Services
builder.Services.AddScoped<FlutterVersionChecker>();
builder.Services.AddScoped<CodeReviewService>();
builder.Services.AddScoped<TestGeneratorService>();
builder.Services.AddScoped<NavigationMigrationService>();
builder.Services.AddScoped<ScreenGeneratorService>();
builder.Services.AddScoped<PluginCreatorService>();
builder.Services.AddScoped<FileWriterService>();

// CORS konfigürasyonu (Flutter uygulamaları için)
builder.Services.AddCors(options =>
{
  options.AddPolicy("FlutterPolicy", policy =>
  {
    policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
  });
});

// Logging konfigürasyonu
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("FlutterPolicy");

// Controller routing
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => new
{
  Service = "Flutter MCP Server",
  Status = "Running",
  Version = "1.0.0",
  Documentation = "/swagger",
  Timestamp = DateTime.UtcNow
});

app.Run();
