using Domain.Entities;

namespace Domain.Interfaces;

public interface IPodcastQuizRepository : IRepository<PodcastQuiz>
{
    Task<IEnumerable<PodcastQuiz>> GetByEpisodeIdAsync(string episodeId);
    Task<bool> DeleteByEpisodeIdAsync(string episodeId);
}
