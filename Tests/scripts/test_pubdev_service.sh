#!/bin/zsh

# PubDevService Test Script
# Tests pub.dev package search and analysis functionality

API_BASE="http://localhost:5171/api/command/execute"

echo "🔍 Testing PubDevService Integration..."
echo "========================================="

# Test 1: Search Popular Packages
echo "📦 Test 1: Searching popular HTTP packages..."
curl -X POST "$API_BASE" \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "test-search-http",
    "command": "searchPubDevPackages",
    "params": {
      "searchTerm": "http",
      "category": "popular",
      "includeAnalysis": true
    },
    "dryRun": false
  }' \
  | jq '.'

echo -e "\n🔥 Test 2: Searching trending state management packages..."
curl -X POST "$API_BASE" \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "test-search-state",
    "command": "searchPubDevPackages",
    "params": {
      "searchTerm": "state management",
      "category": "trending",
      "includeAnalysis": false
    },
    "dryRun": false
  }' \
  | jq '.'

echo -e "\n🔬 Test 3: Analyze specific package (provider)..."
curl -X POST "$API_BASE" \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "test-analyze-provider",
    "command": "analyzePackage",
    "params": {
      "packageName": "provider"
    },
    "dryRun": false
  }' \
  | jq '.'

echo -e "\n⚡ Test 4: Analyze HTTP package..."
curl -X POST "$API_BASE" \
  -H "Content-Type: application/json" \
  -d '{
    "commandId": "test-analyze-http",
    "command": "analyzePackage",
    "params": {
      "packageName": "http"
    },
    "dryRun": false
  }' \
  | jq '.'

echo -e "\n✅ PubDevService tests completed!"
echo "Check the responses above for package search results and analysis data."
