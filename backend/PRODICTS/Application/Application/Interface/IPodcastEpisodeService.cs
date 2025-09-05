using Application.Models.DTOs;

namespace Application.Interface;

public interface IPodcastEpisodeService
{
    // Public endpoints
    Task<IEnumerable<PodcastEpisodeResponseDto>> GetBySeriesIdAsync(string seriesId);
    Task<IEnumerable<PodcastEpisodeResponseDto>> GetBySeasonIdAsync(string seasonId);
    Task<PodcastEpisodeResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<PodcastEpisodeResponseDto>> GetLatestEpisodesAsync(int count = 10);
    
    // Admin endpoints
    Task<IEnumerable<PodcastEpisodeResponseDto>> GetAllAsync();
    Task<PodcastEpisodeResponseDto> CreateAsync(CreatePodcastEpisodeDto dto);
    Task<PodcastEpisodeResponseDto?> UpdateAsync(string id, UpdatePodcastEpisodeDto dto);
    Task<bool> DeleteAsync(string id);
}
