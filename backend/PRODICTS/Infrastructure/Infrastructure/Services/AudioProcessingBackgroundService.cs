using System.Text;
using System.Text.Json;
using Application.Interface;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Models;
using Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Services;

public class AudioProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<AudioProcessingBackgroundService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public AudioProcessingBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<RabbitMqSettings> settings,
        ILogger<AudioProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audio Processing Background Service is starting");
        
        try
        {
            InitializeRabbitMq();
            await base.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Audio Processing Background Service");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audio Processing Background Service is running");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Keep the service running
                await Task.Delay(1000, stoppingToken);
                
                // Check connection health
                if (_connection?.IsOpen != true || _channel?.IsOpen != true)
                {
                    _logger.LogWarning("RabbitMQ connection lost, attempting to reconnect...");
                    InitializeRabbitMq();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Audio Processing Background Service");
                await Task.Delay(5000, stoppingToken); // Wait before retrying
            }
        }
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = _settings.AutoRecovery,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_settings.NetworkRecoveryInterval)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Set QoS to process one message at a time
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            // Declare exchange and queue (in case they don't exist)
            _channel.ExchangeDeclare(
                exchange: _settings.AudioProcessingExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.AudioProcessingQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: _settings.AudioProcessingQueue,
                exchange: _settings.AudioProcessingExchange,
                routingKey: _settings.AudioProcessingRoutingKey);

            // Set up consumer
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var processingMessage = JsonSerializer.Deserialize<AudioProcessingMessage>(message);
                    if (processingMessage != null)
                    {
                        await ProcessAudioMessage(processingMessage);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        _logger.LogError("Failed to deserialize audio processing message");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing audio message: {Message}", message);
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: _settings.AudioProcessingQueue, autoAck: false, consumer: consumer);

            _logger.LogInformation("RabbitMQ consumer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private async Task ProcessAudioMessage(AudioProcessingMessage message)
    {
        using var scope = _serviceProvider.CreateScope();
        var ffmpegService = scope.ServiceProvider.GetRequiredService<IFfmpegService>();
        var episodeService = scope.ServiceProvider.GetRequiredService<IPodcastEpisodeService>();

        try
        {
            _logger.LogInformation("Starting audio processing for episode: {EpisodeId}", message.EpisodeId);

            // Update episode status to Processing
            await UpdateEpisodeStatus(episodeService, message.EpisodeId, ProcessingStatus.Processing);

            // Convert relative path to full path
            var originalFullPath = ConvertToFullPath(message.OriginalFilePath);
            
            // Get audio metadata from original file
            var (durationSeconds, originalFileSize) = await ffmpegService.GetAudioMetadataAsync(originalFullPath);

            var audioQualities = new List<AudioQuality>();

            // Add original quality
            audioQualities.Add(new AudioQuality
            {
                Quality = "original",
                Url = message.OriginalFilePath,
                FileSize = originalFileSize,
                Bitrate = 0, // Original bitrate (unknown)
                IsProcessed = true,
                ProcessedAt = DateTime.UtcNow
            });

            // Process each quality level
            foreach (var qualityRequest in message.QualityLevels)
            {
                try
                {
                    _logger.LogInformation("Processing {Quality} quality for episode: {EpisodeId}", 
                        qualityRequest.Quality, message.EpisodeId);

                    // Convert paths to full paths
                    var inputFullPath = ConvertToFullPath(message.OriginalFilePath);
                    var outputFullPath = ConvertToFullPath(qualityRequest.OutputPath);

                    var success = await ffmpegService.ConvertAudioQualityAsync(
                        inputFullPath,
                        outputFullPath,
                        qualityRequest.Bitrate);

                    if (success && File.Exists(outputFullPath))
                    {
                        var outputFileInfo = new FileInfo(outputFullPath);
                        
                        audioQualities.Add(new AudioQuality
                        {
                            Quality = qualityRequest.Quality,
                            Url = qualityRequest.OutputPath,
                            FileSize = outputFileInfo.Length,
                            Bitrate = qualityRequest.Bitrate,
                            IsProcessed = true,
                            ProcessedAt = DateTime.UtcNow
                        });

                        _logger.LogInformation("Successfully processed {Quality} quality for episode: {EpisodeId}", 
                            qualityRequest.Quality, message.EpisodeId);
                    }
                    else
                    {
                        _logger.LogError("Failed to process {Quality} quality for episode: {EpisodeId}", 
                            qualityRequest.Quality, message.EpisodeId);
                        
                        audioQualities.Add(new AudioQuality
                        {
                            Quality = qualityRequest.Quality,
                            Url = qualityRequest.OutputPath,
                            FileSize = 0,
                            Bitrate = qualityRequest.Bitrate,
                            IsProcessed = false,
                            ProcessedAt = null
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing {Quality} quality for episode: {EpisodeId}", 
                        qualityRequest.Quality, message.EpisodeId);
                }
            }

            // Update episode with processed audio qualities
            var updateDto = new Application.Models.DTOs.UpdatePodcastEpisodeDto
            {
                DurationSeconds = durationSeconds,
                AudioQualities = audioQualities,
                ProcessingStatus = ProcessingStatus.Completed,
                ProcessingCompletedAt = DateTime.UtcNow
            };

            await episodeService.UpdateAsync(message.EpisodeId, updateDto);

            _logger.LogInformation("Audio processing completed successfully for episode: {EpisodeId}", message.EpisodeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audio processing failed for episode: {EpisodeId}", message.EpisodeId);
            
            // Update episode status to Failed
            await UpdateEpisodeStatus(episodeService, message.EpisodeId, ProcessingStatus.Failed, ex.Message);
        }
    }

    private async Task UpdateEpisodeStatus(IPodcastEpisodeService episodeService, string episodeId, 
        ProcessingStatus status, string? errorMessage = null)
    {
        try
        {
            var updateDto = new Application.Models.DTOs.UpdatePodcastEpisodeDto
            {
                ProcessingStatus = status,
                ProcessingErrorMessage = errorMessage
            };

            if (status == ProcessingStatus.Processing)
                updateDto.ProcessingStartedAt = DateTime.UtcNow;
            else if (status == ProcessingStatus.Completed)
                updateDto.ProcessingCompletedAt = DateTime.UtcNow;

            await episodeService.UpdateAsync(episodeId, updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update episode status. EpisodeId: {EpisodeId}, Status: {Status}", 
                episodeId, status);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Audio Processing Background Service is stopping");
        
        try
        {
            _channel?.Close();
            _connection?.Close();
            await base.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping Audio Processing Background Service");
        }
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred while disposing Audio Processing Background Service");
        }
    }
    
    private static string ConvertToFullPath(string relativePath)
    {
        // Relative path'leri full path'e çevir
        // "podcasts/seriesId/seasonId/episodeId/file.mp3" -> "C:\...\public\podcasts\seriesId\seasonId\episodeId\file.mp3"
        
        if (Path.IsPathRooted(relativePath))
            return relativePath; // Already full path
            
        var publicDirectory = Path.Combine(Directory.GetCurrentDirectory(), "public");
        return Path.Combine(publicDirectory, relativePath);
    }
}
