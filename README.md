# Homelab RAG API

A **Retrieval-Augmented Generation (RAG)** system built with .NET 10 that allows you to chat with your homelab documentation using AI. Includes a modern React chat interface.

## 🎯 What It Does

- **Ingests** markdown documentation from your ObsidianVault
- **Generates embeddings** using Ollama (Llama 3.2)
- **Stores** 3072-dimensional vectors in PostgreSQL with pgvector
- **Answers questions** based on your documentation with source citations
- **Beautiful chat UI** with conversation history and source display

## 🏗️ Architecture

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────────┐
│   React     │────▶│  .NET API    │────▶│  Ollama     │────▶│  Llama 3.2   │
│  Frontend   │     │  (RAG Logic) │     │  (Embedder) │     │  (LLM)       │
└─────────────┘     └──────┬───────┘     └─────────────┘     └──────────────┘
                           │
                     ┌─────▼────────┐
                     │  PostgreSQL  │
                     │  + pgvector  │
                     └──────────────┘
```

**Flow:**
1. User asks question in chat UI
2. API generates embedding via Ollama
3. Vector similarity search in PostgreSQL
4. Retrieve top-K similar document chunks
5. Send chunks + question to Llama 3.2
6. Display AI answer with sources in UI

## 🛠️ Tech Stack

**Backend:**
- **.NET 10** (LTS) - Web API
- **PostgreSQL 17** with **pgvector** - Vector database
- **Ollama** (Llama 3.2) - Local LLM for embeddings & text generation
- **Entity Framework Core 10** - ORM

**Frontend:**
- **React 18** + **TypeScript**
- **TailwindCSS** + **shadcn/ui** - Modern UI components
- **Nginx** - Static file serving + API proxy

**Infrastructure:**
- **Docker Compose** - Local development
- **Kubernetes** - Production deployment (planned)

## 📋 Prerequisites

- .NET 10 SDK
- Docker & Docker Compose
- Ollama running with Llama 3.2 model
  ```bash
  ollama pull llama3.2
  ```

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

Start all services (PostgreSQL, API, Frontend):

```bash
docker compose up --build
```

Apply migrations:
```bash
cd HomelabRAG.API
dotnet ef database update
```

**Access:**
- Frontend: http://localhost:8080
- API: http://localhost:5000
- Database: localhost:5432

### Option 2: Manual Setup

#### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Apply Database Migrations

```bash
cd HomelabRAG.API
dotnet ef database update
```

### 3. Run the API

```bash
dotnet run --urls="http://localhost:5000"
```

### 4. Verify Health

```bash
curl http://localhost:5000/healthz
# Response: {"status":"healthy","timestamp":"..."}
```

## 📚 API Endpoints

### Health Check
```bash
GET /healthz
```

### Ingest Single Document
```bash
POST /api/documents/ingest
Content-Type: application/json

{
  "filePath": "/path/to/document.md"
}
```

### Ingest Directory (Batch)
```bash
POST /api/documents/ingest-directory
Content-Type: application/json

{
  "directoryPath": "/path/to/markdown/files"
}
```

### List Documents
```bash
GET /api/documents
```

### Query (Ask Questions)
```bash
POST /api/query
Content-Type: application/json

{
  "question": "How do I deploy to Kubernetes?",
  "topK": 5  // Optional: number of similar chunks to retrieve (default: 5)
}
```

## 💡 Usage Examples

### Ingest Your Documentation
```bash
curl -X POST http://localhost:5000/api/documents/ingest-directory \
  -H "Content-Type: application/json" \
  -d '{"directoryPath": "/home/user/ObsidianVault/02-Knowledge"}'
```

### Ask a Question
```bash
curl -X POST http://localhost:5000/api/query \
  -H "Content-Type: application/json" \
  -d '{"question": "How do I install Flux CD?"}'
```

**Example Response:**
```json
{
  "question": "How do I install Flux CD?",
  "answer": "Based on the provided documentation, you can install Flux CD by running: flux install --version 2.7.5",
  "sources": [
    {
      "fileName": "GitOps-Flux-Setup.md",
      "chunkIndex": 0,
      "preview": "# GitOps with Flux CD - Setup Guide..."
    }
  ],
  "chunksUsed": 5
}
```

## 🔧 Configuration

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=homelab_rag;Username=postgres;Password=postgres"
  },
  "Ollama": {
    "BaseUrl": "http://192.168.50.10:11434",
    "Model": "llama3.2"
  }
}
```

## 📊 Database Schema

### Documents
- `Id` - Primary key
- `FileName` - Document filename
- `FilePath` - Full path to source file
- `Content` - Full document content
- `IngestedAt` - Timestamp

### DocumentChunks
- `Id` - Primary key
- `DocumentId` - Foreign key to Documents
- `Content` - Text chunk (max ~1000 chars)
- `ChunkIndex` - Position in document
- `Embedding` - 3072-dimensional vector (Llama 3.2)

## 🎓 How It Works

1. **Document Ingestion**
   - Reads markdown files
   - Splits into ~1000 character chunks (paragraph boundaries)
   - Generates embeddings via Ollama API
   - Stores in PostgreSQL with pgvector

2. **Query Processing**
   - Converts question to embedding
   - Performs cosine similarity search
   - Retrieves top-K most similar chunks
   - Sends chunks + question to Llama 3.2
   - Returns AI-generated answer with sources

3. **Vector Search**
   - Uses pgvector extension for efficient similarity search
   - Cosine distance metric
   - Indexed for performance

## 🚀 Deployment to Kubernetes

*(Coming soon: Kubernetes manifests, GitOps with Flux)*

## 📝 Interview Talking Points

- "Built a production RAG system with .NET 10 and PostgreSQL pgvector"
- "Integrated local LLM (Llama 3.2) for embeddings and text generation"
- "Implemented vector similarity search for semantic document retrieval"
- "Chunking strategy optimized for 3072-dimensional embeddings"
- "Real-world application: Chat with homelab documentation"

## 🐛 Troubleshooting

### Port Already in Use
```bash
pkill -f "dotnet run"
```

### Database Connection Issues
```bash
docker compose ps  # Verify PostgreSQL is running
docker compose logs postgres
```

### Ollama Not Reachable
```bash
curl http://192.168.50.10:11434/api/tags  # Test Ollama API
```

### Vector Dimension Mismatch
Ensure `RAGDbContext.cs` has correct dimension:
```csharp
.HasColumnType("vector(3072)")  // Match Llama 3.2 embedding size
```

## 📖 Resources

- [pgvector Documentation](https://github.com/pgvector/pgvector)
- [Ollama API Reference](https://github.com/ollama/ollama/blob/main/docs/api.md)
- [RAG Fundamentals](https://www.pinecone.io/learn/retrieval-augmented-generation/)

## 🎯 Future Enhancements

- [ ] Add streaming responses
- [ ] Implement conversation memory
- [ ] Add document re-ranking
- [ ] Deploy to K3s cluster
- [ ] GitOps with Flux CD
- [ ] Prometheus metrics
- [ ] Simple web UI
- [ ] Support for PDF documents
- [ ] Multi-model support (switch between Llama/Phi-3)

## 📄 License

MIT

## 👨‍💻 Author

Built as part of Berlin Platform Engineer interview preparation - Week 8 AI Integration
