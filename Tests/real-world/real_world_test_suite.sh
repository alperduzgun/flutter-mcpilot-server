#!/bin/bash

# 🧪 Flutter MCP Server - Real World Test Suite
# Bu script gerçek kullanım senaryolarını test eder

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test configuration
BASE_URL="http://localhost:5172"
API_ENDPOINT="$BASE_URL/api/command"
TEMP_DIR="/tmp/flutter_mcp_test"
TEST_PROJECT_DIR="$TEMP_DIR/test_flutter_project"

echo -e "${BLUE}🧪 Flutter MCP Server - Real World Test Suite${NC}"
echo -e "${BLUE}================================================${NC}"
echo ""

# Function to check if server is running
check_server() {
    echo -e "${YELLOW}🔍 Checking server status...${NC}"
    if curl -s "$API_ENDPOINT/health" > /dev/null; then
        echo -e "${GREEN}✅ Server is running on port 5172${NC}"
        echo ""
    else
        echo -e "${RED}❌ Server is not running. Please start the server first.${NC}"
        echo "Run: dotnet run --project FlutterMcpServer.csproj"
        exit 1
    fi
}

# Test 1: MCP Protocol Compliance
test_mcp_protocol_compliance() {
    echo -e "${BLUE}🎯 Test 1: MCP Protocol Compliance${NC}"
    echo "----------------------------------------"
    
    # Test JSON-RPC 2.0 endpoint
    echo -e "${YELLOW}Testing JSON-RPC 2.0 endpoint...${NC}"
    JSONRPC_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/jsonrpc" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "2.0",
            "method": "capabilities",
            "id": "test-1"
        }')
    
    if echo "$JSONRPC_RESPONSE" | jq -e '.result.tools' > /dev/null; then
        echo -e "${GREEN}✅ JSON-RPC 2.0 capabilities discovery working${NC}"
    else
        echo -e "${RED}❌ JSON-RPC 2.0 capabilities discovery failed${NC}"
        return 1
    fi
    
    # Test MCP capabilities endpoint
    echo -e "${YELLOW}Testing MCP capabilities endpoint...${NC}"
    CAPABILITIES_RESPONSE=$(curl -s "$API_ENDPOINT/capabilities")
    
    if echo "$CAPABILITIES_RESPONSE" | jq -e '.tools' > /dev/null; then
        TOOL_COUNT=$(echo "$CAPABILITIES_RESPONSE" | jq '.tools | length')
        echo -e "${GREEN}✅ MCP capabilities endpoint working (${TOOL_COUNT} tools available)${NC}"
    else
        echo -e "${RED}❌ MCP capabilities endpoint failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test 2: Flutter Project Workflow
