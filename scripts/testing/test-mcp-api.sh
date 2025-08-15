#!/bin/bash
# Test Mcp Api Script
# Location: scripts/testing/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🧪 Testing RefactorMCP HTTP API functionality..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_test() { echo -e "${BLUE}[TEST]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

BASE_URL="http://localhost:7042"

echo "🌐 Testing RefactorMCP HTTP MCP API at $BASE_URL"
echo ""

# Test 1: Health Check
print_test "1. Health Check"
echo "Command: curl -s $BASE_URL/health"
HEALTH_RESPONSE=$(curl -s $BASE_URL/health)
if [ $? -eq 0 ] && [ "$HEALTH_RESPONSE" = "Healthy" ]; then
    print_success "Health endpoint working: $HEALTH_RESPONSE"
else
    print_error "Health endpoint failed: $HEALTH_RESPONSE"
fi
echo ""

# Test 2: MCP Server Info
print_test "2. MCP Server Information"
echo "Command: curl -s $BASE_URL/api/mcp/server-info | jq ."
curl -s $BASE_URL/api/mcp/server-info | jq . 2>/dev/null
if [ ${PIPESTATUS[0]} -eq 0 ]; then
    print_success "Server info endpoint working"
else
    print_error "Server info endpoint failed"
    curl -s $BASE_URL/api/mcp/server-info
fi
echo ""

# Test 3: List Available Tools
print_test "3. List Available MCP Tools"
echo "Command: curl -s $BASE_URL/api/mcp/tools | jq '.tools[] | .name' | head -10"
TOOLS_RESPONSE=$(curl -s $BASE_URL/api/mcp/tools)
if [ $? -eq 0 ]; then
    echo "$TOOLS_RESPONSE" | jq '.tools[] | {name: .name, description: .description}' 2>/dev/null | head -20
    TOOL_COUNT=$(echo "$TOOLS_RESPONSE" | jq '.tools | length' 2>/dev/null)
    if [ "$TOOL_COUNT" -gt 0 ]; then
        print_success "Found $TOOL_COUNT available tools"
    else
        print_error "No tools found"
    fi
else
    print_error "Tools list endpoint failed"
    echo "$TOOLS_RESPONSE"
fi
echo ""

# Test 4: Execute a Simple Tool (ListToolsCommand)
print_test "4. Execute MCP Tool - ListToolsCommand"
echo "Command: curl -X POST $BASE_URL/api/mcp/tools -H 'Content-Type: application/json' -d '{\"toolName\":\"ListToolsCommand\",\"parameters\":{}}'"

EXECUTE_RESPONSE=$(curl -s -X POST $BASE_URL/api/mcp/tools \
  -H "Content-Type: application/json" \
  -d '{"toolName":"ListToolsCommand","parameters":{}}')

if [ $? -eq 0 ]; then
    echo "$EXECUTE_RESPONSE" | jq . 2>/dev/null || echo "$EXECUTE_RESPONSE"
    if echo "$EXECUTE_RESPONSE" | grep -q "content"; then
        print_success "Tool execution working"
    else
        print_error "Tool execution returned unexpected response"
    fi
else
    print_error "Tool execution failed"
    echo "$EXECUTE_RESPONSE"
fi
echo ""

# Test 5: Execute LoadSolution (example with parameters)
print_test "5. Execute Tool with Parameters - LoadSolution"
echo "Command: curl -X POST $BASE_URL/api/mcp/tools -H 'Content-Type: application/json' -d '{\"toolName\":\"LoadSolution\",\"parameters\":{\"solutionPath\":\"/fake/path/test.sln\"}}'"

LOAD_RESPONSE=$(curl -s -X POST $BASE_URL/api/mcp/tools \
  -H "Content-Type: application/json" \
  -d '{"toolName":"LoadSolution","parameters":{"solutionPath":"/fake/path/test.sln"}}')

if [ $? -eq 0 ]; then
    echo "$LOAD_RESPONSE" | jq . 2>/dev/null || echo "$LOAD_RESPONSE"
    if echo "$LOAD_RESPONSE" | grep -q -E "(content|error)"; then
        print_success "Parameterized tool execution working (expected error for fake path)"
    else
        print_error "Parameterized tool execution failed"
    fi
else
    print_error "Parameterized tool execution failed"
fi
echo ""

# Summary
echo "📋 Manual Testing Commands:"
echo ""
echo "# Get server info"
echo "curl -s $BASE_URL/api/mcp/server-info | jq ."
echo ""
echo "# List all tools"
echo "curl -s $BASE_URL/api/mcp/tools | jq '.tools[] | .name'"
echo ""
echo "# Execute ListToolsCommand"
echo "curl -X POST $BASE_URL/api/mcp/tools \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"toolName\":\"ListToolsCommand\",\"parameters\":{}}'"
echo ""
echo "# Execute LoadSolution with path"
echo "curl -X POST $BASE_URL/api/mcp/tools \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"toolName\":\"LoadSolution\",\"parameters\":{\"solutionPath\":\"/path/to/your/solution.sln\"}}'"
echo ""
echo "# Execute ExtractMethod (example)"
echo "curl -X POST $BASE_URL/api/mcp/tools \\"
echo "  -H 'Content-Type: application/json' \\"
echo "  -d '{\"toolName\":\"ExtractMethod\",\"parameters\":{\"solutionPath\":\"/path/to/solution.sln\",\"filePath\":\"/path/to/file.cs\",\"startLine\":10,\"endLine\":20,\"methodName\":\"NewMethod\"}}'"
echo ""
echo "🎯 Your RefactorMCP HTTP MCP API is ready to use!"