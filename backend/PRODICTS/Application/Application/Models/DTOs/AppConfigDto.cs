namespace Application.Models.DTOs;

public class AppConfigDto
{
    public string Id { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string IosPackageName { get; set; } = string.Empty;
    public string IosVersion { get; set; } = string.Empty;
    public string IosBuildNumber { get; set; } = string.Empty;
    public string AndroidPackageName { get; set; } = string.Empty;
    public string AndroidVersion { get; set; } = string.Empty;
    public string AndroidBuildNumber { get; set; } = string.Empty;
}