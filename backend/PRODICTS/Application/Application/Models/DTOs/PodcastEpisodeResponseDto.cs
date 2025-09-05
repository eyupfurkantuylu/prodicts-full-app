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
    public string AudioUrl { get; set; } = string.Empty;
    public string AudioQualities { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
    
    // Series bilgileri
    public string SeriesTitle { get; set; } = string.Empty;
    public string SeriesDescription { get; set; } = string.Empty;
    public string? SeriesThumbnailUrl { get; set; }
    
    // Season bilgileri
    public int SeasonNumber { get; set; }
    public string SeasonTitle { get; set; } = string.Empty;
    public string SeasonDescription { get; set; } = string.Empty;
}
