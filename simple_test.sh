echo "Testing FileWriterService with simple commands..."

echo "1. Checking server status:"
echo "GET http://localhost:5171/" > test.txt
echo ""

echo "2. Testing if port 5171 is responding:"
if command -v nc >/dev/null 2>&1; then
    echo "quit" | nc -w 1 localhost 5171 && echo "Port 5171 is open" || echo "Port 5171 is closed"
else
    echo "netcat not available, skipping port check"
fi

echo ""
echo "3. Checking for existing test file:"
if [ -f "/tmp/test_flutter_widget.dart" ]; then
    echo "✅ Test file already exists, removing it..."
    rm -f /tmp/test_flutter_widget.dart
else
    echo "ℹ️  No existing test file found"
fi

echo ""
echo "4. Manual test instructions:"
echo "   - Open test_filewriter.http in VS Code"
echo "   - Use 'Send Request' button above each HTTP request"
echo "   - First test the GET request to check server health"
echo "   - Then test the POST request to create a file"
echo "   - Check if file is created with: ls -la /tmp/test_flutter_widget.dart"

echo ""
echo "5. Current server processes:"
ps aux | grep dotnet | grep -v grep || echo "No dotnet processes found"
