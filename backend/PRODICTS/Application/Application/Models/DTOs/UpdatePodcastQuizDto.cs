namespace Application.Models.DTOs;

public class UpdatePodcastQuizDto
{
    public string? Question { get; set; }
    public string? Answers { get; set; }
    public int? CorrectAnswerIndex { get; set; }
}
