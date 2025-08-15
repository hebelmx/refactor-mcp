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