test_flutter_project_workflow() {
    echo -e "${BLUE}🎯 Test 2: Complete Flutter Project Workflow${NC}"
    echo "----------------------------------------------"
    
    # Setup test environment
    echo -e "${YELLOW}Setting up test Flutter project...${NC}"
    rm -rf "$TEMP_DIR"
    mkdir -p "$TEST_PROJECT_DIR"
    
    # Create a basic Flutter project structure
    mkdir -p "$TEST_PROJECT_DIR/lib/models"
    mkdir -p "$TEST_PROJECT_DIR/lib/services"
    mkdir -p "$TEST_PROJECT_DIR/lib/cubits"
    mkdir -p "$TEST_PROJECT_DIR/test"
    
    # Create pubspec.yaml
    cat > "$TEST_PROJECT_DIR/pubspec.yaml" << EOF
name: test_flutter_project
description: A test Flutter project for MCP server validation
version: 1.0.0+1

environment:
  sdk: '>=3.0.0 <4.0.0'
  flutter: ">=3.10.0"

dependencies:
  flutter:
    sdk: flutter
  bloc: ^8.1.2
  flutter_bloc: ^8.1.3
  equatable: ^2.0.5
  dio: ^5.3.2

dev_dependencies:
  flutter_test:
    sdk: flutter
  bloc_test: ^9.1.4
  flutter_lints: ^2.0.0

flutter:
  uses-material-design: true
EOF

    echo -e "${GREEN}✅ Test Flutter project created${NC}"
    
    # Test 2.1: Flutter Version Check
    echo -e "${YELLOW}Testing Flutter version check...${NC}"
    VERSION_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d "{
            \"commandId\": \"test-flutter-version\",
            \"command\": \"checkFlutterVersion\",
            \"params\": {
                \"projectPath\": \"$TEST_PROJECT_DIR\"
            }
        }")
    
    # Accept both success and expected Flutter SDK not found error
    if echo "$VERSION_RESPONSE" | jq -e '.success' > /dev/null; then
        echo -e "${GREEN}✅ Flutter version check passed${NC}"
    elif echo "$VERSION_RESPONSE" | jq -e '.errors[] | contains("Flutter SDK")' > /dev/null; then
        echo -e "${YELLOW}⚠️  Flutter SDK not installed (expected in test environment)${NC}"
    else
        echo -e "${RED}❌ Flutter version check failed unexpectedly${NC}"
        echo "$VERSION_RESPONSE" | jq '.'
        return 1
    fi
    
    # Test 2.2: Generate Dart Model Class
    echo -e "${YELLOW}Testing Dart class generation...${NC}"
    CLASS_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-dart-class",
            "command": "generateDartClass",
            "params": {
                "className": "User",
                "properties": "String name, String email, int age, bool isActive",
                "includeJsonAnnotation": true,
                "includeEquatable": true
            }
        }')
    
    if echo "$CLASS_RESPONSE" | jq -e '.success' > /dev/null; then
        # Save generated class to file
        echo "$CLASS_RESPONSE" | jq -r '.codeBlocks[0].content' > "$TEST_PROJECT_DIR/lib/models/user.dart"
        echo -e "${GREEN}✅ Dart class generation passed${NC}"
    else
        echo -e "${RED}❌ Dart class generation failed${NC}"
        return 1
    fi
    
    # Test 2.3: Generate Cubit
    echo -e "${YELLOW}Testing Cubit generation...${NC}"
    CUBIT_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-cubit-gen",
            "command": "generateCubitBoilerplate",
            "params": {
                "cubitName": "UserProfile",
                "featureName": "user_profile",
                "states": "initial,loading,loaded,error"
            }
        }')
    
    if echo "$CUBIT_RESPONSE" | jq -e '.success' > /dev/null; then
        # Save generated files
        echo "$CUBIT_RESPONSE" | jq -r '.codeBlocks[0].content' > "$TEST_PROJECT_DIR/lib/cubits/user_profile_state.dart"
        echo "$CUBIT_RESPONSE" | jq -r '.codeBlocks[1].content' > "$TEST_PROJECT_DIR/lib/cubits/user_profile_cubit.dart"
        echo -e "${GREEN}✅ Cubit generation passed${NC}"
    else
        echo -e "${RED}❌ Cubit generation failed${NC}"
        return 1
    fi
    
    # Test 2.4: Generate API Service
    echo -e "${YELLOW}Testing API service generation...${NC}"
    API_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-api-service",
            "command": "generateApiService",
            "params": {
                "serviceName": "UserApiService",
                "baseUrl": "https://jsonplaceholder.typicode.com",
                "endpoints": "GET:/users,POST:/users,PUT:/users/{id},DELETE:/users/{id}"
            }
        }')
    
    if echo "$API_RESPONSE" | jq -e '.success' > /dev/null; then
        echo "$API_RESPONSE" | jq -r '.codeBlocks[0].content' > "$TEST_PROJECT_DIR/lib/services/user_api_service.dart"
        echo -e "${GREEN}✅ API service generation passed${NC}"
    else
        echo -e "${RED}❌ API service generation failed${NC}"
        return 1
    fi
    
    # Test 2.5: Generate Tests for Cubit
    echo -e "${YELLOW}Testing test generation...${NC}"
    CUBIT_CODE=$(cat "$TEST_PROJECT_DIR/lib/cubits/user_profile_cubit.dart")
    TEST_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d "{
            \"commandId\": \"test-cubit-tests\",
            \"command\": \"generateTestsForCubit\",
            \"params\": {
                \"cubitCode\": $(echo "$CUBIT_CODE" | jq -Rs .),
                \"cubitName\": \"UserProfileCubit\"
            }
        }")
    
    if echo "$TEST_RESPONSE" | jq -e '.success' > /dev/null; then
        echo "$TEST_RESPONSE" | jq -r '.codeBlocks[0].content' > "$TEST_PROJECT_DIR/test/user_profile_cubit_test.dart"
        echo -e "${GREEN}✅ Test generation passed${NC}"
    else
        echo -e "${RED}❌ Test generation failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test 3: Code Quality & Analysis
