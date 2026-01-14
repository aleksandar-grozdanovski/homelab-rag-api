# Testing the RAG Frontend Locally

## Quick Start

### 1. Start all services with Docker Compose

```bash
cd /home/acedxl/Documents/HomeLab/repo/homelab-rag-api
docker compose up --build
```

This will start:
- **PostgreSQL** with pgvector on port 5432
- **.NET RAG API** on port 5000
- **React Frontend** (nginx) on port 8080

### 2. Apply database migrations

In a new terminal:
```bash
cd HomelabRAG.API
dotnet ef database update
```

### 3. Ingest your documentation

```bash
curl -X POST http://localhost:5000/api/documents/ingest-directory \
  -H "Content-Type: application/json" \
  -d '{"directoryPath": "/home/acedxl/Documents/HomeLab/ObsidianVault/02-Knowledge"}'
```

### 4. Open the chat interface

Visit: **http://localhost:8080**

## Architecture

```
┌─────────────────┐
│  Browser        │
│  localhost:8080 │
└────────┬────────┘
         │
    ┌────▼──────────────────┐
    │  Nginx Frontend       │
    │  - Serves React app   │
    │  - Proxies /api/*     │
    └────────┬──────────────┘
             │
        ┌────▼───────────────────┐
        │  .NET RAG API          │
        │  localhost:5000        │
        │  - POST /api/query     │
        │  - GET /api/documents  │
        └────────┬───────────────┘
                 │
        ┌────────▼────────────────┐
        │  PostgreSQL + pgvector   │
        │  localhost:5432          │
        └──────────────────────────┘
```

## Testing Features

### Chat Interface
1. Click on a suggestion chip or type a question
2. Watch the AI generate answers with sources
3. Click "X sources" to expand source citations
4. Start a new chat from the header

### Client-side Storage
- All conversations are saved in browser localStorage
- Refresh the page - your chats persist!
- Clear all: Open browser dev tools → Application → Local Storage → Clear

### API Integration
The frontend calls:
- `POST /api/query` with `{question, topK}`
- Receives `{answer, sources[], chunksUsed}`
- Displays sources with fileName, chunkIndex, preview

## Troubleshooting

### Frontend shows "API error"
Check that .NET API is running:
```bash
curl http://localhost:5000/healthz
```

### No sources showing up
Verify documents are ingested:
```bash
curl http://localhost:5000/api/documents
```

### Database connection error
Check PostgreSQL is healthy:
```bash
docker logs homelab-rag-postgres
```

## Development Mode

To run frontend in dev mode with hot reload:

```bash
cd HomelabRAG.Frontend
npm run dev
# Opens on http://localhost:5173
# API calls will go to http://localhost:5000
```

## Next Steps

Once tested locally, you can:
1. Deploy to K3s cluster
2. Add to homelab-gitops repository
3. Configure Caddy reverse proxy for `rag.home.arpa`
4. Add to Pi-hole DNS records
