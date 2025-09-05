using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Persistence.Context;

namespace Persistence.Repositories;

public class PodcastSeasonRepository : BaseRepository<PodcastSeason>, IPodcastSeasonRepository
{
    public PodcastSeasonRepository(MongoDbContext context) : base(context, "PodcastSeasons")
    {
    }

    public async Task<IEnumerable<PodcastSeason>> GetBySeriesIdAsync(string seriesId)
    {
        var filter = Builders<PodcastSeason>.Filter.Eq(x => x.PodcastSeriesId, seriesId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<PodcastSeason>> GetActiveSeasonsBySeriesIdAsync(string seriesId)
    {
        var filter = Builders<PodcastSeason>.Filter.And(
            Builders<PodcastSeason>.Filter.Eq(x => x.PodcastSeriesId, seriesId),
            Builders<PodcastSeason>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<PodcastSeason?> GetBySeriesIdAndSeasonNumberAsync(string seriesId, int seasonNumber)
    {
        var filter = Builders<PodcastSeason>.Filter.And(
            Builders<PodcastSeason>.Filter.Eq(x => x.PodcastSeriesId, seriesId),
            Builders<PodcastSeason>.Filter.Eq(x => x.SeasonNumber, seasonNumber)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
