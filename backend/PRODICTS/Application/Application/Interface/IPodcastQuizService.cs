using Application.Models.DTOs;

namespace Application.Interface;

public interface IPodcastQuizService
{
    // Public endpoints
    Task<IEnumerable<PodcastQuizResponseDto>> GetByEpisodeIdAsync(string episodeId);
    Task<PodcastQuizResponseDto?> GetByIdAsync(string id);
    
    // Admin endpoints
    Task<IEnumerable<PodcastQuizResponseDto>> GetAllAsync();
    Task<PodcastQuizResponseDto> CreateAsync(CreatePodcastQuizDto dto);
    Task<PodcastQuizResponseDto?> UpdateAsync(string id, UpdatePodcastQuizDto dto);
    Task<bool> DeleteAsync(string id);
}
