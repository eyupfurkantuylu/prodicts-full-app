using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Persistence.Context;

namespace Persistence.Repositories;

public class PodcastSeriesRepository : BaseRepository<PodcastSeries>, IPodcastSeriesRepository
{
    public PodcastSeriesRepository(MongoDbContext context) : base(context, "PodcastSeries")
    {
    }

    public async Task<IEnumerable<PodcastSeries>> GetActiveSeriesAsync()
    {
        var filter = Builders<PodcastSeries>.Filter.Eq(x => x.IsActive, true);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<PodcastSeries?> GetByTitleAsync(string title)
    {
        var filter = Builders<PodcastSeries>.Filter.Eq(x => x.Title, title);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
