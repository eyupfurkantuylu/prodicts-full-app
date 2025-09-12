using Application.Interface;
using Application.Models.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class AppConfigService : IAppConfigService
{
    private readonly IAppConfigRepository _appConfigRepository;

    public AppConfigService(IAppConfigRepository appConfigRepository)
    {
        _appConfigRepository = appConfigRepository;
    }


    public async Task<List<AppConfigDto>> GetAllAsync()
    {
        var appConfigs = await _appConfigRepository.GetAllAsync();
        
        var result = new List<AppConfigDto>();
        foreach (var appConfig in appConfigs)
        {
            result.Add(MapToAppConfigDto(appConfig));
        }
        return result;
    }

    public async Task<AppConfigDto?> GetByIdAsync(string id)
    {
        var appConfig = await _appConfigRepository.GetByIdAsync(id);
        return appConfig != null ? MapToAppConfigDto(appConfig) : null;
    }

    public async Task<AppConfigDto> CreateAsync(CreateAppConfigDto dto)
    {
        var request = new AppConfig
        {
            AppName = dto.AppName,
            IosPackageName = dto.IosPackageName,
            AndroidPackageName = dto.AndroidPackageName,
            IosBuildNumber = dto.IosBuildNumber,
            AndroidBuildNumber = dto.AndroidBuildNumber,
            IosVersion = dto.IosVersion,
            AndroidVersion = dto.AndroidVersion
        };
        var result = await _appConfigRepository.CreateAsync(request);
        request.Id = result.Id;
        return MapToAppConfigDto(request);
    }

    public async Task<AppConfigDto?> UpdateAsync(string id, UpdateAppConfigDto dto)
    {
        var appConfig = await _appConfigRepository.GetByIdAsync(id);
        if (appConfig == null)
            return null;
        if(!string.IsNullOrEmpty(dto.AppName))
            appConfig.AppName = dto.AppName;
        if(!string.IsNullOrEmpty(dto.IosPackageName))
            appConfig.IosPackageName = dto.IosPackageName;
        if(!string.IsNullOrEmpty(dto.AndroidPackageName))
            appConfig.AndroidPackageName = dto.AndroidPackageName;
        if(!string.IsNullOrEmpty(dto.IosBuildNumber))
            appConfig.IosBuildNumber = dto.IosBuildNumber;
        if(!string.IsNullOrEmpty(dto.AndroidBuildNumber))
            appConfig.AndroidBuildNumber = dto.AndroidBuildNumber;
        if(!string.IsNullOrEmpty(dto.IosVersion))
            appConfig.IosVersion = dto.IosVersion;
        if (!string.IsNullOrEmpty(dto.AndroidVersion))
            appConfig.AndroidVersion = dto.AndroidVersion;
        appConfig.UpdatedAt = DateTime.UtcNow;
        await _appConfigRepository.UpdateAsync(appConfig);
        return MapToAppConfigDto(appConfig);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var appConfig = await _appConfigRepository.GetByIdAsync(id);
        if (appConfig == null)
            return false;
        await _appConfigRepository.DeleteAsync(id);
        return true;
    }

    private static AppConfigDto MapToAppConfigDto(AppConfig appConfig)
    {
        return new AppConfigDto
        {
            Id = appConfig.Id,
            AppName = appConfig.AppName,
            IosPackageName = appConfig.IosPackageName,
            AndroidPackageName = appConfig.AndroidPackageName,
            IosBuildNumber = appConfig.IosBuildNumber,
            AndroidBuildNumber = appConfig.AndroidBuildNumber,
            IosVersion = appConfig.IosVersion,
            AndroidVersion = appConfig.AndroidVersion
        };
    }
    
}