using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using FlutterMcpServer.Handlers;
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
  private readonly CommandHandlerManager _commandHandlerManager;
  private readonly McpProtocolService _mcpProtocolService;
  private readonly McpCapabilitiesService _mcpCapabilitiesService;
  private readonly McpCommandRegistry _mcpCommandRegistry;

  public CommandController(ILogger<CommandController> logger,
                         CommandHandlerManager commandHandlerManager,
                         McpProtocolService mcpProtocolService,
                         McpCapabilitiesService mcpCapabilitiesService,
                         McpCommandRegistry mcpCommandRegistry)
  {
    _logger = logger;
    _commandHandlerManager = commandHandlerManager;
    _mcpProtocolService = mcpProtocolService;
    _mcpCapabilitiesService = mcpCapabilitiesService;
    _mcpCommandRegistry = mcpCommandRegistry;
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

    try
    {
      _logger.LogInformation("MCP Command başlatıldı: {Command} - {CommandId}",
          command.Command, command.CommandId);

      // Komut doğrulama
      if (string.IsNullOrWhiteSpace(command.Command))
      {
        var errorResponse = new McpResponse
        {
          CommandId = command.CommandId,
          Success = false,
          Purpose = "Geçersiz komut",
          Errors = { "Komut adı boş olamaz." }
        };
        return BadRequest(errorResponse);
      }

      // CommandHandlerManager ile komut işleme (Step 22 - Modular Architecture)
      var response = await _commandHandlerManager.ExecuteCommandAsync(command);
      response.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

      _logger.LogInformation("MCP Command tamamlandı: {Command} - {ExecutionTime}ms",
          command.Command, response.ExecutionTimeMs);

      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "MCP Command hatası: {Command} - {CommandId}",
          command.Command, command.CommandId);

      var errorResponse = new McpResponse
      {
        CommandId = command.CommandId,
        Success = false,
        Purpose = "İç sunucu hatası",
        Errors = { $"İç hata: {ex.Message}" },
        ExecutionTimeMs = stopwatch.ElapsedMilliseconds
      };

      return StatusCode(500, errorResponse);
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
    // CommandHandlerManager'dan desteklenen komutları al (Step 22 - Modular Architecture)
    var commandsByCategory = _commandHandlerManager.GetAllSupportedCommands();

    var commandsList = new List<object>();
    foreach (var category in commandsByCategory)
    {
      foreach (var command in category.Value)
      {
        commandsList.Add(new
        {
          Command = command,
          Category = category.Key,
          Description = GetCommandDescription(command)
        });
      }
    }

    return Ok(new
    {
      Commands = commandsList,
      TotalCount = commandsList.Count,
      Categories = commandsByCategory.Keys.ToList(),
      HandlerInfo = _commandHandlerManager.GetHandlerInfo()
    });
  }

  private static string GetCommandDescription(string command)
  {
    return command.ToLowerInvariant() switch
    {
      "checkflutterversion" => "Flutter SDK sürüm kontrolü",
      "reviewcode" => "Kod incelemesi ve refactor önerileri",
      "generatetestsforcubit" => "Test üretimi ve kapsam genişletici",
      "migratenavigationsystem" => "Navigator → GoRouter dönüşümü",
      "generatescreen" => "Prompt-to-Widget UI üretimi",
      "createflutterplugin" => "Plugin/Feature şablonu üretimi",
      "writetofile" => "Güvenli dosya yazma ve oluşturma",
      "analyzefeaturecomplexity" => "Proje karmaşıklığı ve mimari analizi",
      "loadprojectpreferences" => "Proje ayarlarını yükleme",
      "searchflutterdocs" => "Flutter dokümantasyon arama",
      "searchpubdevpackages" => "pub.dev paket arama",
      "analyzepackage" => "Paket detay analizi",
      "generatedartclass" => "Dart sınıf üretimi (JSON serialization, Equatable)",
      "generatecubitboilerplate" => "Cubit/State boilerplate üretimi",
      "generateapiservice" => "HTTP API servis üretimi",
      "generatethememodule" => "Material Design 3 tema modülü üretimi",
      _ => "MCP komut açıklaması"
    };
  }



  #region MCP Protocol Layer Endpoints

  /// <summary>
  /// JSON-RPC 2.0 endpoint for AI clients (MCP Protocol)
  /// Handles JSON-RPC requests according to MCP specification
  /// </summary>
  /// <param name="request">JSON-RPC 2.0 request object</param>
  /// <returns>JSON-RPC 2.0 response object</returns>
  /// <remarks>
  /// This endpoint provides JSON-RPC 2.0 compatibility for AI clients implementing
  /// the Model Context Protocol (MCP). It supports:
  /// 
  /// - JSON-RPC 2.0 request/response format
  /// - MCP server capabilities discovery
  /// - Tool execution via JSON-RPC method calls
  /// - Proper error handling with JSON-RPC error codes
  /// 
  /// **Example JSON-RPC Request:**
  /// ```json
  /// {
  ///   "jsonrpc": "2.0",
  ///   "method": "checkFlutterVersion",
  ///   "params": {
  ///     "projectPath": "/path/to/flutter/project"
  ///   },
  ///   "id": "1"
  /// }
  /// ```
  /// </remarks>
  [HttpPost("jsonrpc")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> HandleJsonRpcRequest([FromBody] JsonElement request)
  {
    try
    {
      _logger.LogInformation("Received JSON-RPC 2.0 request");

      // Validate JSON-RPC format
      if (!_mcpProtocolService.ValidateJsonRpcRequest(request, out var errorMessage))
      {
        // Get request ID for error response
        string errorRequestId = "null";
        if (request.TryGetProperty("id", out var errorIdProp))
        {
          errorRequestId = errorIdProp.ValueKind switch
          {
            JsonValueKind.String => errorIdProp.GetString() ?? "null",
            JsonValueKind.Number => errorIdProp.GetInt32().ToString(),
            _ => "null"
          };
        }

        var errorResponse = _mcpProtocolService.CreateJsonRpcError(
          errorRequestId,
          Services.McpJsonRpcErrorCodes.InvalidRequest,
          errorMessage ?? "Invalid JSON-RPC request format"
        );
        return BadRequest(errorResponse);
      }

      // Extract method and check if it's a special MCP method
      var method = request.GetProperty("method").GetString();

      // Handle id field - it could be string, number, or null
      string requestId = "null";
      if (request.TryGetProperty("id", out var idProp))
      {
        requestId = idProp.ValueKind switch
        {
          JsonValueKind.String => idProp.GetString() ?? "null",
          JsonValueKind.Number => idProp.GetInt32().ToString(),
          _ => "null"
        };
      }

      // Handle MCP discovery methods
      if (method == "initialize" || method == "capabilities")
      {
        var capabilities = await _mcpProtocolService.GetServerCapabilitiesAsync();
        return Ok(new
        {
          jsonrpc = "2.0",
          id = requestId,
          result = capabilities
        });
      }

      // Handle MCP tools/list method
      if (method == "tools/list")
      {
        // Get capabilities directly from service
        var capabilities = await _mcpCapabilitiesService.GetCapabilitiesAsync();
        return Ok(new
        {
          jsonrpc = "2.0",
          id = requestId,
          result = new { tools = capabilities.Tools }
        });
      }

      // Handle MCP tools/call method
      if (method == "tools/call")
      {
        // Extract tool name and arguments from params
        if (request.TryGetProperty("params", out var paramsElement))
        {
          if (paramsElement.TryGetProperty("name", out var nameElement))
          {
            var toolName = nameElement.GetString();
            var arguments = paramsElement.TryGetProperty("arguments", out var argsElement) ? argsElement : (JsonElement?)null;

            // Create MCP command for the tool
            var toolCommand = new McpCommand
            {
              Command = toolName ?? string.Empty,
              Params = arguments,
              CommandId = requestId,
              DryRun = false
            };

            // Execute the command
            _logger.LogInformation("MCP Tool çağrıldı: {ToolName} - {RequestId}", toolName, requestId);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _commandHandlerManager.ExecuteCommandAsync(toolCommand);
            stopwatch.Stop();
            _logger.LogInformation("MCP Tool tamamlandı: {ToolName} - {ElapsedMs}ms", toolName, stopwatch.ElapsedMilliseconds);

            // Return tool result in MCP format
            return Ok(new
            {
              jsonrpc = "2.0",
              id = requestId,
              result = new
              {
                content = new[]
                {
                  new
                  {
                    type = "text",
                    text = result.Notes.FirstOrDefault() ?? string.Join("\n", result.CodeBlocks)
                  }
                },
                isError = !result.Success
              }
            });
          }
        }

        // Invalid tools/call request
        var toolsCallError = _mcpProtocolService.CreateJsonRpcError(requestId, Services.McpJsonRpcErrorCodes.InvalidParams, "tools/call requires 'name' parameter");
        return BadRequest(toolsCallError);
      }

      // Convert JSON-RPC to internal MCP command
      var mcpCommand = _mcpProtocolService.ConvertJsonRpcToMcpCommand(request);
      if (mcpCommand == null)
      {
        var errorResponse = _mcpProtocolService.CreateJsonRpcError(
          requestId,
          Services.McpJsonRpcErrorCodes.InvalidParams,
          "Failed to convert JSON-RPC request to MCP command"
        );
        return BadRequest(errorResponse);
      }

      // Execute the command using existing logic
      var actionResult = await ExecuteCommand(mcpCommand);

      // Extract the actual response from ActionResult
      McpResponse mcpResponse;
      if (actionResult.Result is OkObjectResult okResult && okResult.Value is McpResponse response)
      {
        mcpResponse = response;
      }
      else if (actionResult.Value != null)
      {
        mcpResponse = actionResult.Value;
      }
      else
      {
        var errorResponse = _mcpProtocolService.CreateJsonRpcError(
          requestId,
          Services.McpJsonRpcErrorCodes.InternalError,
          "Failed to execute command"
        );
        return StatusCode(500, errorResponse);
      }

      // Convert response to JSON-RPC format
      var jsonRpcResponse = _mcpProtocolService.CreateJsonRpcResponse(requestId, mcpResponse);

      return Ok(jsonRpcResponse);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing JSON-RPC request");

      var errorResponse = _mcpProtocolService.CreateJsonRpcError(
        request.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "null" : "null",
        Services.McpJsonRpcErrorCodes.InternalError,
        "Internal server error",
        new { message = ex.Message }
      );

      return StatusCode(500, errorResponse);
    }
  }

  /// <summary>
  /// MCP server capabilities endpoint for AI client discovery
  /// Returns available tools, prompts, and resources
  /// </summary>
  /// <returns>MCP server capabilities object</returns>
  [HttpGet("capabilities")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetCapabilities()
  {
    try
    {
      var capabilities = await _mcpCapabilitiesService.GetCapabilitiesAsync();
      return Ok(capabilities);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving MCP capabilities");
      return StatusCode(500, new { error = "Failed to retrieve capabilities", message = ex.Message });
    }
  }

  /// <summary>
  /// Command registry endpoint for exploring available commands
  /// Returns metadata about all supported MCP commands
  /// </summary>
  /// <returns>List of available command metadata</returns>
  [HttpGet("registry")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAvailableCommands()
  {
    try
    {
      var commands = await _mcpCommandRegistry.GetAvailableCommandsAsync();
      return Ok(new { commands = commands, count = commands.Count });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving available commands");
      return StatusCode(500, new { error = "Failed to retrieve commands", message = ex.Message });
    }
  }

  #endregion
}
