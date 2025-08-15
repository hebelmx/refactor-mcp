#!/bin/bash

# Comprehensive build script for RefactorMCP
# Builds all projects, runs tests, and organizes artifacts in external Refactor folder

set -e  # Exit on error

# Script configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SOLUTION_FILE="$SOLUTION_ROOT/RefactorMCP.sln"

# Default parameters
CONFIGURATION="${CONFIGURATION:-Release}"
PLATFORM="${PLATFORM:-Any CPU}"
OUTPUT_ROOT="${OUTPUT_ROOT:-../Refactor}"
CLEAN="${CLEAN:-false}"
SKIP_TESTS="${SKIP_TESTS:-false}"
SKIP_PACKAGING="${SKIP_PACKAGING:-false}"
VERBOSE="${VERBOSE:-false}"

# Build information
BUILD_ID=$(date -u +"%Y%m%d-%H%M%S")
BUILD_VERSION="1.0.0-dev-$BUILD_ID"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Output functions
log_success() { echo -e "${GREEN}✅ $1${NC}"; }
log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }
log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
log_error() { echo -e "${RED}❌ $1${NC}"; }
log_header() { echo -e "\n${MAGENTA}🔨 $1${NC}"; }

# Help function
show_help() {
    cat << EOF
RefactorMCP Comprehensive Build Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    -c, --configuration CONFIG    Build configuration (Debug|Release) [default: Release]
    -p, --platform PLATFORM      Target platform [default: Any CPU] 
    -o, --output OUTPUT          Output root directory [default: ../Refactor]
    --clean                      Clean before building
    --skip-tests                 Skip test execution
    --skip-packaging            Skip NuGet package creation
    --verbose                   Enable verbose output
    -h, --help                  Show this help message

EXAMPLES:
    $0                                      # Build with defaults
    $0 -c Debug --clean --verbose          # Debug build with clean
    $0 --skip-tests --skip-packaging       # Build only, no tests or packages

ENVIRONMENT VARIABLES:
    CONFIGURATION, PLATFORM, OUTPUT_ROOT, CLEAN, SKIP_TESTS, SKIP_PACKAGING, VERBOSE
    
EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -p|--platform)
            PLATFORM="$2"
            shift 2
            ;;
        -o|--output)
            OUTPUT_ROOT="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --skip-packaging)
            SKIP_PACKAGING=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Validate configuration
if [[ ! "$CONFIGURATION" =~ ^(Debug|Release)$ ]]; then
    log_error "Invalid configuration: $CONFIGURATION. Must be Debug or Release."
    exit 1
fi

# Validate solution file
if [[ ! -f "$SOLUTION_FILE" ]]; then
    log_error "Solution file not found: $SOLUTION_FILE"
    exit 1
fi

# Create output directory structure
OUTPUT_PATH=$(realpath "$SOLUTION_ROOT/$OUTPUT_ROOT")
BUILD_OUTPUTS=(
    "$OUTPUT_PATH"
    "$OUTPUT_PATH/build/$CONFIGURATION"
    "$OUTPUT_PATH/tests/$CONFIGURATION" 
    "$OUTPUT_PATH/packages"
    "$OUTPUT_PATH/reports/$CONFIGURATION"
    "$OUTPUT_PATH/logs/$CONFIGURATION"
    "$OUTPUT_PATH/artifacts/$CONFIGURATION"
    "$OUTPUT_PATH/intermediate/$CONFIGURATION"
    "$OUTPUT_PATH/symbols/$CONFIGURATION"
    "$OUTPUT_PATH/docs"
    "$OUTPUT_PATH/distributions"
)

# Build timestamp and logging
BUILD_START=$(date)
BUILD_LOG="$OUTPUT_PATH/logs/$CONFIGURATION/build-$BUILD_ID.log"

log_build() {
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    echo "[$timestamp] $1" | tee -a "$BUILD_LOG"
    [[ "$VERBOSE" == "true" ]] && echo "$1"
}

# Setup build environment
log_header "Setting up build environment"
log_info "Build ID: $BUILD_ID"
log_info "Configuration: $CONFIGURATION"
log_info "Platform: $PLATFORM"
log_info "Output Root: $OUTPUT_PATH"

# Create all output directories
for dir in "${BUILD_OUTPUTS[@]}"; do
    mkdir -p "$dir"
    [[ "$VERBOSE" == "true" ]] && log_info "Created directory: $dir"
done

log_build "=== RefactorMCP Comprehensive Build Started ==="
log_build "Configuration: $CONFIGURATION"
log_build "Platform: $PLATFORM"
log_build "Output: $OUTPUT_PATH"

