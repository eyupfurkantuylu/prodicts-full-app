using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class PodcastEpisode
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("podcastSeriesId")]
    public string PodcastSeriesId { get; set; } = string.Empty;
    
    [BsonElement("podcastSeasonId")]
    public string PodcastSeasonId { get; set; } = string.Empty;
    
    [BsonElement("episodeNumber")]
    public int EpisodeNumber { get; set; } 
    [BsonElement("title")]
    public string Title { get; set; }
    
    [BsonElement("description")] 
    public string Description { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("durationSeconds")]
    public int DurationSeconds { get; set; }

    [BsonElement("audioUrl")]
    public string AudioUrl { get; set; }

    [BsonElement("audioQualities")]
    public string AudioQualities { get; set; }

    [BsonElement("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [BsonElement("releaseDate")]
    public DateTime ReleaseDate { get; set; }
    
        
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
    
}