# Flutter MCP Server - Production Deployment Guide

## 🚀 Overview

This guide provides complete instructions for deploying the Flutter MCP Server in production environments. The server has been thoroughly tested and validated for real-world usage.

## ✅ Pre-Deployment Checklist

- [x] 26 development steps completed
- [x] MCP Protocol Layer fully implemented (JSON-RPC 2.0)
- [x] 16 core commands operational across 7 handler categories
- [x] Real-world testing suite passed
- [x] AI client integration configurations ready
- [x] Comprehensive error handling implemented
- [x] Performance testing validated

## 📋 System Requirements

### Minimum Requirements
- **.NET 9.0 Runtime** or higher
- **Memory**: 512 MB RAM minimum, 1 GB recommended
- **Storage**: 100 MB free space
- **Network**: HTTP/HTTPS access on chosen port

### Optional Requirements (for full functionality)
- **Flutter SDK**: For Flutter-specific commands
- **Git**: For repository operations
- **Internet Access**: For pub.dev and Flutter docs integration

## 🏗️ Deployment Options

### Option 1: Local Development Server

```bash
# Clone repository
git clone <repository-url>
cd FlutterMcpServer

# Build and run
dotnet restore
dotnet build
dotnet run --urls "http://localhost:5172"
```

### Option 2: Production Server (Linux)

```bash
# Build for production
dotnet publish -c Release -o ./publish

# Run as service
sudo ./publish/FlutterMcpServer --urls "http://0.0.0.0:5172"
```

### Option 3: Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5172

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlutterMcpServer.dll"]
```

```bash
# Build and run Docker container
docker build -t flutter-mcp-server .
docker run -p 5172:5172 flutter-mcp-server
```

## 🔧 Configuration

### Environment Variables

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5172
export MCP_SERVER_NAME="Flutter MCP Server"
export MCP_LOG_LEVEL=Information
```

### Configuration Files

Update `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "McpServer": {
    "Name": "Flutter MCP Server",
    "Version": "1.0.0",
    "MaxConcurrentExecutions": 10,
    "RequestTimeout": 300000
  }
}
```

## 🔒 Security Considerations

### API Security
- Enable HTTPS in production
- Implement rate limiting (already configured)
- Consider API key authentication for public deployments
- Monitor and log all requests

### Network Security
```bash
# Firewall configuration (Ubuntu/CentOS)
sudo ufw allow 5172
sudo firewall-cmd --permanent --add-port=5172/tcp
```

## 🤖 AI Client Integration

### Claude Desktop Configuration

Create/update `~/.config/claude-desktop/config.json`:

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

### Generic MCP Client Configuration

```json
{
  "name": "flutter-mcp-server",
  "version": "1.0.0",
  "description": "AI-powered Flutter development assistant",
  "mcp": {
    "server": {
      "name": "Flutter MCP Server",
      "version": "1.0.0",
      "protocol": "2.0",
      "endpoints": {
        "http": "http://localhost:5172/api/command",
        "jsonrpc": "http://localhost:5172/api/command/jsonrpc",
        "capabilities": "http://localhost:5172/api/command/capabilities"
      }
    }
  }
}
```

## 📊 Monitoring & Health Checks

### Health Check Endpoints

```bash
# Server health
curl http://localhost:5172/

# API health
curl http://localhost:5172/api/command/health

# MCP capabilities
curl http://localhost:5172/api/command/capabilities
```

### Performance Monitoring

```bash
# Check server response time
time curl -s http://localhost:5172/ > /dev/null

# Monitor server logs
tail -f /var/log/flutter-mcp-server.log

# Check memory usage
ps aux | grep FlutterMcpServer
```

## 🧪 Production Testing

Run the included test suite to validate deployment:

```bash
cd Tests/real-world
chmod +x real_world_test_suite.sh
./real_world_test_suite.sh
```

Expected results:
- ✅ MCP Protocol Compliance
- ✅ Code Generation Functions
- ✅ External API Integration
- ✅ File System Operations
- ✅ Error Handling

## 🎯 Available Commands (16 Total)

### Environment Commands
- `checkFlutterVersion` - Check Flutter SDK version
- `loadProjectPreferences` - Load project configuration

### Code Generation Commands
- `generateDartClass` - Generate Dart model classes
- `generateCubitBoilerplate` - Generate Cubit/Bloc boilerplate
- `generateApiService` - Generate HTTP API services
- `generateThemeModule` - Generate Material Design themes

### Analysis Commands
- `reviewCode` - Code review and suggestions
- `analyzeFeatureComplexity` - Project complexity analysis

### Testing Commands
- `generateTestsForCubit` - Generate Cubit unit tests

### Documentation Commands
- `searchFlutterDocs` - Search Flutter documentation
- `createFlutterPlugin` - Generate plugin templates

### Package Commands
- `searchPubDevPackages` - Search pub.dev packages
- `analyzePackage` - Analyze package details

### File System Commands
- `writeToFile` - Safe file operations
- `generateScreen` - Generate UI screens
- `migrateNavigationSystem` - Navigator to GoRouter migration

## 🚨 Troubleshooting

### Common Issues

1. **Port Already in Use**
   ```bash
   lsof -ti:5172 | xargs kill -9
   ```

2. **Permission Denied**
   ```bash
   sudo chown -R $USER:$USER ./publish
   chmod +x ./publish/FlutterMcpServer
   ```

3. **Missing Dependencies**
   ```bash
   dotnet --list-runtimes
   # Install .NET 9.0 if missing
   ```

### Debug Mode

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --verbosity detailed
```

## 📞 Support & Maintenance

### Logging
- Application logs: `/var/log/flutter-mcp-server/`
- Error logs: Check `server.log` in application directory
- Performance logs: Built-in ASP.NET Core logging

### Updates
```bash
git pull origin main
dotnet build
systemctl restart flutter-mcp-server
```

## 🎉 Deployment Success Checklist

- [ ] Server starts without errors
- [ ] Health endpoints respond correctly
- [ ] MCP capabilities discovery working
- [ ] JSON-RPC 2.0 compliance verified
- [ ] AI client can connect and execute commands
- [ ] All 16 commands operational
- [ ] Performance meets requirements
- [ ] Security measures implemented
- [ ] Monitoring configured
- [ ] Backup procedures established

---

**🎯 Production Status: READY**

The Flutter MCP Server has successfully completed 26 development steps, comprehensive testing, and is production-ready for AI-assisted Flutter development workflows.

For technical support or feature requests, refer to the project documentation and issue tracker.
