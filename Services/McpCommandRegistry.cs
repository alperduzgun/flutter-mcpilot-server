namespace FlutterMcpServer.Services;

/// <summary>
/// Registry service that manages available MCP commands and their metadata.
/// Provides command discovery and validation capabilities for AI clients.
/// </summary>
public class McpCommandRegistry
{
  private readonly ILogger<McpCommandRegistry> _logger;
  private readonly List<McpCommandInfo> _commands;

  public McpCommandRegistry(ILogger<McpCommandRegistry> logger)
  {
    _logger = logger;
    _commands = InitializeCommands();
  }

  /// <summary>
  /// Gets all available commands with their metadata.
  /// </summary>
  public Task<List<McpCommandInfo>> GetAvailableCommandsAsync()
  {
    _logger.LogInformation("Retrieving {CommandCount} available commands", _commands.Count);
    return Task.FromResult(_commands);
  }

  /// <summary>
  /// Checks if a command is supported.
  /// </summary>
  public bool IsCommandSupported(string commandName)
  {
    var isSupported = _commands.Any(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
    _logger.LogInformation("Command '{CommandName}' supported: {IsSupported}", commandName, isSupported);
    return isSupported;
  }

  /// <summary>
  /// Gets command metadata by name.
  /// </summary>
  public McpCommandInfo? GetCommandInfo(string commandName)
  {
    return _commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>
  /// Validates command parameters against the command schema.
  /// </summary>
  public ValidationResult ValidateCommand(string commandName, Dictionary<string, object>? parameters)
  {
    var commandInfo = GetCommandInfo(commandName);
    if (commandInfo == null)
    {
      return new ValidationResult(false, $"Command '{commandName}' not found");
    }

    var errors = new List<string>();

    // Check required parameters
    foreach (var param in commandInfo.Parameters.Where(p => p.Required))
    {
      if (parameters == null || !parameters.ContainsKey(param.Name))
      {
        errors.Add($"Required parameter '{param.Name}' is missing");
      }
    }

    // Check parameter types (basic validation)
    if (parameters != null)
    {
      foreach (var param in parameters)
      {
        var paramInfo = commandInfo.Parameters.FirstOrDefault(p => p.Name == param.Key);
        if (paramInfo == null)
        {
          errors.Add($"Unknown parameter '{param.Key}'");
        }
        // TODO: Add type validation here if needed
      }
    }

    return new ValidationResult(errors.Count == 0, errors.Count > 0 ? string.Join("; ", errors) : null);
  }

  /// <summary>
  /// Initializes the command registry with all available commands.
  /// </summary>
  private List<McpCommandInfo> InitializeCommands()
  {
    return new List<McpCommandInfo>
        {
            // Flutter Version and Environment
            new McpCommandInfo
            {
                Name = "checkFlutterVersion",
                Description = "Check Flutter SDK version and project compatibility",
                Category = "environment",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "projectPath",
                        Type = "string",
                        Description = "Path to Flutter project directory",
                        Required = false,
                        DefaultValue = ".",
                        Examples = new List<string> { ".", "/path/to/flutter/project", "./my_app" }
                    }
                }
            },

            // Code Generation
            new McpCommandInfo
            {
                Name = "generateDartClass",
                Description = "Generate Dart class with properties, constructors, and methods",
                Category = "codegen",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "className",
                        Type = "string",
                        Description = "Name of the class to generate",
                        Required = true,
                        Examples = new List<string> { "User", "Product", "LoginResponse" }
                    },
                    new McpParameterInfo
                    {
                        Name = "properties",
                        Type = "string",
                        Description = "Properties in format 'Type name, Type name'",
                        Required = false,
                        DefaultValue = "",
                        Examples = new List<string> { "String name, int age, bool isActive" }
                    },
                    new McpParameterInfo
                    {
                        Name = "includeJsonAnnotation",
                        Type = "boolean",
                        Description = "Include JSON serialization annotations",
                        Required = false,
                        DefaultValue = false
                    },
                    new McpParameterInfo
                    {
                        Name = "includeEquatable",
                        Type = "boolean",
                        Description = "Include Equatable implementation",
                        Required = false,
                        DefaultValue = false
                    }
                }
            },

