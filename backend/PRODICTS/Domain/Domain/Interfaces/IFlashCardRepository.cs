using Domain.Entities;

namespace Domain.Interfaces;

public interface IFlashCardRepository : IRepository<FlashCard>
{
    Task<IEnumerable<FlashCard>> GetByUserIdAsync(string userId);
    Task<IEnumerable<FlashCard>> GetByGroupIdAsync(string groupId);
    Task<IEnumerable<FlashCard>> GetDueForReviewAsync(string userId);
    Task<FlashCard?> GetByUserIdAndIdAsync(string userId, string id);
    Task UpdateReviewAsync(string id, int currentStep, DateTime nextReviewDate);
    Task<bool> DeleteByGroupIdAsync(string groupId);
}
