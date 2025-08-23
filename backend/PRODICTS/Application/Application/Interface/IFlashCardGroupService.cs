using Application.Models.DTOs;

namespace Application.Interface;

public interface IFlashCardGroupService
{
    Task<IEnumerable<FlashCardGroupResponseDto>> GetByUserIdAsync(string userId);
    Task<FlashCardGroupResponseDto?> GetByIdAsync(string id, string userId);
    Task<FlashCardGroupResponseDto> CreateAsync(CreateFlashCardGroupDto dto, string userId);
    Task<FlashCardGroupResponseDto?> UpdateAsync(string id, UpdateFlashCardGroupDto dto, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}
