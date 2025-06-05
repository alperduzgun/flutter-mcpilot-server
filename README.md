# 🧠 Flutter MCP Server – AI-Powered Modular Command Processor for Flutter

**Flutter MCP Server** is a modular, .NET-based AI assistant that automates and enhances your Flutter development process.  
From generating Cubit tests to navigation refactors and code review – all handled intelligently with powerful commands.

> 🚀 Boost your productivity. ✨ Learn with every AI output. 📦 Keep your Flutter codebase clean and modular.

---

## 🚀 Features / Özellikler

- ✅ **AI-powered Code Generation** (Cubit, Widget, Service, Model)  
  > Flutter için AI destekli Cubit, Widget, Model ve Servis üretimi
- 🧪 **blocTest-based Test Generation**  
  > blocTest desteğiyle otomatik test üretimi
- 🔍 **AI Code Review and Refactor Suggestions**  
  > Kod incelenir ve refactor önerileri sunulur
- 🔄 **Navigation Refactor** (Navigator → GoRouter)  
  > Navigation yapısı otomatik olarak GoRouter'a dönüştürülür
- 📦 **Flutter.dev & Pub.dev Search**  
  > AI destekli dokümantasyon ve paket araştırması
- 📁 **Feature Complexity Analysis**  
  > Özellik modülü karmaşıklık puanı (0–100)
- 🧠 **Learning Cards & AI Notes**  
  > Öğretici açıklamalar her komutun sonunda
- 🧪 **Dry-Run Support**  
  > Dosyaya yazmadan sonucu önizleme

---

## 📦 Project Structure / Proje Yapısı

```
flutter-mcp-server/
├── Controllers/              # API endpoint'leri
├── Services/                 # Kod/Test üretici, review servisleri
├── Handlers/                # Her komutun karşılığı olan handler'lar
├── Models/                   # MCP komut modelleri
└── Config/                   # flutter_config.json gibi ayarlar
```

---

## 🧩 Supported MCP Commands / Desteklenen Komutlar

| Command                     | Description                                        | Açıklama                                          |
|----------------------------|----------------------------------------------------|---------------------------------------------------|
| `checkFlutterVersion`      | Validates Flutter SDK compatibility                | Flutter sürüm kontrolü yapar                      |
| `reviewCode`               | Performs AI-based static analysis                  | Kodunuzu analiz eder ve geliştirici notlar sunar  |
| `generateTestsForCubit`    | blocTest tabanlı Cubit test dosyası üretir        | Otomatik test üretimi                             |
| `migrateNavigationSystem`  | Converts Navigator routes to GoRouter              | Navigation sistemini dönüştürür                   |
| `generateScreen`           | Prompt'tan ekran üretir                            | UI widget oluşturur                               |
| `createFlutterPlugin`      | Plugin yapısı için başlangıç şablonu               | Flutter plugin temeli sunar                       |
| `analyzeFeatureComplexity` | Modül karmaşıklığını puanlar                       | Yapı analiz skoru üretir                          |

---

## ✅ Local Setup / Kurulum

```bash
git clone https://github.com/your-org/flutter-mcp-server.git
cd flutter-mcp-server
dotnet run
```

> Requires .NET 7+ and a Flutter project to target.

---

## 🧪 Dry Run Mode – Safe Preview

```json
{
  "command": "generateTestsForCubit",
  "params": {
    "path": "lib/feature/cart/cart_cubit.dart"
  },
  "dryRun": true
}
```

> No file is written. You get a preview JSON with notes.

---

## 🧠 Developer Insights – Learn as You Code

Flutter MCP Server teaches you **why** it makes each change:

> 🧠 `copyWith()` improves immutability in state updates.  
> 📘 Learn this for safer state transitions in Flutter Cubits.

---

## 📈 Logs & Metrics

Every execution is logged in `project_log.txt`. You’ll see:

- ⏱️ Estimated Time Saved  
- 🎯 Style Consistency Score  
- 📘 AI Learn Notes  
- ⚠️ Complexity Warnings  
- 🔍 Code Smell Suggestions

---

## 🌍 Why Flutter MCP Server?

> Flutter is fast, but repetitive.  
> Flutter MCP Server automates boilerplate and **teaches while coding.**

Build faster, smarter, and cleaner Flutter code – with guidance on every step.

---

## 📄 License

MIT License – Free for commercial and educational use.

---

## ✨ Roadmap

- [ ] VS Code extension for one-click actions  
- [ ] Dart/Flutter CLI SDK  
- [ ] Web playground UI  
- [ ] Customizable prompt system

---

## 🙌 Contributing

Pull requests are welcome.  
To add a new MCP command, create a new Handler + Service combo and open an issue.

---

## 🔍 Keywords / Anahtar Kelimeler

`flutter mcp server`, `ai flutter tools`, `flutter codegen`, `dotnet flutter assistant`, `flutter modular backend`, `flutter refactor`, `bloc test generator`

---

🔥 Powered by .NET • Designed for Flutter
