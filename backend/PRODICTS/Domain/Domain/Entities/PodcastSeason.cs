using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class PodcastSeason
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("podcastSeriesId")]
    public string PodcastSeriesId { get; set; } = string.Empty;

    [BsonElement("seasonNumber")]
    public int SeasonNumber { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }
    
    [BsonElement("description")] 
    public string Description { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
        
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
    
}