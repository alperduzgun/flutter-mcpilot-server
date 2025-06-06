using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles environment-related commands like Flutter version checking
/// </summary>
public class EnvironmentCommandHandler : ICommandHandler
{
  private readonly FlutterVersionChecker _flutterVersionChecker;
  private readonly ILogger<EnvironmentCommandHandler> _logger;

  public string Category => "environment";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "checkFlutterVersion"
    };

  public EnvironmentCommandHandler(
      FlutterVersionChecker flutterVersionChecker,
      ILogger<EnvironmentCommandHandler> logger)
  {
    _flutterVersionChecker = flutterVersionChecker;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing environment command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "checkflutterversion" => await _flutterVersionChecker.CheckFlutterVersionAsync(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen environment komutu",
      Errors = { $"'{command.Command}' komutu EnvironmentCommandHandler tarafından desteklenmiyor." }
    };
  }
}
