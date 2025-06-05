using FlutterMcpServer.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlutterMcpServer.Services
{

/// <summary>
/// Flutter SDK sürüm kontrolü ve uyumluluk yönetimi servisi
/// Sistemde yüklü Flutter sürümünü kontrol eder ve uyumluluk önerileri sunar
/// </summary>
public class FlutterVersionChecker
{
    private readonly ILogger<FlutterVersionChecker> _logger;

    public FlutterVersionChecker(ILogger<FlutterVersionChecker> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Flutter sürüm kontrolü ana metodu
    /// </summary>
    /// <param name="command">MCP komutu</param>
    /// <returns>Flutter sürüm bilgileri ve öneriler</returns>
    public async Task<McpResponse> CheckFlutterVersionAsync(McpCommand command)
    {
        var response = new McpResponse
        {
            CommandId = command.CommandId,
            Purpose = "Flutter SDK sürüm kontrolü yapıldı"
        };

        try
        {
            _logger.LogInformation("Flutter sürüm kontrolü başlatıldı - CommandId: {CommandId}", command.CommandId);

            // Flutter yüklü mü kontrol et
            var isFlutterInstalled = await IsFlutterInstalledAsync();
            if (!isFlutterInstalled)
            {
                response.Success = false;
                response.Errors.Add("Flutter SDK sistemde bulunamadı. Lütfen Flutter'ı yükleyin.");
                response.Notes.Add("📥 Flutter SDK'yı https://flutter.dev/docs/get-started/install adresinden indirebilirsiniz.");
                return response;
            }

            // Flutter sürüm bilgilerini al
            var versionInfo = await GetFlutterVersionInfoAsync();
            if (versionInfo == null)
            {
                response.Success = false;
                response.Errors.Add("Flutter sürüm bilgileri alınamadı.");
                return response;
            }

            // Dry-run modunda mı?
            if (command.DryRun)
            {
                response = CreateDryRunResponse(command, versionInfo);
                response.AiReview = GenerateAiReview(versionInfo);
                return response;
            }

            // Sürüm analizi ve önerileri
            response.Success = true;
            response.CodeBlocks.Add(new CodeBlock
            {
                File = "flutter_version_info.json",
                Content = JsonSerializer.Serialize(versionInfo, new JsonSerializerOptions { WriteIndented = true }),
                Language = "json",
                Operation = "create"
            });

            // Notlar ve öğretici açıklamalar
            response.Notes.AddRange(GenerateVersionNotes(versionInfo));
            response.LearnNotes.AddRange(GenerateLearnNotes(versionInfo));

            // Karmaşıklık ve zaman tasarrufu
            response.ComplexityScore = CalculateComplexityScore(versionInfo);
            response.SavedEstTime = "5-10 dakika manuel kontrol süresi";

            _logger.LogInformation("Flutter sürüm kontrolü tamamlandı - Sürüm: {Version}", versionInfo.FlutterVersion);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutter sürüm kontrolü hatası - CommandId: {CommandId}", command.CommandId);
            
            response.Success = false;
            response.Errors.Add($"Sürüm kontrolü sırasında hata: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Flutter'ın sistemde yüklü olup olmadığını kontrol eder
    /// </summary>
    private async Task<bool> IsFlutterInstalledAsync()
    {
        try
        {
            var result = await RunCommandAsync("flutter", "--version");
            return result.Success && result.Output.Contains("Flutter");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Flutter sürüm bilgilerini detaylı şekilde alır
    /// </summary>
    private async Task<FlutterVersionInfo?> GetFlutterVersionInfoAsync()
    {
        try
        {
            // Flutter version komutu
            var versionResult = await RunCommandAsync("flutter", "--version");
            if (!versionResult.Success) return null;

            // Doctor komutu
            var doctorResult = await RunCommandAsync("flutter", "doctor -v");

            // Dart version komutu
            var dartResult = await RunCommandAsync("dart", "--version");

            return ParseFlutterVersionInfo(versionResult.Output, doctorResult.Output, dartResult.Output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutter sürüm bilgileri alınırken hata");
            return null;
        }
    }

    /// <summary>
    /// Komut satırı işlemlerini çalıştırır
    /// </summary>
    private async Task<CommandResult> RunCommandAsync(string command, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return new CommandResult { Success = false };

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            return new CommandResult
            {
                Success = process.ExitCode == 0,
                Output = output,
                Error = error,
                ExitCode = process.ExitCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Komut çalıştırma hatası: {Command} {Arguments}", command, arguments);
            return new CommandResult { Success = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Flutter sürüm çıktısını parse eder
    /// </summary>
    private FlutterVersionInfo ParseFlutterVersionInfo(string versionOutput, string doctorOutput, string dartOutput)
    {
        var info = new FlutterVersionInfo();

        // Flutter version regex
        var flutterVersionMatch = Regex.Match(versionOutput, @"Flutter (\d+\.\d+\.\d+)");
        if (flutterVersionMatch.Success)
        {
            info.FlutterVersion = flutterVersionMatch.Groups[1].Value;
        }

        // Channel
        var channelMatch = Regex.Match(versionOutput, @"channel (\w+)");
        if (channelMatch.Success)
        {
            info.Channel = channelMatch.Groups[1].Value;
        }

        // Dart version
        var dartVersionMatch = Regex.Match(dartOutput, @"Dart SDK version: (\d+\.\d+\.\d+)");
        if (dartVersionMatch.Success)
        {
            info.DartVersion = dartVersionMatch.Groups[1].Value;
        }

        // Doctor issues
        info.DoctorIssues = ExtractDoctorIssues(doctorOutput);
        info.IsStable = info.Channel?.ToLower() == "stable";
        info.HasIssues = info.DoctorIssues.Any();

        return info;
    }

    /// <summary>
    /// Flutter doctor çıktısından sorunları çıkarır
    /// </summary>
    private List<string> ExtractDoctorIssues(string doctorOutput)
    {
        var issues = new List<string>();
        var lines = doctorOutput.Split('\n');

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("✗") || line.Trim().StartsWith("!"))
            {
                issues.Add(line.Trim());
            }
        }

        return issues;
    }

    #region Response Generators

    private McpResponse CreateDryRunResponse(McpCommand command, FlutterVersionInfo versionInfo)
    {
        return new McpResponse
        {
            CommandId = command.CommandId,
            Success = true,
            Purpose = "Flutter sürüm kontrolü önizlemesi",
            Notes = 
            {
                "🔍 DRY-RUN: Gerçek sürüm kontrolü yapılmadı",
                $"📱 Flutter Sürümü: {versionInfo.FlutterVersion}",
                $"🎯 Kanal: {versionInfo.Channel}",
                $"⚡ Dart Sürümü: {versionInfo.DartVersion}"
            },
            LearnNotes = 
            {
                "🧠 Dry-run modu, komutları güvenle test etmenizi sağlar",
                "📘 Sürüm kontrolü, proje uyumluluğu için kritiktir"
            }
        };
    }

    private List<string> GenerateVersionNotes(FlutterVersionInfo versionInfo)
    {
        var notes = new List<string>
        {
            $"📱 Flutter Sürümü: {versionInfo.FlutterVersion}",
            $"🎯 Kanal: {versionInfo.Channel}",
            $"⚡ Dart Sürümü: {versionInfo.DartVersion}"
        };

        if (!versionInfo.IsStable)
        {
            notes.Add("⚠️ Stable kanal önerilir (production projeler için)");
        }

        if (versionInfo.HasIssues)
        {
            notes.Add($"🔧 {versionInfo.DoctorIssues.Count} sorun tespit edildi");
        }
        else
        {
            notes.Add("✅ Flutter doctor sorun bildirmedi");
        }

        return notes;
    }

    private List<string> GenerateLearnNotes(FlutterVersionInfo versionInfo)
    {
        return new List<string>
        {
            "🧠 Flutter sürüm kontrolü, proje kararlılığını garanti eder. Ekip üyeleri arasında sürüm uyumsuzluğı, beklenmeyen hatalar yaratabilir.",
            "📘 Stable kanal kullanımı, production uygulamaları için önerilir. Beta/dev kanalları yeni özellikler içerir ancak kararsız olabilir.",
            "🔍 Flutter doctor komutu, sistem konfigürasyonunu kontrol eder. Sorunları erken tespit etmek, geliştirme sürecini hızlandırır.",
            "⚡ Dart sürümü Flutter ile sıkı bağlıdır. SDK güncellemeleri her zaman uyumlu versiyonları içerir."
        };
    }

    private List<string> GenerateAiReview(FlutterVersionInfo versionInfo)
    {
        var review = new List<string>();

        if (!versionInfo.IsStable)
        {
            review.Add("🔍 Production projeler için stable kanal önerilir");
        }

        if (versionInfo.HasIssues)
        {
            review.Add("💡 Flutter doctor sorunları çözülmeli");
        }

        if (IsVersionOutdated(versionInfo.FlutterVersion))
        {
            review.Add("⬆️ Flutter sürümü güncellenebilir");
        }

        return review;
    }

    private int CalculateComplexityScore(FlutterVersionInfo versionInfo)
    {
        int score = 10; // Base score

        if (!versionInfo.IsStable) score += 20;
        if (versionInfo.HasIssues) score += (versionInfo.DoctorIssues.Count * 15);
        if (IsVersionOutdated(versionInfo.FlutterVersion)) score += 25;

        return Math.Min(score, 100);
    }

    private bool IsVersionOutdated(string version)
    {
        // Basit outdated kontrolü - gerçek uygulamada API'den kontrol edilebilir
        if (Version.TryParse(version, out var v))
        {
            return v < new Version("3.24.0"); // Örnek threshold
        }
        return false;
    }

    #endregion
}

/// <summary>
/// Flutter sürüm bilgileri modeli
/// </summary>
public class FlutterVersionInfo
{
    public string FlutterVersion { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string DartVersion { get; set; } = string.Empty;
    public bool IsStable { get; set; }
    public bool HasIssues { get; set; }
    public List<string> DoctorIssues { get; set; } = new();
}

/// <summary>
/// Komut satırı işlem sonucu
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int ExitCode { get; set; }
}

}
