using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Persistence.Context;

namespace Persistence.Repositories;

public class FlashCardGroupRepository : BaseRepository<FlashCardGroup>, IFlashCardGroupRepository
{
    public FlashCardGroupRepository(MongoDbContext context) : base(context, "FlashCardGroups")
    {
    }

    public async Task<IEnumerable<FlashCardGroup>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<FlashCardGroup>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<FlashCardGroup?> GetByUserIdAndIdAsync(string userId, string id)
    {
        var filter = Builders<FlashCardGroup>.Filter.And(
            Builders<FlashCardGroup>.Filter.Eq(x => x.UserId, userId),
            Builders<FlashCardGroup>.Filter.Eq(x => x.Id, id)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByUserIdAndNameAsync(string userId, string name)
    {
        var filter = Builders<FlashCardGroup>.Filter.And(
            Builders<FlashCardGroup>.Filter.Eq(x => x.UserId, userId),
            Builders<FlashCardGroup>.Filter.Eq(x => x.Name, name)
        );
        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }
}
