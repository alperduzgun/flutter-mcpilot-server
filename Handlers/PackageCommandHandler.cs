using System.Text.Json;
using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles package-related commands like searching pub.dev and analyzing packages
/// </summary>
public class PackageCommandHandler : ICommandHandler
{
  private readonly PubDevService _pubDevService;
  private readonly ILogger<PackageCommandHandler> _logger;

  public string Category => "packages";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "searchPubDevPackages",
        "analyzePackage"
    };

  public PackageCommandHandler(
      PubDevService pubDevService,
      ILogger<PackageCommandHandler> logger)
  {
    _pubDevService = pubDevService;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing package command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "searchpubdevpackages" => await HandleSearchPubDevPackages(command),
      "analyzepackage" => await HandleAnalyzePackage(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private async Task<McpResponse> HandleSearchPubDevPackages(McpCommand command)
  {
    var searchTerm = "";
    var category = "popular";
    var includeAnalysis = true;

    if (command.Params.HasValue)
    {
      var paramsJson = command.Params.Value;
      if (paramsJson.TryGetProperty("searchTerm", out var searchTermElement))
        searchTerm = searchTermElement.GetString() ?? "";
      if (paramsJson.TryGetProperty("category", out var categoryElement))
        category = categoryElement.GetString() ?? "popular";
      if (paramsJson.TryGetProperty("includeAnalysis", out var analysisElement))
        includeAnalysis = analysisElement.GetBoolean();
    }

    var result = await _pubDevService.SearchPubDevPackages(searchTerm, category, includeAnalysis);

    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = true,
      Purpose = $"pub.dev package search for '{searchTerm}' in category '{category}'",
      CodeBlocks = { new CodeBlock
            {
                File = "pubdev_search_results.json",
                Content = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                Language = "json",
                Operation = "create"
            }
            },
      Notes = { $"🧠 Found packages for '{searchTerm}' in '{category}' category", "📘 Check package maintenance status and compatibility" },
      LearnNotes = { "💡 Popular packages often have better community support", "🎯 Consider package size impact on app bundle", "⚡ Always verify package maintenance status" }
    };
  }

  private async Task<McpResponse> HandleAnalyzePackage(McpCommand command)
  {
    var packageName = "";

    if (command.Params.HasValue)
    {
      var paramsJson = command.Params.Value;
      if (paramsJson.TryGetProperty("packageName", out var packageNameElement))
        packageName = packageNameElement.GetString() ?? "";
    }

    var result = await _pubDevService.AnalyzePackage(packageName);

    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = true,
      Purpose = $"Detailed analysis of package '{packageName}'",
      CodeBlocks = { new CodeBlock
            {
                File = $"package_analysis_{packageName}.json",
                Content = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                Language = "json",
                Operation = "create"
            }
            },
      Notes = { $"🧠 Analyzed package '{packageName}' for quality and compatibility", "📘 Check health score and maintenance status", "⚡ Review security analysis before production use" },
      LearnNotes = { "💡 Package health scores indicate overall quality", "🎯 Always check compatibility with your Flutter version", "⚡ Security analysis helps identify potential vulnerabilities" }
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen paket komutu",
      Errors = { $"'{command.Command}' komutu PackageCommandHandler tarafından desteklenmiyor." }
    };
  }
}
