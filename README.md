# 🧠 Flutter MCPilot (MCP Server) – AI-Powered Modular Command Processor for Flutter

**Flutter MCP Server** is not just another AI code tool.  
It is a **project-aware**, **modular**, and **educational** backend assistant specifically built for Flutter.

**Flutter MCP Server**, klasik bir Copilot değildir.  
Projenizi tanır, önceki kararları hatırlar, yeni kararlarınızı ona göre şekillendirir. Kod yazarken size öğretir.

---

## 🚀 Why Flutter MCP Server? / Neden Flutter MCP Server?

While **Copilot Agent** helps complete lines, **Flutter MCP Server** builds complete features.  
It analyzes your architecture, tracks your past preferences, and produces maintainable output with reasoned explanations.

**Copilot Agent**, satır önerileri sunar.  
**Flutter MCP Server** ise bütünsel çözümler üretir, proje mimarinizi okur, geçmiş tercihlerinizle gelecek çıktılarınızı optimize eder.

---

### 🆚 Copilot Agent vs Flutter MCP Server

| Feature / Özellik                      | Copilot Agent         | Flutter MCP Server                            |
|---------------------------------------|------------------------|------------------------------------------------|
| Inline code suggestion / Kod tahmini  | ✅                     | 🚫 Not suggestion-based                        |
| Modular command processor             | ❌                     | ✅ Clean handler-driven architecture           |
| Project memory / Geçmiş karar analizi | ❌                     | ✅ Learns from history                         |
| SDK & docs integration                | ❌                     | ✅ Flutter SDK + flutter.dev analysis          |
| Test & refactor tools                 | ❌                     | ✅ blocTest, GoRouter migration, complexity    |
| Educational notes / Öğretici notlar   | ❌                     | ✅ Explains WHY, not just WHAT                 |
| dryRun & logging                      | ❌                     | ✅ Secure previews + logging                   |

---

## 📦 What It Does / Neler Yapar?

### 🏗️ **Modular Command Architecture**
- **7 Specialized Handlers** for different development aspects
- **Dynamic Command Discovery** with category-based organization
- **Centralized Error Handling** and command routing
- **Extensible Design** for easy addition of new command categories

### 🛠️ **Code Generation & Analysis**
- ✅ **Dart Classes** - JSON serializable, Equatable support
- ✅ **Cubit Generation** - BLoC pattern with state management
- ✅ **API Services** - HTTP client with error handling
- ✅ **Material Design 3 Themes** - Complete theme modules
- 🧪 **Test Generation** - blocTest-based unit tests
- 🔍 **Code Review** - Quality analysis and refactor suggestions

### 🔄 **Project Migration & Optimization**
- 🔄 **Navigator → GoRouter** migration
- 📁 **Feature Complexity Analysis** - Architecture scoring
- 🚀 **Plugin Creation** - Flutter plugin boilerplate
- 📱 **Screen Generation** - Prompt-to-Widget UI creation

### 📚 **Documentation & Package Management**
- 📚 **Flutter Docs Search** - flutter.dev integration
- 📦 **pub.dev Package Discovery** - Smart package recommendations
- 🔍 **Package Analysis** - Compatibility and security checks

### 🛡️ **Development Safety & Learning**
- 🧪 **dryRun Mode** - Safe previews before execution
- 🧠 **Educational Notes** - Learn WHY, not just WHAT
- 📊 **Command Telemetry** - Performance tracking and insights
- 🔒 **Secure File Operations** - Path traversal protection

### 🌐 **Multiple Interface Support**
- **REST API** - Traditional HTTP endpoint integration
- **JSON-RPC 2.0** - AI client compatible protocol
- **MCP Protocol** - Model Context Protocol compliance  
  > Önizleme moduyla güvenli çalıştırma

---

## 🎯 Available Commands / Mevcut Komutlar

### 🌍 **Environment Commands**
- `checkFlutterVersion` - Flutter SDK version validation and project compatibility

