# Homelab RAG API - Deployment Guide

## Overview

This directory contains deployment configurations and scripts for the Homelab RAG API on the dellbox server.

## Architecture

- **Production**: Port 8083 (frontend), Port 5001 (API), Port 5433 (PostgreSQL)
- **Test**: Port 8084 (frontend), Port 5002 (API), Port 5434 (PostgreSQL)

## GitHub Actions CI/CD

The project uses GitHub Actions to build and publish Docker images to GitHub Container Registry (GHCR).

### Workflow Triggers

- **Push to `main`**: Builds and pushes `latest` tag
- **Push to `test`**: Builds and pushes `test` tag  
- **Pull requests**: Builds but doesn't push

### Images Published

- `ghcr.io/aleksandar-grozdanovski/homelab-rag-api:latest` (or `:test`)
- `ghcr.io/aleksandar-grozdanovski/homelab-rag-frontend:latest` (or `:test`)

## Deployment to Dellbox

### Prerequisites

1. SSH access to dellbox server
2. Docker and docker-compose installed on dellbox
3. GitHub Container Registry authentication (if images are private)

### Production Deployment

```bash
# On dellbox
cd /srv/docker/apps/homelab-rag
./deploy.sh
```

Or remotely:
```bash
ssh dellbox 'cd /srv/docker/apps/homelab-rag && ./deploy.sh'
```

### Test Environment Deployment

```bash
# On dellbox
cd /srv/docker/apps/homelab-rag-test  
./deploy-test.sh
```

### Manual Deployment

1. Copy deployment files to dellbox:
```bash
scp -r deploy/* dellbox:/srv/docker/apps/homelab-rag/
```

2. SSH to dellbox and run deployment:
```bash
ssh dellbox
cd /srv/docker/apps/homelab-rag
chmod +x *.sh
./deploy.sh
```

## Configuration

### LLM Provider Settings

The API supports two LLM providers:

#### Groq (Cloud, Fast)
```yaml
environment:
  - LLMProvider=groq
  - GroqSettings__Enabled=true
  - GroqSettings__ApiKey=YOUR_API_KEY
  - GroqSettings__ApiUrl=https://api.groq.com/openai/v1
  - GroqSettings__Model=llama-3.3-70b-versatile
```

#### Ollama (Local, Private)
```yaml
environment:
  - LLMProvider=ollama
  - Ollama__BaseUrl=http://192.168.50.10:11434
  - Ollama__Model=llama3.2
```

### Environment Variables

Edit `docker-compose.yml` to configure:

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Ollama__BaseUrl`: Ollama server URL (default: http://192.168.50.10:11434)
- `GroqSettings__ApiKey`: Groq API key for cloud LLM
- `LLMProvider`: Choose between "ollama" or "groq"

## Accessing the Application

### Production
- Frontend: http://homelab-rag.home.arpa or http://192.168.50.10:8083
- API: http://192.168.50.10:5001
- Health check: `curl http://192.168.50.10:5001/healthz`

### Test
- Frontend: http://192.168.50.10:8084
- API: http://192.168.50.10:5002

## DNS Configuration (Optional)

Add to local DNS or `/etc/hosts`:
```
192.168.50.10  homelab-rag.home.arpa
```

## Troubleshooting

### Check logs
```bash
docker compose logs -f api
docker compose logs -f frontend
docker compose logs -f postgres
```

### Restart services
```bash
docker compose restart
```

### Pull latest images manually
```bash
docker compose pull
docker compose up -d
```

### Verify database connection
```bash
docker exec homelab-rag-postgres psql -U postgres -c '\l'
```

### Check API health
```bash
curl http://localhost:5001/healthz
```

## Backup and Restore

### Backup PostgreSQL data
```bash
docker exec homelab-rag-postgres pg_dump -U postgres homelab_rag > backup.sql
```

### Restore PostgreSQL data
```bash
cat backup.sql | docker exec -i homelab-rag-postgres psql -U postgres homelab_rag
```

## Security Notes

- Change default PostgreSQL password in production
- Store API keys in environment files outside the repository
- Consider using Docker secrets for sensitive data
- Restrict network access using firewall rules

## Port Conflicts

If ports are already in use, modify the `ports` section in `docker-compose.yml`:

```yaml
ports:
  - "NEW_PORT:80"  # Change NEW_PORT to an available port
```

## Updates and Rollbacks

### Update to latest
```bash
./deploy.sh  # Pulls latest images and restarts
```

### Rollback
```bash
# Specify a specific image tag
docker compose pull ghcr.io/aleksandar-grozdanovski/homelab-rag-api:<commit-sha>
docker compose up -d
```
