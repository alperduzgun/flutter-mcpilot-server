using FlutterMcpServer.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlutterMcpServer.Services;

/// <summary>
/// Flutter projesi karmaşıklık analizi ve mimari değerlendirme servisi
/// Proje yapısını analiz eder, refactor önerileri sunar ve teknik borç tespit eder
/// </summary>
public class ProjectAnalyzer
{
  private readonly ILogger<ProjectAnalyzer> _logger;

  public ProjectAnalyzer(ILogger<ProjectAnalyzer> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Proje karmaşıklık analizi ana metodu
  /// </summary>
  /// <param name="command">MCP komutu</param>
  /// <returns>Karmaşıklık analizi ve refactor önerileri</returns>
  public async Task<McpResponse> AnalyzeFeatureComplexityAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Proje karmaşıklığı analiz edildi ve refactor önerileri sunuldu"
    };

    try
    {
      _logger.LogInformation("Proje karmaşıklık analizi başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? projectPath = null;
      string? featureName = null;
      bool includeTests = false;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;

        if (paramsElement.TryGetProperty("projectPath", out var pathElement))
        {
          projectPath = pathElement.GetString();
        }

        if (paramsElement.TryGetProperty("featureName", out var featureElement))
        {
          featureName = featureElement.GetString();
        }

        if (paramsElement.TryGetProperty("includeTests", out var testsElement))
        {
          includeTests = testsElement.GetBoolean();
        }
      }

      // Proje analizi yap
      var analysisResult = await PerformComplexityAnalysis(projectPath, featureName, includeTests);

      response.Notes.AddRange(analysisResult.Messages);
      response.LearnNotes.AddRange(analysisResult.Insights);

      if (analysisResult.Recommendations.Any())
      {
        response.Notes.Add("🔧 Refactor Önerileri:");
        response.Notes.AddRange(analysisResult.Recommendations);
      }

      response.Success = true;
      _logger.LogInformation("Proje karmaşıklık analizi tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Proje analizi hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Analiz hatası: {ex.Message}");
      return response;
    }
  }

  /// <summary>
  /// Karmaşıklık analizi gerçekleştir
  /// </summary>
  private async Task<ComplexityAnalysisResult> PerformComplexityAnalysis(string? projectPath, string? featureName, bool includeTests)
  {
    var result = new ComplexityAnalysisResult();

    await Task.Run(() =>
    {
      result.Messages.Add("🔍 Proje karmaşıklık analizi başlatıldı");

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
            var pubspecAnalysis = AnalyzePubspecComplexity(pubspecPath);
            result.Messages.AddRange(pubspecAnalysis.Messages);
            result.Recommendations.AddRange(pubspecAnalysis.Recommendations);
          }

          // lib dizini analizi
          var libPath = Path.Combine(projectPath, "lib");
          if (Directory.Exists(libPath))
          {
            var codeAnalysis = AnalyzeCodeComplexity(libPath, featureName);
            result.Messages.AddRange(codeAnalysis.Messages);
            result.Recommendations.AddRange(codeAnalysis.Recommendations);
            result.Insights.AddRange(codeAnalysis.Insights);
          }

          // Test analizi (eğer isteniyorsa)
          if (includeTests)
          {
            var testPath = Path.Combine(projectPath, "test");
            if (Directory.Exists(testPath))
            {
              var testAnalysis = AnalyzeTestComplexity(testPath);
              result.Messages.AddRange(testAnalysis.Messages);
              result.Recommendations.AddRange(testAnalysis.Recommendations);
            }
          }
        }
        else
        {
          result.Messages.Add("❌ Belirtilen proje dizini bulunamadı");
        }
      }
      else
      {
        // Genel analiz örnekleri (proje yolu verilmediğinde)
        result.Messages.Add("📊 Genel karmaşıklık analizi kuralları uygulandı");
        result = GenerateGenericComplexityAnalysis(featureName);
      }

      result.Messages.Add("✅ Karmaşıklık analizi tamamlandı");
    });

    return result;
  }

  /// <summary>
  /// pubspec.yaml bağımlılık karmaşıklığı analizi
  /// </summary>
  private ComplexityAnalysisResult AnalyzePubspecComplexity(string pubspecPath)
  {
    var result = new ComplexityAnalysisResult();

    try
    {
      var content = File.ReadAllText(pubspecPath);
      result.Messages.Add("📦 pubspec.yaml bağımlılık analizi yapıldı");

      // Bağımlılık sayısı
      var dependencyMatches = Regex.Matches(content, @"^\s+([a-zA-Z_][a-zA-Z0-9_]*):.*$", RegexOptions.Multiline);
      var dependencyCount = dependencyMatches.Count;

      result.Messages.Add($"📚 Toplam bağımlılık sayısı: {dependencyCount}");

      if (dependencyCount > 20)
      {
        result.Recommendations.Add("⚠️ Çok fazla bağımlılık tespit edildi. Kullanılmayan paketleri kaldırın");
      }
      else if (dependencyCount > 10)
      {
        result.Recommendations.Add("📊 Orta seviye bağımlılık. Düzenli olarak gözden geçirin");
      }
      else
      {
        result.Messages.Add("✅ Bağımlılık sayısı optimal seviyede");
      }

      // Kritik paketleri tespit et
      if (content.Contains("state_management") || content.Contains("bloc") || content.Contains("provider") || content.Contains("riverpod"))
      {
        result.Messages.Add("🏗️ State management paketi tespit edildi");
        result.Insights.Add("🧠 State management kullanımı, uygulama mimarisini güçlendirir");
      }

      if (content.Contains("dio") || content.Contains("http"))
      {
        result.Messages.Add("🌐 HTTP client paketi tespit edildi");
        result.Insights.Add("🧠 API çağrıları için error handling ve retry logic eklemeyi unutmayın");
      }

      if (content.Contains("hive") || content.Contains("sqflite") || content.Contains("shared_preferences"))
      {
        result.Messages.Add("💾 Veri saklama paketi tespit edildi");
        result.Insights.Add("🧠 Veri katmanı abstraksiyonu, test edilebilirliği artırır");
      }
    }
    catch (Exception ex)
    {
      result.Messages.Add($"❌ pubspec.yaml analiz hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// Kod dosyalarının karmaşıklık analizi
  /// </summary>
  private ComplexityAnalysisResult AnalyzeCodeComplexity(string libPath, string? featureName)
  {
    var result = new ComplexityAnalysisResult();

    try
    {
      var dartFiles = Directory.GetFiles(libPath, "*.dart", SearchOption.AllDirectories);
      result.Messages.Add($"🎯 {dartFiles.Length} Dart dosyası analiz edildi");

      var totalLines = 0;
      var classCount = 0;
      var widgetCount = 0;
      var cubitsCount = 0;
      var largeFiles = new List<string>();

      foreach (var file in dartFiles)
      {
        var content = File.ReadAllText(file);
        var lines = content.Split('\n').Length;
        totalLines += lines;

        // Sınıf analizi
        var classMatches = Regex.Matches(content, @"class\s+(\w+)", RegexOptions.IgnoreCase);
        classCount += classMatches.Count;

        // Widget analizi
        if (content.Contains("StatefulWidget") || content.Contains("StatelessWidget"))
        {
          widgetCount++;
        }

        // Cubit/Bloc analizi
        if (content.Contains("Cubit") || content.Contains("Bloc"))
        {
          cubitsCount++;
        }

        // Büyük dosyalar
        if (lines > 200)
        {
          largeFiles.Add($"{Path.GetFileName(file)} ({lines} satır)");
        }
      }

      result.Messages.Add($"📊 Proje istatistikleri:");
      result.Messages.Add($"  • Toplam kod satırı: {totalLines:N0}");
      result.Messages.Add($"  • Sınıf sayısı: {classCount}");
      result.Messages.Add($"  • Widget sayısı: {widgetCount}");
      result.Messages.Add($"  • Cubit/Bloc sayısı: {cubitsCount}");

      // Karmaşıklık değerlendirmesi
      if (totalLines > 10000)
      {
        result.Recommendations.Add("📏 Büyük proje tespit edildi. Modülarize etmeyi düşünün");
      }

      if (largeFiles.Count > 0)
      {
        result.Recommendations.Add($"📄 {largeFiles.Count} büyük dosya tespit edildi:");
        result.Recommendations.AddRange(largeFiles.Take(5).Select(f => $"  • {f}"));
        if (largeFiles.Count > 5)
        {
          result.Recommendations.Add($"  ... ve {largeFiles.Count - 5} dosya daha");
        }
      }

      // Feature odaklı analiz
      if (!string.IsNullOrEmpty(featureName))
      {
        var featureFiles = dartFiles.Where(f => f.ToLower().Contains(featureName.ToLower())).ToArray();
        if (featureFiles.Length > 0)
        {
          result.Messages.Add($"🎯 '{featureName}' feature'u için {featureFiles.Length} dosya bulundu");

          var featureLines = 0;
          foreach (var file in featureFiles)
          {
            var content = File.ReadAllText(file);
            featureLines += content.Split('\n').Length;
          }

          result.Messages.Add($"📊 Feature kod satırı: {featureLines}");

          if (featureLines > 1000)
          {
            result.Recommendations.Add($"⚠️ '{featureName}' feature'u büyük. Alt modüllere bölmeyi düşünün");
          }
        }
      }

      // Mimari önerileri
      result.Insights.Add("🧠 Clean Architecture kullanımı, büyük projelerde maintainability sağlar");
      result.Insights.Add("🧠 Feature-first yaklaşımı, ekip collaboration'ını kolaylaştırır");
      result.Insights.Add("🧠 Widget testleri, UI stabilitesi için kritiktir");

    }
    catch (Exception ex)
    {
      result.Messages.Add($"❌ Kod analiz hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// Test dosyalarının karmaşıklık analizi
  /// </summary>
  private ComplexityAnalysisResult AnalyzeTestComplexity(string testPath)
  {
    var result = new ComplexityAnalysisResult();

    try
    {
      var testFiles = Directory.GetFiles(testPath, "*.dart", SearchOption.AllDirectories);
      result.Messages.Add($"🧪 {testFiles.Length} test dosyası analiz edildi");

      var totalTestLines = 0;
      var testCount = 0;

      foreach (var file in testFiles)
      {
        var content = File.ReadAllText(file);
        totalTestLines += content.Split('\n').Length;

        // Test sayısı
        var testMatches = Regex.Matches(content, @"test\s*\(", RegexOptions.IgnoreCase);
        testCount += testMatches.Count;
      }

      result.Messages.Add($"📊 Test istatistikleri:");
      result.Messages.Add($"  • Test kod satırı: {totalTestLines:N0}");
      result.Messages.Add($"  • Test sayısı: {testCount}");

      // Test kapsamı önerisi
      if (testCount < 10)
      {
        result.Recommendations.Add("🧪 Test kapsamı düşük. Daha fazla test yazın");
      }
      else if (testCount > 50)
      {
        result.Messages.Add("✅ İyi test kapsamı tespit edildi");
      }

      result.Insights.Add("🧠 Test-driven development, kod kalitesini artırır");
    }
    catch (Exception ex)
    {
      result.Messages.Add($"❌ Test analiz hatası: {ex.Message}");
    }

    return result;
  }

  /// <summary>
  /// Genel karmaşıklık analizi (proje yolu olmadığında)
  /// </summary>
  private ComplexityAnalysisResult GenerateGenericComplexityAnalysis(string? featureName)
  {
    var result = new ComplexityAnalysisResult();

    result.Messages.Add("💡 Genel karmaşıklık analizi kuralları:");
    result.Messages.Add("  • 200 satırdan uzun dosyalar refactor edilmeli");
    result.Messages.Add("  • 20'den fazla bağımlılık proje karmaşıklığını artırır");
    result.Messages.Add("  • Circular dependency'ler mimari problemine işaret eder");

    result.Recommendations.Add("🏗️ Clean Architecture pattern'i uygulayın");
    result.Recommendations.Add("📦 Feature-first folder structure kullanın");
    result.Recommendations.Add("🧪 En az %70 test coverage hedefleyin");
    result.Recommendations.Add("🔧 Static analysis tools (analyzer, lint) kullanın");

    result.Insights.Add("🧠 SOLID prensiplerine uyum, uzun vadeli maintainability sağlar");
    result.Insights.Add("🧠 Dependency injection, testability ve loose coupling getirir");
    result.Insights.Add("🧠 State management pattern'i, UI complexity'yi azaltır");

    if (!string.IsNullOrEmpty(featureName))
    {
      result.Messages.Add($"🎯 '{featureName}' feature'u için özel öneriler:");
      result.Recommendations.Add($"📁 {featureName}/domain, {featureName}/data, {featureName}/presentation klasörleri oluşturun");
      result.Recommendations.Add($"🔧 {featureName} için ayrı Cubit/Bloc oluşturun");
    }

    return result;
  }

  /// <summary>
  /// Karmaşıklık analizi sonuç modeli
  /// </summary>
  private class ComplexityAnalysisResult
  {
    public List<string> Messages { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<string> Insights { get; set; } = new();
  }
}
