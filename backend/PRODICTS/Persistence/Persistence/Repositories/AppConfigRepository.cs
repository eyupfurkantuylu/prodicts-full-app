using Domain.Entities;
using Domain.Interfaces;
using Persistence.Context;

namespace Persistence.Repositories;

public class AppConfigRepository : BaseRepository<AppConfig>, IAppConfigRepository
{
    public AppConfigRepository(MongoDbContext context) : base(context, nameof(context.AppConfig))
    {
    }
}