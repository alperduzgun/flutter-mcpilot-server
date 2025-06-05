#!/bin/bash

echo "Testing Flutter MCP Server API..."

# Test health endpoint
echo "=== Testing Health Endpoint ==="
curl -s http://localhost:5171/ | jq '.' 2>/dev/null || echo "Health endpoint not responding"

# Test commands list
echo -e "\n=== Testing Commands List ==="
curl -s http://localhost:5171/api/command/commands | jq '.' 2>/dev/null || echo "Commands endpoint not responding"

# Test code review
echo -e "\n=== Testing Code Review ==="
curl -X POST http://localhost:5171/api/command/execute \
  -H "Content-Type: application/json" \
  -d @test_reviewcode.json | jq '.' 2>/dev/null || echo "Code review endpoint not responding"

# Test Flutter version check
echo -e "\n=== Testing Flutter Version Check ==="
curl -X POST http://localhost:5171/api/command/execute \
  -H "Content-Type: application/json" \
  -d @test_checkflutter.json | jq '.' 2>/dev/null || echo "Flutter version check endpoint not responding"

# Test test generation
echo -e "\n=== Testing Test Generation ==="
curl -X POST http://localhost:5171/api/command/execute \
  -H "Content-Type: application/json" \
  -d @test_generatetests.json | jq '.' 2>/dev/null || echo "Test generation endpoint not responding"

# Test navigation migration
echo -e "\n=== Testing Navigation Migration ==="
curl -X POST http://localhost:5171/api/command/execute \
  -H "Content-Type: application/json" \
  -d @test_migratenavigation.json | jq '.' 2>/dev/null || echo "Navigation migration endpoint not responding"
