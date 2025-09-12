namespace Infrastructure.Options;

public class FFmpegSettings
{
    public string FFmpegPath { get; set; } = "ffmpeg";
    public string FFprobePath { get; set; } = "ffprobe";
    public bool UseDocker { get; set; } = false;
    public string DockerImage { get; set; } = "jrottenberg/ffmpeg:latest";
}
