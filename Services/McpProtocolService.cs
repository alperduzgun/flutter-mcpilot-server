using System.Text.Json;
using FlutterMcpServer.Models;

namespace FlutterMcpServer.Services;

/// <summary>
/// Model Context Protocol (MCP) JSON-RPC 2.0 implementation service.
/// Handles protocol-level operations and message formatting for AI clients.
/// </summary>
public class McpProtocolService
{
  private readonly ILogger<McpProtocolService> _logger;
  private readonly McpCapabilitiesService _capabilitiesService;

  public McpProtocolService(
      ILogger<McpProtocolService> logger,
      McpCapabilitiesService capabilitiesService)
  {
    _logger = logger;
    _capabilitiesService = capabilitiesService;
  }

  /// <summary>
  /// Creates a JSON-RPC 2.0 compliant response for AI clients.
  /// </summary>
  public McpJsonRpcResponse CreateJsonRpcResponse(string id, McpResponse mcpResponse)
  {
    _logger.LogInformation("Creating JSON-RPC response for ID: {Id}", id);

    return new McpJsonRpcResponse
    {
      Jsonrpc = "2.0",
      Id = id,
      Result = new McpJsonRpcResult
      {
        Success = mcpResponse.Success,
        Purpose = mcpResponse.Purpose,
        CodeBlocks = mcpResponse.CodeBlocks?.Select(cb => new McpJsonRpcCodeBlock
        {
          File = cb.File,
          Content = cb.Content,
          Language = cb.Language,
          Operation = cb.Operation
        }).ToList() ?? new List<McpJsonRpcCodeBlock>(),
        Notes = mcpResponse.Notes ?? new List<string>(),
        LearnNotes = mcpResponse.LearnNotes ?? new List<string>(),
        Errors = mcpResponse.Errors ?? new List<string>(),
        ExecutionTimeMs = mcpResponse.ExecutionTimeMs,
        Metadata = new Dictionary<string, object>
        {
          ["commandId"] = mcpResponse.CommandId,
          ["timestamp"] = DateTime.UtcNow.ToString("O"),
          ["serverVersion"] = "1.0.0",
          ["protocolVersion"] = "2.0"
        }
      }
    };
  }

  /// <summary>
  /// Creates a JSON-RPC 2.0 error response.
  /// </summary>
  public McpJsonRpcResponse CreateJsonRpcError(string id, int errorCode, string message, object? data = null)
  {
    _logger.LogError("Creating JSON-RPC error response: {ErrorCode} - {Message}", errorCode, message);

    return new McpJsonRpcResponse
    {
      Jsonrpc = "2.0",
      Id = id,
      Error = new McpJsonRpcError
      {
        Code = errorCode,
        Message = message,
        Data = data
      }
    };
  }

  /// <summary>
  /// Validates incoming JSON-RPC 2.0 request format.
  /// </summary>
  public bool ValidateJsonRpcRequest(JsonElement request, out string? errorMessage)
  {
    errorMessage = null;

    try
    {
      // Check JSON-RPC version
      if (!request.TryGetProperty("jsonrpc", out var jsonrpcElement) ||
          jsonrpcElement.GetString() != "2.0")
      {
        errorMessage = "Invalid or missing 'jsonrpc' field. Must be '2.0'";
        return false;
      }

      // Check method
      if (!request.TryGetProperty("method", out var methodElement) ||
          string.IsNullOrEmpty(methodElement.GetString()))
      {
        errorMessage = "Missing or empty 'method' field";
        return false;
      }

      // Check id (optional for notifications, required for requests)
      if (!request.TryGetProperty("id", out var idElement))
      {
        _logger.LogInformation("No 'id' field found - treating as notification");
      }

      return true;
    }
    catch (Exception ex)
    {
      errorMessage = $"JSON-RPC validation error: {ex.Message}";
      return false;
    }
  }

  /// <summary>
  /// Converts JSON-RPC request to internal MCP command format.
  /// </summary>
  public McpCommand? ConvertJsonRpcToMcpCommand(JsonElement request)
  {
    try
    {
      var method = request.GetProperty("method").GetString();
      var hasId = request.TryGetProperty("id", out var idElement);
      var hasParams = request.TryGetProperty("params", out var paramsElement);

      return new McpCommand
      {
        Command = method ?? string.Empty,
        Params = hasParams ? paramsElement : null,
        CommandId = hasId ? idElement.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString(),
        DryRun = false // Default, can be overridden in params
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error converting JSON-RPC request to MCP command");
      return null;
    }
  }

  /// <summary>
  /// Gets server capabilities for AI clients (implements MCP discovery).
  /// </summary>
  public async Task<object> GetServerCapabilitiesAsync()
  {
    var capabilities = await _capabilitiesService.GetCapabilitiesAsync();

    return new
    {
      protocolVersion = "2.0",
      serverInfo = new
      {
        name = "Flutter MCP Server",
        version = "1.0.0",
        description = "AI-powered Flutter development assistant"
      },
      capabilities = capabilities,
      tools = capabilities.Tools,
      prompts = capabilities.Prompts,
      resources = capabilities.Resources
    };
  }
}

#region JSON-RPC 2.0 Models

/// <summary>
/// JSON-RPC 2.0 Response wrapper for MCP responses.
/// </summary>
public class McpJsonRpcResponse
{
  public string Jsonrpc { get; set; } = "2.0";
  public string? Id { get; set; }
  public McpJsonRpcResult? Result { get; set; }
  public McpJsonRpcError? Error { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 Result for successful responses.
/// </summary>
public class McpJsonRpcResult
{
  public bool Success { get; set; }
  public string Purpose { get; set; } = string.Empty;
  public List<McpJsonRpcCodeBlock> CodeBlocks { get; set; } = new();
  public List<string> Notes { get; set; } = new();
  public List<string> LearnNotes { get; set; } = new();
  public List<string> Errors { get; set; } = new();
  public long ExecutionTimeMs { get; set; }
  public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// JSON-RPC 2.0 CodeBlock for generated code.
/// </summary>
public class McpJsonRpcCodeBlock
{
  public string File { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string Language { get; set; } = "dart";
  public string Operation { get; set; } = "create";
}

/// <summary>
/// JSON-RPC 2.0 Error for failed responses.
/// </summary>
public class McpJsonRpcError
{
  public int Code { get; set; }
  public string Message { get; set; } = string.Empty;
  public object? Data { get; set; }
}

#endregion

#region JSON-RPC Error Codes

/// <summary>
/// Standard JSON-RPC 2.0 error codes plus MCP-specific codes.
/// </summary>
public static class McpJsonRpcErrorCodes
{
  // Standard JSON-RPC 2.0 errors
  public const int ParseError = -32700;
  public const int InvalidRequest = -32600;
  public const int MethodNotFound = -32601;
  public const int InvalidParams = -32602;
  public const int InternalError = -32603;

  // MCP-specific errors
  public const int CommandNotSupported = -32001;
  public const int ProjectNotFound = -32002;
  public const int FileSystemError = -32003;
  public const int FlutterSdkError = -32004;
  public const int CodeGenerationError = -32005;
  public const int TestGenerationError = -32006;
  public const int AnalysisError = -32007;
}

#endregion
