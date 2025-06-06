# Examples Directory

This directory contains JSON templates for testing Flutter MCP Server commands through different interfaces (REST API, JSON-RPC 2.0).

## Structure

- `rest-api/` - REST API command examples
- `json-rpc/` - JSON-RPC 2.0 command examples  
- `mcp-protocol/` - MCP Protocol specific examples
- `integration/` - Complex integration test scenarios

## Usage

### REST API Testing
Use the JSON files in `rest-api/` directory with curl, Postman, or any HTTP client:

```bash
curl -X POST http://localhost:5171/api/command/execute \
  -H "Content-Type: application/json" \
  -d @rest-api/check-flutter-version.json
```

### JSON-RPC 2.0 Testing  
Use the JSON files in `json-rpc/` directory with the JSON-RPC endpoint:

```bash
curl -X POST http://localhost:5171/api/command/jsonrpc \
  -H "Content-Type: application/json" \
  -d @json-rpc/check-flutter-version.json
```

### MCP Protocol Testing
Use MCP-compliant clients with the examples in `mcp-protocol/` directory.

## Command Categories

1. **Environment** - Flutter SDK version checking, environment validation
2. **Code Generation** - Dart classes, Cubits, API services, themes
3. **Analysis** - Code review, complexity analysis  
4. **Testing** - Test generation and templates
5. **Documentation** - Flutter docs search and integration
6. **Packages** - pub.dev search and package analysis
7. **File System** - File operations, screen generation, plugin creation

## Testing Scenarios

Each category has examples for:
- ✅ **Success cases** - Valid inputs and expected outputs
- ❌ **Error cases** - Invalid inputs and error handling
- 🔄 **Edge cases** - Boundary conditions and special scenarios
