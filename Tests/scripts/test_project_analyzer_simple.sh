#!/bin/zsh

echo "🧪 ProjectAnalyzer Service Test Suite"
echo "======================================"

SERVER_URL="http://localhost:5171/api/command/execute"

# Test 1: Basic complexity analysis
echo
echo "Test 1: Basic complexity analysis (no parameters)"
echo "Response:"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_001","command":"analyzeFeatureComplexity"}' | \
  jq '.' | head -20

# Test 2: Generic analysis with feature name
echo
echo "Test 2: Generic analysis with feature name"
echo "Response:"
curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_002","command":"analyzeFeatureComplexity","params":{"featureName":"Authentication"}}' | \
  jq '.' | head -15

# Test 3: Check execution time
echo
echo "Test 3: Execution time check"
RESPONSE=$(curl -s -X POST $SERVER_URL \
  -H "Content-Type: application/json" \
  -d '{"commandId":"../json/test_003","command":"analyzeFeatureComplexity"}')

echo "Success: $(echo $RESPONSE | jq -r '.success')"
echo "Execution Time: $(echo $RESPONSE | jq -r '.executionTimeMs')ms"

echo
echo "✅ ProjectAnalyzer Test Suite Completed"
