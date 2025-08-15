using Domain.Entities;
using Domain.Interfaces;
using Persistence.Context;

namespace Persistence.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(MongoDbContext context) : base(context, nameof(context.RefreshTokens))
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await FindOneAsync(rt => rt.Token == token);
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        var tokens = await FindAsync(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow);
        return tokens.ToList();
    }

    public async Task<List<RefreshToken>> GetTokensByDeviceIdAsync(string deviceId)
    {
        var tokens = await FindAsync(rt => rt.DeviceId == deviceId);
        return tokens.ToList();
    }

    public async Task RevokeTokenAsync(string token, string? revokedByIp = null)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken == null) return;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = revokedByIp;

        await UpdateAsync(refreshToken);
    }

    public async Task RevokeAllUserTokensAsync(string userId, string? revokedByIp = null)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId);
        
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            await UpdateAsync(token);
        }
    }

    public async Task RevokeDeviceTokensAsync(string deviceId, string? revokedByIp = null)
    {
        var deviceTokens = await GetTokensByDeviceIdAsync(deviceId);
        var activeTokens = deviceTokens.Where(t => t.IsActive);
        
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            await UpdateAsync(token);
        }
    }

    public async Task<bool> IsTokenActiveAsync(string token)
    {
        var refreshToken = await GetByTokenAsync(token);
        return refreshToken?.IsActive ?? false;
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await FindAsync(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsUsed || rt.IsRevoked);
        
        foreach (var token in expiredTokens)
        {
            // 7 günden eski expired token'ları sil
            if (token.ExpiresAt < DateTime.UtcNow.AddDays(-7))
            {
                await DeleteAsync(token.Id);
            }
        }
    }
}
