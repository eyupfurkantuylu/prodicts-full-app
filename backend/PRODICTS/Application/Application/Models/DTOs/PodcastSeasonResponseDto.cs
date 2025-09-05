namespace Application.Models.DTOs;

public class PodcastSeasonResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string PodcastSeriesId { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
