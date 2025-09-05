using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class PodcastSeasonService : IPodcastSeasonService
{
    private readonly IPodcastSeasonRepository _podcastSeasonRepository;

    public PodcastSeasonService(IPodcastSeasonRepository podcastSeasonRepository)
    {
        _podcastSeasonRepository = podcastSeasonRepository;
    }

    // Public endpoints
    public async Task<IEnumerable<PodcastSeasonResponseDto>> GetBySeriesIdAsync(string seriesId)
    {
        var seasons = await _podcastSeasonRepository.GetActiveSeasonsBySeriesIdAsync(seriesId);
        return seasons.Select(MapToResponseDto);
    }

    public async Task<PodcastSeasonResponseDto?> GetByIdAsync(string id)
    {
        var season = await _podcastSeasonRepository.GetByIdAsync(id);
        return season != null ? MapToResponseDto(season) : null;
    }

    // Admin endpoints
    public async Task<IEnumerable<PodcastSeasonResponseDto>> GetAllAsync()
    {
        var seasons = await _podcastSeasonRepository.GetAllAsync();
        return seasons.Select(MapToResponseDto);
    }

    public async Task<PodcastSeasonResponseDto> CreateAsync(CreatePodcastSeasonDto dto)
    {
        var season = new PodcastSeason
        {
            PodcastSeriesId = dto.PodcastSeriesId,
            SeasonNumber = dto.SeasonNumber,
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdSeason = await _podcastSeasonRepository.CreateAsync(season);
        return MapToResponseDto(createdSeason);
    }

    public async Task<PodcastSeasonResponseDto?> UpdateAsync(string id, UpdatePodcastSeasonDto dto)
    {
        var existingSeason = await _podcastSeasonRepository.GetByIdAsync(id);
        if (existingSeason == null)
            return null;

        if (dto.SeasonNumber.HasValue) existingSeason.SeasonNumber = dto.SeasonNumber.Value;
        if (dto.Title != null) existingSeason.Title = dto.Title;
        if (dto.Description != null) existingSeason.Description = dto.Description;
        if (dto.IsActive.HasValue) existingSeason.IsActive = dto.IsActive.Value;

        var updatedSeason = await _podcastSeasonRepository.UpdateAsync(existingSeason);
        return MapToResponseDto(updatedSeason);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _podcastSeasonRepository.DeleteAsync(id);
    }

    private static PodcastSeasonResponseDto MapToResponseDto(PodcastSeason season)
    {
        return new PodcastSeasonResponseDto
        {
            Id = season.Id,
            PodcastSeriesId = season.PodcastSeriesId,
            SeasonNumber = season.SeasonNumber,
            Title = season.Title,
            Description = season.Description,
            CreatedAt = season.CreatedAt,
            IsActive = season.IsActive
        };
    }
}
