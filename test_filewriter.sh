#!/bin/bash

echo "Testing FileWriterService..."

# Test health endpoint first
echo "1. Testing health endpoint:"
curl -s http://localhost:5171/

echo -e "\n\n2. Testing FileWriter command:"
curl -s -X POST http://localhost:5171/api/flutter/command \
  -H "Content-Type: application/json" \
  -d @test_writefile.json

echo -e "\n\n3. Checking if file was created:"
ls -la /tmp/test_flutter_widget.dart

echo -e "\n\n4. File contents:"
cat /tmp/test_flutter_widget.dart

echo -e "\n\nTest completed."
