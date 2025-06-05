#!/bin/bash

# ConfigService Advanced Test Script
# Tests all new features: YAML parsing, caching, async operations, error handling

echo "🧪 ConfigService Advanced Feature Testing"
echo "=========================================="

SERVER_URL="http://localhost:5171/api/command/execute"

# Test 1: Basic Configuration Loading
echo "📋 Test 1: Basic Configuration (Real Project Path)"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "config-advanced-001",
    "command": "loadProjectPreferences",
    "params": {
      "projectPath": "/Users/alper/Documents/Development/Personal/FlutterMcpServer",
      "includeDevDependencies": true,
      "includeLintRules": true
    }
  }' | jq '.success, .executionTimeMs, .notes | length'

echo "✅ Test 1 completed"
echo ""

# Test 2: Generic Configuration Advice
echo "📋 Test 2: Generic Configuration Advice (No Project Path)"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "config-advanced-002",
    "command": "loadProjectPreferences",
    "params": {
      "includeDevDependencies": false,
      "includeLintRules": false
    }
  }' | jq '.success, .executionTimeMs, .learnNotes | length'

echo "✅ Test 2 completed"
echo ""

# Test 3: Invalid Project Path (Error Handling)
echo "📋 Test 3: Invalid Project Path (Error Handling)"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "config-advanced-003",
    "command": "loadProjectPreferences",
    "params": {
      "projectPath": "/nonexistent/path/flutter_project",
      "includeDevDependencies": true,
      "includeLintRules": true
    }
  }' | jq '.success, .executionTimeMs, .notes'

echo "✅ Test 3 completed"
echo ""

# Test 4: Cache Performance (Run same request twice)
echo "📋 Test 4: Cache Performance Test"
echo "First request (should cache):"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "config-advanced-004a",
    "command": "loadProjectPreferences",
    "params": {
      "projectPath": "/Users/alper/Documents/Development/Personal/FlutterMcpServer",
      "includeDevDependencies": true,
      "includeLintRules": false
    }
  }' | jq '.executionTimeMs'

echo "Second request (should use cache):"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "config-advanced-004b",
    "command": "loadProjectPreferences",
    "params": {
      "projectPath": "/Users/alper/Documents/Development/Personal/FlutterMcpServer",
      "includeDevDependencies": true,
      "includeLintRules": false
    }
  }' | jq '.executionTimeMs'

echo "✅ Test 4 completed"
echo ""

# Test 5: Parallel Processing Test (Multiple flags)
echo "📋 Test 5: Parallel Processing with All Options"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "config-advanced-005",
    "command": "loadProjectPreferences",
    "params": {
      "projectPath": "/Users/alper/Documents/Development/Personal/FlutterMcpServer",
      "includeDevDependencies": true,
      "includeLintRules": true
    }
  }' | jq '.success, .executionTimeMs, .notes | length, .learnNotes | length'

echo "✅ Test 5 completed"
echo ""

echo "🎉 ConfigService Advanced Testing Complete!"
echo "All async operations, caching, YAML parsing, and error handling features tested."
