using FlutterMcpServer.Models;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Manages all command handlers and routes commands to appropriate handlers
/// </summary>
public class CommandHandlerManager
{
  private readonly IEnumerable<ICommandHandler> _handlers;
  private readonly ILogger<CommandHandlerManager> _logger;

  public CommandHandlerManager(
      IEnumerable<ICommandHandler> handlers,
      ILogger<CommandHandlerManager> logger)
  {
    _handlers = handlers;
    _logger = logger;
  }

  /// <summary>
  /// Finds the appropriate handler for the given command
  /// </summary>
  /// <param name="commandName">The command name to find a handler for</param>
  /// <returns>The handler that can process the command, or null if none found</returns>
  public ICommandHandler? FindHandler(string commandName)
  {
    var handler = _handlers.FirstOrDefault(h => h.CanHandle(commandName));

    if (handler != null)
    {
      _logger.LogInformation("Found handler '{HandlerType}' for command '{Command}'",
          handler.GetType().Name, commandName);
    }
    else
    {
      _logger.LogWarning("No handler found for command '{Command}'", commandName);
    }

    return handler;
  }

  /// <summary>
  /// Executes a command using the appropriate handler
  /// </summary>
  /// <param name="command">The command to execute</param>
  /// <returns>The command execution result</returns>
  public async Task<McpResponse> ExecuteCommandAsync(McpCommand command)
  {
    var handler = FindHandler(command.Command);

    if (handler == null)
    {
      return CreateUnsupportedCommandResponse(command);
    }

    try
    {
      _logger.LogInformation("Executing command '{Command}' with handler '{HandlerType}'",
          command.Command, handler.GetType().Name);

      return await handler.HandleAsync(command);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error executing command '{Command}' with handler '{HandlerType}'",
          command.Command, handler.GetType().Name);

      return CreateErrorResponse(command, ex);
    }
  }

  /// <summary>
  /// Gets all available commands from all handlers
  /// </summary>
  /// <returns>Dictionary of commands grouped by category</returns>
  public Dictionary<string, List<string>> GetAllSupportedCommands()
  {
    var commandsByCategory = new Dictionary<string, List<string>>();

    foreach (var handler in _handlers)
    {
      if (!commandsByCategory.ContainsKey(handler.Category))
      {
        commandsByCategory[handler.Category] = new List<string>();
      }

      commandsByCategory[handler.Category].AddRange(handler.SupportedCommands);
    }

    return commandsByCategory;
  }

  /// <summary>
  /// Gets all available handlers with their metadata
  /// </summary>
  /// <returns>List of handler information</returns>
  public List<object> GetHandlerInfo()
  {
    return _handlers.Select(h => new
    {
      HandlerType = h.GetType().Name,
      Category = h.Category,
      SupportedCommands = h.SupportedCommands.ToList(),
      CommandCount = h.SupportedCommands.Count()
    }).ToList<object>();
  }

  private static McpResponse CreateUnsupportedCommandResponse(McpCommand command)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Desteklenmeyen komut",
      Errors = { $"'{command.Command}' komutu için uygun handler bulunamadı." },
      Notes = { "Desteklenen komutlar için /api/command/commands endpoint'ini kullanın." }
    };
  }

  private static McpResponse CreateErrorResponse(McpCommand command, Exception ex)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = false,
      Purpose = "Komut işleme hatası",
      Errors = { $"Komut işlenirken hata oluştu: {ex.Message}" },
      Notes = { "Detaylar için server loglarını kontrol edin." }
    };
  }
}
