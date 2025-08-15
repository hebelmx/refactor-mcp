#!/bin/bash
# Debug Docker Env Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔍 DEBUG: Finding what's setting DOCKER_HOST..."

echo "=== Current Environment ==="
echo "DOCKER_HOST: ${DOCKER_HOST:-'(not set)'}"
echo "USER: $(whoami)"
echo "HOME: $HOME"
echo "SHELL: $SHELL"

echo ""
echo "=== Process Environment ==="
cat /proc/$$/environ | tr '\0' '\n' | grep -i docker || echo "No Docker vars in process environment"

echo ""
echo "=== Shell Configuration Files ==="
find ~ -maxdepth 2 -name ".*rc" -o -name ".*profile" | while read file; do
    if [ -f "$file" ] && grep -q -i "docker.*sock\|podman" "$file" 2>/dev/null; then
        echo "Found in $file:"
        grep -n -i "docker.*sock\|podman" "$file"
        echo ""
    fi
done

echo "=== System Environment Files ==="
for file in /etc/environment /etc/profile; do
    if [ -f "$file" ] && grep -q -i docker "$file" 2>/dev/null; then
        echo "Found in $file:"
        grep -n -i docker "$file"
        echo ""
    fi
done

echo "=== Systemd User Environment ==="
systemctl --user show-environment | grep -i docker || echo "No Docker vars in systemd"

echo "=== Running Processes ==="
ps aux | grep -v grep | grep -i "docker\|podman" || echo "No Docker/Podman processes"

echo ""
echo "=== MANUAL FIX ==="
echo "Run these commands to force clean Docker environment:"
echo ""
echo "# Clean current session"
echo "unset DOCKER_HOST"
echo "export DOCKER_HOST='unix:///var/run/docker.sock'"
echo ""
echo "# Test Docker"
echo "docker ps"
echo ""
echo "# If that works, add to shell config:"
echo "echo 'export DOCKER_HOST=\"unix:///var/run/docker.sock\"' >> ~/.bashrc"
echo "echo 'export DOCKER_HOST=\"unix:///var/run/docker.sock\"' >> ~/.zshrc"