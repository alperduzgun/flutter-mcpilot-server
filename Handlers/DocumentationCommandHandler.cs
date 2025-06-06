using System.Text.Json;
using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles documentation search commands
/// </summary>
public class DocumentationCommandHandler : ICommandHandler
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<DocumentationCommandHandler> _logger;

  public string Category => "docs";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "searchFlutterDocs"
    };

  public DocumentationCommandHandler(
      IServiceProvider serviceProvider,
      ILogger<DocumentationCommandHandler> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing documentation command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "searchflutterdocs" => await HandleSearchFlutterDocs(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private async Task<McpResponse> HandleSearchFlutterDocs(McpCommand command)
  {
    var docService = _serviceProvider.GetRequiredService<FlutterDocService>();

    var searchTerm = "";
    var category = "widgets";

    if (command.Params.HasValue)
    {
      var paramsJson = command.Params.Value;
      if (paramsJson.TryGetProperty("searchTerm", out var searchTermElement))
        searchTerm = searchTermElement.GetString() ?? "";
      if (paramsJson.TryGetProperty("category", out var categoryElement))
        category = categoryElement.GetString() ?? "widgets";
    }

    var result = await docService.SearchFlutterDocs(searchTerm, category);

    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = true,
      Purpose = $"Flutter documentation search for '{searchTerm}'",
      Notes = { $"🧠 Found documentation for '{searchTerm}' in category '{category}'", "📘 Check multiple categories for comprehensive coverage" },
      LearnNotes = { "💡 Flutter docs are the best source for widget examples", "🎯 Use specific search terms for better results" }
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen dokümantasyon komutu",
      Errors = { $"'{command.Command}' komutu DocumentationCommandHandler tarafından desteklenmiyor." }
    };
  }
}