            new McpCommandInfo
            {
                Name = "generateCubitBoilerplate",
                Description = "Generate Cubit state management boilerplate code",
                Category = "codegen",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "cubitName",
                        Type = "string",
                        Description = "Name of the Cubit class",
                        Required = true,
                        Examples = new List<string> { "AuthCubit", "UserProfileCubit", "CounterCubit" }
                    },
                    new McpParameterInfo
                    {
                        Name = "featureName",
                        Type = "string",
                        Description = "Feature name for file organization",
                        Required = false,
                        Examples = new List<string> { "auth", "user_profile", "counter" }
                    },
                    new McpParameterInfo
                    {
                        Name = "states",
                        Type = "string",
                        Description = "Comma-separated list of states",
                        Required = false,
                        DefaultValue = "initial,loading,success,error",
                        Examples = new List<string> { "initial,loading,success,error", "idle,processing,completed" }
                    }
                }
            },

            new McpCommandInfo
            {
                Name = "generateApiService",
                Description = "Generate API service class with HTTP methods",
                Category = "codegen",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "serviceName",
                        Type = "string",
                        Description = "Name of the API service class",
                        Required = true,
                        Examples = new List<string> { "UserApiService", "AuthApiService", "ProductApiService" }
                    },
                    new McpParameterInfo
                    {
                        Name = "baseUrl",
                        Type = "string",
                        Description = "Base URL for API endpoints",
                        Required = true,
                        Examples = new List<string> { "https://api.example.com", "https://jsonplaceholder.typicode.com" }
                    },
                    new McpParameterInfo
                    {
                        Name = "endpoints",
                        Type = "string",
                        Description = "HTTP endpoints in format 'METHOD:path,METHOD:path'",
                        Required = true,
                        Examples = new List<string> { "GET:/users,POST:/users,PUT:/users/{id},DELETE:/users/{id}" }
                    }
                }
            },

            new McpCommandInfo
            {
                Name = "generateThemeModule",
                Description = "Generate comprehensive theme module with colors and typography",
                Category = "codegen",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "themeName",
                        Type = "string",
                        Description = "Name of the theme class",
                        Required = true,
                        Examples = new List<string> { "AppTheme", "LightTheme", "DarkTheme" }
                    },
                    new McpParameterInfo
                    {
                        Name = "primaryColor",
                        Type = "string",
                        Description = "Primary color in hex format",
                        Required = false,
                        DefaultValue = "0xFF2196F3",
                        Examples = new List<string> { "0xFF2196F3", "0xFFE91E63", "0xFF4CAF50" }
                    },
                    new McpParameterInfo
                    {
                        Name = "isDarkTheme",
                        Type = "boolean",
                        Description = "Generate dark theme variant",
                        Required = false,
                        DefaultValue = false
                    }
                }
            },

            // Code Review and Analysis
            new McpCommandInfo
            {
                Name = "reviewCode",
                Description = "Analyze Dart/Flutter code for quality and best practices",
                Category = "analysis",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "code",
                        Type = "string",
                        Description = "Dart/Flutter code to review",
                        Required = true
                    },
                    new McpParameterInfo
                    {
                        Name = "fileName",
                        Type = "string",
                        Description = "File name for context",
                        Required = false,
                        Examples = new List<string> { "user_model.dart", "login_page.dart" }
                    }
                }
            },

            new McpCommandInfo
            {
                Name = "analyzeFeatureComplexity",
                Description = "Analyze Flutter project structure and complexity",
                Category = "analysis",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "projectPath",
                        Type = "string",
                        Description = "Path to Flutter project root",
                        Required = true,
                        Examples = new List<string> { ".", "/path/to/flutter/project" }
                    },
                    new McpParameterInfo
                    {
                        Name = "analysisDepth",
                        Type = "string",
                        Description = "Analysis depth: basic, detailed, comprehensive",
                        Required = false,
                        DefaultValue = "basic",
                        Examples = new List<string> { "basic", "detailed", "comprehensive" }
                    }
                }
            },

            // Test Generation
            new McpCommandInfo
            {
                Name = "generateTestsForCubit",
                Description = "Generate comprehensive tests for Cubit classes",
                Category = "testing",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "cubitCode",
                        Type = "string",
                        Description = "Cubit source code to generate tests for",
                        Required = true
                    },
                    new McpParameterInfo
                    {
                        Name = "cubitName",
                        Type = "string",
                        Description = "Name of the Cubit class",
                        Required = true,
                        Examples = new List<string> { "AuthCubit", "CounterCubit" }
                    }
                }
            },

            // Documentation and Search
            new McpCommandInfo
            {
                Name = "searchFlutterDocs",
                Description = "Search Flutter documentation for widgets, APIs, and guides",
                Category = "docs",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "query",
                        Type = "string",
                        Description = "Search query for Flutter documentation",
                        Required = true,
                        Examples = new List<string> { "ListView", "navigation", "state management", "TextField" }
                    },
                    new McpParameterInfo
                    {
                        Name = "category",
                        Type = "string",
                        Description = "Documentation category to search in",
                        Required = false,
                        Examples = new List<string> { "widgets", "packages", "guides", "api" }
                    }
                }
            },

            new McpCommandInfo
            {
                Name = "searchPubDevPackages",
                Description = "Search pub.dev for Flutter packages",
                Category = "packages",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "query",
                        Type = "string",
                        Description = "Package search query",
                        Required = true,
                        Examples = new List<string> { "http", "state management", "navigation", "database" }
                    },
                    new McpParameterInfo
                    {
                        Name = "maxResults",
                        Type = "integer",
                        Description = "Maximum number of results to return",
                        Required = false,
                        DefaultValue = 10
                    }
                }
            },

            new McpCommandInfo
            {
                Name = "analyzePackage",
                Description = "Analyze a specific pub.dev package for suitability",
                Category = "packages",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "packageName",
                        Type = "string",
                        Description = "Name of the package to analyze",
                        Required = true,
                        Examples = new List<string> { "dio", "bloc", "provider", "go_router" }
                    }
                }
            },

            // File Operations
            new McpCommandInfo
            {
                Name = "writeToFile",
                Description = "Write generated code to file system",
                Category = "filesystem",
                Parameters = new List<McpParameterInfo>
                {
                    new McpParameterInfo
                    {
                        Name = "filePath",
                        Type = "string",
                        Description = "Target file path",
                        Required = true,
                        Examples = new List<string> { "lib/models/user.dart", "lib/services/api_service.dart" }
                    },
                    new McpParameterInfo
                    {
                        Name = "content",
                        Type = "string",
                        Description = "File content to write",
                        Required = true
                    },
                    new McpParameterInfo
                    {
                        Name = "createDirectories",
                        Type = "boolean",
                        Description = "Create directories if they don't exist",
                        Required = false,
                        DefaultValue = true
                    }
                }
            }
        };
  }
}

#region Command Registry Models

/// <summary>
/// Information about an available MCP command.
/// </summary>
public class McpCommandInfo
{
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public List<McpParameterInfo> Parameters { get; set; } = new();
}

/// <summary>
/// Information about a command parameter.
/// </summary>
public class McpParameterInfo
{
  public string Name { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public bool Required { get; set; }
  public object? DefaultValue { get; set; }
  public List<string> Examples { get; set; } = new();
}

/// <summary>
/// Result of command validation.
/// </summary>
public class ValidationResult
{
  public bool IsValid { get; set; }
  public string? ErrorMessage { get; set; }

  public ValidationResult(bool isValid, string? errorMessage = null)
  {
    IsValid = isValid;
    ErrorMessage = errorMessage;
  }
}

#endregion
