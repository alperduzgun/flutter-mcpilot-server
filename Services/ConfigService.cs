using System.Collections.Concurrent;
using System.Text.Json;
using FlutterMcpServer.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlutterMcpServer.Services;

/// <summary>
/// Proje konfigürasyonu ve ayar yönetimi servisi
/// pubspec.yaml, analysis_options.yaml ve proje ayarlarını okur ve yönetir
/// </summary>
public class ConfigService
{
  private readonly ILogger<ConfigService> _logger;
  private readonly IDeserializer _yamlDeserializer;
  private readonly ConcurrentDictionary<string, object> _configCache;

  public ConfigService(ILogger<ConfigService> logger)
  {
    _logger = logger;
    _yamlDeserializer = new DeserializerBuilder()
      .WithNamingConvention(UnderscoredNamingConvention.Instance)
      .IgnoreUnmatchedProperties()
      .Build();
    _configCache = new ConcurrentDictionary<string, object>();
  }

  /// <summary>
  /// Proje ayarlarını yükleme ana metodu
  /// </summary>
  /// <param name="command">MCP komutu</param>
  /// <returns>Proje konfigürasyon bilgileri</returns>
  public async Task<McpResponse> LoadProjectPreferencesAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Proje ayarları ve konfigürasyonu yüklendi"
    };

    try
    {
      _logger.LogInformation("Proje ayarları yükleme işlemi başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? projectPath = null;
      bool includeDevDependencies = false;
      bool includeLintRules = true;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;

        if (paramsElement.TryGetProperty("projectPath", out var pathElement))
        {
          projectPath = pathElement.GetString();
        }

        if (paramsElement.TryGetProperty("includeDevDependencies", out var devDepsElement))
        {
          includeDevDependencies = devDepsElement.GetBoolean();
        }

        if (paramsElement.TryGetProperty("includeLintRules", out var lintElement))
        {
          includeLintRules = lintElement.GetBoolean();
        }
      }

      // Proje konfigürasyonunu yükle
      var configResult = await LoadProjectConfiguration(projectPath, includeDevDependencies, includeLintRules);

      response.Notes.AddRange(configResult.Messages);
      response.LearnNotes.AddRange(configResult.Insights);

      if (configResult.ConfigData.Any())
      {
        response.Notes.Add("📋 Bulunan Konfigürasyonlar:");
        response.Notes.AddRange(configResult.ConfigData);
      }

      response.Success = true;
      _logger.LogInformation("Proje ayarları yükleme tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Proje ayarları yükleme hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Konfigürasyon yükleme hatası: {ex.Message}");
      return response;
    }
  }

  /// <summary>
  /// Proje konfigürasyonunu yükle ve analiz et
  /// </summary>
  private async Task<ConfigurationResult> LoadProjectConfiguration(string? projectPath, bool includeDevDependencies, bool includeLintRules)
  {
    var result = new ConfigurationResult();

    try
    {
      result.Messages.Add("⚙️ Proje konfigürasyonu yükleniyor");

      // Proje yolu kontrolü ve validation
      if (!string.IsNullOrEmpty(projectPath))
      {
        if (!Directory.Exists(projectPath))
        {
          result.Messages.Add("❌ Belirtilen proje dizini bulunamadı");
          return result;
        }

        // Path traversal saldırılarına karşı güvenlik kontrolü
        var fullPath = Path.GetFullPath(projectPath);
        if (!fullPath.StartsWith(Path.GetFullPath(Environment.CurrentDirectory)))
        {
          _logger.LogWarning("Güvenlik: Proje yolu current directory dışında: {ProjectPath}", projectPath);
        }

        result.Messages.Add($"📁 Proje dizini bulundu: {Path.GetFileName(projectPath)}");

        // Paralel dosya işlemleri
        var tasks = new List<Task<ConfigurationResult>>();

        // pubspec.yaml analizi
        var pubspecPath = Path.Combine(projectPath, "pubspec.yaml");
        if (File.Exists(pubspecPath))
        {
          tasks.Add(LoadPubspecConfigurationAsync(pubspecPath, includeDevDependencies));
        }

        // analysis_options.yaml analizi
        if (includeLintRules)
        {
          var analysisOptionsPath = Path.Combine(projectPath, "analysis_options.yaml");
          if (File.Exists(analysisOptionsPath))
          {
            tasks.Add(LoadLintConfigurationAsync(analysisOptionsPath));
          }
        }

        // .vscode/settings.json analizi
        var vscodeSettingsPath = Path.Combine(projectPath, ".vscode", "settings.json");
        if (File.Exists(vscodeSettingsPath))
        {
          tasks.Add(LoadVSCodeConfigurationAsync(vscodeSettingsPath));
        }

        // README.md analizi
        var readmePath = Path.Combine(projectPath, "README.md");
        if (File.Exists(readmePath))
        {
          tasks.Add(LoadReadmeConfigurationAsync(readmePath));
        }

        // Tüm dosya işlemlerini paralel olarak bekle
        var results = await Task.WhenAll(tasks);

        foreach (var configResult in results)
        {
          result.Messages.AddRange(configResult.Messages);
          result.ConfigData.AddRange(configResult.ConfigData);
          result.Insights.AddRange(configResult.Insights);
        }
      }
      else
      {
        // Genel Flutter konfigürasyon önerileri
        result.Messages.Add("💡 Genel Flutter konfigürasyon önerileri");
        result = GenerateDefaultConfigurationAdvice();
      }

      result.Messages.Add("✅ Konfigürasyon yükleme tamamlandı");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Konfigürasyon yükleme genel hatası");
      result.Messages.Add($"❌ Konfigürasyon yükleme hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// pubspec.yaml konfigürasyonu yükle (Async)
  /// </summary>
  private async Task<ConfigurationResult> LoadPubspecConfigurationAsync(string pubspecPath, bool includeDevDependencies)
  {
    var result = new ConfigurationResult();

    try
    {
      // Cache kontrolü
      var cacheKey = $"pubspec_{pubspecPath}_{includeDevDependencies}";
      if (_configCache.TryGetValue(cacheKey, out var cachedResult) && cachedResult is ConfigurationResult cached)
      {
        _logger.LogDebug("pubspec.yaml cache'den yüklendi: {Path}", pubspecPath);
        return cached;
      }

      var content = await File.ReadAllTextAsync(pubspecPath);
      result.Messages.Add("📦 pubspec.yaml konfigürasyonu yüklendi");

      // YAML parsing ile proper deserialization
      try
      {
        var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);

        // Proje adı
        if (yamlObject.TryGetValue("name", out var nameObj))
        {
          result.ConfigData.Add($"📝 Proje Adı: {nameObj}");
        }

        // Sürüm
        if (yamlObject.TryGetValue("version", out var versionObj))
        {
          result.ConfigData.Add($"🏷️ Versiyon: {versionObj}");
        }

        // Environment kontrolü
        if (yamlObject.TryGetValue("environment", out var envObj) && envObj is Dictionary<object, object> env)
        {
          if (env.TryGetValue("flutter", out var flutterVersionObj))
          {
            result.ConfigData.Add($"🎯 Flutter SDK: {flutterVersionObj}");
          }
          if (env.TryGetValue("sdk", out var sdkVersionObj))
          {
            result.ConfigData.Add($"🎯 Dart SDK: {sdkVersionObj}");
          }
        }

        // Dependencies analizi
        if (yamlObject.TryGetValue("dependencies", out var depsObj) && depsObj is Dictionary<object, object> deps)
        {
          result.ConfigData.Add($"📚 Ana bağımlılık sayısı: {deps.Count}");

          // Flutter bağımlılık kontrolü
          if (deps.ContainsKey("flutter"))
          {
            result.ConfigData.Add("✅ Flutter framework bağımlılığı mevcut");
          }

          // Popüler packages kontrolü
          var popularPackages = new[] { "cupertino_icons", "http", "provider", "bloc", "get_it", "shared_preferences" };
          var foundPackages = popularPackages.Where(pkg => deps.ContainsKey(pkg)).ToList();
          if (foundPackages.Any())
          {
            result.ConfigData.Add($"🔧 Popüler paketler: {string.Join(", ", foundPackages)}");
          }
        }

        // Dev Dependencies analizi
        if (includeDevDependencies && yamlObject.TryGetValue("dev_dependencies", out var devDepsObj) && devDepsObj is Dictionary<object, object> devDeps)
        {
          result.ConfigData.Add($"🔧 Dev bağımlılık sayısı: {devDeps.Count}");

          if (devDeps.ContainsKey("flutter_lints"))
          {
            result.ConfigData.Add("✅ flutter_lints mevcut");
          }
          if (devDeps.ContainsKey("flutter_test"))
          {
            result.ConfigData.Add("✅ flutter_test mevcut");
          }
        }
      }
      catch (Exception yamlEx)
      {
        _logger.LogWarning(yamlEx, "YAML parsing hatası, fallback string parsing kullanılıyor");
        // Fallback to string parsing
        LoadPubspecWithStringParsing(content, result, includeDevDependencies);
      }

      // Öneriler
      result.Insights.Add("🧠 pubspec.yaml'da bağımlılıkları düzenli olarak güncelleyin");
      result.Insights.Add("🧠 Kullanılmayan bağımlılıkları kaldırın");

      if (!content.Contains("flutter_lints"))
      {
        result.Insights.Add("⚠️ flutter_lints eklenmesi önerilir");
      }

      // Cache'e ekle
      _configCache.TryAdd(cacheKey, result);

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "pubspec.yaml okuma hatası: {Path}", pubspecPath);
      result.Messages.Add($"❌ pubspec.yaml okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// String parsing fallback metodu
  /// </summary>
  private void LoadPubspecWithStringParsing(string content, ConfigurationResult result, bool includeDevDependencies)
  {
    var lines = content.Split('\n');

    foreach (var line in lines)
    {
      var trimmedLine = line.Trim();

      if (trimmedLine.StartsWith("name:"))
      {
        var name = trimmedLine.Substring(5).Trim();
        result.ConfigData.Add($"📝 Proje Adı: {name}");
      }

      if (trimmedLine.StartsWith("version:"))
      {
        var version = trimmedLine.Substring(8).Trim();
        result.ConfigData.Add($"🏷️ Versiyon: {version}");
      }
    }

    var dependencySection = content.Contains("dependencies:");
    var devDependencySection = content.Contains("dev_dependencies:");

    if (dependencySection)
    {
      var depMatches = System.Text.RegularExpressions.Regex.Matches(content, @"^\s+([a-zA-Z_][a-zA-Z0-9_]*):.*$", System.Text.RegularExpressions.RegexOptions.Multiline);
      result.ConfigData.Add($"📚 Toplam bağımlılık: {depMatches.Count}");
    }

    if (devDependencySection && includeDevDependencies)
    {
      result.ConfigData.Add("🔧 Dev dependencies mevcut");
    }
  }

  /// <summary>
  /// analysis_options.yaml lint konfigürasyonu yükle (Async)
  /// </summary>
  private async Task<ConfigurationResult> LoadLintConfigurationAsync(string analysisOptionsPath)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = await File.ReadAllTextAsync(analysisOptionsPath);
      result.Messages.Add("🔍 analysis_options.yaml lint kuralları yüklendi");

      // YAML parsing ile proper deserialization
      try
      {
        var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);

        // Include kontrolü
        if (yamlObject.TryGetValue("include", out var includeObj))
        {
          if (includeObj.ToString()?.Contains("flutter_lints") == true)
          {
            result.ConfigData.Add("✅ flutter_lints paketi dahil edilmiş");
          }
        }

        // Analyzer kontrolü
        if (yamlObject.TryGetValue("analyzer", out var analyzerObj))
        {
          result.ConfigData.Add("🔧 Analyzer yapılandırması mevcut");
        }

        // Linter rules kontrolü
        if (yamlObject.TryGetValue("linter", out var linterObj) && linterObj is Dictionary<object, object> linter)
        {
          if (linter.TryGetValue("rules", out var rulesObj) && rulesObj is List<object> rules)
          {
            result.ConfigData.Add($"📋 Aktif lint kuralı: {rules.Count}");
          }
        }
      }
      catch (Exception yamlEx)
      {
        _logger.LogWarning(yamlEx, "YAML parsing hatası, fallback regex parsing kullanılıyor");

        // Fallback: Regex ile kural sayısını hesapla
        var ruleCount = System.Text.RegularExpressions.Regex.Matches(content, @"^\s+-\s+\w+", System.Text.RegularExpressions.RegexOptions.Multiline).Count;
        result.ConfigData.Add($"📋 Aktif lint kuralı: {ruleCount}");

        if (content.Contains("include: package:flutter_lints"))
        {
          result.ConfigData.Add("✅ flutter_lints paketi dahil edilmiş");
        }

        if (content.Contains("analyzer:"))
        {
          result.ConfigData.Add("🔧 Analyzer yapılandırması mevcut");
        }
      }

      result.Insights.Add("🧠 Lint kuralları kod kalitesini artırır");
      result.Insights.Add("🧠 CI/CD pipeline'da lint kontrolü zorunlu olmalı");

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "analysis_options.yaml okuma hatası: {Path}", analysisOptionsPath);
      result.Messages.Add($"❌ analysis_options.yaml okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// VS Code konfigürasyonu yükle (Async)
  /// </summary>
  private async Task<ConfigurationResult> LoadVSCodeConfigurationAsync(string vscodeSettingsPath)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = await File.ReadAllTextAsync(vscodeSettingsPath);
      result.Messages.Add("💻 VS Code ayarları yüklendi");

      // JSON parsing ile proper deserialization
      try
      {
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        // Dart/Flutter settings kontrolü
        var dartSettings = new List<string>();
        foreach (var property in root.EnumerateObject())
        {
          if (property.Name.StartsWith("dart."))
          {
            dartSettings.Add(property.Name);
          }
        }

        if (dartSettings.Any())
        {
          result.ConfigData.Add($"🎯 Dart/Flutter VS Code ayarları: {dartSettings.Count} adet");

          // Önemli ayarları kontrol et
          if (root.TryGetProperty("editor.formatOnSave", out var formatOnSave) && formatOnSave.GetBoolean())
          {
            result.ConfigData.Add("✨ Format on save aktif");
          }

          if (root.TryGetProperty("dart.enableSdkFormatter", out var enableSdkFormatter) && enableSdkFormatter.GetBoolean())
          {
            result.ConfigData.Add("🎯 Dart SDK formatter aktif");
          }

          if (root.TryGetProperty("dart.lineLength", out var lineLength))
          {
            result.ConfigData.Add($"📏 Line length: {lineLength.GetInt32()}");
          }
        }
      }
      catch (JsonException jsonEx)
      {
        _logger.LogWarning(jsonEx, "JSON parsing hatası, fallback string parsing kullanılıyor");

        // Fallback: String contains kontrolü
        if (content.Contains("dart."))
        {
          result.ConfigData.Add("🎯 Dart/Flutter VS Code ayarları mevcut");
        }

        if (content.Contains("formatOnSave"))
        {
          result.ConfigData.Add("✨ Format on save ayarı mevcut");
        }
      }

      result.Insights.Add("🧠 IDE ayarları ekip standartlarını korur");
      result.Insights.Add("🧠 Automated formatting, kod kalitesini artırır");

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "VS Code ayarları okuma hatası: {Path}", vscodeSettingsPath);
      result.Messages.Add($"❌ VS Code ayarları okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// README.md proje dokümantasyonu analizi (Async)
  /// </summary>
  private async Task<ConfigurationResult> LoadReadmeConfigurationAsync(string readmePath)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = await File.ReadAllTextAsync(readmePath);
      result.Messages.Add("📖 README.md dokümantasyonu bulundu");

      var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
      result.ConfigData.Add($"📝 README kelime sayısı: {wordCount}");

      if (content.ToLower().Contains("installation") || content.ToLower().Contains("kurulum"))
      {
        result.ConfigData.Add("📋 Kurulum talimatları mevcut");
      }

      if (content.ToLower().Contains("usage") || content.ToLower().Contains("kullanım"))
      {
        result.ConfigData.Add("📖 Kullanım rehberi mevcut");
      }

      result.Insights.Add("🧠 İyi dokümantasyon, proje kabulünü artırır");

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "README.md okuma hatası: {Path}", readmePath);
      result.Messages.Add($"❌ README.md okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// Varsayılan konfigürasyon önerileri
  /// </summary>
  private ConfigurationResult GenerateDefaultConfigurationAdvice()
  {
    var result = new ConfigurationResult();

    result.Messages.Add("💡 Genel Flutter proje konfigürasyon rehberi:");

    result.ConfigData.Add("📦 pubspec.yaml zorunlu dosyaları:");
    result.ConfigData.Add("  • name: Proje adı");
    result.ConfigData.Add("  • version: Sürüm bilgisi");
    result.ConfigData.Add("  • environment: Flutter SDK sürümü");
    result.ConfigData.Add("  • dependencies: Ana bağımlılıklar");
    result.ConfigData.Add("  • dev_dependencies: Geliştirme bağımlılıkları");

    result.ConfigData.Add("🔍 analysis_options.yaml önerilen içerik:");
    result.ConfigData.Add("  • include: package:flutter_lints/flutter.yaml");
    result.ConfigData.Add("  • analyzer/exclude: build/, **/*.g.dart");

    result.ConfigData.Add("💻 VS Code önerilen ayarları:");
    result.ConfigData.Add("  • dart.enableSdkFormatter: true");
    result.ConfigData.Add("  • editor.formatOnSave: true");
    result.ConfigData.Add("  • dart.lineLength: 80");

    result.Insights.Add("🧠 Standart konfigürasyon, ekip üretkenliğini artırır");
    result.Insights.Add("🧠 Automated formatting, kod review süresini azaltır");
    result.Insights.Add("🧠 Lint kuralları, potansiyel hataları erkenden yakalar");

    return result;
  }

  /// <summary>
  /// Konfigürasyon yükleme sonuç modeli
  /// </summary>
  public class ConfigurationResult
  {
    public List<string> Messages { get; set; } = new();
    public List<string> ConfigData { get; set; } = new();
    public List<string> Insights { get; set; } = new();
  }
}
