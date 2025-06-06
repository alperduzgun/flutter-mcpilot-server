using FlutterMcpServer.Models;

namespace FlutterMcpServer.Handlers;

/// <summary>
/// Base interface for all MCP command handlers.
/// Provides a common contract for command execution and validation.
/// </summary>
public interface ICommandHandler
{
  /// <summary>
  /// Determines if this handler can process the given command.
  /// </summary>
  /// <param name="commandName">The name of the command to check</param>
  /// <returns>True if this handler can process the command, false otherwise</returns>
  bool CanHandle(string commandName);

  /// <summary>
  /// Executes the command and returns the response.
  /// </summary>
  /// <param name="command">The MCP command to execute</param>
  /// <returns>The command execution result</returns>
  Task<McpResponse> HandleAsync(McpCommand command);

  /// <summary>
  /// Gets the category of commands this handler processes.
  /// </summary>
  string Category { get; }

  /// <summary>
  /// Gets the list of commands this handler supports.
  /// </summary>
  IEnumerable<string> SupportedCommands { get; }
}
