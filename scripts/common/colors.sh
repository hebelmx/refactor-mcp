#!/bin/bash
# Common color definitions and output functions
# Source this file in other scripts: source "$(dirname "$0")/../common/colors.sh"

# Colors
export RED='\033[0;31m'
export GREEN='\033[0;32m'
export YELLOW='\033[1;33m'
export BLUE='\033[0;34m'
export PURPLE='\033[0;35m'
export CYAN='\033[0;36m'
export WHITE='\033[1;37m'
export NC='\033[0m' # No Color

# Output functions
print_header() {
    echo -e "\n${BLUE}╔══════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║${WHITE} $1 ${BLUE}║${NC}"
    echo -e "${BLUE}╚══════════════════════════════════════════╝${NC}\n"
}

print_status() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }
print_debug() { echo -e "${PURPLE}[DEBUG]${NC} $1"; }

# Progress indicators
print_step() { echo -e "${CYAN}[STEP $1]${NC} $2"; }
print_substep() { echo -e "  ${YELLOW}→${NC} $1"; }

# Separators
print_separator() { echo -e "${BLUE}────────────────────────────────────────${NC}"; }
print_section() { 
    echo ""
    echo -e "${BLUE}▶ $1${NC}"
    echo -e "${BLUE}────────────────────────────────────────${NC}"
}