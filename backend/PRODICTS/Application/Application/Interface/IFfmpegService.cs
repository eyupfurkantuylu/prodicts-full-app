namespace Application.Interface;

public interface IFfmpegService
{
    Task<bool> ConvertAudioQualityAsync(string inputPath, string outputPath, int bitrate);
    Task<(int durationSeconds, long fileSize)> GetAudioMetadataAsync(string filePath);
    Task<bool> ValidateAudioFileAsync(string filePath);
}
