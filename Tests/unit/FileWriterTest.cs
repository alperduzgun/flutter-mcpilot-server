using System.Text;
using System.Text.Json;

namespace FlutterMcpServer.Tests;

public class FileWriterTest
{
  public static async Task TestFileWriter()
  {
    Console.WriteLine("Testing FileWriterService...");

    var client = new HttpClient();

    try
    {
      // 1. Health check
      Console.WriteLine("1. Testing health endpoint...");
      var healthResponse = await client.GetAsync("http://localhost:5171/");
      Console.WriteLine($"   Status: {healthResponse.StatusCode}");

      if (healthResponse.IsSuccessStatusCode)
      {
        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"   Response: {healthContent}");
      }

      // 2. Test FileWriter
      Console.WriteLine("\n2. Testing FileWriter service...");

      var testData = new
      {
        command = "writeFile",
        @params = new
        {
          filePath = "/tmp/test_flutter_widget.dart",
          content = "import 'package:flutter/material.dart';\n\nclass TestWidget extends StatelessWidget {\n  const TestWidget({Key? key}) : super(key: key);\n\n  @override\n  Widget build(BuildContext context) {\n    return Container(\n      child: Text('Hello Flutter MCP!'),\n    );\n  }\n}",
          createDirectories = true,
          overwrite = true,
          encoding = "utf-8"
        },
        dryRun = false,
        commandId = "test-write-file-001",
        timestamp = "2025-06-06T02:00:00Z"
      };

      var json = JsonSerializer.Serialize(testData);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await client.PostAsync("http://localhost:5171/api/flutter/command", content);
      Console.WriteLine($"   Status: {response.StatusCode}");

      var responseContent = await response.Content.ReadAsStringAsync();
      Console.WriteLine($"   Response: {responseContent}");

      // 3. Check if file was created
      Console.WriteLine("\n3. Checking if file was created...");
      var filePath = "/tmp/test_flutter_widget.dart";
      if (File.Exists(filePath))
      {
        Console.WriteLine("   ✅ File created successfully!");
        var fileContent = await File.ReadAllTextAsync(filePath);
        Console.WriteLine($"   Content length: {fileContent.Length} characters");
        Console.WriteLine($"   First 100 chars: {fileContent.Substring(0, Math.Min(100, fileContent.Length))}...");
      }
      else
      {
        Console.WriteLine($"   ❌ File not found at {filePath}");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ Error: {ex.Message}");
    }
    finally
    {
      client.Dispose();
    }
  }

  public static async Task Main(string[] args)
  {
    await TestFileWriter();
    Console.WriteLine("\nTest completed. Press any key to exit...");
    Console.ReadKey();
  }
}