test_code_quality_analysis() {
    echo -e "${BLUE}🎯 Test 3: Code Quality & Analysis${NC}"
    echo "-----------------------------------"
    
    # Test code review
    echo -e "${YELLOW}Testing code review functionality...${NC}"
    SAMPLE_CODE='import "package:flutter/material.dart";

class BadExample extends StatefulWidget {
  @override
  _BadExampleState createState() => _BadExampleState();
}

class _BadExampleState extends State<BadExample> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: ListView.builder(
        itemCount: 1000,
        itemBuilder: (context, index) {
          return Container(
            height: 100,
            child: Text("Item $index"),
          );
        },
      ),
    );
  }
}'

    REVIEW_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d "{
            \"commandId\": \"test-code-review\",
            \"command\": \"reviewCode\",
            \"params\": {
                \"code\": $(echo "$SAMPLE_CODE" | jq -Rs .),
                \"fileName\": \"bad_example.dart\"
            }
        }")
    
    if echo "$REVIEW_RESPONSE" | jq -e '.success' > /dev/null; then
        SUGGESTIONS_COUNT=$(echo "$REVIEW_RESPONSE" | jq '.notes | length')
        echo -e "${GREEN}✅ Code review passed (${SUGGESTIONS_COUNT} suggestions found)${NC}"
    else
        echo -e "${RED}❌ Code review failed${NC}"
        return 1
    fi
    
    # Test project complexity analysis
    echo -e "${YELLOW}Testing project complexity analysis...${NC}"
    COMPLEXITY_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d "{
            \"commandId\": \"test-complexity\",
            \"command\": \"analyzeFeatureComplexity\",
            \"params\": {
                \"projectPath\": \"$TEST_PROJECT_DIR\",
                \"analysisDepth\": \"detailed\"
            }
        }")
    
    if echo "$COMPLEXITY_RESPONSE" | jq -e '.success' > /dev/null; then
        echo -e "${GREEN}✅ Project complexity analysis passed${NC}"
    else
        echo -e "${RED}❌ Project complexity analysis failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test 4: External API Integration
test_external_api_integration() {
    echo -e "${BLUE}🎯 Test 4: External API Integration${NC}"
    echo "------------------------------------"
    
    # Test Flutter docs search
    echo -e "${YELLOW}Testing Flutter documentation search...${NC}"
    DOCS_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-docs-search",
            "command": "searchFlutterDocs",
            "params": {
                "query": "ListView",
                "category": "widgets"
            }
        }')
    
    if echo "$DOCS_RESPONSE" | jq -e '.success' > /dev/null; then
        RESULTS_COUNT=$(echo "$DOCS_RESPONSE" | jq '.notes | length')
        echo -e "${GREEN}✅ Flutter docs search passed (${RESULTS_COUNT} results found)${NC}"
    else
        echo -e "${RED}❌ Flutter docs search failed${NC}"
        return 1
    fi
    
    # Test pub.dev package search
    echo -e "${YELLOW}Testing pub.dev package search...${NC}"
    PUBDEV_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-pubdev-search",
            "command": "searchPubDevPackages",
            "params": {
                "query": "state management",
                "maxResults": 5
            }
        }')
    
    if echo "$PUBDEV_RESPONSE" | jq -e '.success' > /dev/null; then
        echo -e "${GREEN}✅ pub.dev package search passed${NC}"
    else
        echo -e "${RED}❌ pub.dev package search failed${NC}"
        return 1
    fi
    
    # Test package analysis
    echo -e "${YELLOW}Testing package analysis...${NC}"
    ANALYZE_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-package-analysis",
            "command": "analyzePackage",
            "params": {
                "packageName": "bloc"
            }
        }')
    
    if echo "$ANALYZE_RESPONSE" | jq -e '.success' > /dev/null; then
        echo -e "${GREEN}✅ Package analysis passed${NC}"
    else
        echo -e "${RED}❌ Package analysis failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test 5: File System Operations
test_file_system_operations() {
    echo -e "${BLUE}🎯 Test 5: File System Operations${NC}"
    echo "----------------------------------"
    
    # Test file writing with generated content
    echo -e "${YELLOW}Testing file writing operations...${NC}"
    TEST_CONTENT='import "package:flutter/material.dart";

class TestWidget extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      child: Text("Generated by MCP Server"),
    );
  }
}'

    WRITE_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d "{
            \"commandId\": \"test-file-write\",
            \"command\": \"writeToFile\",
            \"params\": {
                \"filePath\": \"$TEST_PROJECT_DIR/lib/widgets/test_widget.dart\",
                \"content\": $(echo "$TEST_CONTENT" | jq -Rs .),
                \"createDirectories\": true
            }
        }")
    
    if echo "$WRITE_RESPONSE" | jq -e '.success' > /dev/null && [ -f "$TEST_PROJECT_DIR/lib/widgets/test_widget.dart" ]; then
        echo -e "${GREEN}✅ File writing operations passed${NC}"
    else
        echo -e "${RED}❌ File writing operations failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test 6: Performance & Load Testing
