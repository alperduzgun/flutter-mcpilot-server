using FlutterMcpServer.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlutterMcpServer.Services;

/// <summary>
/// Navigator → GoRouter dönüşüm servisi
/// </summary>
public class NavigationMigrationService
{
  private readonly ILogger<NavigationMigrationService> _logger;

  public NavigationMigrationService(ILogger<NavigationMigrationService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Eski Navigator kullanımını GoRouter'a dönüştürür
  /// </summary>
  public async Task<McpResponse> MigrateNavigationSystemAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Navigator → GoRouter migrasyonu yapıldı"
    };

    try
    {
      _logger.LogInformation("Navigasyon migrasyonu başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? sourceCode = null;
      string? projectPath = null;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;

        if (paramsElement.TryGetProperty("sourceCode", out var codeElement))
        {
          sourceCode = codeElement.GetString();
        }

        if (paramsElement.TryGetProperty("projectPath", out var pathElement))
        {
          projectPath = pathElement.GetString();
        }
      }

      if (string.IsNullOrWhiteSpace(sourceCode))
      {
        response.Success = false;
        response.Errors.Add("Kaynak kod (sourceCode) parametresi gerekli");
        return response;
      }

      // Migrasyon analizi yap
      var migrationResult = await AnalyzeAndMigrateNavigation(sourceCode, projectPath);
      response.Notes.AddRange(migrationResult.Messages);

      if (!string.IsNullOrEmpty(migrationResult.MigratedCode))
      {
        response.LearnNotes.Add("🎯 Navigasyon migrasyonu tamamlandı");
        response.LearnNotes.Add("📦 GoRouter dependency'si eklendi");
        response.LearnNotes.Add("🚀 Type-safe routing aktif");
        response.Notes.Add("Migrated Code:");
        response.Notes.Add(migrationResult.MigratedCode);
      }

      response.Success = true;
      _logger.LogInformation("Navigasyon migrasyonu tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Navigasyon migrasyonu hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Migrasyon hatası: {ex.Message}");
      return response;
    }
  }

  private async Task<NavigationMigrationResult> AnalyzeAndMigrateNavigation(string sourceCode, string? projectPath)
  {
    var result = new NavigationMigrationResult();

    await Task.Run(() =>
    {
      result.Messages.Add("🔍 Navigator kullanım analizi başlatıldı");

      // Navigator.push pattern'larını bul
      var pushMatches = Regex.Matches(sourceCode, @"Navigator\.push\s*\(\s*context\s*,\s*MaterialPageRoute\s*\(\s*builder:\s*\([^)]*\)\s*=>\s*([^)]+)\(\)", RegexOptions.IgnoreCase);
      result.Messages.Add($"📱 {pushMatches.Count} Navigator.push kullanımı bulundu");

      // Navigator.pushNamed pattern'larını bul
      var pushNamedMatches = Regex.Matches(sourceCode, @"Navigator\.pushNamed\s*\(\s*context\s*,\s*['""]([^'""]+)['""]", RegexOptions.IgnoreCase);
      result.Messages.Add($"🏷️ {pushNamedMatches.Count} Navigator.pushNamed kullanımı bulundu");

      if (pushMatches.Count == 0 && pushNamedMatches.Count == 0)
      {
        result.Messages.Add("ℹ️ Migrasyon gerektiren Navigator kullanımı bulunamadı");
        return;
      }

      // GoRouter yapılandırması üret
      result.MigratedCode = GenerateGoRouterConfiguration(sourceCode, pushMatches, pushNamedMatches);
      result.Dependencies = GenerateRequiredDependencies();

      result.Messages.Add("✅ GoRouter konfigürasyonu üretildi");
      result.Messages.Add("📚 Gerekli dependency'ler listelendi");
    });

    return result;
  }

  private string GenerateGoRouterConfiguration(string sourceCode, MatchCollection pushMatches, MatchCollection pushNamedMatches)
  {
    var routes = new List<string>();
    var routeNames = new HashSet<string>();

    // Direct push'ları route'lara dönüştür
    foreach (Match match in pushMatches)
    {
      var widgetName = match.Groups[1].Value.Trim();
      var routeName = ConvertToRouteName(widgetName);

      if (routeNames.Add(routeName))
      {
        routes.Add($@"    GoRoute(
      path: '/{routeName.ToLower()}',
      name: '{routeName}',
      builder: (context, state) => {widgetName}(),
    ),");
      }
    }

    // Named route'ları dönüştür
    foreach (Match match in pushNamedMatches)
    {
      var routePath = match.Groups[1].Value;
      var routeName = ConvertToRouteName(routePath);

      if (routeNames.Add(routeName))
      {
        routes.Add($@"    GoRoute(
      path: '{routePath}',
      name: '{routeName}',
      builder: (context, state) => {ConvertPathToWidget(routePath)}(),
    ),");
      }
    }

    var goRouterConfig = $@"// GoRouter Configuration
// Add this to your main.dart or routing configuration

import 'package:go_router/go_router.dart';

final GoRouter _router = GoRouter(
  routes: <RouteBase>[
    GoRoute(
      path: '/',
      name: 'Home',
      builder: (context, state) => HomePage(),
    ),
{string.Join("\n", routes)}
  ],
);

// Usage in MaterialApp:
// MaterialApp.router(
//   routerConfig: _router,
//   title: 'Your App',
// )

// Navigation examples:
// Instead of: Navigator.push(context, MaterialPageRoute(builder: (context) => SomeScreen()))
// Use: context.go('/someroute')
// Or: context.pushNamed('RouteName')

// Instead of: Navigator.pushNamed(context, '/some-route')
// Use: context.go('/some-route')
// Or: context.pushNamed('SomeRoute')";

    return goRouterConfig;
  }

  private List<string> GenerateRequiredDependencies()
  {
    return new List<string>
    {
      "go_router: ^14.2.7  # Add to pubspec.yaml dependencies"
    };
  }

  private string ConvertToRouteName(string input)
  {
    // Widget adından route name'e dönüştür
    var routeName = input.Replace("Screen", "").Replace("Page", "").Replace("View", "");
    return char.ToUpper(routeName[0]) + routeName.Substring(1);
  }

  private string ConvertPathToWidget(string path)
  {
    // Path'den widget adı üret
    var segments = path.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray();
    if (segments.Length == 0) return "HomePage";

    var widgetName = string.Join("", segments.Select(s => char.ToUpper(s[0]) + s.Substring(1)));
    return widgetName + "Page";
  }

  private class NavigationMigrationResult
  {
    public List<string> Messages { get; set; } = new();
    public string MigratedCode { get; set; } = "";
    public List<string> Dependencies { get; set; } = new();
  }
}
