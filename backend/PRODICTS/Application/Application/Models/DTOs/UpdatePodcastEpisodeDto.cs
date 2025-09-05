namespace Application.Models.DTOs;

public class UpdatePodcastEpisodeDto
{
    public string? PodcastSeasonId { get; set; }
    public int? EpisodeNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? DurationSeconds { get; set; }
    public string? AudioUrl { get; set; }
    public string? AudioQualities { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public bool? IsActive { get; set; }
}
