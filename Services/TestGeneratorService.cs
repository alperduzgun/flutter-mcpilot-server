using FlutterMcpServer.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlutterMcpServer.Services;

/// <summary>
/// Test üretimi ve kapsam genişletici servisi
/// </summary>
public class TestGeneratorService
{
  private readonly ILogger<TestGeneratorService> _logger;

  public TestGeneratorService(ILogger<TestGeneratorService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Cubit için test dosyaları üretir
  /// </summary>
  public async Task<McpResponse> GenerateTestsForCubitAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Cubit test dosyaları üretildi"
    };

    try
    {
      _logger.LogInformation("Test üretimi başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? cubitFilePath = null;
      string? cubitCode = null;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;

        if (paramsElement.TryGetProperty("cubitFile", out var fileElement))
        {
          cubitFilePath = fileElement.GetString();
        }

        if (paramsElement.TryGetProperty("cubitCode", out var codeElement))
        {
          cubitCode = codeElement.GetString();
        }
      }

      if (string.IsNullOrWhiteSpace(cubitCode) && string.IsNullOrWhiteSpace(cubitFilePath))
      {
        response.Success = false;
        response.Errors.Add("Cubit kodu (cubitCode) veya dosya yolu (cubitFile) parametresi gerekli");
        return response;
      }

      // Dosya yolu varsa kodu oku
      if (!string.IsNullOrWhiteSpace(cubitFilePath) && File.Exists(cubitFilePath))
      {
        cubitCode = await File.ReadAllTextAsync(cubitFilePath);
      }

      if (string.IsNullOrWhiteSpace(cubitCode))
      {
        response.Success = false;
        response.Errors.Add("Cubit kodu okunamadı");
        return response;
      }

      // Test üret
      var testResult = await GenerateTestCode(cubitCode, cubitFilePath);
      response.Notes.AddRange(testResult.Messages);

      if (!string.IsNullOrEmpty(testResult.TestCode))
      {
        response.LearnNotes.Add("🧪 Test kodu üretildi");
        response.LearnNotes.Add($"📁 Test dosyası: {testResult.TestFileName}");
        response.Notes.Add("Generated Test Code:");
        response.Notes.Add(testResult.TestCode);
      }

      response.Success = true;
      _logger.LogInformation("Test üretimi tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Test üretimi hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Test üretimi hatası: {ex.Message}");
      return response;
    }
  }

  private async Task<TestGenerationResult> GenerateTestCode(string cubitCode, string? filePath)
  {
    var result = new TestGenerationResult();

    await Task.Run(() =>
    {
      result.Messages.Add("🔧 Cubit analizi başlatıldı");

      // Cubit sınıf adını bul
      var cubitClassName = ExtractCubitClassName(cubitCode);
      if (string.IsNullOrEmpty(cubitClassName))
      {
        result.Messages.Add("❌ Cubit sınıfı bulunamadı");
        return;
      }

      result.Messages.Add($"🎯 Cubit sınıfı tespit edildi: {cubitClassName}");

      // State sınıflarını bul
      var stateClasses = ExtractStateClasses(cubitCode);
      result.Messages.Add($"📊 {stateClasses.Count} state sınıfı tespit edildi");

      // Metotları bul
      var methods = ExtractMethods(cubitCode);
      result.Messages.Add($"⚙️ {methods.Count} metot tespit edildi");

      // Test kodu üret
      result.TestCode = GenerateTestFileContent(cubitClassName, stateClasses, methods);
      result.TestFileName = GenerateTestFileName(cubitClassName, filePath);

      result.Messages.Add("✅ Test kodu üretimi tamamlandı");
    });

    return result;
  }

  private string ExtractCubitClassName(string code)
  {
    var match = Regex.Match(code, @"class\s+(\w+)\s+extends\s+Cubit", RegexOptions.IgnoreCase);
    return match.Success ? match.Groups[1].Value : "";
  }

  private List<string> ExtractStateClasses(string code)
  {
    var states = new List<string>();
    var matches = Regex.Matches(code, @"class\s+(\w+State)\s+", RegexOptions.IgnoreCase);

    foreach (Match match in matches)
    {
      states.Add(match.Groups[1].Value);
    }

    return states;
  }

  private List<string> ExtractMethods(string code)
  {
    var methods = new List<string>();
    var matches = Regex.Matches(code, @"(?:void|Future<[^>]*>|[A-Za-z]+)\s+(\w+)\s*\([^)]*\)\s*(?:async\s*)?{", RegexOptions.IgnoreCase);

    foreach (Match match in matches)
    {
      var methodName = match.Groups[1].Value;
      if (methodName != "initState" && methodName != "dispose" && !methodName.StartsWith("_"))
      {
        methods.Add(methodName);
      }
    }

    return methods;
  }

  private string GenerateTestFileName(string cubitClassName, string? originalFilePath)
  {
    if (!string.IsNullOrEmpty(originalFilePath))
    {
      var directory = Path.GetDirectoryName(originalFilePath) ?? "";
      var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
      return Path.Combine(directory, "test", $"{fileName}_test.dart");
    }

    return $"{cubitClassName.ToLower()}_test.dart";
  }

  private string GenerateTestFileContent(string cubitClassName, List<string> stateClasses, List<string> methods)
  {
    var testCode = $@"import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mocktail/mocktail.dart';

// Import your cubit file here
// import '../path/to/{cubitClassName.ToLower()}.dart';

void main() {{
  group('{cubitClassName} Tests', () {{
    late {cubitClassName} {cubitClassName.ToLower()};

    setUp(() {{
      {cubitClassName.ToLower()} = {cubitClassName}();
    }});

    tearDown(() {{
      {cubitClassName.ToLower()}.close();
    }});

    test('initial state should be correct', () {{
      // TODO: Replace with actual initial state
      expect({cubitClassName.ToLower()}.state, isA<InitialState>());
    }});
";

    // Her state için test ekle
    foreach (var state in stateClasses)
    {
      testCode += $@"
    blocTest<{cubitClassName}, dynamic>(
      'should emit {state} when appropriate',
      build: () => {cubitClassName.ToLower()},
      act: (cubit) {{
        // TODO: Call the method that should emit {state}
      }},
      expect: () => [isA<{state}>()],
    );
";
    }

    // Her metot için test ekle
    foreach (var method in methods)
    {
      testCode += $@"
    group('{method} tests', () {{
      blocTest<{cubitClassName}, dynamic>(
        'should work correctly when {method} is called',
        build: () => {cubitClassName.ToLower()},
        act: (cubit) => cubit.{method}(),
        expect: () => [
          // TODO: Add expected states
        ],
      );
    }});
";
    }

    testCode += @"
  });
}";

    return testCode;
  }

  private class TestGenerationResult
  {
    public List<string> Messages { get; set; } = new();
    public string TestCode { get; set; } = "";
    public string TestFileName { get; set; } = "";
  }
}
