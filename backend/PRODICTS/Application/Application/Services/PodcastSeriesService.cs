using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class PodcastSeriesService : IPodcastSeriesService
{
    private readonly IPodcastSeriesRepository _podcastSeriesRepository;

    public PodcastSeriesService(IPodcastSeriesRepository podcastSeriesRepository)
    {
        _podcastSeriesRepository = podcastSeriesRepository;
    }

    // Public endpoints
    public async Task<IEnumerable<PodcastSeriesResponseDto>> GetAllActiveAsync()
    {
        var series = await _podcastSeriesRepository.GetActiveSeriesAsync();
        return series.Select(MapToResponseDto);
    }

    public async Task<PodcastSeriesResponseDto?> GetByIdAsync(string id)
    {
        var series = await _podcastSeriesRepository.GetByIdAsync(id);
        return series != null ? MapToResponseDto(series) : null;
    }

    // Admin endpoints
    public async Task<IEnumerable<PodcastSeriesResponseDto>> GetAllAsync()
    {
        var series = await _podcastSeriesRepository.GetAllAsync();
        return series.Select(MapToResponseDto);
    }

    public async Task<PodcastSeriesResponseDto> CreateAsync(CreatePodcastSeriesDto dto)
    {
        var series = new PodcastSeries
        {
            Title = dto.Title,
            Description = dto.Description,
            ThumbnailUrl = dto.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdSeries = await _podcastSeriesRepository.CreateAsync(series);
        return MapToResponseDto(createdSeries);
    }

    public async Task<PodcastSeriesResponseDto?> UpdateAsync(string id, UpdatePodcastSeriesDto dto)
    {
        var existingSeries = await _podcastSeriesRepository.GetByIdAsync(id);
        if (existingSeries == null)
            return null;

        if (dto.Title != null) existingSeries.Title = dto.Title;
        if (dto.Description != null) existingSeries.Description = dto.Description;
        if (dto.ThumbnailUrl != null) existingSeries.ThumbnailUrl = dto.ThumbnailUrl;
        if (dto.IsActive.HasValue) existingSeries.IsActive = dto.IsActive.Value;

        var updatedSeries = await _podcastSeriesRepository.UpdateAsync(existingSeries);
        return MapToResponseDto(updatedSeries);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _podcastSeriesRepository.DeleteAsync(id);
    }

    private static PodcastSeriesResponseDto MapToResponseDto(PodcastSeries series)
    {
        return new PodcastSeriesResponseDto
        {
            Id = series.Id,
            Title = series.Title,
            Description = series.Description,
            ThumbnailUrl = series.ThumbnailUrl,
            CreatedAt = series.CreatedAt,
            IsActive = series.IsActive
        };
    }
}
