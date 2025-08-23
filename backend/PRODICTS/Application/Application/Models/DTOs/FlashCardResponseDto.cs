namespace Application.Models.DTOs;

public class FlashCardResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string SourceWord { get; set; } = string.Empty;
    public string TargetWord { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public DateTime NextReviewDate { get; set; }
    public DateTime? FirstLearningDate { get; set; }
    public List<DateTime> ReviewDates { get; set; } = new();
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
