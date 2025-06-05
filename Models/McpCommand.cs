using System.Text.Json;

namespace FlutterMcpServer.Models;

/// <summary>
/// Model Context Protocol (MCP) komut yapısı
/// Flutter geliştirici yardımcısı için temel komut modeli
/// </summary>
public class McpCommand
{
    /// <summary>
    /// Komut adı (örn: "generateTestsForCubit", "reviewCode")
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Komut parametreleri (JSON formatında esnek yapı)
    /// </summary>
    public JsonElement? Params { get; set; }

    /// <summary>
    /// Dry-run modu: true ise sadece önizleme, dosya yazılmaz
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Komut kimliği (telemetri ve takip için)
    /// </summary>
    public string CommandId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Komut çalıştırılma zamanı
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// İsteğe bağlı kullanıcı tanımlayıcısı
    /// </summary>
    public string? UserId { get; set; }
}

/// <summary>
/// MCP komut yanıt yapısı
/// Standart çıktı formatı için kullanılır
/// </summary>
public class McpResponse
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// İşlemin amacı ve ne yaptığı
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Oluşturulan veya değiştirilen dosya yolları
    /// </summary>
    public List<string> Paths { get; set; } = new();

    /// <summary>
    /// Üretilen kod blokları
    /// </summary>
    public List<CodeBlock> CodeBlocks { get; set; } = new();

    /// <summary>
    /// Kullanıcı için notlar ve uyarılar
    /// </summary>
    public List<string> Notes { get; set; } = new();

    /// <summary>
    /// Öğretici açıklamalar
    /// AI destekli geliştirici eğitimi için
    /// </summary>
    public List<string> LearnNotes { get; set; } = new();

    /// <summary>
    /// Hata mesajları (varsa)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Komut kimliği (orijinal istekten)
    /// </summary>
    public string CommandId { get; set; } = string.Empty;

    /// <summary>
    /// İşlem süresi (milisaniye)
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// AI değerlendirme notları (dry-run modunda)
    /// </summary>
    public List<string>? AiReview { get; set; }

    /// <summary>
    /// Karmaşıklık skoru (varsa)
    /// </summary>
    public int? ComplexityScore { get; set; }

    /// <summary>
    /// Tahmini zaman tasarrufu
    /// </summary>
    public string? SavedEstTime { get; set; }
}

/// <summary>
/// Kod bloğu yapısı
/// Dosya içeriği ve meta bilgileri içerir
/// </summary>
public class CodeBlock
{
    /// <summary>
    /// Dosya yolu
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Dosya içeriği
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Kod dili (dart, json, yaml vs.)
    /// </summary>
    public string Language { get; set; } = "dart";

    /// <summary>
    /// İşlem türü (create, update, delete)
    /// </summary>
    public string Operation { get; set; } = "create";
}
