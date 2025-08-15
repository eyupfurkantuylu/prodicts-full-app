using Domain.Entities;
using Domain.Interfaces;
using Persistence.Context;

namespace Persistence.Repositories;

public class AnonymousUserRepository : BaseRepository<AnonymousUser>, IAnonymousUserRepository
{
    public AnonymousUserRepository(MongoDbContext context) : base(context, nameof(context.AnonymousUsers))
    {
    }

    public async Task<AnonymousUser?> GetByDeviceIdAsync(string deviceId)
    {
        return await FindOneAsync(au => au.DeviceId == deviceId && au.IsActive);
    }

    public async Task<bool> DeviceIdExistsAsync(string deviceId)
    {
        return await FindOneAsync(au => au.DeviceId == deviceId && au.IsActive) != null;
    }

    public async Task<List<AnonymousUser>> GetInactiveUsersAsync(DateTime lastActiveThreshold)
    {
        var result = await FindAsync(au => au.LastActiveAt < lastActiveThreshold && au.IsActive);
        return result.ToList();
    }

    public async Task<bool> MarkAsUpgradedAsync(string deviceId, string upgradedUserId)
    {
        var anonymousUser = await GetByDeviceIdAsync(deviceId);
        if (anonymousUser == null) return false;

        anonymousUser.IsUpgraded = true;
        anonymousUser.UpgradedUserId = upgradedUserId;
        anonymousUser.LastSyncAt = DateTime.UtcNow;

        await UpdateAsync(anonymousUser);
        return true;
    }

    public async Task DeleteInactiveUsersAsync(DateTime lastActiveThreshold)
    {
        var inactiveUsers = await GetInactiveUsersAsync(lastActiveThreshold);
        
        foreach (var user in inactiveUsers)
        {
            if (!user.IsUpgraded) // Only delete if not upgraded to registered user
            {
                await DeleteAsync(user.Id);
            }
        }
    }
}
