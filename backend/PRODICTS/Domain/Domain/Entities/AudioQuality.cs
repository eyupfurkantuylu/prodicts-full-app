using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class AudioQuality
{
    [BsonElement("quality")]
    public string Quality { get; set; } = string.Empty; // "64k", "128k", "256k", "original"
    
    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;
    
    [BsonElement("fileSize")]
    public long FileSize { get; set; }
    
    [BsonElement("bitrate")]
    public int Bitrate { get; set; } // kbps
    
    [BsonElement("isProcessed")]
    public bool IsProcessed { get; set; }
    
    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }
}
