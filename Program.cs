using System.Reflection;
using FlutterMcpServer.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Enhanced Swagger/OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Flutter MCP Server API",
    Version = "v1.0.0",
    Description = "AI-Powered .NET MCP Server for Flutter Development Assistant. " +
                    "Provides intelligent code generation, analysis, testing, and project management capabilities for Flutter developers.",
    Contact = new OpenApiContact
    {
      Name = "Flutter MCP Server",
      Email = "support@fluttermcp.dev",
      Url = new Uri("https://github.com/flutter-mcp/server")
    },
    License = new OpenApiLicense
    {
      Name = "MIT License",
      Url = new Uri("https://opensource.org/licenses/MIT")
    }
  });

  // XML Documentation support
  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  if (File.Exists(xmlPath))
  {
    options.IncludeXmlComments(xmlPath);
  }

  // Security definition for future API key support
  options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Name = "X-API-Key",
    Description = "API Key for authentication (future implementation)"
  });

  // Group endpoints by tags
  options.TagActionsBy(api => new[] { "Flutter MCP Commands" });
  options.DocInclusionPredicate((name, api) => true);
});

// MCP Services
builder.Services.AddScoped<FlutterVersionChecker>();
builder.Services.AddScoped<CodeReviewService>();
builder.Services.AddScoped<TestGeneratorService>();
builder.Services.AddScoped<NavigationMigrationService>();
builder.Services.AddScoped<ScreenGeneratorService>();
builder.Services.AddScoped<PluginCreatorService>();
builder.Services.AddScoped<FileWriterService>();
builder.Services.AddScoped<ProjectAnalyzer>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<FlutterDocService>();

// HTTP Client for external API calls
builder.Services.AddHttpClient();

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
  // Enable Swagger UI
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flutter MCP Server API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Flutter MCP Server - API Documentation";
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
    options.ShowExtensions();
  });

  // Alternative OpenAPI endpoint
  app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("FlutterPolicy");

// Controller routing
app.MapControllers();

// Enhanced Health check endpoint with API info
app.MapGet("/", () => new
{
  Service = "Flutter MCP Server",
  Status = "Running",
  Version = "v1.0.0",
  Description = "AI-Powered .NET MCP Server for Flutter Development",
  Documentation = "/swagger",
  ApiEndpoint = "/api/command/execute",
  AvailableCommands = new[]
    {
        "checkFlutterVersion",
        "reviewCode",
        "generateTestsForCubit",
        "migrateNavigationSystem",
        "generateScreen",
        "createFlutterPlugin",
        "analyzeFeatureComplexity",
        "loadProjectPreferences",
        "writeFile"
    },
  Timestamp = DateTime.UtcNow,
  Environment = app.Environment.EnvironmentName
})
.WithName("HealthCheck")
.WithTags("Health")
.WithSummary("API Health Check and Information")
.WithDescription("Returns server status, available commands, and API documentation links.");

app.Run();
