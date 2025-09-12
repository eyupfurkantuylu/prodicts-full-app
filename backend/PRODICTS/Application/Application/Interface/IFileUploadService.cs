using Microsoft.AspNetCore.Http;

namespace Application.Interface;

public interface IFileUploadService
{
    Task<string> UploadAudioFileAsync(IFormFile file, string seriesId, string seasonId, string episodeId);
    Task<string> UploadThumbnailAsync(IFormFile file, string seriesId, string seasonId, string episodeId);
    Task<bool> ValidateAudioFileAsync(IFormFile file);
    Task<bool> ValidateImageFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string filePath);
    Task<(int durationSeconds, long fileSize)> GetAudioMetadataAsync(string filePath);
}
