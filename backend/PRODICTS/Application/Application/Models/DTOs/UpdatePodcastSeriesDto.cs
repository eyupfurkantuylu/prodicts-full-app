namespace Application.Models.DTOs;

public class UpdatePodcastSeriesDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool? IsActive { get; set; }
}
