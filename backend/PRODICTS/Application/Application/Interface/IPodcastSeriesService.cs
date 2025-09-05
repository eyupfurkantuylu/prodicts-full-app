using Application.Models.DTOs;

namespace Application.Interface;

public interface IPodcastSeriesService
{
    // Public endpoints
    Task<IEnumerable<PodcastSeriesResponseDto>> GetAllActiveAsync();
    Task<PodcastSeriesResponseDto?> GetByIdAsync(string id);
    
    // Admin endpoints
    Task<IEnumerable<PodcastSeriesResponseDto>> GetAllAsync();
    Task<PodcastSeriesResponseDto> CreateAsync(CreatePodcastSeriesDto dto);
    Task<PodcastSeriesResponseDto?> UpdateAsync(string id, UpdatePodcastSeriesDto dto);
    Task<bool> DeleteAsync(string id);
}
