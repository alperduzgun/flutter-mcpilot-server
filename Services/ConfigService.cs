using FlutterMcpServer.Models;
using System.Text.Json;

namespace FlutterMcpServer.Services;

/// <summary>
/// Proje konfigürasyonu ve ayar yönetimi servisi
/// pubspec.yaml, analysis_options.yaml ve proje ayarlarını okur ve yönetir
/// </summary>
public class ConfigService
{
  private readonly ILogger<ConfigService> _logger;

  public ConfigService(ILogger<ConfigService> logger)
  {
    _logger = logger;
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

    await Task.Run(() =>
    {
      result.Messages.Add("⚙️ Proje konfigürasyonu yükleniyor");

      // Proje yolu kontrolü
      if (!string.IsNullOrEmpty(projectPath))
      {
        if (Directory.Exists(projectPath))
        {
          result.Messages.Add($"📁 Proje dizini bulundu: {Path.GetFileName(projectPath)}");
          
          // pubspec.yaml analizi
          var pubspecPath = Path.Combine(projectPath, "pubspec.yaml");
          if (File.Exists(pubspecPath))
          {
            var pubspecConfig = LoadPubspecConfiguration(pubspecPath, includeDevDependencies);
            result.Messages.AddRange(pubspecConfig.Messages);
            result.ConfigData.AddRange(pubspecConfig.ConfigData);
            result.Insights.AddRange(pubspecConfig.Insights);
          }

          // analysis_options.yaml analizi
          if (includeLintRules)
          {
            var analysisOptionsPath = Path.Combine(projectPath, "analysis_options.yaml");
            if (File.Exists(analysisOptionsPath))
            {
              var lintConfig = LoadLintConfiguration(analysisOptionsPath);
              result.Messages.AddRange(lintConfig.Messages);
              result.ConfigData.AddRange(lintConfig.ConfigData);
            }
          }

          // .vscode/settings.json analizi (varsa)
          var vscodeSettingsPath = Path.Combine(projectPath, ".vscode", "settings.json");
          if (File.Exists(vscodeSettingsPath))
          {
            var vscodeConfig = LoadVSCodeConfiguration(vscodeSettingsPath);
            result.Messages.AddRange(vscodeConfig.Messages);
            result.ConfigData.AddRange(vscodeConfig.ConfigData);
          }

          // README.md analizi (varsa)
          var readmePath = Path.Combine(projectPath, "README.md");
          if (File.Exists(readmePath))
          {
            var readmeConfig = LoadReadmeConfiguration(readmePath);
            result.Messages.AddRange(readmeConfig.Messages);
            result.Insights.AddRange(readmeConfig.Insights);
          }
        }
        else
        {
          result.Messages.Add("❌ Belirtilen proje dizini bulunamadı");
        }
      }
      else
      {
        // Genel Flutter konfigürasyon önerileri
        result.Messages.Add("💡 Genel Flutter konfigürasyon önerileri");
        result = GenerateDefaultConfigurationAdvice();
      }

      result.Messages.Add("✅ Konfigürasyon yükleme tamamlandı");
    });

    return result;
  }

  /// <summary>
  /// pubspec.yaml konfigürasyonu yükle
  /// </summary>
  private ConfigurationResult LoadPubspecConfiguration(string pubspecPath, bool includeDevDependencies)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = File.ReadAllText(pubspecPath);
      result.Messages.Add("📦 pubspec.yaml konfigürasyonu yüklendi");

      // Temel bilgileri çıkar
      var lines = content.Split('\n');
      
      foreach (var line in lines)
      {
        var trimmedLine = line.Trim();
        
        // Proje adı
        if (trimmedLine.StartsWith("name:"))
        {
          var name = trimmedLine.Substring(5).Trim();
          result.ConfigData.Add($"📝 Proje Adı: {name}");
        }
        
        // Sürüm
        if (trimmedLine.StartsWith("version:"))
        {
          var version = trimmedLine.Substring(8).Trim();
          result.ConfigData.Add($"🏷️ Versiyon: {version}");
        }
        
        // Flutter SDK
        if (trimmedLine.StartsWith("sdk:") && line.Contains("flutter"))
        {
          var sdkVersion = trimmedLine.Substring(4).Trim().Trim('"');
          result.ConfigData.Add($"🎯 Flutter SDK: {sdkVersion}");
        }
      }

      // Bağımlılık sayıları
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

      // Öneriler
      result.Insights.Add("🧠 pubspec.yaml'da bağımlılıkları düzenli olarak güncelleyin");
      result.Insights.Add("🧠 Kullanılmayan bağımlılıkları kaldırın");
      
      if (!content.Contains("flutter_lints"))
      {
        result.Insights.Add("⚠️ flutter_lints eklenmesi önerilir");
      }

    }
    catch (Exception ex)
    {
      result.Messages.Add($"❌ pubspec.yaml okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// analysis_options.yaml lint konfigürasyonu yükle
  /// </summary>
  private ConfigurationResult LoadLintConfiguration(string analysisOptionsPath)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = File.ReadAllText(analysisOptionsPath);
      result.Messages.Add("🔍 analysis_options.yaml lint kuralları yüklendi");

      // Lint kuralı sayısı
      var ruleCount = System.Text.RegularExpressions.Regex.Matches(content, @"^\s+-\s+\w+", System.Text.RegularExpressions.RegexOptions.Multiline).Count;
      result.ConfigData.Add($"📋 Aktif lint kuralı: {ruleCount}");

      // Önemli ayarlar
      if (content.Contains("include: package:flutter_lints"))
      {
        result.ConfigData.Add("✅ flutter_lints paketi dahil edilmiş");
      }

      if (content.Contains("analyzer:"))
      {
        result.ConfigData.Add("🔧 Analyzer yapılandırması mevcut");
      }

      result.Insights.Add("🧠 Lint kuralları kod kalitesini artırır");
      result.Insights.Add("🧠 CI/CD pipeline'da lint kontrolü zorunlu olmalı");

    }
    catch (Exception ex)
    {
      result.Messages.Add($"❌ analysis_options.yaml okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// VS Code konfigürasyonu yükle
  /// </summary>
  private ConfigurationResult LoadVSCodeConfiguration(string vscodeSettingsPath)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = File.ReadAllText(vscodeSettingsPath);
      result.Messages.Add("💻 VS Code ayarları yüklendi");

      if (content.Contains("dart."))
      {
        result.ConfigData.Add("🎯 Dart/Flutter VS Code ayarları mevcut");
      }

      if (content.Contains("formatOnSave"))
      {
        result.ConfigData.Add("✨ Format on save aktif");
      }

      result.Insights.Add("🧠 IDE ayarları ekip standartlarını korur");

    }
    catch (Exception ex)
    {
      result.Messages.Add($"❌ VS Code ayarları okuma hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// README.md proje dokümantasyonu analizi
  /// </summary>
  private ConfigurationResult LoadReadmeConfiguration(string readmePath)
  {
    var result = new ConfigurationResult();

    try
    {
      var content = File.ReadAllText(readmePath);
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
  private class ConfigurationResult
  {
    public List<string> Messages { get; set; } = new();
    public List<string> ConfigData { get; set; } = new();
    public List<string> Insights { get; set; } = new();
  }
}
