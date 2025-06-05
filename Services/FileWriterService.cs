using FlutterMcpServer.Models;
using System.Text.Json;
using System.Text;

namespace FlutterMcpServer.Services;

/// <summary>
/// Dosya yazım ve dosya sistemi işlemleri servisi
/// Flutter proje dosyalarını güvenli şekilde oluşturur ve yönetir
/// </summary>
public class FileWriterService
{
  private readonly ILogger<FileWriterService> _logger;

  public FileWriterService(ILogger<FileWriterService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Dosya yazım ana metodu
  /// </summary>
  /// <param name="command">MCP komutu</param>
  /// <returns>Dosya yazım sonucu</returns>
  public async Task<McpResponse> WriteFileAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Dosya yazım işlemi gerçekleştirildi"
    };

    try
    {
      _logger.LogInformation("Dosya yazım işlemi başlatıldı - CommandId: {CommandId}", command.CommandId);

      // Parametreleri parse et
      var parameters = ParseWriteFileParameters(command.Params);
      if (!parameters.IsValid)
      {
        response.Success = false;
        response.Errors.AddRange(parameters.ValidationErrors);
        return response;
      }

      // Güvenlik kontrolleri
      var securityCheck = ValidateSecurityConstraints(parameters.FilePath);
      if (!securityCheck.IsValid)
      {
        response.Success = false;
        response.Errors.AddRange(securityCheck.ValidationErrors);
        return response;
      }

      // Dry-run modunda mı?
      if (command.DryRun)
      {
        response = CreateDryRunResponse(command, parameters);
        return response;
      }

      // Dosya yazım işlemini gerçekleştir
      var writeResult = await WriteFileToSystem(parameters);
      if (!writeResult.Success)
      {
        response.Success = false;
        response.Errors.AddRange(writeResult.Errors);
        return response;
      }

      // Başarılı yanıt oluştur
      response.Success = true;
      response.Notes.Add($"📁 Dosya başarıyla oluşturuldu: {parameters.FilePath}");
      response.Notes.Add($"📝 İçerik boyutu: {parameters.Content.Length} karakter");
      response.Notes.Add($"🗂️ Dizin: {Path.GetDirectoryName(parameters.FilePath)}");

      // Kod bloğu olarak dosya içeriğini göster (kısa ise)
      if (parameters.Content.Length <= 1000)
      {
        response.CodeBlocks.Add(new CodeBlock
        {
          File = parameters.FilePath,
          Content = parameters.Content,
          Language = DetermineLanguage(parameters.FilePath),
          Operation = "create"
        });
      }

      // Öğretici notlar
      response.LearnNotes.AddRange(GenerateLearnNotes(parameters));

      // Karmaşıklık ve zaman tasarrufu
      response.ComplexityScore = CalculateComplexityScore(parameters);
      response.SavedEstTime = "2-5 dakika manuel dosya oluşturma süresi";

      _logger.LogInformation("Dosya yazım işlemi tamamlandı: {CommandId}", command.CommandId);
      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Dosya yazım hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Dosya yazım hatası: {ex.Message}");
      return response;
    }
  }

  #region Parameter Parsing

  /// <summary>
  /// Dosya yazım parametrelerini parse eder
  /// </summary>
  private WriteFileParameters ParseWriteFileParameters(JsonElement? paramsElement)
  {
    var parameters = new WriteFileParameters();

    if (!paramsElement.HasValue)
    {
      parameters.ValidationErrors.Add("Parametreler bulunamadı");
      return parameters;
    }

    var jsonParams = paramsElement.Value;

    // FilePath (zorunlu)
    if (jsonParams.TryGetProperty("filePath", out var filePathElement))
    {
      parameters.FilePath = filePathElement.GetString() ?? "";
    }
    else
    {
      parameters.ValidationErrors.Add("filePath parametresi zorunludur");
    }

    // Content (zorunlu)
    if (jsonParams.TryGetProperty("content", out var contentElement))
    {
      parameters.Content = contentElement.GetString() ?? "";
    }
    else
    {
      parameters.ValidationErrors.Add("content parametresi zorunludur");
    }

    // CreateDirectories (opsiyonel)
    if (jsonParams.TryGetProperty("createDirectories", out var createDirElement))
    {
      parameters.CreateDirectories = createDirElement.GetBoolean();
    }

    // Overwrite (opsiyonel)
    if (jsonParams.TryGetProperty("overwrite", out var overwriteElement))
    {
      parameters.Overwrite = overwriteElement.GetBoolean();
    }

    // Encoding (opsiyonel)
    if (jsonParams.TryGetProperty("encoding", out var encodingElement))
    {
      parameters.Encoding = encodingElement.GetString() ?? "utf-8";
    }

    parameters.IsValid = parameters.ValidationErrors.Count == 0;
    return parameters;
  }

  #endregion

  #region Security Validation

  /// <summary>
  /// Güvenlik kısıtlamalarını kontrol eder
  /// </summary>
  private ValidationResult ValidateSecurityConstraints(string filePath)
  {
    var result = new ValidationResult();

    if (string.IsNullOrWhiteSpace(filePath))
    {
      result.ValidationErrors.Add("Dosya yolu boş olamaz");
      return result;
    }

    // Path traversal saldırılarını önle
    if (filePath.Contains("..") || filePath.Contains("~"))
    {
      result.ValidationErrors.Add("Güvenlik ihlali: Path traversal tespit edildi");
      return result;
    }

    // Sistem dosyalarını koruma
    var systemPaths = new[] { "/etc/", "/usr/", "/var/", "/bin/", "/sbin/", "C:\\Windows\\", "C:\\Program Files\\" };
    foreach (var systemPath in systemPaths)
    {
      if (filePath.StartsWith(systemPath, StringComparison.OrdinalIgnoreCase))
      {
        result.ValidationErrors.Add("Güvenlik ihlali: Sistem klasörlerine yazım yasak");
        return result;
      }
    }

    // Uzantı kontrolü
    var extension = Path.GetExtension(filePath).ToLower();
    var allowedExtensions = new[] { ".dart", ".yaml", ".yml", ".json", ".md", ".txt", ".html", ".css", ".js", ".ts" };
    if (!allowedExtensions.Contains(extension))
    {
      result.ValidationErrors.Add($"Güvenlik ihlali: '{extension}' uzantısına izin verilmiyor");
      return result;
    }

    result.IsValid = true;
    return result;
  }

  #endregion

  #region File Operations

  /// <summary>
  /// Dosyayı sisteme yazar
  /// </summary>
  private async Task<WriteResult> WriteFileToSystem(WriteFileParameters parameters)
  {
    var result = new WriteResult();

    try
    {
      // Dizin kontrolü ve oluşturma
      var directory = Path.GetDirectoryName(parameters.FilePath);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
      {
        if (parameters.CreateDirectories)
        {
          Directory.CreateDirectory(directory);
          result.Messages.Add($"📁 Dizin oluşturuldu: {directory}");
        }
        else
        {
          result.Errors.Add($"Dizin bulunamadı: {directory}. createDirectories:true yapın.");
          return result;
        }
      }

      // Dosya var mı kontrolü
      if (File.Exists(parameters.FilePath) && !parameters.Overwrite)
      {
        result.Errors.Add($"Dosya zaten mevcut: {parameters.FilePath}. overwrite:true yapın.");
        return result;
      }

      // Encoding belirleme
      var encoding = parameters.Encoding.ToLower() switch
      {
        "utf-8" => Encoding.UTF8,
        "ascii" => Encoding.ASCII,
        "unicode" => Encoding.Unicode,
        _ => Encoding.UTF8
      };

      // Dosyayı yaz
      await File.WriteAllTextAsync(parameters.FilePath, parameters.Content, encoding);

      result.Success = true;
      result.Messages.Add($"✅ Dosya başarıyla yazıldı: {parameters.FilePath}");

      return result;
    }
    catch (Exception ex)
    {
      result.Errors.Add($"Dosya yazım hatası: {ex.Message}");
      return result;
    }
  }

  #endregion

  #region Helper Methods

  /// <summary>
  /// Dosya uzantısına göre dil belirler
  /// </summary>
  private string DetermineLanguage(string filePath)
  {
    var extension = Path.GetExtension(filePath).ToLower();
    return extension switch
    {
      ".dart" => "dart",
      ".yaml" or ".yml" => "yaml",
      ".json" => "json",
      ".md" => "markdown",
      ".html" => "html",
      ".css" => "css",
      ".js" => "javascript",
      ".ts" => "typescript",
      _ => "text"
    };
  }

  /// <summary>
  /// Karmaşıklık skorunu hesaplar
  /// </summary>
  private int CalculateComplexityScore(WriteFileParameters parameters)
  {
    int score = 10; // Base score

    if (parameters.Content.Length > 5000) score += 15;
    if (parameters.CreateDirectories) score += 10;
    if (!parameters.Overwrite && File.Exists(parameters.FilePath)) score += 20;
    if (parameters.FilePath.Contains('/') || parameters.FilePath.Contains('\\')) score += 5;

    return Math.Min(score, 100);
  }

  /// <summary>
  /// Öğretici notlar üretir
  /// </summary>
  private List<string> GenerateLearnNotes(WriteFileParameters parameters)
  {
    var notes = new List<string>
        {
            "🧠 Dosya yazım işlemleri, kod üretimi süreçlerinde kritik rol oynar.",
            "📁 Dizin yapısını otomatik oluşturmak, proje organizasyonunu kolaylaştırır.",
            "🔒 Path traversal saldırıları, güvenlik açıklarının en yaygın türlerinden biridir."
        };

    if (parameters.Encoding != "utf-8")
    {
      notes.Add($"📝 {parameters.Encoding} encoding kullanımı, özel karakter desteği sağlar.");
    }

    return notes;
  }

  #endregion

  #region Response Generators

  /// <summary>
  /// Dry-run yanıtı oluşturur
  /// </summary>
  private McpResponse CreateDryRunResponse(McpCommand command, WriteFileParameters parameters)
  {
    return new McpResponse
    {
      CommandId = command.CommandId,
      Success = true,
      Purpose = "Dosya yazım önizlemesi",
      Notes =
            {
                "🔍 DRY-RUN: Gerçek dosya yazımı yapılmadı",
                $"📁 Hedef dosya: {parameters.FilePath}",
                $"📝 İçerik boyutu: {parameters.Content.Length} karakter",
                $"🗂️ Dizin oluştur: {parameters.CreateDirectories}",
                $"🔄 Üzerine yaz: {parameters.Overwrite}"
            },
      LearnNotes =
            {
                "🧠 Dry-run modu, dosya yazım işlemlerini güvenle test etmenizi sağlar",
                "📘 Dosya güvenlik kontrolleri, zararlı yazılımları önlemek için gereklidir"
            }
    };
  }

  #endregion

  #region Data Models

  /// <summary>
  /// Dosya yazım parametreleri
  /// </summary>
  private class WriteFileParameters
  {
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool CreateDirectories { get; set; } = true;
    public bool Overwrite { get; set; } = false;
    public string Encoding { get; set; } = "utf-8";
    public bool IsValid { get; set; } = false;
    public List<string> ValidationErrors { get; set; } = new();
  }

  /// <summary>
  /// Dosya yazım sonucu
  /// </summary>
  private class WriteResult
  {
    public bool Success { get; set; } = false;
    public List<string> Messages { get; set; } = new();
    public List<string> Errors { get; set; } = new();
  }

  /// <summary>
  /// Validasyon sonucu
  /// </summary>
  private class ValidationResult
  {
    public bool IsValid { get; set; } = false;
    public List<string> ValidationErrors { get; set; } = new();
  }

  #endregion
}
