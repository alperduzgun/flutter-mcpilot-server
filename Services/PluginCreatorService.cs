using FlutterMcpServer.Models;
using System.Text.Json;

namespace FlutterMcpServer.Services;

/// <summary>
/// Flutter plugin/feature şablonu üretim servisi
/// </summary>
public class PluginCreatorService
{
  private readonly ILogger<PluginCreatorService> _logger;

  public PluginCreatorService(ILogger<PluginCreatorService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Flutter plugin oluşturur
  /// </summary>
  public async Task<McpResponse> CreateFlutterPluginAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Flutter plugin şablonu oluşturuldu"
    };

    try
    {
      _logger.LogInformation("Plugin oluşturma başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? pluginName = null;
      string? pluginType = null;
      string? description = null;
      string? platform = null;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;

        if (paramsElement.TryGetProperty("pluginName", out var nameElement))
        {
          pluginName = nameElement.GetString();
        }

        if (paramsElement.TryGetProperty("pluginType", out var typeElement))
        {
          pluginType = typeElement.GetString();
        }

        if (paramsElement.TryGetProperty("description", out var descElement))
        {
          description = descElement.GetString();
        }

        if (paramsElement.TryGetProperty("platform", out var platformElement))
        {
          platform = platformElement.GetString();
        }
      }

      if (string.IsNullOrWhiteSpace(pluginName))
      {
        response.Success = false;
        response.Errors.Add("Plugin adı (pluginName) parametresi gerekli");
        return response;
      }

      // Plugin üret
      var creationResult = await CreatePlugin(pluginName, pluginType, description, platform);
      response.Notes.AddRange(creationResult.Messages);

      if (creationResult.Files.Count > 0)
      {
        response.LearnNotes.Add("🔌 Plugin şablonu oluşturuldu");
        response.LearnNotes.Add("📦 Federated plugin yapısı");
        response.LearnNotes.Add("🏗️ Platform-specific implementations");

        foreach (var file in creationResult.Files)
        {
          response.Notes.Add($"=== {file.FileName} ===");
          response.Notes.Add(file.Content);
          response.Notes.Add("");
        }
      }

      response.Success = true;
      _logger.LogInformation("Plugin oluşturma tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Plugin oluşturma hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Plugin oluşturma hatası: {ex.Message}");
      return response;
    }
  }

  private async Task<PluginCreationResult> CreatePlugin(string pluginName, string? pluginType, string? description, string? platform)
  {
    var result = new PluginCreationResult();

    await Task.Run(() =>
    {
      result.Messages.Add("🔌 Plugin şablonu analizi başlatıldı");

      // Plugin tipini belirle
      var finalPluginType = !string.IsNullOrEmpty(pluginType) ? pluginType : "federated";
      result.Messages.Add($"🏗️ Plugin tipi: {finalPluginType}");

      // Platform desteğini belirle
      var supportedPlatforms = DeterminePlatforms(platform);
      result.Messages.Add($"📱 Desteklenen platformlar: {string.Join(", ", supportedPlatforms)}");

      // Açıklama belirle
      var finalDescription = !string.IsNullOrEmpty(description) ? description : $"A new Flutter plugin for {pluginName}";
      result.Messages.Add($"📝 Açıklama: {finalDescription}");

      // Plugin dosyalarını üret
      result.Files.AddRange(GeneratePluginFiles(pluginName, finalPluginType, finalDescription, supportedPlatforms));

      result.Messages.Add($"📁 {result.Files.Count} dosya oluşturuldu");
      result.Messages.Add("✅ Plugin şablonu hazır");
    });

    return result;
  }

  private List<string> DeterminePlatforms(string? platform)
  {
    var platforms = new List<string>();

    if (string.IsNullOrEmpty(platform))
    {
      platforms.AddRange(new[] { "android", "ios" });
    }
    else
    {
      var specifiedPlatforms = platform.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(p => p.Trim().ToLower())
                                       .ToArray();
      platforms.AddRange(specifiedPlatforms);
    }

    return platforms;
  }

  private List<PluginFile> GeneratePluginFiles(string pluginName, string pluginType, string description, List<string> platforms)
  {
    var files = new List<PluginFile>();
    var snakeCaseName = ConvertToSnakeCase(pluginName);

    // pubspec.yaml
    files.Add(new PluginFile
    {
      FileName = "pubspec.yaml",
      Content = GeneratePubspecContent(snakeCaseName, description, platforms)
    });

    // Main library file
    files.Add(new PluginFile
    {
      FileName = $"lib/{snakeCaseName}.dart",
      Content = GenerateMainLibraryContent(snakeCaseName, pluginName)
    });

    // Platform interface
    files.Add(new PluginFile
    {
      FileName = $"lib/{snakeCaseName}_platform_interface.dart",
      Content = GeneratePlatformInterfaceContent(snakeCaseName, pluginName)
    });

    // Method channel implementation
    files.Add(new PluginFile
    {
      FileName = $"lib/{snakeCaseName}_method_channel.dart",
      Content = GenerateMethodChannelContent(snakeCaseName, pluginName)
    });

    // Android implementation
    if (platforms.Contains("android"))
    {
      files.Add(new PluginFile
      {
        FileName = $"android/src/main/kotlin/com/example/{snakeCaseName}/{ConvertToPascalCase(snakeCaseName)}Plugin.kt",
        Content = GenerateAndroidContent(snakeCaseName, pluginName)
      });
    }

    // iOS implementation
    if (platforms.Contains("ios"))
    {
      files.Add(new PluginFile
      {
        FileName = $"ios/Classes/{ConvertToPascalCase(snakeCaseName)}Plugin.swift",
        Content = GenerateIosContent(snakeCaseName, pluginName)
      });
    }

    // Example app
    files.Add(new PluginFile
    {
      FileName = "example/lib/main.dart",
      Content = GenerateExampleContent(snakeCaseName, pluginName)
    });

    return files;
  }

  private string GeneratePubspecContent(string pluginName, string description, List<string> platforms)
  {
    var platformsYaml = string.Join("\n", platforms.Select(p => $"      {p}:\n        package: {pluginName}_{p}\n        pluginClass: {ConvertToPascalCase(pluginName)}Plugin"));

    return $@"name: {pluginName}
description: {description}
version: 0.0.1
homepage:

environment:
  sdk: '>=3.1.0 <4.0.0'
  flutter: '>=3.3.0'

dependencies:
  flutter:
    sdk: flutter
  plugin_platform_interface: ^2.0.2

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^2.0.0

flutter:
  plugin:
    platforms:
{platformsYaml}";
  }

  private string GenerateMainLibraryContent(string pluginName, string displayName)
  {
    var className = ConvertToPascalCase(pluginName);

    return $@"library {pluginName};

import '{pluginName}_platform_interface.dart';

/// Main class for {displayName} plugin
class {className} {{
  /// Gets the platform version
  Future<String?> getPlatformVersion() {{
    return {className}Platform.instance.getPlatformVersion();
  }}

  /// Example method
  Future<String?> getSampleData() {{
    return {className}Platform.instance.getSampleData();
  }}
}}";
  }

  private string GeneratePlatformInterfaceContent(string pluginName, string displayName)
  {
    var className = ConvertToPascalCase(pluginName);

    return $@"import 'package:plugin_platform_interface/plugin_platform_interface.dart';

import '{pluginName}_method_channel.dart';

/// The interface that implementations of {pluginName} must implement.
abstract class {className}Platform extends PlatformInterface {{
  /// Constructs a {className}Platform.
  {className}Platform() : super(token: _token);

  static final Object _token = Object();

  static {className}Platform _instance = MethodChannel{className}();

  /// The default instance of [{className}Platform] to use.
  static {className}Platform get instance => _instance;

  /// Platform-specific implementations should set this with their own
  /// platform-specific class that extends [{className}Platform] when
  /// they register themselves.
  static set instance({className}Platform instance) {{
    PlatformInterface.verifyToken(instance, _token);
    _instance = instance;
  }}

  /// Gets the platform version
  Future<String?> getPlatformVersion() {{
    throw UnimplementedError('platformVersion() has not been implemented.');
  }}

  /// Gets sample data
  Future<String?> getSampleData() {{
    throw UnimplementedError('getSampleData() has not been implemented.');
  }}
}}";
  }

  private string GenerateMethodChannelContent(string pluginName, string displayName)
  {
    var className = ConvertToPascalCase(pluginName);

    return $@"import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';

import '{pluginName}_platform_interface.dart';

/// An implementation of [{className}Platform] that uses method channels.
class MethodChannel{className} extends {className}Platform {{
  /// The method channel used to interact with the native platform.
  @visibleForTesting
  final methodChannel = const MethodChannel('{pluginName}');

  @override
  Future<String?> getPlatformVersion() async {{
    final version = await methodChannel.invokeMethod<String>('getPlatformVersion');
    return version;
  }}

  @override
  Future<String?> getSampleData() async {{
    final data = await methodChannel.invokeMethod<String>('getSampleData');
    return data;
  }}
}}";
  }

  private string GenerateAndroidContent(string pluginName, string displayName)
  {
    var className = ConvertToPascalCase(pluginName);

    return $@"package com.example.{pluginName}

import androidx.annotation.NonNull

import io.flutter.embedding.engine.plugins.FlutterPlugin
import io.flutter.plugin.common.MethodCall
import io.flutter.plugin.common.MethodChannel
import io.flutter.plugin.common.MethodChannel.MethodCallHandler
import io.flutter.plugin.common.MethodChannel.Result

/** {className}Plugin */
class {className}Plugin: FlutterPlugin, MethodCallHandler {{
  private lateinit var channel : MethodChannel

  override fun onAttachedToEngine(@NonNull flutterPluginBinding: FlutterPlugin.FlutterPluginBinding) {{
    channel = MethodChannel(flutterPluginBinding.binaryMessenger, ""{pluginName}"")
    channel.setMethodCallHandler(this)
  }}

  override fun onMethodCall(@NonNull call: MethodCall, @NonNull result: Result) {{
    when (call.method) {{
      ""getPlatformVersion"" -> {{
        result.success(""Android ${{android.os.Build.VERSION.RELEASE}}"")
      }}
      ""getSampleData"" -> {{
        result.success(""Sample data from Android"")
      }}
      else -> {{
        result.notImplemented()
      }}
    }}
  }}

  override fun onDetachedFromEngine(@NonNull binding: FlutterPlugin.FlutterPluginBinding) {{
    channel.setMethodCallHandler(null)
  }}
}}";
  }

  private string GenerateIosContent(string pluginName, string displayName)
  {
    var className = ConvertToPascalCase(pluginName);

    return $@"import Flutter
import UIKit

public class {className}Plugin: NSObject, FlutterPlugin {{
  public static func register(with registrar: FlutterPluginRegistrar) {{
    let channel = FlutterMethodChannel(name: ""{pluginName}"", binaryMessenger: registrar.messenger())
    let instance = {className}Plugin()
    registrar.addMethodCallDelegate(instance, channel: channel)
  }}

  public func handle(_ call: FlutterMethodCall, result: @escaping FlutterResult) {{
    switch call.method {{
    case ""getPlatformVersion"":
      result(""iOS "" + UIDevice.current.systemVersion)
    case ""getSampleData"":
      result(""Sample data from iOS"")
    default:
      result(FlutterMethodNotImplemented)
    }}
  }}
}}";
  }

  private string GenerateExampleContent(string pluginName, string displayName)
  {
    var className = ConvertToPascalCase(pluginName);

    return $@"import 'package:flutter/material.dart';
import 'dart:async';

import 'package:flutter/services.dart';
import 'package:{pluginName}/{pluginName}.dart';

void main() {{
  runApp(const MyApp());
}}

class MyApp extends StatefulWidget {{
  const MyApp({{super.key}});

  @override
  State<MyApp> createState() => _MyAppState();
}}

class _MyAppState extends State<MyApp> {{
  String _platformVersion = 'Unknown';
  String _sampleData = 'Unknown';
  final _{pluginName}Plugin = {className}();

  @override
  void initState() {{
    super.initState();
    initPlatformState();
  }}

  Future<void> initPlatformState() async {{
    String platformVersion;
    String sampleData;
    
    try {{
      platformVersion = await _{pluginName}Plugin.getPlatformVersion() ?? 'Unknown platform version';
      sampleData = await _{pluginName}Plugin.getSampleData() ?? 'Unknown sample data';
    }} on PlatformException {{
      platformVersion = 'Failed to get platform version.';
      sampleData = 'Failed to get sample data.';
    }}

    if (!mounted) return;

    setState(() {{
      _platformVersion = platformVersion;
      _sampleData = sampleData;
    }});
  }}

  @override
  Widget build(BuildContext context) {{
    return MaterialApp(
      home: Scaffold(
        appBar: AppBar(
          title: const Text('{displayName} Example'),
        ),
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text('Running on: $_platformVersion\\n'),
              Text('Sample Data: $_sampleData\\n'),
              ElevatedButton(
                onPressed: initPlatformState,
                child: const Text('Refresh'),
              ),
            ],
          ),
        ),
      ),
    );
  }}
}}";
  }

  private string ConvertToSnakeCase(string input)
  {
    return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
  }

  private string ConvertToPascalCase(string input)
  {
    return string.Join("", input.Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1)));
  }

  private class PluginCreationResult
  {
    public List<string> Messages { get; set; } = new();
    public List<PluginFile> Files { get; set; } = new();
  }

  private class PluginFile
  {
    public string FileName { get; set; } = "";
    public string Content { get; set; } = "";
  }
}
