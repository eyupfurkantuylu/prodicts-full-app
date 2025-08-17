using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class FlashCardService : IFlashCardService
{
    private readonly IFlashCardRepository _flashCardRepository;
    private readonly IFlashCardGroupRepository _flashCardGroupRepository;

    public FlashCardService(
        IFlashCardRepository flashCardRepository,
        IFlashCardGroupRepository flashCardGroupRepository)
    {
        _flashCardRepository = flashCardRepository;
        _flashCardGroupRepository = flashCardGroupRepository;
    }

    public async Task<IEnumerable<FlashCardResponseDto>> GetByUserIdAsync(string userId)
    {
        var flashCards = await _flashCardRepository.GetByUserIdAsync(userId);
        return flashCards.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<FlashCardResponseDto>> GetByGroupIdAsync(string groupId, string userId)
    {
        // Önce grubun kullanıcıya ait olduğunu kontrol et
        var group = await _flashCardGroupRepository.GetByUserIdAndIdAsync(userId, groupId);
        if (group == null)
            return Enumerable.Empty<FlashCardResponseDto>();

        var flashCards = await _flashCardRepository.GetByGroupIdAsync(groupId);
        return flashCards.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<FlashCardResponseDto>> GetDueForReviewAsync(string userId)
    {
        var flashCards = await _flashCardRepository.GetDueForReviewAsync(userId);
        return flashCards.Select(MapToResponseDto);
    }

    public async Task<FlashCardResponseDto?> GetByIdAsync(string id, string userId)
    {
        var flashCard = await _flashCardRepository.GetByUserIdAndIdAsync(userId, id);
        return flashCard != null ? MapToResponseDto(flashCard) : null;
    }

    public async Task<FlashCardResponseDto> CreateAsync(CreateFlashCardDto dto, string userId)
    {
        // Grubun kullanıcıya ait olduğunu kontrol et
        var group = await _flashCardGroupRepository.GetByUserIdAndIdAsync(userId, dto.GroupId);
        if (group == null)
            throw new UnauthorizedAccessException("Grup bulunamadı veya erişim yetkiniz yok.");

        var flashCard = new FlashCard
        {
            GroupId = dto.GroupId,
            UserId = userId,
            SourceWord = dto.SourceWord,
            TargetWord = dto.TargetWord,
            NextReviewDate = DateTime.UtcNow
        };

        await _flashCardRepository.CreateAsync(flashCard);
        return MapToResponseDto(flashCard);
    }

    public async Task<FlashCardResponseDto?> UpdateAsync(string id, UpdateFlashCardDto dto, string userId)
    {
        var flashCard = await _flashCardRepository.GetByUserIdAndIdAsync(userId, id);
        if (flashCard == null)
            return null;

        if (!string.IsNullOrEmpty(dto.SourceWord))
            flashCard.SourceWord = dto.SourceWord;

        if (!string.IsNullOrEmpty(dto.TargetWord))
            flashCard.TargetWord = dto.TargetWord;

        flashCard.UpdatedAt = DateTime.UtcNow;

        await _flashCardRepository.UpdateAsync(flashCard);
        return MapToResponseDto(flashCard);
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var flashCard = await _flashCardRepository.GetByUserIdAndIdAsync(userId, id);
        if (flashCard == null)
            return false;

        await _flashCardRepository.DeleteAsync(id);
        return true;
    }

    public async Task<FlashCardResponseDto?> ReviewAsync(string id, ReviewFlashCardDto dto, string userId)
    {
        var flashCard = await _flashCardRepository.GetByUserIdAndIdAsync(userId, id);
        if (flashCard == null)
            return null;

        int newStep = dto.IsCorrect ? flashCard.CurrentStep + 1 : Math.Max(0, flashCard.CurrentStep - 1);
        DateTime nextReviewDate = CalculateNextReviewDate(newStep);

        await _flashCardRepository.UpdateReviewAsync(id, newStep, nextReviewDate);

        // Güncellenmiş kartı tekrar getir
        flashCard = await _flashCardRepository.GetByIdAsync(id);
        return flashCard != null ? MapToResponseDto(flashCard) : null;
    }

    private static DateTime CalculateNextReviewDate(int step)
    {
        var intervals = new[] { 0, 1, 3, 7, 14, 30, 90 }; // günler
        var days = step < intervals.Length ? intervals[step] : 365;
        return DateTime.UtcNow.AddDays(days);
    }

    private static FlashCardResponseDto MapToResponseDto(FlashCard flashCard)
    {
        return new FlashCardResponseDto
        {
            Id = flashCard.Id,
            GroupId = flashCard.GroupId,
            SourceWord = flashCard.SourceWord,
            TargetWord = flashCard.TargetWord,
            CurrentStep = flashCard.CurrentStep,
            NextReviewDate = flashCard.NextReviewDate,
            FirstLearningDate = flashCard.FirstLearningDate,
            ReviewDates = flashCard.ReviewDates,
            IsCompleted = flashCard.IsCompleted,
            CreatedAt = flashCard.CreatedAt,
            UpdatedAt = flashCard.UpdatedAt
        };
    }
}
