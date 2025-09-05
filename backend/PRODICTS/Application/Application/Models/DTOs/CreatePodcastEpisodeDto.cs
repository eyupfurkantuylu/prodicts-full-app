namespace Application.Models.DTOs;

public class CreatePodcastEpisodeDto
{
    public string PodcastSeriesId { get; set; } = string.Empty;
    public string PodcastSeasonId { get; set; } = string.Empty;
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string AudioQualities { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
}
