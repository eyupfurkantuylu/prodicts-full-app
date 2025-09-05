using Domain.Entities;

namespace Domain.Interfaces;

public interface IPodcastSeriesRepository : IRepository<PodcastSeries>
{
    Task<IEnumerable<PodcastSeries>> GetActiveSeriesAsync();
    Task<PodcastSeries?> GetByTitleAsync(string title);
}
