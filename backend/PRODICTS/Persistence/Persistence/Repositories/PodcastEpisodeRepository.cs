using Domain.Entities;
using Domain.Interfaces;
using MongoDB.Driver;
using Persistence.Context;

namespace Persistence.Repositories;

public class PodcastEpisodeRepository : BaseRepository<PodcastEpisode>, IPodcastEpisodeRepository
{
    public PodcastEpisodeRepository(MongoDbContext context) : base(context, "PodcastEpisodes")
    {
    }

    public async Task<IEnumerable<PodcastEpisode>> GetBySeriesIdAsync(string seriesId)
    {
        var filter = Builders<PodcastEpisode>.Filter.Eq(x => x.PodcastSeriesId, seriesId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<PodcastEpisode>> GetActiveEpisodesBySeriesIdAsync(string seriesId)
    {
        var filter = Builders<PodcastEpisode>.Filter.And(
            Builders<PodcastEpisode>.Filter.Eq(x => x.PodcastSeriesId, seriesId),
            Builders<PodcastEpisode>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<PodcastEpisode>> GetBySeasonIdAsync(string seasonId)
    {
        var filter = Builders<PodcastEpisode>.Filter.Eq(x => x.PodcastSeasonId, seasonId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<PodcastEpisode>> GetActiveEpisodesBySeasonIdAsync(string seasonId)
    {
        var filter = Builders<PodcastEpisode>.Filter.And(
            Builders<PodcastEpisode>.Filter.Eq(x => x.PodcastSeasonId, seasonId),
            Builders<PodcastEpisode>.Filter.Eq(x => x.IsActive, true)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<PodcastEpisode?> GetBySeriesIdAndEpisodeNumberAsync(string seriesId, int episodeNumber)
    {
        var filter = Builders<PodcastEpisode>.Filter.And(
            Builders<PodcastEpisode>.Filter.Eq(x => x.PodcastSeriesId, seriesId),
            Builders<PodcastEpisode>.Filter.Eq(x => x.EpisodeNumber, episodeNumber)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PodcastEpisode>> GetLatestEpisodesAsync(int count)
    {
        var filter = Builders<PodcastEpisode>.Filter.Eq(x => x.IsActive, true);
        return await _collection.Find(filter)
            .SortByDescending(x => x.ReleaseDate)
            .Limit(count)
            .ToListAsync();
    }
}
