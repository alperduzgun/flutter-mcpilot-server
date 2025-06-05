using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using FlutterMcpServer.Models;
using FlutterMcpServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlutterMcpServer.Controllers;

/// <summary>
/// Model Context Protocol (MCP) ana komut controller'ı
/// Tüm Flutter geliştirici yardımcısı komutları bu API'den işlenir
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Flutter MCP Commands")]
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
  private readonly ProjectAnalyzer _projectAnalyzer;
  private readonly ConfigService _configService;

  public CommandController(ILogger<CommandController> logger,
                         FlutterVersionChecker flutterVersionChecker,
                         CodeReviewService codeReviewService,
                         TestGeneratorService testGeneratorService,
                         NavigationMigrationService navigationMigrationService,
                         ScreenGeneratorService screenGeneratorService,
                         PluginCreatorService pluginCreatorService,
                         FileWriterService fileWriterService,
                         ProjectAnalyzer projectAnalyzer,
                         ConfigService configService)
  {
    _logger = logger;
    _flutterVersionChecker = flutterVersionChecker;
    _codeReviewService = codeReviewService;
    _testGeneratorService = testGeneratorService;
    _navigationMigrationService = navigationMigrationService;
    _screenGeneratorService = screenGeneratorService;
    _pluginCreatorService = pluginCreatorService;
    _fileWriterService = fileWriterService;
    _projectAnalyzer = projectAnalyzer;
    _configService = configService;
  }

  /// <summary>
  /// MCP komutlarını işleyen ana endpoint
  /// </summary>
  /// <param name="command">MCP komut objesi - JSON formatında gönderilmelidir</param>
  /// <returns>MCP yanıt objesi - İşlem sonucu, kod blokları, notlar ve öneriler içerir</returns>
  /// <remarks>
  /// Bu endpoint tüm Flutter geliştirici yardımcısı komutlarını işler:
  /// 
  /// **Kullanılabilir Komutlar:**
  /// - `checkFlutterVersion`: Flutter SDK sürümünü kontrol eder
  /// - `reviewCode`: Dart/Flutter kod analizi ve iyileştirme önerileri
  /// - `generateTestsForCubit`: Cubit/Bloc için test dosyaları üretir
  /// - `migrateNavigationSystem`: Navigator'dan GoRouter'a geçiş
  /// - `generateScreen`: Prompt'tan UI widget'ları oluşturur
  /// - `createFlutterPlugin`: Flutter plugin şablonu üretir
  /// - `analyzeFeatureComplexity`: Proje karmaşıklık analizi
  /// - `loadProjectPreferences`: Proje konfigürasyonu yükler
  /// - `writeFile`: Güvenli dosya yazım işlemleri
  /// 
  /// **Örnek İstek:**
  /// ```json
  /// {
  ///   "commandId": "unique-command-id",
  ///   "command": "checkFlutterVersion",
  ///   "params": {
  ///     "projectPath": "/path/to/flutter/project"
  ///   }
  /// }
  /// ```
  /// 
  /// **Örnek Yanıt:**
  /// ```json
  /// {
  ///   "success": true,
  ///   "purpose": "Flutter sürüm kontrolü tamamlandı",
  ///   "notes": ["Flutter 3.24.0 kullanılıyor"],
  ///   "commandId": "unique-command-id",
  ///   "executionTimeMs": 45
  /// }
  /// ```
  /// </remarks>
  /// <response code="200">Komut başarıyla işlendi</response>
  /// <response code="400">Geçersiz komut veya parametreler</response>
  /// <response code="500">Sunucu hatası</response>
  [HttpPost("execute")]
  [ProducesResponseType<McpResponse>(StatusCodes.Status200OK)]
  [ProducesResponseType<McpResponse>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<McpResponse>(StatusCodes.Status500InternalServerError)]
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
            new { Command = "writeFile", Description = "Güvenli dosya yazma ve oluşturma" },
            new { Command = "analyzeFeatureComplexity", Description = "Proje karmaşıklığı ve mimari analizi" },
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
    return await _projectAnalyzer.AnalyzeFeatureComplexityAsync(command);
  }

  private async Task<McpResponse> HandleLoadPreferences(McpCommand command)
  {
    return await _configService.LoadProjectPreferencesAsync(command);
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
