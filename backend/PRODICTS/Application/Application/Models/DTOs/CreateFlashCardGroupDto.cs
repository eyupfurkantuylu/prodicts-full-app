namespace Application.Models.DTOs;

public class CreateFlashCardGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SourceLanguage { get; set; } = "EN";
    public string TargetLanguage { get; set; } = "TR";
}
