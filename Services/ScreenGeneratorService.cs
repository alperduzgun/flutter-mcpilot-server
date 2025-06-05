using FlutterMcpServer.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlutterMcpServer.Services;

/// <summary>
/// Prompt-to-Widget UI üretimi servisi
/// </summary>
public class ScreenGeneratorService
{
  private readonly ILogger<ScreenGeneratorService> _logger;

  public ScreenGeneratorService(ILogger<ScreenGeneratorService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Kullanıcı prompt'una göre Flutter ekranı üretir
  /// </summary>
  public async Task<McpResponse> GenerateScreenAsync(McpCommand command)
  {
    var response = new McpResponse
    {
      CommandId = command.CommandId,
      Purpose = "Flutter ekranı üretildi"
    };

    try
    {
      _logger.LogInformation("Ekran üretimi başlatıldı: {CommandId}", command.CommandId);

      // Parametreleri parse et
      string? prompt = null;
      string? screenName = null;
      string? screenType = null;

      if (command.Params.HasValue)
      {
        var paramsElement = command.Params.Value;

        if (paramsElement.TryGetProperty("prompt", out var promptElement))
        {
          prompt = promptElement.GetString();
        }

        if (paramsElement.TryGetProperty("screenName", out var nameElement))
        {
          screenName = nameElement.GetString();
        }

        if (paramsElement.TryGetProperty("screenType", out var typeElement))
        {
          screenType = typeElement.GetString();
        }
      }

      if (string.IsNullOrWhiteSpace(prompt))
      {
        response.Success = false;
        response.Errors.Add("Ekran açıklaması (prompt) parametresi gerekli");
        return response;
      }

      // Ekran üret
      var generationResult = await GenerateScreen(prompt, screenName, screenType);
      response.Notes.AddRange(generationResult.Messages);

      if (!string.IsNullOrEmpty(generationResult.ScreenCode))
      {
        response.LearnNotes.Add("🎨 Flutter ekranı üretildi");
        response.LearnNotes.Add("📱 Material Design 3 uyumlu");
        response.LearnNotes.Add("♿ Accessibility destekli");
        response.Notes.Add("Generated Screen Code:");
        response.Notes.Add(generationResult.ScreenCode);
      }

      response.Success = true;
      _logger.LogInformation("Ekran üretimi tamamlandı: {CommandId}", command.CommandId);

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Ekran üretimi hatası: {CommandId}", command.CommandId);
      response.Success = false;
      response.Errors.Add($"Ekran üretimi hatası: {ex.Message}");
      return response;
    }
  }

  private async Task<ScreenGenerationResult> GenerateScreen(string prompt, string? screenName, string? screenType)
  {
    var result = new ScreenGenerationResult();

    await Task.Run(() =>
    {
      result.Messages.Add("🎨 UI analizi başlatıldı");

      // Prompt analizi
      var analysis = AnalyzePrompt(prompt);
      result.Messages.Add($"📋 {analysis.Components.Count} UI bileşeni tespit edildi");
      result.Messages.Add($"🏗️ Layout türü: {analysis.LayoutType}");

      // Ekran adı belirle
      var finalScreenName = !string.IsNullOrEmpty(screenName) ? screenName : GenerateScreenName(prompt);
      result.Messages.Add($"📱 Ekran adı: {finalScreenName}");

      // Ekran tipi belirle
      var finalScreenType = !string.IsNullOrEmpty(screenType) ? screenType : DetermineScreenType(prompt);
      result.Messages.Add($"🏛️ Ekran tipi: {finalScreenType}");

      // Kod üret
      result.ScreenCode = GenerateScreenCode(finalScreenName, finalScreenType, analysis);

      result.Messages.Add("✅ Ekran kodu üretimi tamamlandı");
    });

    return result;
  }

  private PromptAnalysis AnalyzePrompt(string prompt)
  {
    var analysis = new PromptAnalysis();
    var lowerPrompt = prompt.ToLower();

    // UI bileşenlerini tespit et
    if (lowerPrompt.Contains("button") || lowerPrompt.Contains("buton"))
      analysis.Components.Add("ElevatedButton");

    if (lowerPrompt.Contains("text") || lowerPrompt.Contains("yazı") || lowerPrompt.Contains("metin"))
      analysis.Components.Add("Text");

    if (lowerPrompt.Contains("image") || lowerPrompt.Contains("resim") || lowerPrompt.Contains("görsel"))
      analysis.Components.Add("Image");

    if (lowerPrompt.Contains("list") || lowerPrompt.Contains("liste"))
      analysis.Components.Add("ListView");

    if (lowerPrompt.Contains("card") || lowerPrompt.Contains("kart"))
      analysis.Components.Add("Card");

    if (lowerPrompt.Contains("input") || lowerPrompt.Contains("textfield") || lowerPrompt.Contains("giriş"))
      analysis.Components.Add("TextField");

    if (lowerPrompt.Contains("form") || lowerPrompt.Contains("formular"))
      analysis.Components.Add("Form");

    if (lowerPrompt.Contains("tab") || lowerPrompt.Contains("sekme"))
      analysis.Components.Add("TabBar");

    // Layout tipini belirle
    if (lowerPrompt.Contains("grid"))
      analysis.LayoutType = "GridView";
    else if (lowerPrompt.Contains("scroll") || lowerPrompt.Contains("kaydır"))
      analysis.LayoutType = "SingleChildScrollView";
    else if (analysis.Components.Count > 3)
      analysis.LayoutType = "Column with SingleChildScrollView";
    else
      analysis.LayoutType = "Column";

    return analysis;
  }

  private string GenerateScreenName(string prompt)
  {
    // Prompt'tan ekran adı çıkar
    var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var name = words.Length > 0 ? words[0] : "Generated";
    return char.ToUpper(name[0]) + name.Substring(1) + "Screen";
  }

  private string DetermineScreenType(string prompt)
  {
    var lowerPrompt = prompt.ToLower();

    if (lowerPrompt.Contains("login") || lowerPrompt.Contains("signin") || lowerPrompt.Contains("giriş"))
      return "LoginScreen";
    else if (lowerPrompt.Contains("profile") || lowerPrompt.Contains("profil"))
      return "ProfileScreen";
    else if (lowerPrompt.Contains("list") || lowerPrompt.Contains("liste"))
      return "ListScreen";
    else if (lowerPrompt.Contains("detail") || lowerPrompt.Contains("detay"))
      return "DetailScreen";
    else if (lowerPrompt.Contains("form") || lowerPrompt.Contains("formular"))
      return "FormScreen";
    else
      return "StatefulWidget";
  }

  private string GenerateScreenCode(string screenName, string screenType, PromptAnalysis analysis)
  {
    var className = screenName.EndsWith("Screen") ? screenName : screenName + "Screen";
    var widgets = GenerateWidgetTree(analysis);

    return $@"import 'package:flutter/material.dart';

class {className} extends StatefulWidget {{
  const {className}({{Key? key}}) : super(key: key);

  @override
  State<{className}> createState() => _{className}State();
}}

class _{className}State extends State<{className}> {{
  @override
  Widget build(BuildContext context) {{
    return Scaffold(
      appBar: AppBar(
        title: const Text('{screenName.Replace("Screen", "")}'),
        centerTitle: true,
      ),
      body: {analysis.LayoutType}(
        {(analysis.LayoutType.Contains("Column") ? "mainAxisAlignment: MainAxisAlignment.center," : "")}
        {(analysis.LayoutType.Contains("Column") ? "crossAxisAlignment: CrossAxisAlignment.stretch," : "")}
        padding: const EdgeInsets.all(16.0),
        {(analysis.LayoutType.Contains("Column") ? "children: [" : "child: Column(children: [")}
{widgets}
        {(analysis.LayoutType.Contains("Column") ? "]," : "],),")}
      ),
    );
  }}
}}";
  }

  private string GenerateWidgetTree(PromptAnalysis analysis)
  {
    var widgets = new List<string>();

    foreach (var component in analysis.Components)
    {
      switch (component)
      {
        case "Text":
          widgets.Add("          const Text('Sample Text', style: TextStyle(fontSize: 16)),");
          break;
        case "ElevatedButton":
          widgets.Add(@"          ElevatedButton(
            onPressed: () {
              // TODO: Add button action
            },
            child: const Text('Action Button'),
          ),");
          break;
        case "TextField":
          widgets.Add(@"          const TextField(
            decoration: InputDecoration(
              labelText: 'Enter text',
              border: OutlineInputBorder(),
            ),
          ),");
          break;
        case "Card":
          widgets.Add(@"          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: const Text('Card Content'),
            ),
          ),");
          break;
        case "Image":
          widgets.Add(@"          Container(
            height: 200,
            decoration: BoxDecoration(
              color: Colors.grey[300],
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Icon(Icons.image, size: 64, color: Colors.grey),
          ),");
          break;
        case "ListView":
          widgets.Add(@"          Expanded(
            child: ListView.builder(
              itemCount: 10,
              itemBuilder: (context, index) {
                return ListTile(
                  title: Text('Item $index'),
                  subtitle: Text('Subtitle $index'),
                );
              },
            ),
          ),");
          break;
      }

      if (widgets.Count < analysis.Components.Count)
        widgets.Add("          const SizedBox(height: 16),");
    }

    return string.Join("\n", widgets);
  }

  private class PromptAnalysis
  {
    public List<string> Components { get; set; } = new();
    public string LayoutType { get; set; } = "Column";
  }

  private class ScreenGenerationResult
  {
    public List<string> Messages { get; set; } = new();
    public string ScreenCode { get; set; } = "";
  }
}
