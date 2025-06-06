# Configuration Files

This directory contains configuration files and templates for the Flutter MCP Server.

## Configuration Categories

### 1. MCP Protocol Configuration
- `mcp-server-config.json` - Base MCP server configuration
- `mcp-capabilities.json` - Server capabilities definition
- `mcp-tools-registry.json` - Available tools/commands registry

### 2. Flutter Project Templates
- `flutter-project-defaults.json` - Default Flutter project settings
- `flutter-coding-standards.json` - Coding standards and rules
- `flutter-package-preferences.json` - Preferred packages and versions

### 3. Code Generation Templates
- `dart-class-template.json` - Dart class generation templates
- `cubit-template.json` - Cubit/Bloc boilerplate templates
- `api-service-template.json` - API service generation templates
- `theme-template.json` - Material Design 3 theme templates

### 4. Testing Configuration
- `test-generation-config.json` - Test generation preferences
- `test-coverage-config.json` - Coverage requirements

### 5. Documentation Configuration
- `flutter-docs-sources.json` - Flutter documentation sources
- `pub-dev-search-config.json` - pub.dev search preferences

## Usage

These configuration files are used by various services to provide consistent behavior across the MCP server. They can be customized based on project requirements and user preferences.

## Environment Variables

The server supports the following environment variables for configuration:

- `FLUTTER_MCP_CONFIG_PATH` - Custom path to configuration files
- `FLUTTER_MCP_LOG_LEVEL` - Logging level (Debug, Information, Warning, Error)
- `FLUTTER_MCP_ENABLE_CACHE` - Enable/disable caching (true/false)
- `FLUTTER_MCP_MAX_CONCURRENT_OPERATIONS` - Maximum concurrent operations
