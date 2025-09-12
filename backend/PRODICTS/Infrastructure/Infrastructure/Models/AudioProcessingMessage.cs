namespace Infrastructure.Models;

public class AudioProcessingMessage
{
    public string EpisodeId { get; set; } = string.Empty;
    public string OriginalFilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    public List<AudioQualityRequest> QualityLevels { get; set; } = new();
}

public class AudioQualityRequest
{
    public string Quality { get; set; } = string.Empty; // "64k", "128k", "256k"
    public int Bitrate { get; set; } // kbps
    public string OutputPath { get; set; } = string.Empty;
}
