using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class UpdateAppConfigDto
{
    public required string Id { get; set; }
    [Required]
    public required string AppName { get; set; } 
    [Required]
    public required string IosPackageName { get; set; } 
    [Required]
    public required string IosVersion { get; set; } 
    [Required]
    public required string AndroidPackageName { get; set; }
    [Required]
    public required string AndroidVersion { get; set; } 
    public string IosBuildNumber { get; set; } = string.Empty;
    public string AndroidBuildNumber { get; set; } = string.Empty;
}