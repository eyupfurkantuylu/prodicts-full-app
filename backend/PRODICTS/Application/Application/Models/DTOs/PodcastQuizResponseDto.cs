namespace Application.Models.DTOs;

public class PodcastQuizResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string PodcastEpisodeId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answers { get; set; } = string.Empty;
    public int CorrectAnswerIndex { get; set; }
}
