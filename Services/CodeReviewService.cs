using FlutterMcpServer.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlutterMcpServer.Services;

/// <summary>
/// Kod incelemesi ve refactor önerileri servisi
/// </summary>
public class CodeReviewService
{
  private readonly ILogger<CodeReviewService> _logger;

  public CodeReviewService(ILogger<CodeReviewService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Kod incelemesi ana metodu
  /// </summary>
  public async Task<McpResponse> ReviewCodeAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Kod incelemesi ve kalite analizi yapıldı"
    };

    try
    {
      _logger.LogInformation("Kod incelemesi başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? codeContent = null;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;
        if (paramsElement.TryGetProperty("code", out var codeElement))
        {
          codeContent = codeElement.GetString();
        }
      }

      if (string.IsNullOrWhiteSpace(codeContent))
      {
        response.Success = false;
        response.Errors.Add("Kod içeriği (code) parametresi gerekli");
        return response;
      }

      // Basit analiz yap
      var analysisResult = await AnalyzeCode(codeContent);
      response.Notes.AddRange(analysisResult);

      response.Success = true;
      _logger.LogInformation("Kod incelemesi tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Kod incelemesi hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"İnceleme hatası: {ex.Message}");
      return response;
    }
  }

  private async Task<List<string>> AnalyzeCode(string code)
  {
    var results = new List<string>();

    await Task.Run(() =>
    {
      results.Add("🔍 Kod analizi başlatıldı");

      // Flutter widget kontrolü
      if (code.Contains("StatefulWidget"))
      {
        results.Add("🎯 StatefulWidget tespit edildi");
      }

      // ListView kontrolü
      if (code.Contains("ListView(") && code.Contains("children:"))
      {
        results.Add("📊 ListView kullanılıyor - büyük listeler için ListView.builder önerilir");
      }

      // HTTP kontrolü
      if (code.Contains("http://"))
      {
        results.Add("🔒 HTTP tespit edildi - HTTPS kullanılması önerilir");
      }

      // Boş catch kontrolü
      if (Regex.IsMatch(code, @"catch\s*\([^)]*\)\s*\{\s*\}"))
      {
        results.Add("❌ Boş catch blokları tespit edildi");
      }

      // TODO kontrolü
      var todoCount = Regex.Matches(code, @"//\s*TODO", RegexOptions.IgnoreCase).Count;
      if (todoCount > 0)
      {
        results.Add($"📝 {todoCount} adet TODO yorumu tespit edildi");
      }

      results.Add("✅ Kod analizi tamamlandı");
    });

    return results;
  }
}
