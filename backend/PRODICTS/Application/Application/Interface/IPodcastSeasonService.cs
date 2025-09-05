using Application.Models.DTOs;

namespace Application.Interface;

public interface IPodcastSeasonService
{
    // Public endpoints
    Task<IEnumerable<PodcastSeasonResponseDto>> GetBySeriesIdAsync(string seriesId);
    Task<PodcastSeasonResponseDto?> GetByIdAsync(string id);
    
    // Admin endpoints
    Task<IEnumerable<PodcastSeasonResponseDto>> GetAllAsync();
    Task<PodcastSeasonResponseDto> CreateAsync(CreatePodcastSeasonDto dto);
    Task<PodcastSeasonResponseDto?> UpdateAsync(string id, UpdatePodcastSeasonDto dto);
    Task<bool> DeleteAsync(string id);
}
