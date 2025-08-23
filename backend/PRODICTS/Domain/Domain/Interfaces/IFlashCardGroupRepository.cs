using Domain.Entities;

namespace Domain.Interfaces;

public interface IFlashCardGroupRepository : IRepository<FlashCardGroup>
{
    Task<IEnumerable<FlashCardGroup>> GetByUserIdAsync(string userId);
    Task<FlashCardGroup?> GetByUserIdAndIdAsync(string userId, string id);
    Task<bool> ExistsByUserIdAndNameAsync(string userId, string name);
}
