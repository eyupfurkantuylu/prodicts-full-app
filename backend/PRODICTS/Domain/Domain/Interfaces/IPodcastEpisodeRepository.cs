using Domain.Entities;

namespace Domain.Interfaces;

public interface IPodcastEpisodeRepository : IRepository<PodcastEpisode>
{
    Task<IEnumerable<PodcastEpisode>> GetBySeriesIdAsync(string seriesId);
    Task<IEnumerable<PodcastEpisode>> GetActiveEpisodesBySeriesIdAsync(string seriesId);
    Task<IEnumerable<PodcastEpisode>> GetBySeasonIdAsync(string seasonId);
    Task<IEnumerable<PodcastEpisode>> GetActiveEpisodesBySeasonIdAsync(string seasonId);
    Task<PodcastEpisode?> GetBySeriesIdAndEpisodeNumberAsync(string seriesId, int episodeNumber);
    Task<IEnumerable<PodcastEpisode>> GetLatestEpisodesAsync(int count);
}
