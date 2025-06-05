#!/bin/zsh

echo "=== FileWriterService Test ==="
echo "Date: $(date)"
echo

# Function to test endpoint
test_endpoint() {
    local method=$1
    local url=$2
    local data=$3
    local description=$4
    
    echo "Testing: $description"
    echo "Method: $method"
    echo "URL: $url"
    
    if [ -n "$data" ]; then
        echo "Data: $data"
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}\n" -X "$method" "$url" \
                   -H "Content-Type: application/json" \
                   -d "$data" 2>&1)
    else
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}\n" -X "$method" "$url" 2>&1)
    fi
    
    echo "Response:"
    echo "$response"
    echo "---"
    echo
}

# Wait a bit for server to be ready
echo "Waiting for server to be ready..."
sleep 5

# Test 1: Health check
test_endpoint "GET" "http://localhost:5171/" "" "Health Check Endpoint"

# Test 2: FileWriter service
filewriter_data='{
  "command": "writeFile",
  "params": {
    "filePath": "/tmp/test_flutter_widget.dart",
    "content": "import '\''package:flutter/material.dart'\'';\n\nclass TestWidget extends StatelessWidget {\n  const TestWidget({Key? key}) : super(key: key);\n\n  @override\n  Widget build(BuildContext context) {\n    return Container(\n      child: Text('\''Hello Flutter MCP!'\''),\n    );\n  }\n}",
    "createDirectories": true,
    "overwrite": true,
    "encoding": "utf-8"
  },
  "dryRun": false,
  "commandId": "test-write-file-001",
  "timestamp": "2025-06-06T02:00:00Z"
}'

test_endpoint "POST" "http://localhost:5171/api/command/execute" "$filewriter_data" "FileWriter Service"

# Test 3: Check if file was created
echo "Checking if file was created..."
if [ -f "/tmp/test_flutter_widget.dart" ]; then
    echo "✅ SUCCESS: File was created!"
    echo "File size: $(wc -c < /tmp/test_flutter_widget.dart) bytes"
    echo "First 100 characters:"
    head -c 100 /tmp/test_flutter_widget.dart
    echo
    echo "..."
else
    echo "❌ FAILED: File was not created"
fi

echo
echo "=== Test Complete ==="
