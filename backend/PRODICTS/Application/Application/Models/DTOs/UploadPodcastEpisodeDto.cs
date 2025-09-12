using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class UploadPodcastEpisodeDto
{
    [Required]
    public string PodcastSeriesId { get; set; } = string.Empty;
    
    [Required]
    public string PodcastSeasonId { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Episode number must be greater than 0")]
    public int EpisodeNumber { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? ReleaseDate { get; set; }
    
    public string? ThumbnailUrl { get; set; }
}
