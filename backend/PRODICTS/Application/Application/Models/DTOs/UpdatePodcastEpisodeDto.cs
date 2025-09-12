using Domain.Entities;
using Domain.Enums;

namespace Application.Models.DTOs;

public class UpdatePodcastEpisodeDto
{
    public string? PodcastSeasonId { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? DurationSeconds { get; set; }
    public string? OriginalAudioUrl { get; set; }
    public List<AudioQuality>? AudioQualities { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public bool? IsActive { get; set; }
    public ProcessingStatus? ProcessingStatus { get; set; }
    public string? OriginalFileName { get; set; }
    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? ProcessingCompletedAt { get; set; }
    public string? ProcessingErrorMessage { get; set; }
}
