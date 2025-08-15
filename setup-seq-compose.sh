#!/bin/bash
# Quick Docker Compose Setup for Seq (Interactive)

echo "🐳 Setting up Seq with Docker Compose..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Check Docker Compose
print_status "Checking Docker Compose..."
if docker compose version >/dev/null 2>&1; then
    COMPOSE_CMD="docker compose"
    print_success "Found Docker Compose V2"
elif command -v docker-compose >/dev/null 2>&1; then
    COMPOSE_CMD="docker-compose"
    print_success "Found Docker Compose V1"
else
    print_error "Docker Compose not found. Installing..."
    # Install Docker Compose V2 plugin
    sudo apt-get update
    sudo apt-get install -y docker-compose-plugin
    COMPOSE_CMD="docker compose"
fi

# Setup Seq data directory (with sudo prompt)
print_status "Setting up Seq data directory..."
echo "This will create /opt/seq/data directory and set permissions."
echo "You'll be prompted for your password:"

sudo mkdir -p /opt/seq/data
sudo chown 5341:5341 /opt/seq/data
sudo chmod 775 /opt/seq/data

print_success "Seq data directory created"

# Handle existing container properly
print_status "Handling existing Seq containers..."
if docker ps -a --format "{{.Names}}" | grep -q "^refactor-mcp-seq$"; then
    print_status "Found existing refactor-mcp-seq container"
    docker stop refactor-mcp-seq 2>/dev/null || true
    docker rm refactor-mcp-seq 2>/dev/null || true
    print_success "Existing container removed"
fi

# Start with Docker Compose
print_status "Starting Seq with Docker Compose..."
$COMPOSE_CMD up -d

# Wait for Seq to be ready
print_status "Waiting for Seq to start..."
for i in {1..30}; do
    if curl -s http://localhost:5341/api >/dev/null 2>&1; then
        print_success "✅ Seq is running and ready!"
        break
    fi
    sleep 1
    echo -n "."
done

if [ $i -eq 30 ]; then
    print_error "❌ Seq failed to start"
    $COMPOSE_CMD logs seq
    exit 1
fi

# Update systemd service dependency
print_status "Updating RefactorMCP service to depend on Seq..."
TEMP_SERVICE=$(mktemp)

# Read current service file and add dependency
sudo cat /etc/systemd/system/refactor-mcp.service | \
    sed 's/^After=.*/After=network.target docker.service/' | \
    sed '/^ExecStart=/i ExecStartPre=/bin/sleep 3' > "$TEMP_SERVICE"

# Replace service file
sudo cp "$TEMP_SERVICE" /etc/systemd/system/refactor-mcp.service
rm "$TEMP_SERVICE"

# Reload and restart
sudo systemctl daemon-reload
sudo systemctl restart refactor-mcp

print_success "✅ Docker Compose setup completed!"

echo ""
echo "🐳 Docker Compose Commands:"
echo "  $COMPOSE_CMD up -d         # Start services"
echo "  $COMPOSE_CMD down          # Stop services"
echo "  $COMPOSE_CMD logs -f seq   # Follow Seq logs"
echo "  $COMPOSE_CMD ps            # Show status"
echo "  $COMPOSE_CMD restart seq   # Restart Seq"
echo ""
echo "🌐 Services:"
echo "  Seq:           http://localhost:5341"
echo "  RefactorMCP:   http://localhost:7042"
echo ""

# Test everything
print_status "Testing services..."
if curl -s http://localhost:5341 >/dev/null 2>&1; then
    print_success "✅ Seq web interface: http://localhost:5341"
else
    print_error "❌ Seq web interface not accessible"
fi

if curl -s http://localhost:7042/health >/dev/null 2>&1; then
    print_success "✅ RefactorMCP health: http://localhost:7042/health"
else
    print_error "❌ RefactorMCP not responding"
fi

print_success "🎉 Setup complete! Seq now runs with Docker Compose with:"
echo "  ✅ Persistent storage: /opt/seq/data"
echo "  ✅ Auto-restart: unless-stopped"
echo "  ✅ Startup delay: 3 seconds before RefactorMCP"
echo "  ✅ Systemd integration: Managed by Docker Compose"