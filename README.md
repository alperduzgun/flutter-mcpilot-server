# 🧠 Flutter MCP Server – AI-Powered Modular Command Processor for Flutter

**Flutter MCP Server** is not just a Copilot.  
It’s a project-aware, modular, AI-powered backend that understands your **Flutter architecture**, remembers your past decisions, and helps you build clean, testable, and scalable code — while teaching you why.

---

## 🚀 Why Flutter MCP Server? – Beyond Copilot Agent Mode

While **Copilot Agent** is a powerful assistant for generic code suggestions,  
**Flutter MCP Server** is **architected for full-project intelligence** — it doesn't just complete code;  
it **understands your architecture**, **remembers your past choices**, and **shapes future decisions** accordingly.

> ✨ This is not a code copilot.  
> 🧠 This is an AI **modular command processor**, built for **Flutter-centric thinking** and **project continuity**.

---

### 🆚 Copilot Agent vs Flutter MCP Server

| Feature                          | Copilot Agent         | Flutter MCP Server                            |
|----------------------------------|------------------------|------------------------------------------------|
| Inline Code Suggestion           | ✅                     | 🚫 Not suggestion-based                        |
| Modular Command Protocol         | ❌                     | ✅ Clean, extensible handler structure         |
| Knows Project Architecture       | ❌                     | ✅ Understands layers, services, UI, state     |
| Remembers Past Decisions         | ❌                     | ✅ Learns from command logs                    |
| Guides Future Code Consistency   | ❌                     | ✅ Decision-aware evolution                    |
| SDK & Doc Integration            | ❌                     | ✅ Uses Flutter SDK & flutter.dev              |
| Navigation Refactor              | ❌                     | ✅ GoRouter conversion with reasoning          |
| blocTest Test Generator          | ❌                     | ✅ Modular and clean unit tests                |
| Inline Learning Notes            | ❌                     | ✅ 🧠 Learns with you, not just for you         |
| Dry Run Mode + Project Logs      | ❌                     | ✅ Previews, logs, and complexity scoring      |

---

## 🚀 Türkçe: Projeyi Tanır, Geçmişe Bakar, Geleceğe Karar Verir

Copilot Agent sadece satır bazlı öneriler sunar.  
Ama **Flutter MCP Server**, projenizi bir bütün olarak algılar:

- ✅ Geçmişte yazdığınız handler'lara, modüllere, test yapılarına bakar  
- ✅ Yeni komutlarda önceki tercihlerinizi dikkate alır  
- ✅ Tutarlı, uyumlu ve sürdürülebilir kod üretir  
- ✅ Her çıktının "neden" öyle üretildiğini size anlatır

> 🧠 Bu bir yazım asistanı değil, proje stratejisti.  
> 📘 Kod geçmişinizi inceler, gelecek kararlarını bu temelde verir.

---

## 📦 What It Does

- ✅ Generates Cubit, model, and widget code  
- 🧪 Produces blocTest tests  
- 🔄 Refactors Navigator → GoRouter  
- 🔍 Reviews code & gives improvement tips  
- 📁 Analyzes feature modularity  
- 🧠 Drops learning notes with each command  
- 📚 Reads Flutter SDK & flutter.dev docs  
- 🧪 Supports dryRun mode

---

## 📦 Architecture / Yapı

```
flutter-mcp-server/
├── Controllers/              # API endpoint'leri
├── Services/                 # Kod/Test üretici, review servisleri
├── Handlers/                # Her komutun karşılığı olan handler'lar
├── Models/                   # MCP komut modelleri
└── Config/                   # flutter_config.json gibi ayarlar
```

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

## 📄 License

MIT License – Free for commercial and educational use.

---

## ✨ Roadmap

- [ ] VS Code extension  
- [ ] Dart SDK / CLI client  
- [ ] Web playground  
- [ ] Custom prompt templating
