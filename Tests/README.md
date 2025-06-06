# Flutter MCP Server - Test Suite

Bu klasör Flutter MCP Server projesinin tüm test dosyalarını organize eder.

## 📁 Klasör Yapısı

### `json/` - API Test JSON Dosyaları
REST API ve JSON-RPC endpoint'leri için test verisi dosyaları:
- `test_checkflutter.json` - Flutter sürüm kontrolü testi
- `test_generate_*.json` - Kod üretimi testleri (Dart class, Cubit, API service, theme)
- `test_analyze*.json` - Kod analizi testleri
- `test_search*.json` - Arama işlevi testleri (docs, pub.dev)
- `test_mcp_*.json` - MCP Protocol testleri
- `test_config_*.json` - Konfigürasyon testleri

### `scripts/` - Test Automation Scripts
Otomatik test çalıştırma betikleri:
- `comprehensive_test.sh` - Tüm testleri çalıştıran ana script
- `simple_test.sh` - Temel işlevsellik testleri
- `test_api.sh` - API endpoint testleri
- `test_*_service.sh` - Belirli servis testleri
- `test_mcp_protocol.sh` - MCP Protocol testleri

### `http/` - HTTP Client Test Dosyaları
VS Code REST Client veya Postman için HTTP test dosyaları:
- `test_filewriter.http` - Dosya yazma API testleri

### `unit/` - Unit Test Sınıfları
C# unit test dosyaları:
- `FileWriterTest.cs` - FileWriter servisi unit testleri
- `CodeGeneratorTest.cs` - Code generator unit testleri

## 🚀 Test Çalıştırma

### Sistem Gereksinimleri
- .NET 9.0 SDK
- curl (HTTP client)
- jq (JSON processor) - macOS'ta varsayılan olarak yüklü
- bash/zsh shell

### Tüm Testleri Çalıştır
```bash
cd Tests/scripts
./comprehensive_test.sh
```

### Belirli Kategori Testleri
```bash
# API testleri
./test_api.sh

# MCP Protocol testleri  
./test_mcp_protocol.sh

# Pub.dev servis testleri
./test_pubdev_service.sh
```

### Tek Komut Testi
```bash
# JSON dosyası ile test
curl -X POST http://localhost:5171/api/command/execute \
  -H "Content-Type: application/json" \
  -d @json/test_checkflutter.json
```

## 📋 Test Kategorileri

### 🛠️ Çekirdek İşlevsellik
- Flutter sürüm kontrolü
- Dosya yazma ve okuma
- Konfigürasyon yönetimi

### 🎨 Kod Üretimi
- Dart sınıf üretimi
- Cubit boilerplate
- API service oluşturma
- Tema modülü üretimi

### 🔍 Analiz ve Arama
- Kod kalitesi analizi
- Proje karmaşıklık analizi
- Flutter docs arama
- pub.dev paket arama

### 🔌 Protocol Desteği
- REST API endpoints
- JSON-RPC 2.0 format
- MCP Protocol compliance

## 🎯 Test Stratejisi

1. **Unit Tests** - Servis sınıfları için izole testler
2. **Integration Tests** - API endpoint testleri
3. **Protocol Tests** - MCP uyumluluk testleri
4. **Performance Tests** - Response time ve resource kullanımı

## 📊 Test Metrikleri

Testler aşağıdaki metrikleri ölçer:
- ✅ Response time (<100ms hedefi)
- ✅ Success rate (%100 hedefi)
- ✅ Error handling (graceful degradation)
- ✅ MCP Protocol compliance
