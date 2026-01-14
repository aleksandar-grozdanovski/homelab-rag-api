namespace HomelabRAG.API.Models;

public class Document
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string Content { get; set; }
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
