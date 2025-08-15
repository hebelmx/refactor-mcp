# RefactorMCP Scripts Directory

This directory contains organized shell scripts following 5S methodology for maintaining RefactorMCP.

## 📁 Directory Structure

```
scripts/
├── setup/          # Initial setup and installation scripts
├── maintenance/    # Regular maintenance and updates
├── troubleshooting/# Debugging and problem resolution
├── testing/        # Testing and validation scripts
└── archive/        # Historical scripts (kept for reference)
```

## 🔧 Script Categories

### Setup Scripts
- **setup-services.sh** - Complete initial setup with Seq and systemd
- **setup-environment.sh** - Environment preparation and dependencies

### Maintenance Scripts  
- **deploy-updates.sh** - Deploy application updates
- **manage-services.sh** - Start/stop/restart services
- **backup-config.sh** - Backup configuration files

### Troubleshooting Scripts
- **fix-common-issues.sh** - Automated fixes for common problems
- **debug-service.sh** - Service debugging and diagnostics
- **check-health.sh** - Comprehensive health checks

### Testing Scripts
- **test-mcp-api.sh** - MCP API functionality tests
- **test-endpoints.sh** - HTTP endpoint validation
- **load-test.sh** - Performance testing

## 📋 Usage Standards

### Naming Convention
- Use kebab-case: `script-name.sh`
- Include action prefix: `setup-`, `fix-`, `test-`, `deploy-`
- Be descriptive: `fix-json-serialization.sh` not `fix.sh`

### Script Structure
All scripts follow this template:
```bash
#!/bin/bash
# Script purpose and description
# Usage: ./script-name.sh [options]

set -e  # Exit on error

# Colors and utility functions
source "$(dirname "$0")/../common/colors.sh"
source "$(dirname "$0")/../common/utils.sh"

# Main function
main() {
    print_header "Script Name"
    # Implementation
}

main "$@"
```

### Documentation
- Each script has a header comment explaining purpose
- Complex scripts include usage examples
- Error handling and user feedback
- Standardized output formatting

## 🚀 Quick Start

```bash
# Initial setup
./scripts/setup/setup-services.sh

# Regular maintenance
./scripts/maintenance/deploy-updates.sh

# Troubleshooting
./scripts/troubleshooting/debug-service.sh

# Testing
./scripts/testing/test-mcp-api.sh
```

## 📝 Maintenance

- Review and clean archive/ monthly
- Update documentation when adding new scripts
- Follow standardized error handling
- Test scripts before deployment

---
*Last updated: $(date)*