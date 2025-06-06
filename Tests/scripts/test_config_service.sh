#!/bin/bash

# ConfigService Test Script
# Tests the loadProjectPreferences command with different scenarios

echo "🧪 ConfigService Test Suite"
echo "=========================="

# Test configuration
BASE_URL="http://localhost:5171"
ENDPOINT="/api/command/execute"
TEST_DIR="/Users/alper/Documents/Development/Personal/FlutterMcpServer"

echo "📋 Test Environment:"
echo "   Base URL: $BASE_URL"
echo "   Endpoint: $ENDPOINT"
echo "   Test Directory: $TEST_DIR"
echo ""

# Test 1: Basic project config loading
echo "🔍 Test 1: Basic Project Config Loading"
echo "---------------------------------------"
TEST_FILE="$TEST_DIR/../json/test_config_basic.json"
if [ -f "$TEST_FILE" ]; then
    echo "📄 Request: $(cat "$TEST_FILE")"
    echo ""
    echo "📤 Sending request..."
    
    RESPONSE=$(curl -s -w "\n%{http_code}" \
        -X POST \
        -H "Content-Type: application/json" \
        -d @"$TEST_FILE" \
        "$BASE_URL$ENDPOINT")
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n -1)
    
    echo "📥 Response (HTTP $HTTP_CODE):"
    echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
    echo ""
else
    echo "❌ Test file not found: $TEST_FILE"
fi

echo "---"

# Test 2: Generic config loading (no project path)
echo "🔍 Test 2: Generic Config Loading"
echo "--------------------------------"
TEST_FILE="$TEST_DIR/../json/test_config_generic.json"
if [ -f "$TEST_FILE" ]; then
    echo "📄 Request: $(cat "$TEST_FILE")"
    echo ""
    echo "📤 Sending request..."
    
    RESPONSE=$(curl -s -w "\n%{http_code}" \
        -X POST \
        -H "Content-Type: application/json" \
        -d @"$TEST_FILE" \
        "$BASE_URL$ENDPOINT")
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n -1)
    
    echo "📥 Response (HTTP $HTTP_CODE):"
    echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
    echo ""
else
    echo "❌ Test file not found: $TEST_FILE"
fi

echo "---"

# Test 3: Error handling - invalid project path
echo "🔍 Test 3: Error Handling - Invalid Project Path"
echo "-----------------------------------------------"
echo "📤 Sending request with invalid path..."

INVALID_REQUEST='{"commandId":"config-test-003","command":"loadProjectPreferences","params":{"projectPath":"/nonexistent/path","includeDevDependencies":true}}'

RESPONSE=$(curl -s -w "\n%{http_code}" \
    -X POST \
    -H "Content-Type: application/json" \
    -d "$INVALID_REQUEST" \
    "$BASE_URL$ENDPOINT")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n -1)

echo "📥 Response (HTTP $HTTP_CODE):"
echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
echo ""

echo "✅ ConfigService tests completed!"
echo "================================"
