using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles file system operations and miscellaneous commands
/// </summary>
public class FileSystemCommandHandler : ICommandHandler
{
  private readonly FileWriterService _fileWriterService;
  private readonly NavigationMigrationService _navigationMigrationService;
  private readonly ScreenGeneratorService _screenGeneratorService;
  private readonly PluginCreatorService _pluginCreatorService;
  private readonly ConfigService _configService;
  private readonly ILogger<FileSystemCommandHandler> _logger;

  public string Category => "filesystem";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "writeToFile",
        "migrateNavigationSystem",
        "generateScreen",
        "createFlutterPlugin",
        "loadProjectPreferences"
    };

  public FileSystemCommandHandler(
      FileWriterService fileWriterService,
      NavigationMigrationService navigationMigrationService,
      ScreenGeneratorService screenGeneratorService,
      PluginCreatorService pluginCreatorService,
      ConfigService configService,
      ILogger<FileSystemCommandHandler> logger)
  {
    _fileWriterService = fileWriterService;
    _navigationMigrationService = navigationMigrationService;
    _screenGeneratorService = screenGeneratorService;
    _pluginCreatorService = pluginCreatorService;
    _configService = configService;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing filesystem command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "writetofile" or "writefile" => await _fileWriterService.WriteFileAsync(command),
      "migratenavigationsystem" => await _navigationMigrationService.MigrateNavigationSystemAsync(command),
      "generatescreen" => await _screenGeneratorService.GenerateScreenAsync(command),
      "createflutterplugin" => await _pluginCreatorService.CreateFlutterPluginAsync(command),
      "loadprojectpreferences" => await _configService.LoadProjectPreferencesAsync(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen dosya sistemi komutu",
      Errors = { $"'{command.Command}' komutu FileSystemCommandHandler tarafından desteklenmiyor." }
    };
  }
}
