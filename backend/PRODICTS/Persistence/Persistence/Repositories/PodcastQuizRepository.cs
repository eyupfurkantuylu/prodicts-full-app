using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Persistence.Context;

namespace Persistence.Repositories;

public class PodcastQuizRepository : BaseRepository<PodcastQuiz>, IPodcastQuizRepository
{
    public PodcastQuizRepository(MongoDbContext context) : base(context, "PodcastQuizzes")
    {
    }

    public async Task<IEnumerable<PodcastQuiz>> GetByEpisodeIdAsync(string episodeId)
    {
        var filter = Builders<PodcastQuiz>.Filter.Eq(x => x.PodcastEpisodeId, episodeId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<bool> DeleteByEpisodeIdAsync(string episodeId)
    {
        var filter = Builders<PodcastQuiz>.Filter.Eq(x => x.PodcastEpisodeId, episodeId);
        var result = await _collection.DeleteManyAsync(filter);
        return result.DeletedCount > 0;
    }
}
