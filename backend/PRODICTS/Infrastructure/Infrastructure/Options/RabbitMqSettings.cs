namespace Infrastructure.Options;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";
    
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public bool AutoRecovery { get; set; } = true;
    public int NetworkRecoveryInterval { get; set; } = 10;
    
    // Queue Names
    public string AudioProcessingQueue { get; set; } = "audio-processing-queue";
    public string AudioProcessingExchange { get; set; } = "audio-processing-exchange";
    public string AudioProcessingRoutingKey { get; set; } = "audio.process";
}
