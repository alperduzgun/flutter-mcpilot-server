using FlutterMcpServer.Models;
using FlutterMcpServer.Services;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Handles code generation commands like generating Dart classes, Cubits, API services, and themes
/// </summary>
public class CodeGenerationCommandHandler : ICommandHandler
{
  private readonly CodeGenerator _codeGenerator;
  private readonly ILogger<CodeGenerationCommandHandler> _logger;

  public string Category => "codegen";

  public IEnumerable<string> SupportedCommands => new[]
  {
        "generateDartClass",
        "generateCubitBoilerplate",
        "generateApiService",
        "generateThemeModule"
    };

  public CodeGenerationCommandHandler(
      CodeGenerator codeGenerator,
      ILogger<CodeGenerationCommandHandler> logger)
  {
    _codeGenerator = codeGenerator;
    _logger = logger;
  }

  public bool CanHandle(string commandName)
  {
    return SupportedCommands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
  }

  public async Task<McpResponse> HandleAsync(McpCommand command)
  {
    _logger.LogInformation("Executing code generation command: {Command}", command.Command);

    return command.Command.ToLowerInvariant() switch
    {
      "generatedartclass" => await _codeGenerator.GenerateDartClassAsync(command),
      "generatecubitboilerplate" or "generatecubit" => await _codeGenerator.GenerateCubitBoilerplateAsync(command),
      "generateapiservice" => await _codeGenerator.GenerateApiServiceAsync(command),
      "generatethememodule" or "generatetheme" => await _codeGenerator.GenerateThemeModuleAsync(command),
      _ => CreateUnsupportedCommandResponse(command)
    };
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen kod üretimi komutu",
      Errors = { $"'{command.Command}' komutu CodeGenerationCommandHandler tarafından desteklenmiyor." }
    };
  }
}