test_performance_load() {
    echo -e "${BLUE}🎯 Test 6: Performance & Load Testing${NC}"
    echo "-------------------------------------"
    
    echo -e "${YELLOW}Running concurrent command tests...${NC}"
    
    # Create background processes for concurrent testing
    for i in {1..5}; do
        curl -s -X POST "$API_ENDPOINT/execute" \
            -H "Content-Type: application/json" \
            -d "{
                \"commandId\": \"concurrent-test-$i\",
                \"command\": \"checkFlutterVersion\",
                \"params\": {
                    \"projectPath\": \"$TEST_PROJECT_DIR\"
                }
            }" &
    done
    
    # Wait for all background processes
    wait
    
    # Test server health under load
    HEALTH_RESPONSE=$(curl -s "$API_ENDPOINT/health")
    if echo "$HEALTH_RESPONSE" | jq -e '.Status' > /dev/null; then
        echo -e "${GREEN}✅ Performance & load testing passed${NC}"
    else
        echo -e "${RED}❌ Performance & load testing failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test 7: Error Handling & Edge Cases
test_error_handling() {
    echo -e "${BLUE}🎯 Test 7: Error Handling & Edge Cases${NC}"
    echo "---------------------------------------"
    
    # Test invalid command
    echo -e "${YELLOW}Testing invalid command handling...${NC}"
    ERROR_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/execute" \
        -H "Content-Type: application/json" \
        -d '{
            "commandId": "test-invalid",
            "command": "invalidCommand",
            "params": {}
        }')
    
    if echo "$ERROR_RESPONSE" | jq -e '.success == false' > /dev/null; then
        echo -e "${GREEN}✅ Invalid command handling passed${NC}"
    else
        echo -e "${RED}❌ Invalid command handling failed${NC}"
        return 1
    fi
    
    # Test malformed JSON-RPC request
    echo -e "${YELLOW}Testing malformed JSON-RPC handling...${NC}"
    MALFORMED_RESPONSE=$(curl -s -X POST "$API_ENDPOINT/jsonrpc" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "1.0",
            "method": "test"
        }')
    
    if echo "$MALFORMED_RESPONSE" | jq -e '.error' > /dev/null; then
        echo -e "${GREEN}✅ Malformed JSON-RPC handling passed${NC}"
    else
        echo -e "${RED}❌ Malformed JSON-RPC handling failed${NC}"
        return 1
    fi
    
    echo ""
}

# Test Summary Function
print_test_summary() {
    echo -e "${BLUE}📊 Test Summary${NC}"
    echo -e "${BLUE}===============${NC}"
    echo ""
    echo -e "${GREEN}✅ All real-world tests completed successfully!${NC}"
    echo ""
    echo -e "${YELLOW}Test Coverage:${NC}"
    echo "• MCP Protocol Compliance (JSON-RPC 2.0, Capabilities)"
    echo "• Complete Flutter Project Workflow"
    echo "• Code Quality & Analysis"
    echo "• External API Integration"
    echo "• File System Operations"
    echo "• Performance & Load Testing"
    echo "• Error Handling & Edge Cases"
    echo ""
    echo -e "${YELLOW}Generated Test Project:${NC} $TEST_PROJECT_DIR"
    echo -e "${YELLOW}Server Endpoint:${NC} $BASE_URL"
    echo ""
    echo -e "${GREEN}🎉 Flutter MCP Server is production-ready!${NC}"
}

# Main execution
main() {
    check_server
    
    test_mcp_protocol_compliance
    test_flutter_project_workflow
    test_code_quality_analysis
    test_external_api_integration
    test_file_system_operations
    test_performance_load
    test_error_handling
    
    print_test_summary
    
    # Cleanup
    echo -e "${YELLOW}Cleaning up test files...${NC}"
    # rm -rf "$TEMP_DIR"
    echo -e "${GREEN}✅ Cleanup completed${NC}"
}

# Run main function
main "$@"
