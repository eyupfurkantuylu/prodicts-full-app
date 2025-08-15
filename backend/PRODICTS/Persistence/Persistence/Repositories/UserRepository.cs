using Domain.Entities;
using Domain.Interfaces;
using Persistence.Context;

namespace Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context) : base(context, nameof(context.Users))
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await FindOneAsync(u => u.Email == email);
    }

    public async Task<User?> GetByProviderIdAsync(string providerId, string providerName)
    {
        return await FindOneAsync(u => u.Providers.Any(p => p.ProviderId == providerId && p.ProviderName == providerName));
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await FindOneAsync(u => u.Email == email) != null;
    }
}
