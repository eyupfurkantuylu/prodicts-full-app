using Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _baseUploadPath;
    private readonly string _baseThumbnailPath;
    private readonly long _maxFileSizeBytes = 500 * 1024 * 1024; // 500MB
    private readonly long _maxImageSizeBytes = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedAudioExtensions = { ".mp3" };
    private readonly string[] _allowedAudioMimeTypes = { "audio/mpeg", "audio/mp3" };
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedImageMimeTypes = { "image/jpeg", "image/jpg", "image/png", "image/webp" };

    public FileUploadService(ILogger<FileUploadService> logger)
    {
        _logger = logger;
        _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "public", "podcasts");
        _baseThumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "public", "thumbnails");
        
        // Ensure directories exist
        if (!Directory.Exists(_baseUploadPath))
            Directory.CreateDirectory(_baseUploadPath);
        
        if (!Directory.Exists(_baseThumbnailPath))
            Directory.CreateDirectory(_baseThumbnailPath);
    }

    public async Task<string> UploadAudioFileAsync(IFormFile file, string seriesId, string seasonId, string episodeId)
    {
        try
        {
            // Validate file
            var isValid = await ValidateAudioFileAsync(file);
            if (!isValid)
                throw new ArgumentException("Invalid audio file");

            // Create hierarchical directory structure: public/podcasts/{seriesId}/{seasonId}/{episodeId}/
            var episodeDirectory = Path.Combine(_baseUploadPath, seriesId, seasonId, episodeId);
            if (!Directory.Exists(episodeDirectory))
                Directory.CreateDirectory(episodeDirectory);

            // Generate filename with timestamp
            var fileExtension = Path.GetExtension(file.FileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"original_{timestamp}{fileExtension}";
            var filePath = Path.Combine(episodeDirectory, fileName);

            // Save file
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            // Return relative URL path for database storage
            var relativeUrl = Path.Combine("podcasts", seriesId, seasonId, episodeId, fileName)
                .Replace(Path.DirectorySeparatorChar, '/'); // Use forward slashes for URLs

            _logger.LogInformation("Audio file uploaded successfully. SeriesId: {SeriesId}, SeasonId: {SeasonId}, EpisodeId: {EpisodeId}, FilePath: {FilePath}", 
                seriesId, seasonId, episodeId, relativeUrl);

            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload audio file for episode: {EpisodeId}", episodeId);
            throw;
        }
    }

    public async Task<string> UploadThumbnailAsync(IFormFile file, string seriesId, string seasonId, string episodeId)
    {
        try
        {
            // Validate image file
            var isValid = await ValidateImageFileAsync(file);
            if (!isValid)
                throw new ArgumentException("Invalid image file");

            // Create organized directory structure: /thumbnails/seriesId/seasonId/episodeId/
            var thumbnailDirectory = Path.Combine(_baseThumbnailPath, seriesId, seasonId, episodeId);
            if (!Directory.Exists(thumbnailDirectory))
                Directory.CreateDirectory(thumbnailDirectory);

            // Generate filename with timestamp to avoid conflicts
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"thumbnail_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";
            var filePath = Path.Combine(thumbnailDirectory, fileName);

            // Save file
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            // Return relative URL path for database storage
            var relativeUrl = Path.Combine("thumbnails", seriesId, seasonId, episodeId, fileName)
                .Replace(Path.DirectorySeparatorChar, '/'); // Use forward slashes for URLs

            _logger.LogInformation("Thumbnail uploaded successfully. SeriesId: {SeriesId}, SeasonId: {SeasonId}, EpisodeId: {EpisodeId}, FilePath: {FilePath}", 
                seriesId, seasonId, episodeId, relativeUrl);

            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload thumbnail. SeriesId: {SeriesId}, SeasonId: {SeasonId}, EpisodeId: {EpisodeId}", 
                seriesId, seasonId, episodeId);
            throw;
        }
    }

    public Task<bool> ValidateAudioFileAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            // Check file size
            if (file.Length > _maxFileSizeBytes)
            {
                _logger.LogWarning("File size exceeds limit. Size: {FileSize}MB", file.Length / 1024 / 1024);
                return Task.FromResult(false);
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedAudioExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid audio file extension: {Extension}", extension);
                return Task.FromResult(false);
            }

            // Check MIME type
            if (!_allowedAudioMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Invalid audio MIME type: {MimeType}", file.ContentType);
                return Task.FromResult(false);
            }

            // Additional validation can be added here (e.g., file header validation)
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating audio file");
            return Task.FromResult(false);
        }
    }

    public Task<bool> ValidateImageFileAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            // Check file size
            if (file.Length > _maxImageSizeBytes)
            {
                _logger.LogWarning("Image file size exceeds limit. Size: {FileSize}MB", file.Length / 1024 / 1024);
                return Task.FromResult(false);
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid image file extension: {Extension}", extension);
                return Task.FromResult(false);
            }

            // Check MIME type
            if (!_allowedImageMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Invalid image MIME type: {MimeType}", file.ContentType);
                return Task.FromResult(false);
            }

            // Additional validation can be added here (e.g., image header validation)
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image file");
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public Task<(int durationSeconds, long fileSize)> GetAudioMetadataAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Audio file not found: {filePath}");

            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Length;

            // Basic file size info - duration will be extracted by FFmpeg service
            _logger.LogInformation("Audio metadata retrieved. FilePath: {FilePath}, FileSize: {FileSize}", 
                filePath, fileSize);

            return Task.FromResult((0, fileSize)); // Duration will be set by FFmpeg service later
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audio metadata for file: {FilePath}", filePath);
            throw;
        }
    }
}