### 🛠️ **Code Generation Commands**  
- `generateDartClass` - Create Dart classes with JSON serialization and Equatable
- `generateCubitBoilerplate` - Generate BLoC pattern Cubit with state management
- `generateApiService` - Create HTTP API service classes with error handling
- `generateThemeModule` - Generate Material Design 3 theme modules

### 🔍 **Analysis Commands**
- `reviewCode` - Comprehensive code quality analysis and refactor suggestions
- `analyzeFeatureComplexity` - Project architecture and complexity scoring

### 🧪 **Testing Commands**
- `generateTestsForCubit` - Create blocTest-based unit tests for Cubits

### 📚 **Documentation Commands**
- `searchFlutterDocs` - Search flutter.dev documentation with category filtering

### 📦 **Package Commands**
- `searchPubDevPackages` - Discover packages on pub.dev with smart filtering
- `analyzePackage` - Detailed package analysis with compatibility checks

### 📁 **File System Commands**
- `writeToFile` - Secure file operations with path traversal protection
- `migrateNavigationSystem` - Convert Navigator to GoRouter patterns
- `generateScreen` - Create Flutter screens from natural language prompts
- `createFlutterPlugin` - Generate Flutter plugin boilerplate
- `loadProjectPreferences` - Load and analyze project configuration

---

## 📂 Project Structure / Proje Yapısı

```
flutter-mcp-server/
├── Controllers/              # API endpoints / API uçları
│   └── CommandController.cs  # Simplified with handler manager integration
├── Services/                 # Code, test, review services / Servis katmanları  
│   ├── CodeGenerator.cs      # Dart class, Cubit, API service generation
│   ├── FlutterVersionChecker.cs # SDK version validation
│   ├── CodeReviewService.cs  # Code quality analysis
│   ├── FlutterDocService.cs  # flutter.dev documentation search
│   └── PubDevService.cs      # pub.dev package discovery
├── Handlers/                 # Modular command handlers / Komut işleyiciler
│   ├── ICommandHandler.cs    # Base interface for all handlers
│   ├── CommandHandlerManager.cs # Central command routing
│   ├── EnvironmentCommandHandler.cs # Flutter SDK operations
│   ├── CodeGenerationCommandHandler.cs # Code generation commands
│   ├── AnalysisCommandHandler.cs # Code review and complexity
│   ├── TestingCommandHandler.cs # Test generation
│   ├── DocumentationCommandHandler.cs # Flutter docs search
│   ├── PackageCommandHandler.cs # pub.dev operations
│   └── FileSystemCommandHandler.cs # File operations and migrations
├── Models/                   # Command models / Veri modelleri
│   └── McpCommand.cs         # MCP protocol command structure
├── Config/                   # Configuration templates / Yapılandırma şablonları
│   ├── mcp-capabilities.json # MCP protocol capabilities
│   ├── dart-class-template.json # Dart class generation templates
│   ├── cubit-template.json   # Cubit state management templates
│   ├── api-service-template.json # API service templates
│   └── theme-template.json   # Material Design 3 theme templates
└── Examples/                 # Command examples / Komut örnekleri
    ├── rest-api/            # REST API examples
    ├── json-rpc/            # JSON-RPC 2.0 examples
    ├── mcp-protocol/        # MCP protocol examples
    └── integration/         # Integration test scenarios
```

---

## ✅ Getting Started / Başlarken

```bash
git clone https://github.com/your-org/flutter-mcp-server.git
cd flutter-mcp-server
dotnet run
```

> Requires .NET 7+  
> .NET 7+ gerektirir

---

## 🧪 Dry Run Example / Dry Run Örneği

```json
{
  "command": "generateTestsForCubit",
  "params": {
    "path": "lib/feature/cart/cart_cubit.dart"
  },
  "dryRun": true
}
```

You’ll get a safe preview without writing files.  
> Dosyaya yazmadan örnek çıktı alırsınız.

---

## 🧠 Learn as You Build / Kodlarken Öğren

> 🧠 `copyWith()` improves state immutability.  
> 📘 `copyWith()` kullanmak, Cubit güncellemelerinde daha güvenli geçiş sağlar.

---

