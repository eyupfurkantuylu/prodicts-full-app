using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class CreateAppConfigDto
{
    [Required]
    public string AppName { get; set; } = string.Empty;
    [Required]
    public string IosPackageName { get; set; } = string.Empty;
    [Required]
    public string IosVersion { get; set; } = string.Empty;
    [Required]
    public string AndroidPackageName { get; set; } = string.Empty;
    [Required]
    public string AndroidVersion { get; set; } = string.Empty;
    
    public string IosBuildNumber { get; set; } = string.Empty;
    public string AndroidBuildNumber { get; set; } = string.Empty;
}