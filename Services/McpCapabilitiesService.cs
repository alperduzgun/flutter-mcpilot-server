using FlutterMcpServer.Models;

namespace FlutterMcpServer.Services;

/// <summary>
/// Service that manages and provides MCP server capabilities for AI client discovery.
/// Implements the MCP capabilities protocol for tool/resource/prompt discovery.
/// </summary>
public class McpCapabilitiesService
{
  private readonly ILogger<McpCapabilitiesService> _logger;
  private readonly McpCommandRegistry _commandRegistry;

  public McpCapabilitiesService(
      ILogger<McpCapabilitiesService> logger,
      McpCommandRegistry commandRegistry)
  {
    _logger = logger;
    _commandRegistry = commandRegistry;
  }

  /// <summary>
  /// Gets comprehensive server capabilities for MCP client discovery.
  /// </summary>
  public async Task<McpCapabilities> GetCapabilitiesAsync()
  {
    _logger.LogInformation("Generating MCP server capabilities");

    var commands = await _commandRegistry.GetAvailableCommandsAsync();

    return new McpCapabilities
    {
      Tools = GenerateTools(commands),
      Prompts = GeneratePrompts(),
      Resources = GenerateResources(),
      Sampling = new McpSamplingCapability
      {
        Supported = false // We don't support sampling yet
      }
    };
  }

  /// <summary>
  /// Generates tool definitions for each available command.
  /// Tools are executable functions that AI can call.
  /// </summary>
  private List<McpTool> GenerateTools(List<McpCommandInfo> commands)
  {
    var tools = new List<McpTool>();

    foreach (var command in commands)
    {
      tools.Add(new McpTool
      {
        Name = command.Name,
        Description = command.Description,
        InputSchema = new McpToolInputSchema
        {
          Type = "object",
          Properties = command.Parameters.ToDictionary(
                  p => p.Name,
                  p => new McpToolParameter
                  {
                    Type = p.Type,
                    Description = p.Description,
                    Required = p.Required,
                    Default = p.DefaultValue,
                    Examples = p.Examples
                  }
              ),
          Required = command.Parameters.Where(p => p.Required).Select(p => p.Name).ToList()
        }
      });
    }

    return tools;
  }

  /// <summary>
  /// Generates prompt templates for common Flutter development tasks.
  /// Prompts help AI understand how to construct proper commands.
  /// </summary>
  private List<McpPrompt> GeneratePrompts()
  {
    return new List<McpPrompt>
        {
            new McpPrompt
            {
                Name = "generate_flutter_class",
                Description = "Generate a Dart class with properties and methods",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "className", Description = "Name of the class to generate", Required = true },
                    new McpPromptArgument { Name = "properties", Description = "List of properties (e.g., 'String name, int age')", Required = false },
                    new McpPromptArgument { Name = "includeJsonAnnotation", Description = "Include JSON serialization", Required = false }
                }
            },
            new McpPrompt
            {
                Name = "create_cubit_boilerplate",
                Description = "Generate Cubit state management boilerplate",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "cubitName", Description = "Name of the Cubit", Required = true },
                    new McpPromptArgument { Name = "featureName", Description = "Feature name for file organization", Required = false },
                    new McpPromptArgument { Name = "states", Description = "Comma-separated list of states", Required = false }
                }
            },
            new McpPrompt
            {
                Name = "generate_api_service",
                Description = "Generate API service class with HTTP methods",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "serviceName", Description = "Name of the service class", Required = true },
                    new McpPromptArgument { Name = "baseUrl", Description = "Base URL for API endpoints", Required = true },
                    new McpPromptArgument { Name = "endpoints", Description = "HTTP endpoints (e.g., 'GET:/users,POST:/users')", Required = true }
                }
            },
            new McpPrompt
            {
                Name = "analyze_flutter_project",
                Description = "Analyze Flutter project structure and complexity",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "projectPath", Description = "Path to Flutter project root", Required = true },
                    new McpPromptArgument { Name = "analysisType", Description = "Type of analysis (complexity, dependencies, structure)", Required = false }
                }
            }
        };
  }

  /// <summary>
  /// Generates resource definitions for file templates and documentation.
  /// Resources are read-only content that AI can access for context.
  /// </summary>
  private List<McpResource> GenerateResources()
  {
    return new List<McpResource>
        {
            new McpResource
            {
                Uri = "template://dart-class",
                Name = "Dart Class Template",
                Description = "Template for generating Dart classes",
                MimeType = "text/plain"
            },
            new McpResource
            {
                Uri = "template://cubit-boilerplate",
                Name = "Cubit Boilerplate Template",
                Description = "Template for generating Cubit state management",
                MimeType = "text/plain"
            },
            new McpResource
            {
                Uri = "template://api-service",
                Name = "API Service Template",
                Description = "Template for generating API service classes",
                MimeType = "text/plain"
            },
            new McpResource
            {
                Uri = "docs://flutter-best-practices",
                Name = "Flutter Best Practices",
                Description = "Curated Flutter development best practices",
                MimeType = "text/markdown"
            },
            new McpResource
            {
                Uri = "config://flutter-project",
                Name = "Flutter Project Configuration",
                Description = "Current Flutter project configuration and metadata",
                MimeType = "application/json"
            }
        };
  }

  /// <summary>
  /// Gets specific capability by name (for dynamic capability queries).
  /// </summary>
  public async Task<object?> GetCapabilityAsync(string capabilityName)
  {
    _logger.LogInformation("Getting capability: {CapabilityName}", capabilityName);

    return capabilityName.ToLower() switch
    {
      "tools" => (await GetCapabilitiesAsync()).Tools,
      "prompts" => (await GetCapabilitiesAsync()).Prompts,
      "resources" => (await GetCapabilitiesAsync()).Resources,
      "sampling" => (await GetCapabilitiesAsync()).Sampling,
      _ => null
    };
  }
}

#region MCP Capability Models

/// <summary>
/// Main capabilities structure for MCP server.
/// </summary>
public class McpCapabilities
{
  public List<McpTool> Tools { get; set; } = new();
  public List<McpPrompt> Prompts { get; set; } = new();
  public List<McpResource> Resources { get; set; } = new();
  public McpSamplingCapability Sampling { get; set; } = new();
}

/// <summary>
/// Tool definition for executable commands.
/// </summary>
public class McpTool
{
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public McpToolInputSchema InputSchema { get; set; } = new();
}

/// <summary>
/// Input schema for tool parameters.
/// </summary>
public class McpToolInputSchema
{
  public string Type { get; set; } = "object";
  public Dictionary<string, McpToolParameter> Properties { get; set; } = new();
  public List<string> Required { get; set; } = new();
}

/// <summary>
/// Individual tool parameter definition.
/// </summary>
public class McpToolParameter
{
  public string Type { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public bool Required { get; set; }
  public object? Default { get; set; }
  public List<string> Examples { get; set; } = new();
}

/// <summary>
/// Prompt template for AI guidance.
/// </summary>
public class McpPrompt
{
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public List<McpPromptArgument> Arguments { get; set; } = new();
}

/// <summary>
/// Prompt argument definition.
/// </summary>
public class McpPromptArgument
{
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public bool Required { get; set; }
}

/// <summary>
/// Resource definition for templates and documentation.
/// </summary>
public class McpResource
{
  public string Uri { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string MimeType { get; set; } = string.Empty;
}

/// <summary>
/// Sampling capability (for AI model sampling control).
/// </summary>
public class McpSamplingCapability
{
  public bool Supported { get; set; }
}

#endregion
