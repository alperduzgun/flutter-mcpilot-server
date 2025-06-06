using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles code analysis and review commands
/// </summary>
public class AnalysisCommandHandler : ICommandHandler
{
  private readonly CodeReviewService _codeReviewService;
  private readonly ProjectAnalyzer _projectAnalyzer;
  private readonly ILogger<AnalysisCommandHandler> _logger;

  public string Category => "analysis";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "reviewCode",
        "analyzeFeatureComplexity"
    };

  public AnalysisCommandHandler(
      CodeReviewService codeReviewService,
      ProjectAnalyzer projectAnalyzer,
      ILogger<AnalysisCommandHandler> logger)
  {
    _codeReviewService = codeReviewService;
    _projectAnalyzer = projectAnalyzer;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing analysis command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "reviewcode" => await _codeReviewService.ReviewCodeAsync(command),
      "analyzefeaturecomplexity" => await _projectAnalyzer.AnalyzeFeatureComplexityAsync(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen analiz komutu",
      Errors = { $"'{command.Command}' komutu AnalysisCommandHandler tarafından desteklenmiyor." }
    };
  }
}
