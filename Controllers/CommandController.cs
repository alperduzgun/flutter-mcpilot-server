using FlutterMcpServer.Models;
using FlutterMcpServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace FlutterMcpServer.Controllers;

/// <summary>
/// Model Context Protocol (MCP) ana komut controller'ı
/// Tüm Flutter geliştirici yardımcısı komutları bu API'den işlenir
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommandController : ControllerBase
{
  private readonly ILogger<CommandController> _logger;
  private readonly FlutterVersionChecker _flutterVersionChecker;
  private readonly CodeReviewService _codeReviewService;
  private readonly TestGeneratorService _testGeneratorService;
  private readonly NavigationMigrationService _navigationMigrationService;
  private readonly ScreenGeneratorService _screenGeneratorService;
  private readonly PluginCreatorService _pluginCreatorService;
  private readonly FileWriterService _fileWriterService;

  public CommandController(ILogger<CommandController> logger,
                         FlutterVersionChecker flutterVersionChecker,
                         CodeReviewService codeReviewService,
                         TestGeneratorService testGeneratorService,
                         NavigationMigrationService navigationMigrationService,
                         ScreenGeneratorService screenGeneratorService,
                         PluginCreatorService pluginCreatorService,
                         FileWriterService fileWriterService)
  {
    _logger = logger;
    _flutterVersionChecker = flutterVersionChecker;
    _codeReviewService = codeReviewService;
    _testGeneratorService = testGeneratorService;
    _navigationMigrationService = navigationMigrationService;
    _screenGeneratorService = screenGeneratorService;
    _pluginCreatorService = pluginCreatorService;
    _fileWriterService = fileWriterService;
  }

  /// <summary>
  /// MCP komutlarını işleyen ana endpoint
  /// </summary>
  /// <param name="command">MCP komut objesi</param>
  /// <returns>MCP yanıt objesi</returns>
  [HttpPost("execute")]
  public async Task<ActionResult<McpResponse>> ExecuteCommand([FromBody] McpCommand command)
  {
    var stopwatch = Stopwatch.StartNew();
    var response = new McpResponse
    {
      CommandId = command.CommandId
    };

    try
    {
      _logger.LogInformation("MCP Command başlatıldı: {Command} - {CommandId}",
          command.Command, command.CommandId);

      // Komut doğrulama
      if (string.IsNullOrWhiteSpace(command.Command))
      {
        response.Success = false;
        response.Errors.Add("Komut adı boş olamaz.");
        return BadRequest(response);
      }

      // Komut yönlendirme
      response = command.Command.ToLowerInvariant() switch
      {
        "checkflutterversion" => await HandleCheckFlutterVersion(command),
        "reviewcode" => await HandleReviewCode(command),
        "generatetestsforcubit" => await HandleGenerateTests(command),
        "migratenavigationsystem" => await HandleMigrateNavigation(command),
        "generatescreen" => await HandleGenerateScreen(command),
        "createflutterplugin" => await HandleCreatePlugin(command),
        "writefile" => await HandleWriteFile(command),
        "analyzefeaturecomplexity" => await HandleAnalyzeComplexity(command),
        "loadprojectpreferences" => await HandleLoadPreferences(command),
        _ => HandleUnsupportedCommand(command)
      };

      response.Success = true;
      response.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

      _logger.LogInformation("MCP Command tamamlandı: {Command} - {ExecutionTime}ms",
          command.Command, response.ExecutionTimeMs);

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "MCP Command hatası: {Command} - {CommandId}",
          command.Command, command.CommandId);

      response.Success = false;
      response.Errors.Add($"İç hata: {ex.Message}");
      response.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

      return StatusCode(500, response);
    }
  }

  /// <summary>
  /// Sistem durumu kontrolü
  /// </summary>
  [HttpGet("health")]
  public IActionResult Health()
  {
    return Ok(new
    {
      Status = "Healthy",
      Service = "Flutter MCP Server",
      Version = "1.0.0",
      Timestamp = DateTime.UtcNow
    });
  }

  /// <summary>
  /// Desteklenen komutların listesi
  /// </summary>
  [HttpGet("commands")]
  public IActionResult GetSupportedCommands()
  {
    var commands = new[]
    {
            new { Command = "checkFlutterVersion", Description = "Flutter SDK sürüm kontrolü" },
            new { Command = "reviewCode", Description = "Kod incelemesi ve refactor önerileri" },
            new { Command = "generateTestsForCubit", Description = "Test üretimi ve kapsam genişletici" },
            new { Command = "migrateNavigationSystem", Description = "Navigator → GoRouter dönüşümü" },
            new { Command = "generateScreen", Description = "Prompt-to-Widget UI üretimi" },
            new { Command = "createFlutterPlugin", Description = "Plugin/Feature şablonu üretimi" },
            new { Command = "analyzeFeatureComplexity", Description = "Modül karmaşıklığı analizi" },
            new { Command = "loadProjectPreferences", Description = "Proje ayarlarını yükleme" }
        };

    return Ok(commands);
  }

  #region Command Handlers (Placeholder implementations)

  private async Task<McpResponse> HandleCheckFlutterVersion(McpCommand command)
  {
    return await _flutterVersionChecker.CheckFlutterVersionAsync(command);
  }

  private async Task<McpResponse> HandleReviewCode(McpCommand command)
  {
    return await _codeReviewService.ReviewCodeAsync(command);
  }

  private async Task<McpResponse> HandleGenerateTests(McpCommand command)
  {
    return await _testGeneratorService.GenerateTestsForCubitAsync(command);
  }

  private async Task<McpResponse> HandleMigrateNavigation(McpCommand command)
  {
    return await _navigationMigrationService.MigrateNavigationSystemAsync(command);
  }

  private async Task<McpResponse> HandleGenerateScreen(McpCommand command)
  {
    return await _screenGeneratorService.GenerateScreenAsync(command);
  }

  private async Task<McpResponse> HandleCreatePlugin(McpCommand command)
  {
    return await _pluginCreatorService.CreateFlutterPluginAsync(command);
  }

  private async Task<McpResponse> HandleWriteFile(McpCommand command)
  {
    return await _fileWriterService.WriteFileAsync(command);
  }

  private async Task<McpResponse> HandleAnalyzeComplexity(McpCommand command)
  {
    // TODO: ProjectAnalyzer service'i ile implement edilecek
    return await Task.FromResult(new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Proje karmaşıklığı analiz edildi",
      Notes = { "⚠️ Bu handler henüz implement edilmedi" },
      LearnNotes = { "🧠 Karmaşıklık analizi, refactor zamanını belirler" }
    });
  }

  private async Task<McpResponse> HandleLoadPreferences(McpCommand command)
  {
    // TODO: Config okuma servisi ile implement edilecek
    return await Task.FromResult(new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Proje ayarları yüklendi",
      Notes = { "⚠️ Bu handler henüz implement edilmedi" },
      LearnNotes = { "🧠 Proje konfigürasyonu, ekip standartlarını korur" }
    });
  }

  private McpResponse HandleUnsupportedCommand(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen komut",
      Errors = { $"'{command.Command}' komutu henüz desteklenmiyor." },
      Notes = { "Desteklenen komutlar için /api/command/commands endpoint'ini kullanın." }
    };
  }

  #endregion
}
