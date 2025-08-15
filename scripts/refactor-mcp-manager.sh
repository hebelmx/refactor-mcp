#!/bin/bash
# RefactorMCP Main Management Script
# Usage: ./refactor-mcp-manager.sh [setup|maintain|troubleshoot|test]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common/colors.sh"
source "$SCRIPT_DIR/common/utils.sh"

show_help() {
    print_header "RefactorMCP Manager"
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  setup         - Initial setup and installation"
    echo "  maintain      - Maintenance and updates"
    echo "  troubleshoot  - Debugging and problem resolution"
    echo "  test          - Run tests and validation"
    echo "  health        - Quick health check"
    echo ""
    echo "Examples:"
    echo "  $0 setup      # Initial installation"
    echo "  $0 health     # Check system status"
    echo "  $0 test       # Run API tests"
}

quick_health() {
    print_header "RefactorMCP Health Check"
    
    # Check service
    if check_service_running "$SERVICE_NAME"; then
        print_success "✅ Service is running"
    else
        print_error "❌ Service is not running"
        return 1
    fi
    
    # Test endpoints
    test_endpoint "http://localhost:$HTTP_PORT/health" "Health endpoint"
    test_endpoint "http://localhost:$HTTP_PORT/api/mcp/server-info" "MCP API"
    test_endpoint "http://localhost:$SEQ_PORT" "Seq logging"
    
    show_summary
}

case "${1:-help}" in
    setup)
        print_header "Setup Mode"
        echo "Available setup scripts:"
        ls -1 "$SCRIPT_DIR/setup/"
        ;;
    maintain)
        print_header "Maintenance Mode"
        echo "Available maintenance scripts:"
        ls -1 "$SCRIPT_DIR/maintenance/"
        ;;
    troubleshoot)
        print_header "Troubleshoot Mode"
        echo "Available troubleshooting scripts:"
        ls -1 "$SCRIPT_DIR/troubleshooting/"
        ;;
    test)
        print_header "Test Mode"
        "$SCRIPT_DIR/testing/test-mcp-api.sh"
        ;;
    health)
        quick_health
        ;;
    help|*)
        show_help
        ;;
esac
