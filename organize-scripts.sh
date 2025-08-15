#!/bin/bash
# 5S Organization Script - Sort, Set in Order, Shine, Standardize, Sustain

echo "🗂️  Applying 5S methodology to RefactorMCP scripts..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_step() { echo -e "${BLUE}[STEP]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }

# 1. SORT - Categorize existing scripts
print_step "1. SORT - Categorizing existing scripts"

# Setup scripts
SETUP_SCRIPTS=(
    "setup-services.sh"
    "setup-self-contained.sh"
    "install-dependencies.sh"
)

# Maintenance scripts
MAINTENANCE_SCRIPTS=(
    "deploy-http-mcp.sh"
    "deploy-updates.sh"
    "change-ports.sh"
    "fix-navigation-pages.sh"
    "fix-json-serialization.sh"
)

# Troubleshooting scripts
TROUBLESHOOTING_SCRIPTS=(
    "fix-service.sh"
    "fix-core-dump.sh"
    "fix-port-conflict.sh"
    "fix-https-issue.sh"
    "fix-https-with-cert.sh"
    "cleanup-podman.sh"
    "force-docker-reset.sh"
    "debug-crash.sh"
    "debug-docker-env.sh"
)

# Testing scripts
TESTING_SCRIPTS=(
    "test-mcp-api.sh"
    "start-services-manual.sh"
)

# 2. SET IN ORDER - Move scripts to organized directories
print_step "2. SET IN ORDER - Moving scripts to organized directories"

move_scripts() {
    local category="$1"
    shift
    local scripts=("$@")
    
    for script in "${scripts[@]}"; do
        if [ -f "$script" ]; then
            mv "$script" "scripts/$category/"
            print_success "Moved $script → scripts/$category/"
        fi
    done
}

move_scripts "setup" "${SETUP_SCRIPTS[@]}"
move_scripts "maintenance" "${MAINTENANCE_SCRIPTS[@]}"
move_scripts "troubleshooting" "${TROUBLESHOOTING_SCRIPTS[@]}"
move_scripts "testing" "${TESTING_SCRIPTS[@]}"

# 3. SHINE - Create clean, standardized scripts
print_step "3. SHINE - Creating clean, standardized scripts"

# Create main management script
cat > scripts/refactor-mcp-manager.sh << 'EOF'
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
EOF

chmod +x scripts/refactor-mcp-manager.sh

# 4. STANDARDIZE - Apply consistent formatting to moved scripts
print_step "4. STANDARDIZE - Applying consistent formatting"

# Update script headers and add common functions
for category in setup maintenance troubleshooting testing; do
    for script in scripts/$category/*.sh; do
        if [ -f "$script" ]; then
            # Add source lines for common utilities if not present
            if ! grep -q "source.*colors.sh" "$script" 2>/dev/null; then
                # Create a temporary file with updated content
                {
                    echo "#!/bin/bash"
                    echo "# $(basename "$script" .sh | tr '-' ' ' | sed 's/\b\w/\U&/g') Script"
                    echo "# Location: scripts/$category/"
                    echo ""
                    echo "set -e"
                    echo ""
                    echo "SCRIPT_DIR=\"\$(cd \"\$(dirname \"\${BASH_SOURCE[0]}\")\" && pwd)\""
                    echo "source \"\$SCRIPT_DIR/../common/colors.sh\""
                    echo "source \"\$SCRIPT_DIR/../common/utils.sh\""
                    echo ""
                    # Add the rest of the original script (skip the shebang)
                    tail -n +2 "$script"
                } > "$script.tmp"
                
                mv "$script.tmp" "$script"
                chmod +x "$script"
            fi
        fi
    done
done

# 5. SUSTAIN - Create maintenance procedures
print_step "5. SUSTAIN - Creating maintenance procedures"

cat > scripts/5s-maintenance.sh << 'EOF'
#!/bin/bash
# 5S Maintenance Script
# Run this monthly to maintain script organization

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common/colors.sh"

print_header "5S Maintenance Check"

# Check for scripts in wrong locations
print_section "Checking script organization"

if ls *.sh 2>/dev/null | grep -v organize-scripts.sh; then
    print_warning "Found scripts in root directory that should be organized:"
    ls -1 *.sh 2>/dev/null | grep -v organize-scripts.sh
    echo "Consider moving them to appropriate directories."
else
    print_success "No unorganized scripts found"
fi

# Archive old scripts
print_section "Archive management"
if [ -d "scripts/archive" ]; then
    ARCHIVE_COUNT=$(find scripts/archive -name "*.sh" | wc -l)
    print_status "Found $ARCHIVE_COUNT scripts in archive"
    
    if [ $ARCHIVE_COUNT -gt 20 ]; then
        print_warning "Archive has many scripts. Consider cleanup."
    fi
else
    mkdir -p scripts/archive
    print_status "Created archive directory"
fi

# Update documentation
print_section "Documentation check"
if [ -f "scripts/README.md" ]; then
    # Update last modified date
    sed -i "s/Last updated: .*/Last updated: $(date)/" scripts/README.md
    print_success "Updated README.md timestamp"
else
    print_error "scripts/README.md not found"
fi

print_success "5S maintenance completed"
EOF

chmod +x scripts/5s-maintenance.sh

# Create archive info
echo "# Archived Scripts

This directory contains scripts that are no longer actively used but kept for historical reference.

- Scripts moved here during 5S reorganization
- Outdated or superseded scripts
- Temporary/experimental scripts

Review and clean this directory monthly.
" > scripts/archive/README.md

print_success "✅ 5S Organization Complete!"

echo ""
echo "📁 New Structure:"
echo "scripts/"
echo "├── setup/           $(ls scripts/setup/ | wc -l) scripts"
echo "├── maintenance/     $(ls scripts/maintenance/ | wc -l) scripts"
echo "├── troubleshooting/ $(ls scripts/troubleshooting/ | wc -l) scripts"
echo "├── testing/         $(ls scripts/testing/ | wc -l) scripts"
echo "├── common/          Shared utilities"
echo "└── archive/         Historical scripts"
echo ""
echo "🎯 Main entry point: ./scripts/refactor-mcp-manager.sh"
echo ""
echo "📋 Next steps:"
echo "1. Test the manager: ./scripts/refactor-mcp-manager.sh health"
echo "2. Run monthly: ./scripts/5s-maintenance.sh"
echo "3. Update documentation as needed"