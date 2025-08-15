using Application.Models.DTOs;

namespace Application.Interface;

public interface IJwtService
{
    string GenerateToken(UserResponseDto user);
    string GenerateAnonymousToken(string deviceId);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
    string? GetDeviceIdFromToken(string token);
    bool IsAnonymousToken(string token);
    DateTime GetTokenExpiration(string token);
}
