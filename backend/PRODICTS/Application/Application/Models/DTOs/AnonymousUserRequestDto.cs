using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class AnonymousUserRequestDto
{
    [Required]
    public string DeviceId { get; set; } = string.Empty;
    
    public string? DeviceType { get; set; }
    
    public string? AppVersion { get; set; }
}