## 📈 Logs & Telemetry / Loglama ve Telemetri

Each execution is saved to `project_log.txt`.  
Tüm komutlar `project_log.txt` içinde kayıtlı tutulur:

- ⏱️ Estimated Time Saved / Zaman tasarrufu  
- 🎯 Style Score / Stil uyum puanı  
- 📘 Learning Notes / Öğrenme notları  
- ⚠️ Complexity Score / Karmaşıklık analizi  
- 🔍 Code Review Hints / Kod kokusu uyarıları

---

## 📄 License / Lisans

MIT License – Free to use for commercial and personal projects.  
MIT Lisansı – Ticari ve bireysel kullanımda ücretsizdir.

---

## ✨ Roadmap / Yol Haritası

### 🎯 **Phase 1: Core Features Completion**

- [x] ✅ Flutter Version Checker
- [x] ✅ Code Review & Refactor Service  
- [x] ✅ Test Generator for Cubits
- [x] ✅ Navigation Migration (Navigator → GoRouter)
- [x] ✅ Screen Generator (Prompt-to-Widget)
- [x] ✅ Flutter Plugin Creator
- [x] ✅ Project Complexity Analyzer
- [x] ✅ Config Service with YAML/JSON support
- [x] ✅ Secure File Writer with dry-run mode
- [ ] 🔄 FlutterDocService (flutter.dev integration)
- [ ] 🔄 PubDevService (pub.dev package analysis)
- [ ] 🔄 MCP Protocol Layer (JSON-RPC 2.0 compliance)

### 🚀 **Phase 2: Advanced AI Integration**

- [ ] 🧠 AI Prompt Resolver Endpoint  
  > `POST /resolve-prompt` - Natural language to command mapping
- [ ] 🔍 Command Telemetry Dashboard  
  > Real-time metrics with Prometheus/Grafana support
- [ ] 🧩 Dynamic Command Registration  
  > Plugin architecture for extensible commands
- [ ] 🔄 Command Undo/Replay API  
  > `POST /commands/replay` with full command history

### 🛠️ **Phase 3: Developer Experience**

- [ ] 🧪 Enhanced Mock Mode  
  > Advanced test data injection beyond dry-run
- [ ] 📁 File System Snapshot API  
  > Track all MCP-generated file changes
- [ ] 💬 Natural Language Feedback Collector  
  > AI learning from user feedback
- [ ] 🛡️ Command Role Authorization  
  > Admin/Developer/TestUser role-based access

### 🌐 **Phase 4: Platform Extensions**

- [ ] 🌍 Multilingual Instruction Translator  
  > `/instructions?lang=tr` for localized AI prompts
- [ ] 🔌 VS Code Extension  
  > Direct IDE integration
- [ ] 📱 Dart CLI SDK  
  > Command-line interface for CI/CD
- [ ] 🌐 Web Playground  
  > Browser-based testing environment
- [ ] 📝 Customizable Prompt Profiles  
  > User-defined AI behavior templates

### 📊 **Current Status**

✅ **9/12 Core Services** implemented  
✅ **REST API** fully functional (port 5171)  
✅ **Swagger UI** available at `/swagger`  
⏳ **MCP Protocol** layer in progress  
📈 **~75% completion** of Phase 1

### 🎯 **Next Milestones**

1. **Complete MCP Protocol Integration** - AI auto-discovery
2. **Implement FlutterDoc & PubDev Services** - External API integration  
3. **Launch Telemetry Dashboard** - Production monitoring
4. **Beta VS Code Extension** - Developer adoption

---

## 🙌 Contributing / Katkı Sağlayın

Pull requests are welcome. For major changes, please open an issue.  
Pull request gönderebilirsiniz. Büyük değişiklikler için issue açmanız yeterli.

---

## 🔍 Keywords / Anahtar Kelimeler

`flutter mcp`, `ai codegen`, `flutter bloc test`, `navigation refactor`, `flutter sdk analyzer`, `project-aware ai`, `command processor`, `code generator flutter`

---

🚀 Powered by .NET • Designed for Flutter • Built to Teach
