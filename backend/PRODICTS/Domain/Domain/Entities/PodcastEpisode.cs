using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Domain.Enums;

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
    public string Title { get; set; } = string.Empty;
    
    [BsonElement("description")] 
    public string Description { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("durationSeconds")]
    public int DurationSeconds { get; set; }

    [BsonElement("originalAudioUrl")]
    public string OriginalAudioUrl { get; set; } = string.Empty;
    
    // Backward compatibility için eski audioUrl field'ını da destekle
    [BsonElement("audioUrl")]
    [BsonIgnoreIfNull]
    public string? AudioUrl 
    { 
        get => string.IsNullOrEmpty(OriginalAudioUrl) ? null : OriginalAudioUrl;
        set => OriginalAudioUrl = value ?? string.Empty;
    }

    [BsonElement("audioQualities")]
    public List<AudioQuality> AudioQualities { get; set; } = new();

    [BsonElement("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [BsonElement("releaseDate")]
    public DateTime ReleaseDate { get; set; }
    
    [BsonElement("processingStatus")]
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Uploaded;
    
    [BsonElement("originalFileName")]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [BsonElement("processingStartedAt")]
    public DateTime? ProcessingStartedAt { get; set; }
    
    [BsonElement("processingCompletedAt")]
    public DateTime? ProcessingCompletedAt { get; set; }
    
    [BsonElement("processingErrorMessage")]
    public string? ProcessingErrorMessage { get; set; }
    
        
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
    
}