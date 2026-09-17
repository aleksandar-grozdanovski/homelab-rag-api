#!/bin/bash

echo "========================================="
echo "Homelab RAG System - Full Stack Test"
echo "========================================="
echo ""

if [ -z "${API_KEY:-}" ]; then
    echo "API_KEY must be exported before running this test."
    exit 1
fi

# Test API Health
echo "1. Testing API Health..."
HEALTH=$(curl -s http://localhost:5000/healthz | jq -r '.status' 2>/dev/null)
if [ "$HEALTH" = "healthy" ]; then
    echo "   ✅ API is healthy"
else
    echo "   ❌ API is not responding"
    exit 1
fi

# Test Frontend
echo ""
echo "2. Testing Frontend..."
FRONTEND_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080)
if [ "$FRONTEND_STATUS" = "200" ]; then
    echo "   ✅ Frontend is serving"
else
    echo "   ❌ Frontend is not responding (HTTP $FRONTEND_STATUS)"
    exit 1
fi

# Check Documents
echo ""
echo "3. Checking ingested documents..."
DOC_COUNT=$(curl -s -H "X-API-Key: $API_KEY" http://localhost:5000/api/documents | jq 'length' 2>/dev/null)
if [ "$DOC_COUNT" -gt 0 ]; then
    echo "   ✅ Found $DOC_COUNT documents"
else
    echo "   ⚠️  No documents found. Run ingestion:"
    echo "      curl -X POST http://localhost:5000/api/documents/ingest-directory \\"
    echo "        -H 'Content-Type: application/json' \\"
    echo "        -H 'X-API-Key: your-api-key' \\"
    echo "        -d '{\"directoryPath\": \"/documents\"}'"
    exit 1
fi

# Test Query
echo ""
echo "4. Testing RAG Query..."
QUERY_RESULT=$(curl -s -X POST http://localhost:5000/api/query \
  -H "Content-Type: application/json" \
  -H "X-API-Key: $API_KEY" \
  -d '{"question": "How do I install Flux CD?", "topK": 3}' \
  | jq -r '.answer' 2>/dev/null | head -c 100)

if [ ! -z "$QUERY_RESULT" ]; then
    echo "   ✅ Query successful!"
    echo "   Answer preview: $QUERY_RESULT..."
else
    echo "   ❌ Query failed"
    exit 1
fi

echo ""
echo "========================================="
echo "✅ All systems operational!"
echo "========================================="
echo ""
echo "📱 Access points:"
echo "   Frontend: http://localhost:8080"
echo "   API:      http://localhost:5000"
echo "   Docs:     http://localhost:5000/api/documents"
echo ""
echo "🎉 Ready to test in browser!"
echo ""
