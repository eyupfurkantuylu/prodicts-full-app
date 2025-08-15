using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByProviderIdAsync(string providerId, string providerName);
    Task<bool> EmailExistsAsync(string email);
}
