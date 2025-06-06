using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles testing-related commands like test generation
/// </summary>
public class TestingCommandHandler : ICommandHandler
{
  private readonly TestGeneratorService _testGeneratorService;
  private readonly ILogger<TestingCommandHandler> _logger;

  public string Category => "testing";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "generateTestsForCubit"
    };

  public TestingCommandHandler(
      TestGeneratorService testGeneratorService,
      ILogger<TestingCommandHandler> logger)
  {
    _testGeneratorService = testGeneratorService;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing testing command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "generatetestsforcubit" => await _testGeneratorService.GenerateTestsForCubitAsync(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen test komutu",
      Errors = { $"'{command.Command}' komutu TestingCommandHandler tarafından desteklenmiyor." }
    };
  }
}
