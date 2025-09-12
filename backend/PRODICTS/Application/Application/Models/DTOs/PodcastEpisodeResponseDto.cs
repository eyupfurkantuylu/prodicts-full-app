using Domain.Entities;
using Domain.Enums;

namespace Application.Models.DTOs;

public class PodcastEpisodeResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string PodcastSeriesId { get; set; } = string.Empty;
    public string PodcastSeasonId { get; set; } = string.Empty;
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int DurationSeconds { get; set; }
    public string OriginalAudioUrl { get; set; } = string.Empty;
    public List<AudioQuality> AudioQualities { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? ProcessingCompletedAt { get; set; }
    public string? ProcessingErrorMessage { get; set; }
    
    // Series bilgileri
    public string SeriesTitle { get; set; } = string.Empty;
    public string SeriesDescription { get; set; } = string.Empty;
    public string? SeriesThumbnailUrl { get; set; }
    
    // Season bilgileri
    public int SeasonNumber { get; set; }
    public string SeasonTitle { get; set; } = string.Empty;
    public string SeasonDescription { get; set; } = string.Empty;
}
