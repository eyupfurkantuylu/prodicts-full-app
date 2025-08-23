namespace Application.Models.DTOs;

public class UpdateFlashCardGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SourceLanguage { get; set; }
    public string? TargetLanguage { get; set; }
    public bool? IsActive { get; set; }
}
