using Application.Models.DTOs;
using Domain.Entities;

namespace Application.Interface;

public interface IAppConfigService
{
    Task<List<AppConfigDto>> GetAllAsync();
    Task<AppConfigDto?> GetByIdAsync(string id);
    Task<AppConfigDto> CreateAsync(CreateAppConfigDto dto);
    Task<AppConfigDto?> UpdateAsync(string id, UpdateAppConfigDto dto);
    Task<bool> DeleteAsync(string id);
}