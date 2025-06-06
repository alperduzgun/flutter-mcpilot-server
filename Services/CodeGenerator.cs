// filepath: /Users/alper/Documents/Development/Personal/FlutterMcpServer/Services/CodeGenerator.cs

using System.Text;
using System.Text.Json;
using FlutterMcpServer.Models;

namespace FlutterMcpServer.Services;

/// <summary>
/// Advanced code generation service for Flutter/Dart projects.
/// Handles generic code generation tasks like classes, cubits, API services, and themes.
/// </summary>
public class CodeGenerator
{
  private readonly ILogger<CodeGenerator> _logger;

  public CodeGenerator(ILogger<CodeGenerator> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Generates a Dart class with properties, constructors, and methods.
  /// </summary>
  public Task<McpResponse> GenerateDartClassAsync(McpCommand command)
  {
    _logger.LogInformation("Generating Dart class for command: {Command}", command.Command);

    var startTime = DateTime.UtcNow;

    try
    {
      // Parse parameters
      var className = GetParameterValue(command, "className", "MyClass");
      var properties = GetParameterValue(command, "properties", "");
      var includeJsonAnnotation = GetParameterValue(command, "includeJsonAnnotation", "false").ToLower() == "true";
      var includeEquatable = GetParameterValue(command, "includeEquatable", "false").ToLower() == "true";
      var includeToString = GetParameterValue(command, "includeToString", "true").ToLower() == "true";

      var codeBuilder = new StringBuilder();

      // Add imports
      if (includeJsonAnnotation)
      {
        codeBuilder.AppendLine("import 'package:json_annotation/json_annotation.dart';");
        codeBuilder.AppendLine($"part '{className.ToLower()}.g.dart';");
        codeBuilder.AppendLine();
      }

      if (includeEquatable)
      {
        codeBuilder.AppendLine("import 'package:equatable/equatable.dart';");
        codeBuilder.AppendLine();
      }

      // Add class annotation
      if (includeJsonAnnotation)
      {
        codeBuilder.AppendLine("@JsonSerializable()");
      }

      // Class declaration
      var inheritance = includeEquatable ? " extends Equatable" : "";
      codeBuilder.AppendLine($"class {className}{inheritance} {{");

      // Parse and add properties
      var parsedProperties = ParseProperties(properties);
      foreach (var prop in parsedProperties)
      {
        var annotation = includeJsonAnnotation ? "  @JsonKey()\n" : "";
        codeBuilder.AppendLine($"{annotation}  final {prop.Type} {prop.Name};");
      }

      if (parsedProperties.Any())
      {
        codeBuilder.AppendLine();
      }

      // Constructor
      if (parsedProperties.Any())
      {
        codeBuilder.AppendLine($"  const {className}({{");
        foreach (var prop in parsedProperties)
        {
          codeBuilder.AppendLine($"    required this.{prop.Name},");
        }
        codeBuilder.AppendLine("  });");
        codeBuilder.AppendLine();
      }

      // JSON serialization methods
      if (includeJsonAnnotation)
      {
        codeBuilder.AppendLine($"  factory {className}.fromJson(Map<String, dynamic> json) => _${className}FromJson(json);");
        codeBuilder.AppendLine($"  Map<String, dynamic> toJson() => _${className}ToJson(this);");
        codeBuilder.AppendLine();
      }

      // Equatable props
      if (includeEquatable)
      {
        codeBuilder.AppendLine("  @override");
        codeBuilder.AppendLine("  List<Object?> get props => [");
        foreach (var prop in parsedProperties)
        {
          codeBuilder.AppendLine($"    {prop.Name},");
        }
        codeBuilder.AppendLine("  ];");
        codeBuilder.AppendLine();
      }

      // toString method
      if (includeToString && !includeEquatable)
      {
        codeBuilder.AppendLine("  @override");
        codeBuilder.AppendLine("  String toString() {");
        var propsString = string.Join(", ", parsedProperties.Select(p => $"{p.Name}: ${p.Name}"));
        codeBuilder.AppendLine($"    return '{className}({propsString})';");
        codeBuilder.AppendLine("  }");
        codeBuilder.AppendLine();
      }

      codeBuilder.AppendLine("}");

      var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds; return Task.FromResult(new McpResponse
      {
        Success = true,
        Purpose = $"Dart class '{className}' generated successfully",
        CodeBlocks = new List<CodeBlock>
                {
                    new CodeBlock
                    {
                        File = $"{className.ToLower()}.dart",
                        Content = codeBuilder.ToString(),
                        Language = "dart",
                        Operation = "create"
                    }
                },
        ExecutionTimeMs = (long)executionTime,
        CommandId = command.CommandId,
        Notes = new List<string>
                {
                    $"Generated class with {parsedProperties.Count} properties",
                    $"Includes: JSON serialization: {includeJsonAnnotation}, Equatable: {includeEquatable}"
                },
        LearnNotes = new List<string>
                {
                    "🧠 MCP Integration: Dart class generation follows MCP protocol standards",
                    "📘 Flutter Benefits: Generated class is ready for state management and JSON serialization",
                    "🛠️ Practical Usage: Use this class in your models folder for data structures"
                }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating Dart class");
      return Task.FromResult(new McpResponse
      {
        Success = false,
        Purpose = "Error generating Dart class",
        Errors = new List<string> { ex.Message },
        ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
        CommandId = command.CommandId
      });
    }
  }

  /// <summary>
  /// Generates a Cubit boilerplate with states, events, and bloc structure.
  /// </summary>
  public Task<McpResponse> GenerateCubitBoilerplateAsync(McpCommand command)
  {
    _logger.LogInformation("Generating Cubit boilerplate for command: {Command}", command.Command);

    var startTime = DateTime.UtcNow;

    try
    {
      var cubitName = GetParameterValue(command, "cubitName", "MyCubit");
      var featureName = GetParameterValue(command, "featureName", cubitName.Replace("Cubit", ""));
      var includeInitialState = GetParameterValue(command, "includeInitialState", "true").ToLower() == "true";
      var states = GetParameterValue(command, "states", "initial,loading,success,error");

      var stateList = states.Split(',').Select(s => s.Trim()).ToList();

      var codeBlocks = new List<string>();

      // Generate State file
      var stateCode = GenerateCubitStates(featureName, stateList);
      codeBlocks.Add(stateCode);

      // Generate Cubit file
      var cubitCode = GenerateCubitClass(cubitName, featureName, stateList);
      codeBlocks.Add(cubitCode);

      var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

      return Task.FromResult(new McpResponse
      {
        Success = true,
        Purpose = $"Cubit boilerplate for '{cubitName}' generated successfully",
        CodeBlocks = new List<CodeBlock>
        {
          new CodeBlock
          {
            File = $"{featureName.ToLower()}_state.dart",
            Content = stateCode,
            Language = "dart",
            Operation = "create"
          },
          new CodeBlock
          {
            File = $"{featureName.ToLower()}_cubit.dart",
            Content = cubitCode,
            Language = "dart",
            Operation = "create"
          }
        },
        ExecutionTimeMs = (long)executionTime,
        CommandId = command.CommandId,
        Notes = new List<string>
        {
          $"Generated {stateList.Count} states for {featureName}",
          "Dependencies required: flutter_bloc, equatable"
        },
        LearnNotes = new List<string>
        {
          "🧠 MCP Integration: Cubit pattern follows Flutter BLoC architecture",
          "📘 State Management: Use cubits for simple state management without events",
          "🛠️ Practical Usage: Place these files in your feature's cubit folder"
        }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating Cubit boilerplate");
      return Task.FromResult(new McpResponse
      {
        Success = false,
        Purpose = "Error generating Cubit boilerplate",
        Errors = new List<string> { ex.Message },
        ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
        CommandId = command.CommandId
      });
    }
  }

  /// <summary>
  /// Generates an API service class with HTTP methods and error handling.
  /// </summary>
  public Task<McpResponse> GenerateApiServiceAsync(McpCommand command)
  {
    _logger.LogInformation("Generating API service for command: {Command}", command.Command);

    var startTime = DateTime.UtcNow;

    try
    {
      var serviceName = GetParameterValue(command, "serviceName", "ApiService");
      var baseUrl = GetParameterValue(command, "baseUrl", "https://api.example.com");
      var endpoints = GetParameterValue(command, "endpoints", "GET:/users,POST:/users,PUT:/users/{id},DELETE:/users/{id}");
      var includeErrorHandling = GetParameterValue(command, "includeErrorHandling", "true").ToLower() == "true";
      var includeLogging = GetParameterValue(command, "includeLogging", "true").ToLower() == "true";

      var endpointList = ParseEndpoints(endpoints);

      var codeBuilder = new StringBuilder();

      // Imports
      codeBuilder.AppendLine("import 'dart:convert';");
      codeBuilder.AppendLine("import 'package:http/http.dart' as http;");
      if (includeLogging)
      {
        codeBuilder.AppendLine("import 'package:logger/logger.dart';");
      }
      codeBuilder.AppendLine();

      // Class declaration
      codeBuilder.AppendLine($"class {serviceName} {{");
      codeBuilder.AppendLine($"  static const String _baseUrl = '{baseUrl}';");

      if (includeLogging)
      {
        codeBuilder.AppendLine("  final Logger _logger = Logger();");
      }

      codeBuilder.AppendLine();

      // Generate methods for each endpoint
      foreach (var endpoint in endpointList)
      {
        var methodCode = GenerateApiMethod(endpoint, includeErrorHandling, includeLogging);
        codeBuilder.AppendLine(methodCode);
        codeBuilder.AppendLine();
      }

      // Error handling method
      if (includeErrorHandling)
      {
        codeBuilder.AppendLine(GenerateErrorHandlingMethod());
      }

      codeBuilder.AppendLine("}");

      var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

      return Task.FromResult(new McpResponse
      {
        Success = true,
        Purpose = $"API service '{serviceName}' generated successfully",
        CodeBlocks = new List<CodeBlock>
        {
          new CodeBlock
          {
            File = $"{serviceName.ToLower()}.dart",
            Content = codeBuilder.ToString(),
            Language = "dart",
            Operation = "create"
          }
        },
        ExecutionTimeMs = (long)executionTime,
        CommandId = command.CommandId,
        Notes = new List<string>
        {
          $"Generated service with {endpointList.Count} endpoints",
          $"Base URL: {baseUrl}",
          "Dependencies required: http, logger"
        },
        LearnNotes = new List<string>
        {
          "🧠 MCP Integration: API service follows REST principles",
          "📘 HTTP Client: Uses http package for network requests",
          "🛠️ Practical Usage: Place this file in your services folder"
        }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating API service");
      return Task.FromResult(new McpResponse
      {
        Success = false,
        Purpose = "Error generating API service",
        Errors = new List<string> { ex.Message },
        ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
        CommandId = command.CommandId
      });
    }
  }

  /// <summary>
  /// Generates a comprehensive theme module with colors, typography, and component themes.
  /// </summary>
  public Task<McpResponse> GenerateThemeModuleAsync(McpCommand command)
  {
    _logger.LogInformation("Generating theme module for command: {Command}", command.Command);

    var startTime = DateTime.UtcNow;

    try
    {
      var themeName = GetParameterValue(command, "themeName", "AppTheme");
      var primaryColor = GetParameterValue(command, "primaryColor", "0xFF2196F3");
      var isDarkTheme = GetParameterValue(command, "isDarkTheme", "false").ToLower() == "true";
      var includeMaterial3 = GetParameterValue(command, "includeMaterial3", "true").ToLower() == "true";
      var includeCustomColors = GetParameterValue(command, "includeCustomColors", "true").ToLower() == "true";

      var codeBuilder = new StringBuilder();

      // Imports
      codeBuilder.AppendLine("import 'package:flutter/material.dart';");
      if (includeMaterial3)
      {
        codeBuilder.AppendLine("import 'package:flutter/services.dart';");
      }
      codeBuilder.AppendLine();

      // Custom colors class
      if (includeCustomColors)
      {
        codeBuilder.AppendLine(GenerateCustomColorsClass(isDarkTheme));
        codeBuilder.AppendLine();
      }

      // Main theme class
      codeBuilder.AppendLine($"class {themeName} {{");

      // Color scheme
      codeBuilder.AppendLine(GenerateColorScheme(primaryColor, isDarkTheme, includeMaterial3));
      codeBuilder.AppendLine();

      // Theme data
      codeBuilder.AppendLine(GenerateThemeData(isDarkTheme, includeMaterial3));
      codeBuilder.AppendLine();

      // Text theme
      codeBuilder.AppendLine(GenerateTextTheme(isDarkTheme));
      codeBuilder.AppendLine();

      codeBuilder.AppendLine("}");

      var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

      return Task.FromResult(new McpResponse
      {
        Success = true,
        Purpose = $"Theme module '{themeName}' generated successfully",
        CodeBlocks = new List<CodeBlock>
        {
          new CodeBlock
          {
            File = $"{themeName.ToLower()}.dart",
            Content = codeBuilder.ToString(),
            Language = "dart",
            Operation = "create"
          }
        },
        ExecutionTimeMs = (long)executionTime,
        CommandId = command.CommandId,
        Notes = new List<string>
        {
          $"Generated theme for {(isDarkTheme ? "dark" : "light")} mode",
          $"Primary color: {primaryColor}",
          $"Material 3: {includeMaterial3}",
          "Ready to use with MaterialApp theme property"
        },
        LearnNotes = new List<string>
        {
          "🧠 MCP Integration: Theme follows Material Design guidelines",
          "📘 Flutter Theming: Comprehensive theme with colors, typography, and components",
          "🛠️ Practical Usage: Use with MaterialApp(theme: AppTheme.lightTheme)"
        }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating theme module");
      return Task.FromResult(new McpResponse
      {
        Success = false,
        Purpose = "Error generating theme module",
        Errors = new List<string> { ex.Message },
        ExecutionTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
        CommandId = command.CommandId
      });
    }
  }

  #region Helper Methods

  private string GetParameterValue(McpCommand command, string key, string defaultValue)
  {
    if (command.Params.HasValue)
    {
      try
      {
        if (command.Params.Value.TryGetProperty(key, out JsonElement element))
        {
          return element.GetString() ?? defaultValue;
        }
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Error parsing parameter {Key}", key);
      }
    }
    return defaultValue;
  }

  private List<(string Type, string Name)> ParseProperties(string properties)
  {
    var result = new List<(string Type, string Name)>();

    if (string.IsNullOrWhiteSpace(properties))
      return result;

    var props = properties.Split(',');
    foreach (var prop in props)
    {
      var parts = prop.Trim().Split(' ');
      if (parts.Length >= 2)
      {
        result.Add((parts[0], parts[1]));
      }
    }

    return result;
  }

  private List<string> GetRequiredDependencies(bool includeJsonAnnotation, bool includeEquatable)
  {
    var dependencies = new List<string>();

    if (includeJsonAnnotation)
    {
      dependencies.Add("json_annotation");
    }

    if (includeEquatable)
    {
      dependencies.Add("equatable");
    }

    return dependencies;
  }

  private string GenerateCubitStates(string featureName, List<string> states)
  {
    var builder = new StringBuilder();

    builder.AppendLine("import 'package:equatable/equatable.dart';");
    builder.AppendLine();
    builder.AppendLine($"abstract class {featureName}State extends Equatable {{");
    builder.AppendLine("  const " + featureName + "State();");
    builder.AppendLine();
    builder.AppendLine("  @override");
    builder.AppendLine("  List<Object?> get props => [];");
    builder.AppendLine("}");
    builder.AppendLine();

    foreach (var state in states)
    {
      var stateName = $"{featureName}{ToPascalCase(state)}State";
      builder.AppendLine($"class {stateName} extends {featureName}State {{");
      builder.AppendLine($"  const {stateName}();");
      builder.AppendLine("}");
      builder.AppendLine();
    }

    return builder.ToString();
  }

  private string GenerateCubitClass(string cubitName, string featureName, List<string> states)
  {
    var builder = new StringBuilder();

    builder.AppendLine("import 'package:flutter_bloc/flutter_bloc.dart';");
    builder.AppendLine($"import '{featureName.ToLower()}_state.dart';");
    builder.AppendLine();
    builder.AppendLine($"class {cubitName} extends Cubit<{featureName}State> {{");
    builder.AppendLine($"  {cubitName}() : super(const {featureName}{ToPascalCase(states.First())}State());");
    builder.AppendLine();

    // Generate methods for each state
    foreach (var state in states.Skip(1)) // Skip first state as it's initial
    {
      var methodName = $"emit{ToPascalCase(state)}";
      builder.AppendLine($"  void {methodName}() {{");
      builder.AppendLine($"    emit({featureName}{ToPascalCase(state)}State());");
      builder.AppendLine("  }");
      builder.AppendLine();
    }

    builder.AppendLine("}");

    return builder.ToString();
  }

  private List<(string Method, string Path, string MethodName)> ParseEndpoints(string endpoints)
  {
    var result = new List<(string Method, string Path, string MethodName)>();

    var endpointList = endpoints.Split(',');
    foreach (var endpoint in endpointList)
    {
      var parts = endpoint.Trim().Split(':');
      if (parts.Length == 2)
      {
        var method = parts[0].ToUpper();
        var path = parts[1];
        var methodName = GenerateMethodName(method, path);
        result.Add((method, path, methodName));
      }
    }

    return result;
  }

  private string GenerateMethodName(string httpMethod, string path)
  {
    var pathParts = path.Split('/').Where(p => !string.IsNullOrEmpty(p) && !p.Contains("{")).ToList();
    var resourceName = pathParts.LastOrDefault() ?? "resource";

    return httpMethod.ToLower() switch
    {
      "get" => path.Contains("{") ? $"get{ToPascalCase(resourceName)}ById" : $"get{ToPascalCase(resourceName)}",
      "post" => $"create{ToPascalCase(resourceName)}",
      "put" => $"update{ToPascalCase(resourceName)}",
      "delete" => $"delete{ToPascalCase(resourceName)}",
      _ => $"{httpMethod.ToLower()}{ToPascalCase(resourceName)}"
    };
  }

  private string GenerateApiMethod((string Method, string Path, string MethodName) endpoint, bool includeErrorHandling, bool includeLogging)
  {
    var builder = new StringBuilder();
    var hasParams = endpoint.Path.Contains("{");

    builder.AppendLine($"  Future<Map<String, dynamic>?> {endpoint.MethodName}({(hasParams ? "String id, " : "")}{{Map<String, dynamic>? data}}) async {{");

    if (includeLogging)
    {
      builder.AppendLine($"    _logger.i('Calling {endpoint.MethodName}');");
    }

    builder.AppendLine();
    builder.AppendLine("    try {");

    var url = hasParams ? endpoint.Path.Replace("{id}", "$id") : endpoint.Path;
    builder.AppendLine($"      final uri = Uri.parse('$_baseUrl{url}');");

    switch (endpoint.Method.ToUpper())
    {
      case "GET":
        builder.AppendLine("      final response = await http.get(uri);");
        break;
      case "POST":
        builder.AppendLine("      final response = await http.post(");
        builder.AppendLine("        uri,");
        builder.AppendLine("        headers: {'Content-Type': 'application/json'},");
        builder.AppendLine("        body: data != null ? jsonEncode(data) : null,");
        builder.AppendLine("      );");
        break;
      case "PUT":
        builder.AppendLine("      final response = await http.put(");
        builder.AppendLine("        uri,");
        builder.AppendLine("        headers: {'Content-Type': 'application/json'},");
        builder.AppendLine("        body: data != null ? jsonEncode(data) : null,");
        builder.AppendLine("      );");
        break;
      case "DELETE":
        builder.AppendLine("      final response = await http.delete(uri);");
        break;
    }

    builder.AppendLine();

    if (includeErrorHandling)
    {
      builder.AppendLine("      return _handleResponse(response);");
    }
    else
    {
      builder.AppendLine("      if (response.statusCode >= 200 && response.statusCode < 300) {");
      builder.AppendLine("        return jsonDecode(response.body);");
      builder.AppendLine("      }");
      builder.AppendLine("      return null;");
    }

    builder.AppendLine("    } catch (e) {");

    if (includeLogging)
    {
      builder.AppendLine($"      _logger.e('Error in {endpoint.MethodName}: $e');");
    }

    builder.AppendLine("      rethrow;");
    builder.AppendLine("    }");
    builder.AppendLine("  }");

    return builder.ToString();
  }

  private string GenerateErrorHandlingMethod()
  {
    return @"  Map<String, dynamic>? _handleResponse(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      return jsonDecode(response.body);
    } else {
      throw Exception('API Error: ${response.statusCode} - ${response.reasonPhrase}');
    }
  }";
  }

  private string GenerateCustomColorsClass(bool isDarkTheme)
  {
    return @"class AppColors {
  static const Color primary = Color(0xFF2196F3);
  static const Color secondary = Color(0xFFFF9800);
  static const Color error = Color(0xFFE53E3E);
  static const Color warning = Color(0xFFED8936);
  static const Color info = Color(0xFF3182CE);
  static const Color success = Color(0xFF38A169);
  
  // Surface colors
  static const Color surface = Color(0xFFFAFAFA);
  static const Color surfaceDark = Color(0xFF121212);
}";
  }

  private string GenerateColorScheme(string primaryColor, bool isDarkTheme, bool includeMaterial3)
  {
    var schemeType = isDarkTheme ? "dark" : "light";
    return $@"  static ColorScheme get colorScheme => ColorScheme.{schemeType}(
    primary: Color({primaryColor}),
    secondary: const Color(0xFFFF9800),
    error: const Color(0xFFE53E3E),
  );";
  }

  private string GenerateThemeData(bool isDarkTheme, bool includeMaterial3)
  {
    var brightness = isDarkTheme ? "Brightness.dark" : "Brightness.light";
    var useMaterial3 = includeMaterial3 ? "true" : "false";

    return $@"  static ThemeData get {(isDarkTheme ? "dark" : "light")}Theme => ThemeData(
    useMaterial3: {useMaterial3},
    colorScheme: colorScheme,
    textTheme: textTheme,
    appBarTheme: AppBarTheme(
      backgroundColor: colorScheme.surface,
      foregroundColor: colorScheme.onSurface,
      elevation: 0,
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: colorScheme.primary,
        foregroundColor: colorScheme.onPrimary,
      ),
    ),
  );";
  }

  private string GenerateTextTheme(bool isDarkTheme)
  {
    return @"  static TextTheme get textTheme => const TextTheme(
    displayLarge: TextStyle(fontSize: 32, fontWeight: FontWeight.bold),
    displayMedium: TextStyle(fontSize: 28, fontWeight: FontWeight.bold),
    displaySmall: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
    headlineLarge: TextStyle(fontSize: 22, fontWeight: FontWeight.w600),
    headlineMedium: TextStyle(fontSize: 20, fontWeight: FontWeight.w600),
    headlineSmall: TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
    titleLarge: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
    titleMedium: TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
    titleSmall: TextStyle(fontSize: 12, fontWeight: FontWeight.w500),
    bodyLarge: TextStyle(fontSize: 16, fontWeight: FontWeight.normal),
    bodyMedium: TextStyle(fontSize: 14, fontWeight: FontWeight.normal),
    bodySmall: TextStyle(fontSize: 12, fontWeight: FontWeight.normal),
  );";
  }

  private string ToPascalCase(string input)
  {
    if (string.IsNullOrEmpty(input))
      return input;

    return char.ToUpper(input[0]) + input.Substring(1);
  }

  #endregion
}