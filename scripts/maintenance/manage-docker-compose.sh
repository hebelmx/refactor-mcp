#!/bin/bash
# Manage Docker Compose Script
# Location: scripts/maintenance/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"

PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

show_help() {
    print_header "Docker Compose Manager"
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  start         - Start all services"
    echo "  stop          - Stop all services"
    echo "  restart       - Restart all services"
    echo "  status        - Show service status"
    echo "  logs          - Show service logs"
    echo "  pull          - Pull latest images"
    echo "  clean         - Clean unused containers/images"
    echo ""
    echo "Examples:"
    echo "  $0 start      # Start Seq service"
    echo "  $0 logs       # Follow service logs"
    echo "  $0 status     # Check service health"
}

get_compose_cmd() {
    if docker compose version >/dev/null 2>&1; then
        echo "docker compose"
    else
        echo "docker-compose"
    fi
}

start_services() {
    print_header "Starting Docker Compose Services"
    
    cd "$PROJECT_ROOT"
    
    local compose_cmd=$(get_compose_cmd)
    print_status "Using: $compose_cmd"
    
    $compose_cmd up -d
    
    print_success "Services started"
    show_status
}

stop_services() {
    print_header "Stopping Docker Compose Services"
    
    cd "$PROJECT_ROOT"
    
    local compose_cmd=$(get_compose_cmd)
    $compose_cmd down
    
    print_success "Services stopped"
}

restart_services() {
    print_header "Restarting Docker Compose Services"
    
    cd "$PROJECT_ROOT"
    
    local compose_cmd=$(get_compose_cmd)
    $compose_cmd restart
    
    print_success "Services restarted"
    show_status
}

show_status() {
    print_section "Service Status"
    
    cd "$PROJECT_ROOT"
    
    local compose_cmd=$(get_compose_cmd)
    $compose_cmd ps
    
    echo ""
    print_section "Health Checks"
    
    # Test Seq endpoint
    if curl -s http://localhost:5341/api >/dev/null 2>&1; then
        print_success "✅ Seq API: http://localhost:5341"
    else
        print_error "❌ Seq API not responding"
    fi
    
    # Test Seq web interface
    if curl -s http://localhost:5341 >/dev/null 2>&1; then
        print_success "✅ Seq Web: http://localhost:5341"
    else
        print_error "❌ Seq Web interface not responding"
    fi
}

show_logs() {
    print_header "Docker Compose Logs"
    
    cd "$PROJECT_ROOT"
    
    local compose_cmd=$(get_compose_cmd)
    
    if [ "$1" = "follow" ] || [ "$1" = "-f" ]; then
        print_status "Following logs (Ctrl+C to stop)..."
        $compose_cmd logs -f
    else
        print_status "Recent logs:"
        $compose_cmd logs --tail=50
    fi
}

pull_images() {
    print_header "Pulling Latest Images"
    
    cd "$PROJECT_ROOT"
    
    local compose_cmd=$(get_compose_cmd)
    $compose_cmd pull
    
    print_success "Images updated"
    print_status "Run 'restart' to use new images"
}

clean_docker() {
    print_header "Cleaning Docker Resources"
    
    print_status "Removing stopped containers..."
    docker container prune -f
    
    print_status "Removing unused images..."
    docker image prune -f
    
    print_status "Removing unused volumes..."
    docker volume prune -f
    
    print_status "Removing unused networks..."
    docker network prune -f
    
    print_success "Docker cleanup completed"
}

main() {
    case "${1:-help}" in
        start)
            start_services
            ;;
        stop)
            stop_services
            ;;
        restart)
            restart_services
            ;;
        status)
            show_status
            ;;
        logs)
            show_logs "${2:-recent}"
            ;;
        pull)
            pull_images
            ;;
        clean)
            clean_docker
            ;;
        help|*)
            show_help
            ;;
    esac
}

main "$@"