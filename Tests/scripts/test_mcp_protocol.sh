#!/bin/bash

echo "🚀 Testing MCP Protocol Layer Integration"
echo "========================================"

# Test 1: MCP Capabilities endpoint (REST)
echo "📋 Test 1: MCP Capabilities (REST)"
echo "curl -X GET http://localhost:5171/api/command/capabilities"
curl -X GET http://localhost:5171/api/command/capabilities
echo -e "\n"

# Test 2: Command Registry endpoint (REST)  
echo "📋 Test 2: Command Registry (REST)"
echo "curl -X GET http://localhost:5171/api/command/registry"
curl -X GET http://localhost:5171/api/command/registry
echo -e "\n"

# Test 3: JSON-RPC Capabilities
echo "📋 Test 3: JSON-RPC Capabilities"
echo "curl -X POST http://localhost:5171/api/command/jsonrpc"
curl -X POST http://localhost:5171/api/command/jsonrpc \
  -H "Content-Type: application/json" \
  -d @../json/test_mcp_capabilities.json
echo -e "\n"

# Test 4: JSON-RPC Command Execution
echo "📋 Test 4: JSON-RPC Command Execution (checkFlutterVersion)"
echo "curl -X POST http://localhost:5171/api/command/jsonrpc"
curl -X POST http://localhost:5171/api/command/jsonrpc \
  -H "Content-Type: application/json" \
  -d @../json/test_mcp_jsonrpc.json
echo -e "\n"

echo "✅ MCP Protocol Layer tests completed!"
