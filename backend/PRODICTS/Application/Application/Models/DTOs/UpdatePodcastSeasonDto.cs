namespace Application.Models.DTOs;

public class UpdatePodcastSeasonDto
{
    public int? SeasonNumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