# Trap for cleanup on exit
cleanup() {
    local exit_code=$?
    local build_end=$(date)
    
    if [[ $exit_code -ne 0 ]]; then
        log_error "Build failed with exit code: $exit_code"
        log_build "BUILD FAILED with exit code: $exit_code"
        
        # Create failure manifest
        cat > "$OUTPUT_PATH/build-failure.json" << EOF
{
    "buildInfo": {
        "buildId": "$BUILD_ID",
        "configuration": "$CONFIGURATION",
        "platform": "$PLATFORM",
        "startTime": "$BUILD_START",
        "failureTime": "$build_end",
        "success": false,
        "exitCode": $exit_code
    },
    "environment": {
        "hostname": "$(hostname)",
        "user": "$(whoami)",
        "shell": "$SHELL",
        "os": "$(uname -s)",
        "arch": "$(uname -m)"
    }
}
EOF
    else
        log_success "Build completed successfully!"
        log_build "=== Build completed successfully ==="
    fi
}

trap cleanup EXIT

# Clean if requested
if [[ "$CLEAN" == "true" ]]; then
    log_header "Cleaning previous build outputs"
    log_build "Cleaning solution..."
    
    dotnet clean "$SOLUTION_FILE" --configuration "$CONFIGURATION" --verbosity minimal
    
    # Clean output directories
    for dir in "${BUILD_OUTPUTS[@]}"; do
        [[ -d "$dir" ]] && rm -rf "$dir"/* 2>/dev/null || true
    done
    
    log_success "Clean completed"
    log_build "Clean completed successfully"
fi

# Restore packages
log_header "Restoring NuGet packages"
log_build "Restoring packages..."

dotnet restore "$SOLUTION_FILE" --verbosity minimal

log_success "Package restore completed"
log_build "Package restore completed"

# Build solution
log_header "Building solution"
log_build "Building solution with configuration $CONFIGURATION..."

dotnet build "$SOLUTION_FILE" \
    --configuration "$CONFIGURATION" \
    --no-restore \
    --verbosity normal \
    -p:Platform="$PLATFORM" \
    -p:BuildId="$BUILD_ID" \
    -p:AssemblyVersion="$BUILD_VERSION" \
    -p:FileVersion="$BUILD_VERSION" \
    -p:InformationalVersion="$BUILD_VERSION" \
    -p:OutputPath="$OUTPUT_PATH/build/$CONFIGURATION/" \
    -p:BaseIntermediateOutputPath="$OUTPUT_PATH/intermediate/$CONFIGURATION/"

log_success "Build completed successfully"
log_build "Build completed successfully"

# Organize build artifacts
log_header "Organizing build artifacts"
log_build "Copying build artifacts..."

# Source projects to process
declare -A SOURCE_PROJECTS=(
    ["RefactorMCP.Core"]="Library"
    ["RefactorMCP.MCP.Server"]="Library"
    ["RefactorMCP.Web"]="Web"
    ["RefactorMCP.ConsoleApp"]="Executable"
)

for project_name in "${!SOURCE_PROJECTS[@]}"; do
    project_type="${SOURCE_PROJECTS[$project_name]}"
    
    log_info "Processing $project_name ($project_type)..."
    log_build "Processing project: $project_name"
    
    project_build_path="$OUTPUT_PATH/build/$CONFIGURATION/$project_name"
    project_artifact_path="$OUTPUT_PATH/artifacts/$CONFIGURATION/$project_name"
    
    if [[ -d "$project_build_path" ]]; then
        # Create project artifact directory
        mkdir -p "$project_artifact_path"
        
        # Copy all build outputs
        cp -r "$project_build_path"/* "$project_artifact_path/"
        
        # Copy symbols to dedicated symbols folder
        find "$project_build_path" -name "*.pdb" -exec cp {} "$OUTPUT_PATH/symbols/$CONFIGURATION/" \; 2>/dev/null || true
        
        log_success "Copied artifacts for $project_name"
    else
        log_warning "Build output not found for $project_name at $project_build_path"
    fi
done

# Run tests if not skipped
if [[ "$SKIP_TESTS" != "true" ]]; then
    log_header "Running tests"
    log_build "Starting test execution..."
    
    TEST_PROJECTS=(
        "RefactorMCP.Core.Tests"
        "RefactorMCP.MCP.Server.Tests"
        "RefactorMCP.Web.Tests"  
        "RefactorMCP.Tests"
    )
    
    ALL_TESTS_PASSED=true
    TEST_RESULTS=()
    
    for test_project in "${TEST_PROJECTS[@]}"; do
        log_info "Running tests for $test_project..."
        log_build "Running tests: $test_project"
        
        test_output_path="$OUTPUT_PATH/tests/$CONFIGURATION/$test_project"
        mkdir -p "$test_output_path"
        
        test_log_file="$test_output_path/test-results.trx"
        
        test_start_time=$(date +%s)
        
        if dotnet test "test/$test_project" \
            --configuration "$CONFIGURATION" \
            --no-build \
            --verbosity normal \
            --logger "trx;LogFileName=$test_log_file" \
            --collect:"XPlat Code Coverage" \
            --results-directory "$test_output_path"; then
            
            test_end_time=$(date +%s)
            test_duration=$((test_end_time - test_start_time))
            
            log_success "Tests passed for $test_project (Duration: ${test_duration}s)"
            log_build "Tests passed: $test_project in ${test_duration}s"
            
            TEST_RESULTS+=("{\"project\":\"$test_project\",\"success\":true,\"duration\":$test_duration}")
        else
            test_end_time=$(date +%s)
            test_duration=$((test_end_time - test_start_time))
            
            log_error "Tests failed for $test_project"
            log_build "Tests failed: $test_project"
            
            ALL_TESTS_PASSED=false
            TEST_RESULTS+=("{\"project\":\"$test_project\",\"success\":false,\"duration\":$test_duration}")
        fi
    done
    
    # Generate test summary
    test_summary_path="$OUTPUT_PATH/reports/$CONFIGURATION/test-summary.json"
    cat > "$test_summary_path" << EOF
{
    "buildId": "$BUILD_ID",
    "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
    "configuration": "$CONFIGURATION",
    "platform": "$PLATFORM", 
    "allTestsPassed": $([[ "$ALL_TESTS_PASSED" == "true" ]] && echo "true" || echo "false"),
    "testResults": [$(IFS=','; echo "${TEST_RESULTS[*]}")]
}
EOF
    
    log_success "Test summary saved to: $test_summary_path"
    
    if [[ "$ALL_TESTS_PASSED" != "true" ]]; then
        log_warning "Some tests failed. Check individual test results for details."
    fi
fi

# Create NuGet packages if not skipped
if [[ "$SKIP_PACKAGING" != "true" ]]; then
    log_header "Creating NuGet packages"
    log_build "Creating NuGet packages..."
    
    PACKAGEABLE_PROJECTS=(
        "src/RefactorMCP.Core"
        "src/RefactorMCP.MCP.Server"
    )
    
    for project in "${PACKAGEABLE_PROJECTS[@]}"; do
        log_info "Packaging $project..."
        log_build "Creating package for: $project"
        
        if dotnet pack "$project" \
            --configuration "$CONFIGURATION" \
            --no-build \
            --output "$OUTPUT_PATH/packages" \
            -p:PackageVersion="$BUILD_VERSION" \
            --verbosity minimal; then
            
            log_success "Package created for $project"
            log_build "Package created successfully: $project"
        else
            log_warning "Package creation failed for $project"
            log_build "Package creation failed: $project"
        fi
    done
fi

# Generate build manifest
log_header "Generating build manifest"
log_build "Generating build manifest..."

BUILD_END=$(date)
ARTIFACT_COUNT=$(find "$OUTPUT_PATH/artifacts" -type f 2>/dev/null | wc -l)
PACKAGE_COUNT=$(find "$OUTPUT_PATH/packages" -name "*.nupkg" 2>/dev/null | wc -l)

cat > "$OUTPUT_PATH/build-manifest.json" << EOF
{
    "buildInfo": {
        "buildId": "$BUILD_ID",
        "version": "$BUILD_VERSION",
        "configuration": "$CONFIGURATION",
        "platform": "$PLATFORM",
        "startTime": "$BUILD_START",
        "endTime": "$BUILD_END",
        "success": true
    },
    "environment": {
        "hostname": "$(hostname)",
        "user": "$(whoami)",
        "dotnetVersion": "$(dotnet --version)",
        "os": "$(uname -s)",
        "arch": "$(uname -m)"
    },
    "outputs": {
        "buildArtifacts": "$OUTPUT_PATH/artifacts",
        "testResults": "$OUTPUT_PATH/tests", 
        "packages": "$OUTPUT_PATH/packages",
        "reports": "$OUTPUT_PATH/reports",
        "symbols": "$OUTPUT_PATH/symbols"
    },
    "statistics": {
        "artifactCount": $ARTIFACT_COUNT,
        "packageCount": $PACKAGE_COUNT,
        "testProjects": "$([[ "$SKIP_TESTS" == "true" ]] && echo "\"Skipped\"" || echo "${#TEST_PROJECTS[@]}")"
    }
}
EOF

log_success "Build manifest created: $OUTPUT_PATH/build-manifest.json"
log_build "Build manifest generated successfully"

# Create distribution archives
log_header "Creating distribution archives"
log_build "Creating distribution archives..."

# Console app distribution
console_app_path="$OUTPUT_PATH/artifacts/$CONFIGURATION/RefactorMCP.ConsoleApp"
if [[ -d "$console_app_path" ]]; then
    console_zip="$OUTPUT_PATH/distributions/RefactorMCP-Console-$CONFIGURATION-$BUILD_ID.tar.gz"
    tar -czf "$console_zip" -C "$console_app_path" .
    log_success "Console app distribution: $console_zip"
fi

# Web app distribution  
web_app_path="$OUTPUT_PATH/artifacts/$CONFIGURATION/RefactorMCP.Web"
if [[ -d "$web_app_path" ]]; then
    web_zip="$OUTPUT_PATH/distributions/RefactorMCP-Web-$CONFIGURATION-$BUILD_ID.tar.gz"
    tar -czf "$web_zip" -C "$web_app_path" .
    log_success "Web app distribution: $web_zip"
fi

# Final success message
log_header "Build completed successfully! 🎉"
log_success "Build ID: $BUILD_ID"
log_success "Output directory: $OUTPUT_PATH"
log_success "Manifest: $OUTPUT_PATH/build-manifest.json"