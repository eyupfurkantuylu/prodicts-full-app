using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class FlashCardGroupService : IFlashCardGroupService
{
    private readonly IFlashCardGroupRepository _flashCardGroupRepository;
    private readonly IFlashCardRepository _flashCardRepository;

    public FlashCardGroupService(
        IFlashCardGroupRepository flashCardGroupRepository,
        IFlashCardRepository flashCardRepository)
    {
        _flashCardGroupRepository = flashCardGroupRepository;
        _flashCardRepository = flashCardRepository;
    }

    public async Task<IEnumerable<FlashCardGroupResponseDto>> GetByUserIdAsync(string userId)
    {
        var groups = await _flashCardGroupRepository.GetByUserIdAsync(userId);
        return groups.Select(MapToResponseDto);
    }

    public async Task<FlashCardGroupResponseDto?> GetByIdAsync(string id, string userId)
    {
        var group = await _flashCardGroupRepository.GetByUserIdAndIdAsync(userId, id);
        return group != null ? MapToResponseDto(group) : null;
    }

    public async Task<FlashCardGroupResponseDto> CreateAsync(CreateFlashCardGroupDto dto, string userId)
    {
        // Aynı isimde grup var mı kontrol et
        var exists = await _flashCardGroupRepository.ExistsByUserIdAndNameAsync(userId, dto.Name);
        if (exists)
            throw new InvalidOperationException("Bu isimde bir grup zaten mevcut.");

        var group = new FlashCardGroup
        {
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            SourceLanguage = dto.SourceLanguage,
            TargetLanguage = dto.TargetLanguage
        };

        await _flashCardGroupRepository.CreateAsync(group);
        return MapToResponseDto(group);
    }

    public async Task<FlashCardGroupResponseDto?> UpdateAsync(string id, UpdateFlashCardGroupDto dto, string userId)
    {
        var group = await _flashCardGroupRepository.GetByUserIdAndIdAsync(userId, id);
        if (group == null)
            return null;

        // İsim değiştiriliyorsa, aynı isimde başka grup var mı kontrol et
        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != group.Name)
        {
            var exists = await _flashCardGroupRepository.ExistsByUserIdAndNameAsync(userId, dto.Name);
            if (exists)
                throw new InvalidOperationException("Bu isimde bir grup zaten mevcut.");
            
            group.Name = dto.Name;
        }

        if (dto.Description != null)
            group.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.SourceLanguage))
            group.SourceLanguage = dto.SourceLanguage;

        if (!string.IsNullOrEmpty(dto.TargetLanguage))
            group.TargetLanguage = dto.TargetLanguage;

        if (dto.IsActive.HasValue)
            group.IsActive = dto.IsActive.Value;

        group.UpdatedAt = DateTime.UtcNow;

        await _flashCardGroupRepository.UpdateAsync(group);
        return MapToResponseDto(group);
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var group = await _flashCardGroupRepository.GetByUserIdAndIdAsync(userId, id);
        if (group == null)
            return false;

        // Gruba ait tüm kartları sil
        await _flashCardRepository.DeleteByGroupIdAsync(id);
        
        // Grubu sil
        await _flashCardGroupRepository.DeleteAsync(id);
        return true;
    }

    private static FlashCardGroupResponseDto MapToResponseDto(FlashCardGroup group)
    {
        return new FlashCardGroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            SourceLanguage = group.SourceLanguage,
            TargetLanguage = group.TargetLanguage,
            IsActive = group.IsActive,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        };
    }
}
