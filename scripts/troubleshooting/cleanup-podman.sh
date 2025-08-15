#!/bin/bash
# Cleanup Podman Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🧹 Cleaning up Podman traces and fixing Docker environment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 1. Clean up environment variables
print_status "Cleaning up Docker/Podman environment variables..."

# Remove from current session
unset DOCKER_HOST
unset DOCKER_SOCK
unset DOCKER_CONFIG
unset PODMAN_CONNECTION_URI

export DOCKER_HOST=""

print_success "Environment variables cleaned"

# 2. Clean up shell configuration files
print_status "Cleaning Docker/Podman references from shell configs..."

# Remove from .bashrc if exists
if [ -f ~/.bashrc ]; then
    sed -i '/DOCKER_HOST.*podman/d' ~/.bashrc
    sed -i '/PODMAN_CONNECTION/d' ~/.bashrc
    print_status "Cleaned ~/.bashrc"
fi

# Remove from .zshrc if exists
if [ -f ~/.zshrc ]; then
    sed -i '/DOCKER_HOST.*podman/d' ~/.zshrc
    sed -i '/PODMAN_CONNECTION/d' ~/.zshrc
    print_status "Cleaned ~/.zshrc"
fi

# Remove from .profile if exists
if [ -f ~/.profile ]; then
    sed -i '/DOCKER_HOST.*podman/d' ~/.profile
    sed -i '/PODMAN_CONNECTION/d' ~/.profile
    print_status "Cleaned ~/.profile"
fi

# 3. Stop and remove podman services
print_status "Stopping Podman services..."

# Stop user podman services
systemctl --user stop podman.socket 2>/dev/null || true
systemctl --user stop podman.service 2>/dev/null || true
systemctl --user disable podman.socket 2>/dev/null || true
systemctl --user disable podman.service 2>/dev/null || true

# 4. Clean up podman socket files
print_status "Removing Podman socket files..."
rm -f /run/user/$(id -u)/podman/podman.sock 2>/dev/null || true
rm -rf /run/user/$(id -u)/podman 2>/dev/null || true

# 5. Test Docker connection
print_status "Testing Docker connection..."

if command -v docker &> /dev/null; then
    if docker ps >/dev/null 2>&1; then
        print_success "Docker is working correctly!"
        docker version --format "Docker version: {{.Server.Version}}"
    else
        print_warning "Docker daemon might not be running. Trying to start it..."
        sudo systemctl start docker 2>/dev/null || print_error "Could not start Docker service"
        
        # Test again
        if docker ps >/dev/null 2>&1; then
            print_success "Docker is now working!"
        else
            print_error "Docker still not working. You may need to install Docker or check the service."
        fi
    fi
else
    print_error "Docker is not installed. Installing Docker..."
    
    # Install Docker
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $(whoami)
    rm get-docker.sh
    
    print_warning "Docker installed. You may need to log out and back in for group changes to take effect."
fi

# 6. Clean up any existing containers with Podman-style names
print_status "Cleaning up any conflicting containers..."
docker stop refactor-mcp-seq 2>/dev/null || true
docker rm refactor-mcp-seq 2>/dev/null || true

# 7. Start fresh Seq container
print_status "Starting fresh Seq container..."
docker run -d \
    --name refactor-mcp-seq \
    --restart unless-stopped \
    -p 5341:5341 \
    -v /opt/seq/data:/data \
    -e ACCEPT_EULA=Y \
    -e SEQ_FIRSTRUN_ADMINPASSWORDHASH='' \
    datalust/seq:latest 2>/dev/null

if [ $? -eq 0 ]; then
    print_success "Seq container started successfully"
else
    print_error "Failed to start Seq container - you may need sudo access for /opt/seq/data directory"
fi

# 8. Test everything
print_status "Testing services..."

echo ""
echo "=== Environment Check ==="
echo "DOCKER_HOST: ${DOCKER_HOST:-'(not set)'}"
echo "Docker command: $(which docker 2>/dev/null || echo 'not found')"

if docker ps >/dev/null 2>&1; then
    echo ""
    echo "=== Running Containers ==="
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    echo ""
    echo "=== Seq Container Check ==="
    if docker ps -f name=refactor-mcp-seq --format "{{.Names}}" | grep -q refactor-mcp-seq; then
        print_success "Seq container is running on http://localhost:5341"
    else
        print_warning "Seq container is not running"
    fi
fi

print_success "Podman cleanup completed!"
echo ""
echo "📋 Next steps:"
echo "1. If you made shell changes, run: source ~/.bashrc (or ~/.zshrc)"
echo "2. Test Docker: docker ps"
echo "3. Start RefactorMCP service: sudo systemctl start refactor-mcp"
echo "4. Check service status: sudo systemctl status refactor-mcp"