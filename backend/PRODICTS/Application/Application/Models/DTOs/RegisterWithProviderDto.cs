using System.ComponentModel.DataAnnotations;

namespace Application.Models.DTOs;

public class RegisterWithProviderDto
{
    [Required]
    public string ProviderName { get; set; } = string.Empty; // Google, Facebook, Apple

    [Required]
    public string ProviderId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
