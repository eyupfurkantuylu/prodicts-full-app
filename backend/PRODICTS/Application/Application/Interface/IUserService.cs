using Application.Models.DTOs;

namespace Application.Interface;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(string id);
    Task<UserResponseDto?> GetByEmailAsync(string email);
    Task<UserResponseDto?> GetByProviderAsync(string providerId, string providerName);
    Task<UserResponseDto> RegisterAsync(RegisterUserDto registerDto);
    Task<UserResponseDto> LoginAsync(LoginUserDto loginDto);
    Task<UserResponseDto> RegisterWithProviderAsync(RegisterWithProviderDto providerDto);
    Task<UserResponseDto> UpdateAsync(string id, UpdateUserDto updateDto);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> DeleteAsync(string id);
    
    // Anonymous user & sync methods
    Task<UserResponseDto> GetOrCreateAnonymousAsync(string deviceId);
    Task<UserResponseDto> SyncUserDataAsync(SyncUserDto syncDto);
    Task<UserResponseDto> UpgradeAnonymousToRegisteredAsync(string deviceId, RegisterUserDto registerDto);
}
