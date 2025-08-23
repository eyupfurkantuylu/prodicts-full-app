namespace Application.Models.DTOs;

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool EmailVerified { get; set; }
    public List<UserProviderDto> Providers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? SubscriptionProvider { get; set; }
    public string CurrentSubscriptionPlan { get; set; } = string.Empty;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = "User";
}

public class UserProviderDto
{
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
