#!/bin/bash
# Force Docker Reset Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔥 FORCE Docker Reset - Removing ALL Podman traces..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[RESET]${NC} $1"; }
print_success() { echo -e "${GREEN}[OK]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# 1. Nuclear option - remove all Docker/Podman environment variables
print_status "Removing ALL Docker/Podman environment variables..."

unset DOCKER_HOST
unset DOCKER_SOCK
unset DOCKER_CONFIG
unset DOCKER_CONTEXT
unset PODMAN_CONNECTION_URI
unset CONTAINER_HOST
unset CONTAINER_SSHKEY
unset CONTAINER_PASSPHRASE

export DOCKER_HOST=""
export DOCKER_CONTEXT=""

print_success "Environment cleared"

# 2. Check what's setting DOCKER_HOST
print_status "Checking what's setting DOCKER_HOST..."

echo "Current environment:"
env | grep -i docker || echo "No Docker environment variables found"
env | grep -i podman || echo "No Podman environment variables found"

# 3. Check ALL shell configuration files
print_status "Checking ALL shell configuration files for Docker/Podman references..."

FILES_TO_CHECK=(
    ~/.bashrc
    ~/.zshrc
    ~/.profile
    ~/.bash_profile
    ~/.zprofile
    ~/.config/fish/config.fish
    /etc/environment
    ~/.pam_environment
)

for file in "${FILES_TO_CHECK[@]}"; do
    if [ -f "$file" ]; then
        if grep -q -i "docker\|podman" "$file" 2>/dev/null; then
            print_status "Found Docker/Podman references in $file:"
            grep -n -i "docker\|podman" "$file" || true
            
            # Ask if we should clean it (but do it automatically in script)
            print_status "Cleaning $file..."
            sed -i.bak '/DOCKER_HOST/d; /PODMAN/d; /docker.*sock/d; /podman.*sock/d' "$file" 2>/dev/null || true
        fi
    fi
done

# 4. Check systemd user environment
print_status "Checking systemd user environment..."
systemctl --user show-environment | grep -i docker || echo "No Docker variables in systemd user environment"

# 5. Reset systemd user environment
print_status "Resetting systemd user environment..."
systemctl --user unset-environment DOCKER_HOST 2>/dev/null || true
systemctl --user unset-environment DOCKER_SOCK 2>/dev/null || true
systemctl --user unset-environment DOCKER_CONFIG 2>/dev/null || true

# 6. Kill any Docker/Podman processes
print_status "Stopping all Docker/Podman user processes..."
pkill -f podman 2>/dev/null || true
systemctl --user stop podman 2>/dev/null || true
systemctl --user stop podman.socket 2>/dev/null || true

# 7. Remove Podman directories
print_status "Removing Podman runtime directories..."
rm -rf ~/.local/share/containers 2>/dev/null || true
rm -rf ~/.config/containers 2>/dev/null || true
rm -rf /run/user/$(id -u)/podman 2>/dev/null || true
rm -rf /run/user/$(id -u)/containers 2>/dev/null || true

# 8. Reset Docker to system daemon
print_status "Resetting Docker to system daemon..."

# Check if Docker system service is running
if ! sudo systemctl is-active docker >/dev/null 2>&1; then
    print_status "Starting Docker system service..."
    sudo systemctl start docker
fi

# Test with explicit socket
print_status "Testing Docker with explicit system socket..."
DOCKER_HOST="unix:///var/run/docker.sock" docker version >/dev/null 2>&1
if [ $? -eq 0 ]; then
    print_success "Docker system daemon is working!"
    
    # Set the correct socket permanently
    export DOCKER_HOST="unix:///var/run/docker.sock"
    
    # Add to current shell config
    SHELL_CONFIG=""
    if [ -n "$ZSH_VERSION" ]; then
        SHELL_CONFIG="~/.zshrc"
        echo 'export DOCKER_HOST="unix:///var/run/docker.sock"' >> ~/.zshrc
    elif [ -n "$BASH_VERSION" ]; then
        SHELL_CONFIG="~/.bashrc"
        echo 'export DOCKER_HOST="unix:///var/run/docker.sock"' >> ~/.bashrc
    fi
    
    print_success "Set DOCKER_HOST to system daemon in $SHELL_CONFIG"
else
    print_error "Docker system daemon is not working"
fi

# 9. Final test
print_status "Final Docker test..."
docker --version 2>/dev/null || print_error "Docker command not found"
docker ps 2>/dev/null || print_error "Cannot connect to Docker daemon"

if docker ps >/dev/null 2>&1; then
    print_success "🎉 Docker is now working correctly!"
    
    # Show containers
    echo ""
    echo "=== Current Docker Status ==="
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || true
    
    # Start Seq if not running
    if ! docker ps --format "{{.Names}}" | grep -q refactor-mcp-seq; then
        print_status "Starting Seq container..."
        sudo mkdir -p /opt/seq/data
        sudo chown 5341:5341 /opt/seq/data
        
        docker run -d \
            --name refactor-mcp-seq \
            --restart unless-stopped \
            -p 5341:5341 \
            -v /opt/seq/data:/data \
            -e ACCEPT_EULA=Y \
            datalust/seq:latest
    fi
    
    print_success "Environment is clean and Docker is working!"
else
    print_error "❌ Docker is still not working after cleanup"
    echo ""
    echo "Manual steps to try:"
    echo "1. sudo systemctl restart docker"
    echo "2. sudo usermod -aG docker $(whoami)"
    echo "3. Log out and log back in"
    echo "4. export DOCKER_HOST=\"unix:///var/run/docker.sock\""
fi

echo ""
echo "🔄 Please run: source ~/.bashrc (or ~/.zshrc) or start a new terminal"