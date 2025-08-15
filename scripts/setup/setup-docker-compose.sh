#!/bin/bash
# Setup Docker Compose For Seq Script
# Location: scripts/setup/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"

main() {
    print_header "Setting up Docker Compose for Seq"
    
    check_prerequisites
    setup_seq_directory
    setup_docker_compose
    create_systemd_integration
    test_setup
    
    print_success "✅ Docker Compose setup completed!"
    show_summary
}

check_prerequisites() {
    print_step 1 "Checking prerequisites"
    
    if ! check_command "docker"; then
        print_error "Docker is not installed"
        exit 1
    fi
    
    if ! check_command "docker-compose" && ! docker compose version >/dev/null 2>&1; then
        print_error "Docker Compose is not installed"
        exit 1
    fi
    
    print_success "Prerequisites satisfied"
}

setup_seq_directory() {
    print_step 2 "Setting up Seq data directory"
    
    # Create persistent data directory
    sudo mkdir -p /opt/seq/data
    sudo chown 5341:5341 /opt/seq/data
    
    # Set proper permissions
    sudo chmod 755 /opt/seq
    sudo chmod 775 /opt/seq/data
    
    print_success "Seq data directory created: /opt/seq/data"
}

setup_docker_compose() {
    print_step 3 "Setting up Docker Compose"
    
    # Stop any existing manual containers
    print_substep "Stopping existing containers..."
    docker stop refactor-mcp-seq 2>/dev/null || true
    docker rm refactor-mcp-seq 2>/dev/null || true
    
    # Start with Docker Compose
    print_substep "Starting Seq with Docker Compose..."
    if docker compose version >/dev/null 2>&1; then
        # Docker Compose V2
        docker compose up -d seq
    else
        # Docker Compose V1
        docker-compose up -d seq
    fi
    
    print_success "Seq started with Docker Compose"
}

create_systemd_integration() {
    print_step 4 "Creating systemd integration"
    
    # Create systemd service for Docker Compose
    sudo tee /etc/systemd/system/refactor-mcp-seq.service > /dev/null <<EOF
[Unit]
Description=RefactorMCP Seq Logging Service
Documentation=https://docs.datalust.co/docs/getting-started
After=docker.service
Wants=docker.service
RequiresMountsFor=/opt/seq/data

[Service]
Type=oneshot
RemainAfterExit=true
WorkingDirectory=$(pwd)
Environment="COMPOSE_PROJECT_NAME=refactor-mcp"

# Start command
ExecStart=/bin/bash -c 'if docker compose version >/dev/null 2>&1; then docker compose up -d seq; else docker-compose up -d seq; fi'

# Stop command  
ExecStop=/bin/bash -c 'if docker compose version >/dev/null 2>&1; then docker compose down; else docker-compose down; fi'

# Health check
ExecReload=/bin/bash -c 'if docker compose version >/dev/null 2>&1; then docker compose restart seq; else docker-compose restart seq; fi'

TimeoutStartSec=60
TimeoutStopSec=30

[Install]
WantedBy=multi-user.target
EOF

    # Update main RefactorMCP service to depend on Seq
    print_substep "Updating main service dependencies..."
    sudo sed -i '/^After=/c\After=network.target docker.service refactor-mcp-seq.service' /etc/systemd/system/refactor-mcp.service
    sudo sed -i '/^Wants=/c\Wants=docker.service refactor-mcp-seq.service' /etc/systemd/system/refactor-mcp.service
    
    # Add startup delay for proper sequencing
    if ! grep -q "ExecStartPre.*sleep" /etc/systemd/system/refactor-mcp.service; then
        sudo sed -i '/^ExecStart=/i ExecStartPre=/bin/sleep 3' /etc/systemd/system/refactor-mcp.service
    fi
    
    # Reload systemd
    sudo systemctl daemon-reload
    sudo systemctl enable refactor-mcp-seq.service
    
    print_success "Systemd integration configured"
}

test_setup() {
    print_step 5 "Testing setup"
    
    # Wait for Seq to be ready
    print_substep "Waiting for Seq to be ready..."
    for i in {1..30}; do
        if curl -s http://localhost:5341/api >/dev/null 2>&1; then
            print_success "✅ Seq is responding"
            break
        fi
        sleep 1
        echo -n "."
    done
    
    if [ $i -eq 30 ]; then
        print_error "❌ Seq failed to start within 30 seconds"
        return 1
    fi
    
    # Test Docker Compose status
    print_substep "Checking Docker Compose status..."
    if docker compose version >/dev/null 2>&1; then
        docker compose ps
    else
        docker-compose ps
    fi
    
    print_success "Setup testing completed"
}

show_summary() {
    print_section "Docker Compose Summary"
    echo ""
    echo -e "${GREEN}📦 Seq Service:${NC}"
    echo -e "  URL:           http://localhost:5341"
    echo -e "  Container:     refactor-mcp-seq"
    echo -e "  Data:          /opt/seq/data (persistent)"
    echo -e "  Restart:       unless-stopped"
    echo ""
    echo -e "${BLUE}🛠️  Management:${NC}"
    echo -e "  Start:         docker compose up -d"
    echo -e "  Stop:          docker compose down"
    echo -e "  Restart:       docker compose restart seq"
    echo -e "  Logs:          docker compose logs -f seq"
    echo -e "  Status:        docker compose ps"
    echo ""
    echo -e "${YELLOW}🔄 Systemd:${NC}"
    echo -e "  Service:       sudo systemctl {start|stop|restart} refactor-mcp-seq"
    echo -e "  Auto-start:    Enabled on boot"
    echo -e "  Dependency:    RefactorMCP waits 3 seconds after Seq"
    echo ""
}

main "$@"