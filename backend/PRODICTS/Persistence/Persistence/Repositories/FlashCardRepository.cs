using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Persistence.Context;

namespace Persistence.Repositories;

public class FlashCardRepository : BaseRepository<FlashCard>, IFlashCardRepository
{
    public FlashCardRepository(MongoDbContext context) : base(context, "FlashCards")
    {
    }

    public async Task<IEnumerable<FlashCard>> GetByUserIdAsync(string userId)
    {
        var filter = Builders<FlashCard>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<FlashCard>> GetByGroupIdAsync(string groupId)
    {
        var filter = Builders<FlashCard>.Filter.Eq(x => x.GroupId, groupId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<FlashCard>> GetDueForReviewAsync(string userId)
    {
        var filter = Builders<FlashCard>.Filter.And(
            Builders<FlashCard>.Filter.Eq(x => x.UserId, userId),
            Builders<FlashCard>.Filter.Lte(x => x.NextReviewDate, DateTime.UtcNow),
            Builders<FlashCard>.Filter.Eq(x => x.IsCompleted, false)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<FlashCard?> GetByUserIdAndIdAsync(string userId, string id)
    {
        var filter = Builders<FlashCard>.Filter.And(
            Builders<FlashCard>.Filter.Eq(x => x.UserId, userId),
            Builders<FlashCard>.Filter.Eq(x => x.Id, id)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task UpdateReviewAsync(string id, int currentStep, DateTime nextReviewDate)
    {
        var filter = Builders<FlashCard>.Filter.Eq(x => x.Id, id);
        var update = Builders<FlashCard>.Update
            .Set(x => x.CurrentStep, currentStep)
            .Set(x => x.NextReviewDate, nextReviewDate)
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Push(x => x.ReviewDates, DateTime.UtcNow);

        if (currentStep == 1)
        {
            update = update.Set(x => x.FirstLearningDate, DateTime.UtcNow);
        }

        if (currentStep >= 6)
        {
            update = update.Set(x => x.IsCompleted, true);
        }

        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task<bool> DeleteByGroupIdAsync(string groupId)
    {
        var filter = Builders<FlashCard>.Filter.Eq(x => x.GroupId, groupId);
        var result = await _collection.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }
}
