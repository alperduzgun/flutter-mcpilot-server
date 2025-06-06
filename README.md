# 🧠 Flutter MCP Server – AI-Powered Modular Command Processor for Flutter

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

- ✅ Generates Cubit, models, widgets  
  > Cubit, model ve widget üretir
- 🧪 blocTest-based unit tests  
  > blocTest ile test dosyaları oluşturur
- 🔄 Navigator → GoRouter migration  
  > Navigation yapısını otomatik dönüştürür
- 🔍 Code review and refactor suggestions  
  > Kodunuzu analiz edip iyileştirme önerir
- 📁 Complexity score for features  
  > Feature karmaşıklığını puanlar
- 🧠 Learning notes on every command  
  > Her komuttan sonra öğretici notlar
- 📚 SDK & documentation analysis  
  > Flutter SDK & döküman kontrolü
- 🧪 dryRun mode for safe previews  
  > Önizleme moduyla güvenli çalıştırma

---

## 📂 Project Structure / Proje Yapısı

```
flutter-mcp-server/
├── Controllers/              # API endpoints / API uçları
├── Services/                 # Code, test, review services / Servis katmanları
├── Handlers/                 # Command logic / Komut işleyiciler
├── Models/                   # Command models / Veri modelleri
└── Config/                   # Project configs / Proje ayarları
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

- [ ] VS Code Extension / VS Code eklentisi  
- [ ] Dart CLI SDK  
- [ ] Web Playground  
- [ ] Customizable Prompt Profiles / Prompt şablonları

---

## 🙌 Contributing / Katkı Sağlayın

Pull requests are welcome. For major changes, please open an issue.  
Pull request gönderebilirsiniz. Büyük değişiklikler için issue açmanız yeterli.

---

## 🔍 Keywords / Anahtar Kelimeler

`flutter mcp`, `ai codegen`, `flutter bloc test`, `navigation refactor`, `flutter sdk analyzer`, `project-aware ai`, `command processor`, `code generator flutter`

---

🚀 Powered by .NET • Designed for Flutter • Built to Teach
