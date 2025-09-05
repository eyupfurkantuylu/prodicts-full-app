namespace Application.Models.DTOs;

public class CreatePodcastSeasonDto
{
    public string PodcastSeriesId { get; set; } = string.Empty;
    public int SeasonNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
