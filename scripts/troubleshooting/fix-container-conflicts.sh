#!/bin/bash
# Fix Container Conflicts Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"

main() {
    print_header "Fixing Container Conflicts"
    
    fix_seq_container_conflict
    restart_with_compose
    test_services
    
    print_success "✅ Container conflicts resolved!"
    show_summary
}

fix_seq_container_conflict() {
    print_step 1 "Resolving Seq container conflicts"
    
    # Check for existing containers
    if docker ps -a --format "{{.Names}}" | grep -q "^refactor-mcp-seq$"; then
        print_substep "Found existing refactor-mcp-seq container"
        
        # Show container info
        docker ps -a --filter "name=refactor-mcp-seq" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        
        # Stop if running
        if docker ps --format "{{.Names}}" | grep -q "^refactor-mcp-seq$"; then
            print_substep "Stopping running container..."
            docker stop refactor-mcp-seq
        fi
        
        # Remove container
        print_substep "Removing existing container..."
        docker rm refactor-mcp-seq
        
        print_success "Existing container removed"
    else
        print_substep "No conflicting containers found"
    fi
    
    # Clean up any orphaned volumes or networks
    print_substep "Cleaning up Docker resources..."
    docker system prune -f >/dev/null 2>&1 || true
}

restart_with_compose() {
    print_step 2 "Starting services with Docker Compose"
    
    cd "$(dirname "$SCRIPT_DIR")"
    
    local compose_cmd
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    else
        compose_cmd="docker-compose"
    fi
    
    print_substep "Using: $compose_cmd"
    
    # Start Seq
    $compose_cmd up -d seq
    
    # Wait for Seq to be ready
    print_substep "Waiting for Seq to start..."
    for i in {1..20}; do
        if curl -s http://localhost:5341/api >/dev/null 2>&1; then
            print_success "✅ Seq started successfully"
            break
        fi
        sleep 1
        echo -n "."
    done
    
    if [ $i -eq 20 ]; then
        print_error "❌ Seq failed to start"
        $compose_cmd logs seq
        return 1
    fi
}

test_services() {
    print_step 3 "Testing services"
    
    # Test Seq
    if test_endpoint "http://localhost:5341" "Seq web interface"; then
        print_success "Seq is working"
    fi
    
    # Test RefactorMCP if it's running
    if check_service_running "refactor-mcp"; then
        test_endpoint "http://localhost:7042/health" "RefactorMCP health"
    else
        print_substep "RefactorMCP service not running, starting it..."
        sudo systemctl start refactor-mcp
        wait_for_service "refactor-mcp" 15
        test_endpoint "http://localhost:7042/health" "RefactorMCP health"
    fi
}

show_summary() {
    print_section "Container Status"
    
    cd "$(dirname "$SCRIPT_DIR")"
    
    local compose_cmd
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    else
        compose_cmd="docker-compose"  
    fi
    
    echo ""
    echo "Docker Compose Status:"
    $compose_cmd ps
    
    echo ""
    echo "🐳 Container Management:"
    echo "  Start:    $compose_cmd up -d"
    echo "  Stop:     $compose_cmd down"
    echo "  Restart:  $compose_cmd restart seq"
    echo "  Logs:     $compose_cmd logs -f seq"
    echo ""
}

main "$@"