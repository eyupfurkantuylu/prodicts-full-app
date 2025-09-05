using Domain.Entities;

namespace Domain.Interfaces;

public interface IPodcastSeasonRepository : IRepository<PodcastSeason>
{
    Task<IEnumerable<PodcastSeason>> GetBySeriesIdAsync(string seriesId);
    Task<IEnumerable<PodcastSeason>> GetActiveSeasonsBySeriesIdAsync(string seriesId);
    Task<PodcastSeason?> GetBySeriesIdAndSeasonNumberAsync(string seriesId, int seasonNumber);
}
