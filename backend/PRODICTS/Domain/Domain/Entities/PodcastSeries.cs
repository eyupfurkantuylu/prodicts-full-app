using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class PodcastSeries
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("title")]
    public string Title { get; set; } = null!;
    
    [BsonElement("description")]
    public string Description { get; set; } = null!;
    
    [BsonElement("thumbnailUrl")]
    public string ThumbnailUrl { get; set; } = null!;
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}