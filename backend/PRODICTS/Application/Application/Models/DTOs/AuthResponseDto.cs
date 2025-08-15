namespace Application.Models.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = new();
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
