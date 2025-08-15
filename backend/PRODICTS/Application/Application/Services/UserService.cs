using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAnonymousUserRepository _anonymousUserRepository;

    public UserService(IUserRepository userRepository, IAnonymousUserRepository anonymousUserRepository)
    {
        _userRepository = userRepository;
        _anonymousUserRepository = anonymousUserRepository;
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToResponseDto(user) : null;
    }

    public async Task<UserResponseDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? MapToResponseDto(user) : null;
    }

    public async Task<UserResponseDto?> GetByProviderAsync(string providerId, string providerName)
    {
        var user = await _userRepository.GetByProviderIdAsync(providerId, providerName);
        return user != null ? MapToResponseDto(user) : null;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterUserDto registerDto)
    {
        // Email kontrolü
        if (await _userRepository.EmailExistsAsync(registerDto.Email))
        {
            throw new InvalidOperationException("Bu email adresi zaten kullanılıyor.");
        }

        // Şifre hash'leme
        var passwordHash = HashPassword(registerDto.Password);

        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            ProfilePictureUrl = registerDto.ProfilePictureUrl,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto> LoginAsync(LoginUserDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash!))
        {
            throw new UnauthorizedAccessException("Email veya şifre hatalı.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Hesap devre dışı bırakılmış.");
        }

        // Son giriş zamanını güncelle
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto> RegisterWithProviderAsync(RegisterWithProviderDto providerDto)
    {
        // Önce provider ile kullanıcı var mı kontrol et
        var existingUser = await _userRepository.GetByProviderIdAsync(providerDto.ProviderId, providerDto.ProviderName);
        if (existingUser != null)
        {
            // Provider bilgilerini güncelle
            var existingProvider = existingUser.Providers.FirstOrDefault(p => 
                p.ProviderName == providerDto.ProviderName && p.ProviderId == providerDto.ProviderId);
            
            if (existingProvider != null)
            {
                existingProvider.AccessToken = providerDto.AccessToken;
                existingProvider.RefreshToken = providerDto.RefreshToken;
                existingProvider.LastUsedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(existingUser);
            }

            return MapToResponseDto(existingUser);
        }

        // Email ile kullanıcı var mı kontrol et
        var userByEmail = await _userRepository.GetByEmailAsync(providerDto.Email);
        if (userByEmail != null)
        {
            // Mevcut kullanıcıya provider ekle
            userByEmail.Providers.Add(new UserProvider
            {
                ProviderName = providerDto.ProviderName,
                ProviderId = providerDto.ProviderId,
                Email = providerDto.Email,
                DisplayName = providerDto.DisplayName,
                ProfilePictureUrl = providerDto.ProfilePictureUrl,
                AccessToken = providerDto.AccessToken,
                RefreshToken = providerDto.RefreshToken,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            });

            userByEmail.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(userByEmail);
            return MapToResponseDto(userByEmail);
        }

        // Yeni kullanıcı oluştur
        var newUser = new User
        {
            FirstName = providerDto.FirstName ?? string.Empty,
            LastName = providerDto.LastName ?? string.Empty,
            Email = providerDto.Email,
            ProfilePictureUrl = providerDto.ProfilePictureUrl,
            EmailVerified = true, // Provider'dan gelen email doğrulanmış kabul et
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Providers = new List<UserProvider>
            {
                new()
                {
                    ProviderName = providerDto.ProviderName,
                    ProviderId = providerDto.ProviderId,
                    Email = providerDto.Email,
                    DisplayName = providerDto.DisplayName,
                    ProfilePictureUrl = providerDto.ProfilePictureUrl,
                    AccessToken = providerDto.AccessToken,
                    RefreshToken = providerDto.RefreshToken,
                    CreatedAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow
                }
            }
        };

        await _userRepository.CreateAsync(newUser);
        return MapToResponseDto(newUser);
    }

    public async Task<UserResponseDto> UpdateAsync(string id, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");
        }

        // Güncellenebilir alanları kontrol et ve güncelle
        if (!string.IsNullOrEmpty(updateDto.FirstName))
            user.FirstName = updateDto.FirstName;

        if (!string.IsNullOrEmpty(updateDto.LastName))
            user.LastName = updateDto.LastName;

        if (updateDto.ProfilePictureUrl != null)
            user.ProfilePictureUrl = updateDto.ProfilePictureUrl;

        if (updateDto.EmailVerified.HasValue)
            user.EmailVerified = updateDto.EmailVerified.Value;

        if (!string.IsNullOrEmpty(updateDto.CurrentSubscriptionPlan))
            user.CurrentSubscriptionPlan = updateDto.CurrentSubscriptionPlan;

        if (updateDto.SubscriptionExpiresAt.HasValue)
            user.SubscriptionExpiresAt = updateDto.SubscriptionExpiresAt;

        if (updateDto.IsActive.HasValue)
            user.IsActive = updateDto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return MapToResponseDto(user);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userRepository.EmailExistsAsync(email);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        await _userRepository.DeleteAsync(id);
        return true;
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl,
            EmailVerified = user.EmailVerified,
            Providers = user.Providers.Select(p => new UserProviderDto
            {
                ProviderName = p.ProviderName,
                ProviderId = p.ProviderId,
                Email = p.Email,
                DisplayName = p.DisplayName,
                ProfilePictureUrl = p.ProfilePictureUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LastUsedAt = p.LastUsedAt
            }).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            SubscriptionProvider = user.SubscriptionProvider,
            CurrentSubscriptionPlan = user.CurrentSubscriptionPlan,
            SubscriptionExpiresAt = user.SubscriptionExpiresAt,
            IsActive = user.IsActive
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }

    // Anonymous User Methods
    public async Task<UserResponseDto> GetOrCreateAnonymousAsync(string deviceId)
    {
        var anonymousUser = await _anonymousUserRepository.GetByDeviceIdAsync(deviceId);
        
        if (anonymousUser == null)
        {
            anonymousUser = new AnonymousUser
            {
                DeviceId = deviceId,
                DeviceType = "Unknown", // Bu bilgi client'dan gelecek
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                LastSyncAt = DateTime.UtcNow
            };

            await _anonymousUserRepository.CreateAsync(anonymousUser);
        }
        else
        {
            // Update last active time
            anonymousUser.LastActiveAt = DateTime.UtcNow;
            await _anonymousUserRepository.UpdateAsync(anonymousUser);
        }

        return MapAnonymousToResponseDto(anonymousUser);
    }

    public async Task<UserResponseDto> SyncUserDataAsync(SyncUserDto syncDto)
    {
        var anonymousUser = await _anonymousUserRepository.GetByDeviceIdAsync(syncDto.DeviceId);
        
        if (anonymousUser == null)
        {
            throw new KeyNotFoundException("Anonymous user not found for device ID");
        }

        // Update sync data
        anonymousUser.SyncData.FavoriteWords = syncDto.LocalData.FavoriteWords;
        anonymousUser.SyncData.UserPreferences = syncDto.LocalData.UserPreferences;
        anonymousUser.SyncData.TotalWordsLearned = syncDto.LocalData.TotalWordsLearned;
        
        // Add new study sessions
        foreach (var sessionDto in syncDto.LocalData.StudySessions)
        {
            var existingSession = anonymousUser.SyncData.StudySessions
                .FirstOrDefault(s => s.Date.Date == sessionDto.Date.Date);
            
            if (existingSession == null)
            {
                anonymousUser.SyncData.StudySessions.Add(new StudySession
                {
                    Date = sessionDto.Date,
                    WordsStudied = sessionDto.WordsStudied,
                    CorrectAnswers = sessionDto.CorrectAnswers,
                    StudyDuration = sessionDto.StudyDuration
                });
            }
            else
            {
                // Merge study sessions for the same day
                existingSession.WordsStudied += sessionDto.WordsStudied;
                existingSession.CorrectAnswers += sessionDto.CorrectAnswers;
                existingSession.StudyDuration = existingSession.StudyDuration.Add(sessionDto.StudyDuration);
            }
        }

        anonymousUser.LastSyncAt = DateTime.UtcNow;
        anonymousUser.LastActiveAt = DateTime.UtcNow;
        
        await _anonymousUserRepository.UpdateAsync(anonymousUser);
        return MapAnonymousToResponseDto(anonymousUser);
    }

    public async Task<UserResponseDto> UpgradeAnonymousToRegisteredAsync(string deviceId, RegisterUserDto registerDto)
    {
        var anonymousUser = await _anonymousUserRepository.GetByDeviceIdAsync(deviceId);
        if (anonymousUser == null)
        {
            throw new KeyNotFoundException("Anonymous user not found for device ID");
        }

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(registerDto.Email))
        {
            throw new InvalidOperationException("Bu email adresi zaten kullanılıyor.");
        }

        // Create new registered user with anonymous user data
        var passwordHash = HashPassword(registerDto.Password);

        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            ProfilePictureUrl = registerDto.ProfilePictureUrl ?? anonymousUser.SyncData.UserPreferences.GetValueOrDefault("profilePictureUrl")?.ToString(),
            EmailVerified = false,
            DeviceIds = new List<string> { deviceId },
            AnonymousUserId = anonymousUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Mark anonymous user as upgraded
        await _anonymousUserRepository.MarkAsUpgradedAsync(deviceId, user.Id);

        return MapToResponseDto(user);
    }

    private static UserResponseDto MapAnonymousToResponseDto(AnonymousUser anonymousUser)
    {
        return new UserResponseDto
        {
            Id = anonymousUser.Id,
            FirstName = "Anonymous",
            LastName = "User",
            Email = $"anonymous@{anonymousUser.DeviceId}",
            ProfilePictureUrl = anonymousUser.SyncData.UserPreferences.GetValueOrDefault("profilePictureUrl")?.ToString(),
            EmailVerified = false,
            Providers = new List<UserProviderDto>(),
            CreatedAt = anonymousUser.CreatedAt,
            UpdatedAt = anonymousUser.LastActiveAt,
            SubscriptionProvider = null,
            CurrentSubscriptionPlan = "Free",
            SubscriptionExpiresAt = null,
            IsActive = anonymousUser.IsActive
        };
    }
}
