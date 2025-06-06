# Claude Desktop MCP Konfigürasyon Rehberi

## Flutter MCP Server'ını Claude Desktop'ta Kurma

### 1. Server'ı Başlatma

Flutter MCP Server'ını başlatmak için:

```bash
cd /Users/alper/Documents/Development/Personal/FlutterMcpServer
dotnet run
```

Server `http://localhost:5172` adresinde çalışacaktır.

### 2. Claude Desktop Konfigürasyonu

Claude Desktop ayarlarında **"HTTP (HTTP or Server-Sent Events)"** seçeneğini seçin ve aşağıdaki konfigürasyonu kullanın:

#### Seçenek 1: Node.js Proxy ile (Önerilen)

```json
{
  "mcpServers": {
    "flutter-mcp-server": {
      "command": "node",
      "args": ["/Users/alper/Documents/Development/Personal/FlutterMcpServer/mcp_proxy.js"],
      "env": {
        "MCP_SERVER_URL": "http://localhost:5172"
      }
    }
  }
}
```

#### Seçenek 2: Doğrudan HTTP (Alternatif)

```json
{
  "mcpServers": {
    "flutter-mcp-server": {
      "command": "curl",
      "args": [
        "-X", "POST",
        "http://localhost:5172/api/command/jsonrpc",
        "-H", "Content-Type: application/json",
        "-d", "@-"
      ],
      "env": {
        "MCP_SERVER_URL": "http://localhost:5172",
        "MCP_SERVER_NAME": "Flutter MCP Server"
      }
    }
  }
}
```

### 3. Kullanılabilir Flutter MCP Tools

Aşağıdaki 12 adet Flutter geliştirme tool'u mevcuttur:

1. **checkFlutterVersion** - Flutter SDK sürüm kontrolü
2. **generateDartClass** - Dart class oluşturma
3. **generateCubitBoilerplate** - Cubit state management kodu
4. **generateApiService** - API servis class'ları
5. **generateThemeModule** - Flutter tema modülleri
6. **reviewCode** - Dart/Flutter kod analizi
7. **analyzeFeatureComplexity** - Proje karmaşıklık analizi
8. **generateTestsForCubit** - Cubit test dosyaları
9. **searchFlutterDocs** - Flutter dokümantasyon arama
10. **searchPubDevPackages** - pub.dev paket arama
11. **analyzePackage** - Paket uygunluk analizi
12. **writeToFile** - Dosya sistemi yazma işlemleri

### 4. Test Etme

Server'ın çalıştığını test etmek için:

```bash
# Tools listesini kontrol et
echo '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}' | node mcp_proxy.js

# Bir tool'u çalıştır
echo '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"checkFlutterVersion","arguments":{"projectPath":"."}}}' | node mcp_proxy.js
```

### 5. Sorun Giderme

#### Server çalışmıyorsa:
- Port 5172'nin kullanımda olup olmadığını kontrol edin: `lsof -i :5172`
- Server'ı yeniden başlatın: `pkill -f "dotnet run" && dotnet run`

#### Claude Desktop bağlantı sorunu:
- Node.js'in yüklü olduğundan emin olun: `node --version`
- Proxy script'inin executable olduğunu kontrol edin: `chmod +x mcp_proxy.js`
- Server'ın erişilebilir olduğunu kontrol edin: `curl http://localhost:5172/health`

### 6. Başlama Komutları Sırası

1. Terminal'de server'ı başlatın:
   ```bash
   cd /Users/alper/Documents/Development/Personal/FlutterMcpServer
   dotnet run &
   ```

2. Claude Desktop'ı açın ve MCP konfigürasyonunu ekleyin

3. Claude ile konuşmaya başlayın: "Flutter projem için bir User model class'ı oluştur"

## Özellikler

✅ **MCP Protocol 2.0 Uyumlu** - Tam JSON-RPC 2.0 desteği  
✅ **12 Flutter Tool** - Kapsamlı geliştirme araç seti  
✅ **Real-time Communication** - HTTP tabanlı çift yönlü iletişim  
✅ **Error Handling** - Kapsamlı hata yönetimi  
✅ **Logging** - Detaylı işlem logları  
✅ **Production Ready** - Üretim ortamı için hazır  

## Güvenlik

- Server sadece localhost (127.0.0.1) üzerinde çalışır
- Dış ağ bağlantılarına kapalıdır
- Dosya yazma işlemleri proje dizini ile sınırlıdır
