using Domain.Entities;
using Domain.Enums;

namespace Application.Models.DTOs;

public class CreatePodcastEpisodeDto
{
    public string PodcastSeriesId { get; set; } = string.Empty;
    public string PodcastSeasonId { get; set; } = string.Empty;
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string OriginalAudioUrl { get; set; } = string.Empty;
    public List<AudioQuality> AudioQualities { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Uploaded;
    public string OriginalFileName { get; set; } = string.Empty;
}
