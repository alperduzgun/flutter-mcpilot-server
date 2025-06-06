#!/usr/bin/env node

const http = require('http');
const { spawn } = require('child_process');

// MCP Server proxy for HTTP-based Flutter MCP Server
class McpHttpProxy {
    constructor() {
        this.serverUrl = 'http://localhost:5172';
        this.init();
    }

    init() {
        // Send initial capabilities
        this.sendCapabilities();
        
        // Listen for JSON-RPC requests on stdin
        process.stdin.on('data', (data) => {
            this.handleRequest(data.toString());
        });
    }

    sendCapabilities() {
        const capabilities = {
            jsonrpc: "2.0",
            id: 0,
            result: {
                protocolVersion: "2.0",
                capabilities: {
                    tools: {},
                    logging: {}
                },
                serverInfo: {
                    name: "Flutter MCP Server",
                    version: "1.0.0"
                }
            }
        };
        
        process.stdout.write(JSON.stringify(capabilities) + '\n');
    }

    async handleRequest(requestData) {
        let request = null;
        try {
            request = JSON.parse(requestData);
            
            // Handle initialize request locally
            if (request.method === 'initialize') {
                const initResponse = {
                    jsonrpc: "2.0",
                    id: request.id,
                    result: {
                        protocolVersion: "2.0",
                        capabilities: {
                            tools: {},
                            logging: {}
                        },
                        serverInfo: {
                            name: "Flutter MCP Server",
                            version: "1.0.0"
                        }
                    }
                };
                process.stdout.write(JSON.stringify(initResponse) + '\n');
                return;
            }
            
            // Forward other requests to HTTP server
            const response = await this.forwardToHttpServer(request);
            process.stdout.write(JSON.stringify(response) + '\n');
            
        } catch (error) {
            const errorResponse = {
                jsonrpc: "2.0",
                id: request?.id || null,
                error: {
                    code: -32603,
                    message: "Internal error",
                    data: error.message
                }
            };
            process.stdout.write(JSON.stringify(errorResponse) + '\n');
        }
    }

    forwardToHttpServer(request) {
        return new Promise((resolve, reject) => {
            const postData = JSON.stringify(request);
            
            const options = {
                hostname: 'localhost',
                port: 5172,
                path: '/api/command/jsonrpc',
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(postData)
                }
            };

            const req = http.request(options, (res) => {
                let data = '';
                res.on('data', (chunk) => {
                    data += chunk;
                });
                res.on('end', () => {
                    try {
                        const response = JSON.parse(data);
                        resolve(response);
                    } catch (e) {
                        reject(new Error('Invalid JSON response from server'));
                    }
                });
            });

            req.on('error', (error) => {
                reject(error);
            });

            req.write(postData);
            req.end();
        });
    }
}

// Start the proxy
new McpHttpProxy();
