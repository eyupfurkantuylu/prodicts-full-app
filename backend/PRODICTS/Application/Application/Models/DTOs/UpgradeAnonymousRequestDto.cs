using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class UpgradeAnonymousRequestDto
{
    [Required]
    public string DeviceId { get; set; } = string.Empty;
    
    [Required]
    public RegisterUserDto RegisterData { get; set; } = new();
}
