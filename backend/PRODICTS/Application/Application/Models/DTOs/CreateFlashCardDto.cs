namespace Application.Models.DTOs;

public class CreateFlashCardDto
{
    public string GroupId { get; set; } = string.Empty;
    public string SourceWord { get; set; } = string.Empty;
    public string TargetWord { get; set; } = string.Empty;
}
