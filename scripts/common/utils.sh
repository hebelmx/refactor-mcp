#!/bin/bash
# Common utility functions
# Source this file: source "$(dirname "$0")/../common/utils.sh"

# Configuration
REFACTOR_MCP_DIR="/opt/refactor-mcp"
SERVICE_NAME="refactor-mcp"
HTTP_PORT=7042
HTTPS_PORT=7043
SEQ_PORT=5341

# Utility functions
check_command() {
    if ! command -v "$1" &> /dev/null; then
        print_error "Required command '$1' is not installed"
        return 1
    fi
}

check_service_running() {
    if systemctl is-active "$1" >/dev/null 2>&1; then
        return 0
    else
        return 1
    fi
}

check_port_available() {
    if netstat -tlnp 2>/dev/null | grep -q ":$1 "; then
        return 1  # Port is in use
    else
        return 0  # Port is available
    fi
}

wait_for_service() {
    local service="$1"
    local timeout="${2:-30}"
    local count=0
    
    print_status "Waiting for $service to start (timeout: ${timeout}s)..."
    
    while [ $count -lt $timeout ]; do
        if check_service_running "$service"; then
            print_success "$service is running"
            return 0
        fi
        sleep 1
        count=$((count + 1))
        echo -n "."
    done
    
    echo ""
    print_error "$service failed to start within ${timeout}s"
    return 1
}

test_endpoint() {
    local url="$1"
    local description="${2:-endpoint}"
    
    if curl -s --max-time 5 "$url" >/dev/null 2>&1; then
        print_success "✅ $description: $url"
        return 0
    else
        print_error "❌ $description failed: $url"
        return 1
    fi
}

backup_file() {
    local file="$1"
    if [ -f "$file" ]; then
        cp "$file" "${file}.backup.$(date +%Y%m%d_%H%M%S)"
        print_status "Backed up $file"
    fi
}

ensure_directory() {
    local dir="$1"
    local owner="${2:-$(whoami)}"
    
    if [ ! -d "$dir" ]; then
        sudo mkdir -p "$dir"
        sudo chown "$owner:$owner" "$dir"
        print_status "Created directory: $dir"
    fi
}

check_prerequisites() {
    print_step 1 "Checking prerequisites"
    
    local missing=0
    
    for cmd in curl docker systemctl; do
        if ! check_command "$cmd"; then
            missing=1
        fi
    done
    
    if [ $missing -eq 1 ]; then
        print_error "Missing required commands. Please install dependencies."
        exit 1
    fi
    
    print_success "All prerequisites satisfied"
}

show_summary() {
    print_section "Summary"
    echo ""
    echo -e "${GREEN}🌐 RefactorMCP Services:${NC}"
    echo -e "  Dashboard:     http://localhost:$HTTP_PORT"
    echo -e "  Health:        http://localhost:$HTTP_PORT/health"
    echo -e "  MCP API:       http://localhost:$HTTP_PORT/api/mcp"
    echo -e "  Metrics:       http://localhost:$HTTP_PORT/metrics"
    echo -e "  Seq Logs:      http://localhost:$SEQ_PORT"
    echo ""
    echo -e "${BLUE}🛠️  Management:${NC}"
    echo -e "  Service:       sudo systemctl {start|stop|restart} $SERVICE_NAME"
    echo -e "  Logs:          sudo journalctl -u $SERVICE_NAME -f"
    echo -e "  Container:     docker {start|stop|restart} refactor-mcp-seq"
    echo ""
}