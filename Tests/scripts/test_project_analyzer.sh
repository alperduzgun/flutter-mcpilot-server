#!/bin/bash

echo "🧪 ProjectAnalyzer Service Test Suite"
echo "======================================"

SERVER_URL="http://localhost:5171/api/command/execute"

# Test 1: Basic complexity analysis
echo
echo "Test 1: Basic complexity analysis (no parameters)"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_001","command":"analyzeFeatureComplexity"}' | \
  jq -r '.success, .purpose, .notes[0:3][]' | head -5

# Test 2: Generic analysis with feature name
echo
echo "Test 2: Generic analysis with feature name"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_002","command":"analyzeFeatureComplexity","params":{"featureName":"Authentication"}}' | \
  jq -r '.success, .notes[0:2][]'

# Test 3: Project path analysis (current project)
echo
echo "Test 3: Current project analysis"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_003","command":"analyzeFeatureComplexity","params":{"projectPath":"/Users/alper/Documents/Development/Personal/FlutterMcpServer","includeTests":false}}' | \
  jq -r '.success, .notes[0:3][]'

# Test 4: Invalid project path
echo
echo "Test 4: Invalid project path"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_004","command":"analyzeFeatureComplexity","params":{"projectPath":"/invalid/path"}}' | \
  jq -r '.success, .notes[0:2][]'

echo
echo "✅ ProjectAnalyzer Test Suite Completed"
