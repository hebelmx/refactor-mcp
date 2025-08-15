#!/bin/bash
# Deploy RefactorMCP Script
# Location: scripts/maintenance/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"

PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

show_help() {
    print_header "RefactorMCP Deployment Script"
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  --build-only     Build application without deploying"
    echo "  --deploy-only    Deploy pre-built application"
    echo "  --full          Full build and deploy (default)"
    echo "  --no-restart    Don't restart services"
    echo ""
    echo "Examples:"
    echo "  $0               # Full build and deploy"
    echo "  $0 --build-only  # Only build application"
    echo "  $0 --deploy-only # Only deploy existing build"
}

get_compose_cmd() {
    if docker compose version >/dev/null 2>&1; then
        echo "docker compose"
    else
        echo "docker-compose"
    fi
}

handle_existing_seq_container() {
    print_step 1 "Handling existing Seq container"
    
    if docker ps -a --format "{{.Names}}" | grep -q "^refactor-mcp-seq$"; then
        print_substep "Found existing refactor-mcp-seq container"
        
        # Check if it's running
        if docker ps --format "{{.Names}}" | grep -q "^refactor-mcp-seq$"; then
            print_substep "Container is running, stopping it..."
            docker stop refactor-mcp-seq
        fi
        
        # Remove the container so Docker Compose can recreate it
        print_substep "Removing existing container..."
        docker rm refactor-mcp-seq
        
        print_success "Existing container cleaned up"
    else
        print_substep "No existing container found"
    fi
}

setup_seq_services() {
    print_step 2 "Setting up Seq services"
    
    cd "$PROJECT_ROOT"
    
    # Ensure data directory exists
    ensure_directory "/opt/seq/data" "5341:5341"
    
    # Handle existing container
    handle_existing_seq_container
    
    # Start Seq with Docker Compose
    local compose_cmd=$(get_compose_cmd)
    print_substep "Starting Seq with $compose_cmd..."
    
    $compose_cmd up -d seq
    
    # Wait for Seq to be ready
    print_substep "Waiting for Seq to be ready..."
    for i in {1..30}; do
        if curl -s http://localhost:5341/api >/dev/null 2>&1; then
            print_success "✅ Seq is ready"
            break
        fi
        sleep 1
        echo -n "."
    done
    
    if [ $i -eq 30 ]; then
        print_error "❌ Seq failed to start"
        return 1
    fi
}

build_application() {
    print_step 3 "Building RefactorMCP application"
    
    cd "$PROJECT_ROOT"
    export PATH="$HOME/.dotnet:$PATH"
    
    print_substep "Building self-contained application..."
    dotnet publish RefactorMCP.Web/RefactorMCP.Web.csproj \
        --configuration Release \
        --runtime linux-x64 \
        --self-contained true \
        --output ./publish-deploy \
        /p:PublishSingleFile=true \
        /p:PublishTrimmed=false \
        /p:IncludeNativeLibrariesForSelfExtract=true
    
    if [ $? -eq 0 ]; then
        print_success "✅ Application built successfully"
    else
        print_error "❌ Build failed"
        return 1
    fi
}

deploy_application() {
    print_step 4 "Deploying RefactorMCP application"
    
    cd "$PROJECT_ROOT"
    
    # Stop service during deployment
    print_substep "Stopping RefactorMCP service..."
    sudo systemctl stop refactor-mcp 2>/dev/null || true
    
    # Backup current deployment
    if [ -d "$REFACTOR_MCP_DIR" ]; then
        print_substep "Backing up current deployment..."
        sudo cp -r "$REFACTOR_MCP_DIR" "${REFACTOR_MCP_DIR}.backup.$(date +%Y%m%d_%H%M%S)"
    fi
    
    # Deploy new application
    print_substep "Deploying new application..."
    sudo mkdir -p "$REFACTOR_MCP_DIR"
    sudo cp -r ./publish-deploy/* "$REFACTOR_MCP_DIR/"
    sudo chmod +x "$REFACTOR_MCP_DIR/RefactorMCP.Web"
    sudo chown -R abel:abel "$REFACTOR_MCP_DIR"
    
    print_success "✅ Application deployed"
}

restart_services() {
    print_step 5 "Restarting services"
    
    # Start RefactorMCP service (it will wait for Seq)
    print_substep "Starting RefactorMCP service..."
    sudo systemctl start refactor-mcp
    
    # Wait for service to be ready
    if wait_for_service "refactor-mcp" 30; then
        print_success "✅ RefactorMCP service started"
    else
        print_error "❌ RefactorMCP service failed to start"
        sudo journalctl -u refactor-mcp --no-pager -l -n 10
        return 1
    fi
}

test_deployment() {
    print_step 6 "Testing deployment"
    
    # Test endpoints
    local endpoints=(
        "http://localhost:7042/health|Health endpoint"
        "http://localhost:7042/api/mcp/server-info|MCP server info"
        "http://localhost:5341|Seq logging"
    )
    
    for endpoint_info in "${endpoints[@]}"; do
        local url="${endpoint_info%|*}"
        local desc="${endpoint_info#*|}"
        test_endpoint "$url" "$desc"
    done
    
    print_success "✅ Deployment testing completed"
}

cleanup() {
    print_step 7 "Cleanup"
    
    cd "$PROJECT_ROOT"
    
    # Remove build artifacts
    if [ -d "./publish-deploy" ]; then
        rm -rf ./publish-deploy
        print_substep "Removed build artifacts"
    fi
    
    # Clean old backups (keep last 3)
    if ls "${REFACTOR_MCP_DIR}.backup."* >/dev/null 2>&1; then
        print_substep "Cleaning old backups..."
        ls -dt "${REFACTOR_MCP_DIR}.backup."* | tail -n +4 | sudo xargs rm -rf
    fi
    
    print_success "✅ Cleanup completed"
}

full_deploy() {
    print_header "Full RefactorMCP Deployment"
    
    setup_seq_services
    build_application
    deploy_application
    restart_services
    test_deployment
    cleanup
    
    print_success "🎉 Full deployment completed!"
    show_summary
}

build_only() {
    print_header "Building RefactorMCP Application"
    
    build_application
    
    print_success "✅ Build completed!"
    echo "Run with --deploy-only to deploy the built application"
}

deploy_only() {
    print_header "Deploying RefactorMCP Application"
    
    if [ ! -d "./publish-deploy" ]; then
        print_error "No build found. Run with --build-only first or use --full"
        exit 1
    fi
    
    setup_seq_services
    deploy_application
    restart_services
    test_deployment
    cleanup
    
    print_success "✅ Deployment completed!"
    show_summary
}

main() {
    case "${1:-full}" in
        --build-only)
            build_only
            ;;
        --deploy-only)
            deploy_only
            ;;
        --full|full)
            full_deploy
            ;;
        --no-restart)
            setup_seq_services
            build_application  
            deploy_application
            test_deployment
            cleanup
            print_success "✅ Deployment completed (no restart)"
            ;;
        --help|help)
            show_help
            ;;
        *)
            full_deploy
            ;;
    esac
}

main "$@"