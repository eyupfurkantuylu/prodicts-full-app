using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class PodcastEpisodeService : IPodcastEpisodeService
{
    private readonly IPodcastEpisodeRepository _podcastEpisodeRepository;
    private readonly IPodcastSeriesRepository _podcastSeriesRepository;
    private readonly IPodcastSeasonRepository _podcastSeasonRepository;

    public PodcastEpisodeService(
        IPodcastEpisodeRepository podcastEpisodeRepository,
        IPodcastSeriesRepository podcastSeriesRepository,
        IPodcastSeasonRepository podcastSeasonRepository)
    {
        _podcastEpisodeRepository = podcastEpisodeRepository;
        _podcastSeriesRepository = podcastSeriesRepository;
        _podcastSeasonRepository = podcastSeasonRepository;
    }

    // Public endpoints
    public async Task<IEnumerable<PodcastEpisodeResponseDto>> GetBySeriesIdAsync(string seriesId)
    {
        var episodes = await _podcastEpisodeRepository.GetActiveEpisodesBySeriesIdAsync(seriesId);
        return await MapToResponseDtosWithDetails(episodes);
    }

    public async Task<IEnumerable<PodcastEpisodeResponseDto>> GetBySeasonIdAsync(string seasonId)
    {
        var episodes = await _podcastEpisodeRepository.GetActiveEpisodesBySeasonIdAsync(seasonId);
        return await MapToResponseDtosWithDetails(episodes);
    }

    public async Task<PodcastEpisodeResponseDto?> GetByIdAsync(string id)
    {
        var episode = await _podcastEpisodeRepository.GetByIdAsync(id);
        return episode != null ? await MapToResponseDtoWithDetails(episode) : null;
    }

    public async Task<IEnumerable<PodcastEpisodeResponseDto>> GetLatestEpisodesAsync(int count = 10)
    {
        var episodes = await _podcastEpisodeRepository.GetLatestEpisodesAsync(count);
        return await MapToResponseDtosWithDetails(episodes);
    }

    // Admin endpoints
    public async Task<IEnumerable<PodcastEpisodeResponseDto>> GetAllAsync()
    {
        var episodes = await _podcastEpisodeRepository.GetAllAsync();
        return await MapToResponseDtosWithDetails(episodes);
    }

    public async Task<PodcastEpisodeResponseDto> CreateAsync(CreatePodcastEpisodeDto dto)
    {
        var episode = new PodcastEpisode
        {
            PodcastSeriesId = dto.PodcastSeriesId,
            PodcastSeasonId = dto.PodcastSeasonId,
            EpisodeNumber = dto.EpisodeNumber,
            Title = dto.Title,
            Description = dto.Description,
            DurationSeconds = dto.DurationSeconds,
            AudioUrl = dto.AudioUrl,
            AudioQualities = dto.AudioQualities,
            ThumbnailUrl = dto.ThumbnailUrl,
            ReleaseDate = dto.ReleaseDate,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdEpisode = await _podcastEpisodeRepository.CreateAsync(episode);
        return await MapToResponseDtoWithDetails(createdEpisode);
    }

    public async Task<PodcastEpisodeResponseDto?> UpdateAsync(string id, UpdatePodcastEpisodeDto dto)
    {
        var existingEpisode = await _podcastEpisodeRepository.GetByIdAsync(id);
        if (existingEpisode == null)
            return null;

        if (dto.PodcastSeasonId != null) existingEpisode.PodcastSeasonId = dto.PodcastSeasonId;
        if (dto.EpisodeNumber.HasValue) existingEpisode.EpisodeNumber = dto.EpisodeNumber.Value;
        if (dto.Title != null) existingEpisode.Title = dto.Title;
        if (dto.Description != null) existingEpisode.Description = dto.Description;
        if (dto.DurationSeconds.HasValue) existingEpisode.DurationSeconds = dto.DurationSeconds.Value;
        if (dto.AudioUrl != null) existingEpisode.AudioUrl = dto.AudioUrl;
        if (dto.AudioQualities != null) existingEpisode.AudioQualities = dto.AudioQualities;
        if (dto.ThumbnailUrl != null) existingEpisode.ThumbnailUrl = dto.ThumbnailUrl;
        if (dto.ReleaseDate.HasValue) existingEpisode.ReleaseDate = dto.ReleaseDate.Value;
        if (dto.IsActive.HasValue) existingEpisode.IsActive = dto.IsActive.Value;

        var updatedEpisode = await _podcastEpisodeRepository.UpdateAsync(existingEpisode);
        return await MapToResponseDtoWithDetails(updatedEpisode);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _podcastEpisodeRepository.DeleteAsync(id);
    }

    private async Task<IEnumerable<PodcastEpisodeResponseDto>> MapToResponseDtosWithDetails(IEnumerable<PodcastEpisode> episodes)
    {
        var result = new List<PodcastEpisodeResponseDto>();
        foreach (var episode in episodes)
        {
            result.Add(await MapToResponseDtoWithDetails(episode));
        }
        return result;
    }

    private async Task<PodcastEpisodeResponseDto> MapToResponseDtoWithDetails(PodcastEpisode episode)
    {
        var series = await _podcastSeriesRepository.GetByIdAsync(episode.PodcastSeriesId);
        var season = await _podcastSeasonRepository.GetByIdAsync(episode.PodcastSeasonId);

        return new PodcastEpisodeResponseDto
        {
            Id = episode.Id,
            PodcastSeriesId = episode.PodcastSeriesId,
            PodcastSeasonId = episode.PodcastSeasonId,
            EpisodeNumber = episode.EpisodeNumber,
            Title = episode.Title,
            Description = episode.Description,
            CreatedAt = episode.CreatedAt,
            DurationSeconds = episode.DurationSeconds,
            AudioUrl = episode.AudioUrl,
            AudioQualities = episode.AudioQualities,
            ThumbnailUrl = episode.ThumbnailUrl,
            ReleaseDate = episode.ReleaseDate,
            IsActive = episode.IsActive,
            
            // Series bilgileri
            SeriesTitle = series?.Title ?? string.Empty,
            SeriesDescription = series?.Description ?? string.Empty,
            SeriesThumbnailUrl = series?.ThumbnailUrl,
            
            // Season bilgileri
            SeasonNumber = season?.SeasonNumber ?? 0,
            SeasonTitle = season?.Title ?? string.Empty,
            SeasonDescription = season?.Description ?? string.Empty
        };
    }
}
