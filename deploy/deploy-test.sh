#!/usr/bin/env bash
# deploy-test.sh - Deploy test images to dellbox
# Usage: ./deploy-test.sh

set -euo pipefail

REPO_OWNER="${REPO_OWNER:-aleksandar-grozdanovski}"
COMPOSE_DIR="/srv/docker/apps/homelab-rag-test"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Homelab RAG API Test Deployment ==="
echo "Repo owner: $REPO_OWNER"
echo "Compose dir: $COMPOSE_DIR"

# Ensure compose directory exists
mkdir -p "$COMPOSE_DIR"

# Copy docker-compose-test.yml from script dir
if [ -f "$SCRIPT_DIR/docker-compose-test.yml" ]; then
  echo "Copying docker-compose-test.yml to $COMPOSE_DIR..."
  cp "$SCRIPT_DIR/docker-compose-test.yml" "$COMPOSE_DIR/docker-compose.yml"
fi

if [ -f "$COMPOSE_DIR/docker-compose.yml" ]; then
  echo "Deploying test environment via docker-compose..."
  cd "$COMPOSE_DIR"
  
  # Pull latest test images
  echo "Pulling latest test images..."
  docker compose pull
  
  # Stop old containers and start new ones
  echo "Starting containers..."
  docker compose up -d --remove-orphans
  
  # Show running containers
  echo ""
  echo "=== Running containers ==="
  docker compose ps
  
  echo ""
  echo "=== Test Deployment complete ==="
  echo "Frontend: http://localhost:8084"
  echo "API: http://localhost:5002"
else
  echo "ERROR: No docker-compose-test.yml found in $SCRIPT_DIR"
  exit 1
fi
