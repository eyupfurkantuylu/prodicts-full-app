using Domain.Entities;

namespace Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<List<RefreshToken>> GetTokensByDeviceIdAsync(string deviceId);
    Task RevokeTokenAsync(string token, string? revokedByIp = null);
    Task RevokeAllUserTokensAsync(string userId, string? revokedByIp = null);
    Task RevokeDeviceTokensAsync(string deviceId, string? revokedByIp = null);
    Task<bool> IsTokenActiveAsync(string token);
    Task CleanupExpiredTokensAsync();
}
