using Pgvector;

namespace HomelabRAG.API.Models;

public class DocumentChunk
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public required string Content { get; set; }
    public int ChunkIndex { get; set; }
    public Vector? Embedding { get; set; }
    
    public Document? Document { get; set; }
}
