using Application.Models.DTOs;

namespace Application.Interface;

public interface IFlashCardService
{
    Task<IEnumerable<FlashCardResponseDto>> GetByUserIdAsync(string userId);
    Task<IEnumerable<FlashCardResponseDto>> GetByGroupIdAsync(string groupId, string userId);
    Task<IEnumerable<FlashCardResponseDto>> GetDueForReviewAsync(string userId);
    Task<FlashCardResponseDto?> GetByIdAsync(string id, string userId);
    Task<FlashCardResponseDto> CreateAsync(CreateFlashCardDto dto, string userId);
    Task<FlashCardResponseDto?> UpdateAsync(string id, UpdateFlashCardDto dto, string userId);
    Task<bool> DeleteAsync(string id, string userId);
    Task<FlashCardResponseDto?> ReviewAsync(string id, ReviewFlashCardDto dto, string userId);
    Task<FlashCardResponseDto?>  StudyAsync(string id, string userId);
}
