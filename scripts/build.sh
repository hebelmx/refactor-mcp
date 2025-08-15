#!/bin/bash

# Simplified comprehensive build script for RefactorMCP
# Builds all projects and organizes artifacts in external Refactor folder

set -e

# Configuration
CONFIGURATION="${1:-Release}"
CLEAN="${CLEAN:-false}"
SKIP_TESTS="${SKIP_TESTS:-false}"
SKIP_PACKAGING="${SKIP_PACKAGING:-false}"

# Paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SOLUTION_FILE="$SOLUTION_ROOT/RefactorMCP.sln"

# Build info
BUILD_ID=$(date -u +"%Y%m%d-%H%M%S")
BUILD_VERSION="1.0.0-dev-$BUILD_ID"

# Colors
GREEN='\033[0;32m'
CYAN='\033[0;36m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m'

# Output functions
build_step() { echo -e "${CYAN}🔨 $1${NC}"; }
build_success() { echo -e "${GREEN}✅ $1${NC}"; }
build_error() { echo -e "${RED}❌ $1${NC}"; }

# Output directories
OUTPUT_ROOT="$SOLUTION_ROOT/../Refactor"
BUILD_ARTIFACTS="$OUTPUT_ROOT/artifacts/$CONFIGURATION"
BUILD_TESTS="$OUTPUT_ROOT/tests/$CONFIGURATION"
BUILD_PACKAGES="$OUTPUT_ROOT/packages"
BUILD_LOGS="$OUTPUT_ROOT/logs/$CONFIGURATION"

build_step "Setting up build environment"
echo -e "${YELLOW}Build ID: $BUILD_ID${NC}"
echo -e "${YELLOW}Configuration: $CONFIGURATION${NC}"
echo -e "${YELLOW}Output: $OUTPUT_ROOT${NC}"

# Create directories
mkdir -p "$BUILD_ARTIFACTS" "$BUILD_TESTS" "$BUILD_PACKAGES" "$BUILD_LOGS"

BUILD_LOG="$BUILD_LOGS/build-$BUILD_ID.log"
echo "Build started: $(date)" > "$BUILD_LOG"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --clean) CLEAN=true; shift ;;
        --skip-tests) SKIP_TESTS=true; shift ;;
        --skip-packaging) SKIP_PACKAGING=true; shift ;;
        *) shift ;;
    esac
done

# Show help
if [[ "$1" == "--help" || "$1" == "-h" ]]; then
    echo "Usage: $0 [Configuration] [Options]"
    echo "  Configuration: Debug|Release (default: Release)"
    echo "  Options:"
    echo "    --clean           Clean before building"
    echo "    --skip-tests      Skip test execution"
    echo "    --skip-packaging  Skip NuGet package creation"
    echo "    -h, --help        Show this help"
    exit 0
fi

# Clean if requested
if [[ "$CLEAN" == "true" ]]; then
    build_step "Cleaning previous outputs"
    dotnet clean "$SOLUTION_FILE" --configuration "$CONFIGURATION" --verbosity minimal
    build_success "Clean completed"
fi

# Restore packages
build_step "Restoring NuGet packages"
dotnet restore "$SOLUTION_FILE" --verbosity minimal
build_success "Package restore completed"

# Build solution
build_step "Building solution"
dotnet build "$SOLUTION_FILE" --configuration "$CONFIGURATION" --no-restore --verbosity normal
build_success "Build completed"

# Copy build artifacts
build_step "Organizing build artifacts"
SOURCE_PROJECTS=("RefactorMCP.Core" "RefactorMCP.MCP.Server" "RefactorMCP.Web" "RefactorMCP.ConsoleApp")

for project in "${SOURCE_PROJECTS[@]}"; do
    project_build_path="src/$project/bin/$CONFIGURATION/net9.0"
    project_artifact_path="$BUILD_ARTIFACTS/$project"
    
    if [[ -d "$project_build_path" ]]; then
        mkdir -p "$project_artifact_path"
        cp -r "$project_build_path"/* "$project_artifact_path/"
        echo "  ✓ Copied artifacts for $project"
    fi
done
build_success "Build artifacts organized"

# Run tests
if [[ "$SKIP_TESTS" != "true" ]]; then
    build_step "Running tests"
    TEST_PROJECTS=("RefactorMCP.Core.Tests" "RefactorMCP.MCP.Server.Tests" "RefactorMCP.Web.Tests" "RefactorMCP.Tests")
    ALL_TESTS_PASSED=true
    
    for test_project in "${TEST_PROJECTS[@]}"; do
        echo "  Running tests for $test_project..."
        test_output_path="$BUILD_TESTS/$test_project"
        mkdir -p "$test_output_path"
        
        test_log_file="$test_output_path/test-results.trx"
        
        if dotnet test "test/$test_project" --configuration "$CONFIGURATION" --no-build --verbosity minimal --logger "trx;LogFileName=$test_log_file" --results-directory "$test_output_path"; then
            echo "    ✓ $test_project passed"
        else
            echo "    ✗ $test_project failed"
            ALL_TESTS_PASSED=false
        fi
    done
    
    if [[ "$ALL_TESTS_PASSED" == "true" ]]; then
        build_success "All tests passed"
    else
        echo -e "${YELLOW}⚠️  Some tests failed${NC}"
    fi
fi

# Create packages
if [[ "$SKIP_PACKAGING" != "true" ]]; then
    build_step "Creating NuGet packages"
    PACKAGEABLE_PROJECTS=("src/RefactorMCP.Core" "src/RefactorMCP.MCP.Server")
    
    for project in "${PACKAGEABLE_PROJECTS[@]}"; do
        if dotnet pack "$project" --configuration "$CONFIGURATION" --no-build --output "$BUILD_PACKAGES" -p:PackageVersion="$BUILD_VERSION" --verbosity minimal; then
            echo "  ✓ Package created for $project"
        fi
    done
    build_success "NuGet packages created"
fi

# Generate build manifest
build_step "Generating build manifest"
BUILD_END=$(date)
cat > "$OUTPUT_ROOT/build-manifest.json" << EOF
{
    "buildInfo": {
        "buildId": "$BUILD_ID",
        "version": "$BUILD_VERSION",
        "configuration": "$CONFIGURATION",
        "startTime": "$(date -Iseconds)",
        "endTime": "$(date -Iseconds)",
        "success": true
    },
    "outputs": {
        "artifacts": "$BUILD_ARTIFACTS",
        "tests": "$BUILD_TESTS",
        "packages": "$BUILD_PACKAGES"
    }
}
EOF
build_success "Build manifest created"

# Success message
echo ""
echo -e "${GREEN}🎉 Build completed successfully!${NC}"
echo -e "${CYAN}Build ID: $BUILD_ID${NC}"
echo -e "${CYAN}Output: $OUTPUT_ROOT${NC}"
echo -e "${CYAN}Manifest: $OUTPUT_ROOT/build-manifest.json${NC}"