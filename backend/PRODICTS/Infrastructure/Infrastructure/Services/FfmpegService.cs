using System.Diagnostics;
using System.Text.RegularExpressions;
using Application.Interface;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class FfmpegService : IFfmpegService
{
    private readonly ILogger<FfmpegService> _logger;
    private readonly FFmpegSettings _settings;

    public FfmpegService(ILogger<FfmpegService> logger, IOptions<FFmpegSettings> ffmpegSettings)
    {
        _logger = logger;
        _settings = ffmpegSettings.Value;
        
        _logger.LogInformation("FFmpegService initialized with UseDocker: {UseDocker}, DockerImage: {DockerImage}", 
            _settings.UseDocker, _settings.DockerImage);
    }

    public async Task<bool> ConvertAudioQualityAsync(string inputPath, string outputPath, int bitrate)
    {
        try
        {
            if (!File.Exists(inputPath))
            {
                _logger.LogError("Input file not found: {InputPath}", inputPath);
                return false;
            }

            // Ensure output directory exists (both for Docker and native)
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                _logger.LogDebug("Created output directory: {Directory}", outputDirectory);
            }

            // FFmpeg command to convert audio bitrate
            var arguments = $"-i \"{inputPath}\" -codec:a libmp3lame -b:a {bitrate}k -y \"{outputPath}\"";
            var processStartInfo = CreateProcessStartInfo("ffmpeg", arguments);

            _logger.LogInformation("Starting FFmpeg conversion. Input: {InputPath}, Output: {OutputPath}, Bitrate: {Bitrate}k", 
                inputPath, outputPath, bitrate);

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start FFmpeg process");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("FFmpeg conversion completed successfully. Output: {OutputPath}", outputPath);
                return true;
            }
            else
            {
                _logger.LogError("FFmpeg conversion failed. Exit code: {ExitCode}, Error: {Error}", 
                    process.ExitCode, error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during FFmpeg conversion. Input: {InputPath}, Output: {OutputPath}", 
                inputPath, outputPath);
            return false;
        }
    }

    public async Task<(int durationSeconds, long fileSize)> GetAudioMetadataAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Audio file not found: {filePath}");

            // Get file size
            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Length;

            // FFmpeg command to get duration
            var arguments = $"-i \"{filePath}\" -f null -";
            var processStartInfo = CreateProcessStartInfo("ffmpeg", arguments);

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start FFmpeg process for metadata");
                return (0, fileSize);
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            // Parse duration from FFmpeg output
            var durationSeconds = ParseDurationFromOutput(error);

            _logger.LogInformation("Audio metadata extracted. File: {FilePath}, Duration: {Duration}s, Size: {Size} bytes", 
                filePath, durationSeconds, fileSize);

            return (durationSeconds, fileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audio metadata for file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> ValidateAudioFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            // FFmpeg command to validate audio file
            var arguments = $"-v error -i \"{filePath}\" -f null -";
            var processStartInfo = CreateProcessStartInfo("ffmpeg", arguments);

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return false;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            // If exit code is 0 and no errors, file is valid
            var isValid = process.ExitCode == 0 && string.IsNullOrWhiteSpace(error);

            _logger.LogInformation("Audio file validation result. File: {FilePath}, IsValid: {IsValid}", 
                filePath, isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating audio file: {FilePath}", filePath);
            return false;
        }
    }

    private int ParseDurationFromOutput(string output)
    {
        try
        {
            // Parse duration from FFmpeg output like "Duration: 00:03:45.67"
            var durationMatch = Regex.Match(output, @"Duration: (\d{2}):(\d{2}):(\d{2})\.(\d{2})");
            
            if (durationMatch.Success)
            {
                var hours = int.Parse(durationMatch.Groups[1].Value);
                var minutes = int.Parse(durationMatch.Groups[2].Value);
                var seconds = int.Parse(durationMatch.Groups[3].Value);
                
                return hours * 3600 + minutes * 60 + seconds;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse duration from FFmpeg output: {Output}", output);
            return 0;
        }
    }
    
    private ProcessStartInfo CreateProcessStartInfo(string tool, string arguments)
    {
        if (_settings.UseDocker)
        {
            // Docker için path'leri container içi path'e çevir
            var dockerArguments = ConvertArgumentsForDocker(arguments);
            
            // Volume mount: PRODICTS klasörünü /workspace'e mount et
            var currentWorkingDir = Directory.GetCurrentDirectory(); // API klasörü
            var baseProjectDir = Directory.GetParent(currentWorkingDir)?.FullName ?? currentWorkingDir;
            
            // Docker: sadece FFmpeg komutunu çalıştır (directory host'ta yaratıldı)
            var fullDockerArgs = $"run --rm -v \"{baseProjectDir}\":/workspace {_settings.DockerImage} {tool} {dockerArguments}";
            
            _logger.LogDebug("Docker command: docker {Args}", fullDockerArgs);
            
            return new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = fullDockerArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        else
        {
            // Native FFmpeg
            var executablePath = tool == "ffmpeg" ? _settings.FFmpegPath : _settings.FFprobePath;
            
            return new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
    }
    
    private string ConvertArgumentsForDocker(string arguments)
    {
        // Windows path'lerini Docker container path'lerine çevir
        // C:\Users\...\PRODICTS\API\public\file.mp3 -> /workspace/API/public/file.mp3
        var currentWorkingDir = Directory.GetCurrentDirectory(); // API klasörü
        var baseProjectDir = Directory.GetParent(currentWorkingDir)?.FullName; // PRODICTS klasörü
        
        var converted = arguments;
        
        // Eğer base project directory varsa, onu workspace ile değiştir
        if (!string.IsNullOrEmpty(baseProjectDir))
        {
            converted = converted.Replace(baseProjectDir, "/workspace");
        }
        else
        {
            // Fallback: current directory'yi workspace ile değiştir
            converted = converted.Replace(currentWorkingDir, "/workspace/API");
        }
        
        // Windows \ karakterlerini / ile değiştir
        converted = converted.Replace("\\", "/");
        
        _logger.LogDebug("Path conversion: {Original} -> {Converted}", arguments, converted);
        
        return converted;
    }
}
