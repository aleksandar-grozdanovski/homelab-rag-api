#!/usr/bin/env bash
# deploy.sh - Run on dellbox to deploy latest images from GHCR via docker-compose
# Usage: ./deploy.sh [compose-dir]
#
# Prerequisites:
#   - docker-compose.yml in the compose directory
#   - Docker and docker-compose-plugin installed
#   - GHCR images accessible (public or authenticated)

set -euo pipefail

REPO_OWNER="${REPO_OWNER:-aleksandar-grozdanovski}"
COMPOSE_DIR="${1:-/srv/docker/apps/homelab-rag}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Homelab RAG API Deployment ==="
echo "Repo owner: $REPO_OWNER"
echo "Compose dir: $COMPOSE_DIR"

# Ensure compose directory exists
mkdir -p "$COMPOSE_DIR"

# Copy docker-compose.yml from script dir if not present in compose dir
if [ ! -f "$COMPOSE_DIR/docker-compose.yml" ] && [ -f "$SCRIPT_DIR/docker-compose.yml" ]; then
  echo "Copying docker-compose.yml to $COMPOSE_DIR..."
  cp "$SCRIPT_DIR/docker-compose.yml" "$COMPOSE_DIR/docker-compose.yml"
fi

if [ -f "$COMPOSE_DIR/docker-compose.yml" ]; then
  echo "Deploying via docker-compose..."
  cd "$COMPOSE_DIR"
  
  # Ensure .env file exists
  if [ ! -f ".env" ]; then
    echo "WARNING: No .env file found. API key will need to be set manually."
    echo "Copy deploy/.env.dellbox to $COMPOSE_DIR/.env and edit as needed."
  fi
  
  # Pull latest images
  echo "Pulling latest images..."
  docker compose pull
  
  # Stop old containers and start new ones
  echo "Starting containers..."
  docker compose up -d --remove-orphans
  
  # Show running containers
  echo ""
  echo "=== Running containers ==="
  docker compose ps
  
  echo ""
  echo "=== Deployment complete ==="
  echo "Frontend: http://homelab-rag.home.arpa (port 8083)"
  echo "API: http://localhost:5001"
  echo ""
  echo "Health check:"
  echo "  curl http://localhost:5001/healthz"
else
  echo "ERROR: No docker-compose.yml found in $COMPOSE_DIR or $SCRIPT_DIR"
  echo "Please ensure docker-compose.yml exists."
  exit 1
fi
