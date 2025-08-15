using Domain.Entities;

namespace Domain.Interfaces;

public interface IAnonymousUserRepository : IRepository<AnonymousUser>
{
    Task<AnonymousUser?> GetByDeviceIdAsync(string deviceId);
    Task<bool> DeviceIdExistsAsync(string deviceId);
    Task<List<AnonymousUser>> GetInactiveUsersAsync(DateTime lastActiveThreshold);
    Task<bool> MarkAsUpgradedAsync(string deviceId, string upgradedUserId);
    Task DeleteInactiveUsersAsync(DateTime lastActiveThreshold);
}
