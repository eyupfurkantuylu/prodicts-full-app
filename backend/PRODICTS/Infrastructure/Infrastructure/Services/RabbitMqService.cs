using System.Text;
using System.Text.Json;
using Application.Interface;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Services;

public class RabbitMqService : IQueueService, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMqService(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private void EnsureConnection()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        lock (_lock)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

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

                // Declare exchange
                _channel.ExchangeDeclare(
                    exchange: _settings.AudioProcessingExchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                // Declare queue
                _channel.QueueDeclare(
                    queue: _settings.AudioProcessingQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                // Bind queue to exchange
                _channel.QueueBind(
                    queue: _settings.AudioProcessingQueue,
                    exchange: _settings.AudioProcessingExchange,
                    routingKey: _settings.AudioProcessingRoutingKey);

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                throw;
            }
        }
    }

    public async Task PublishAudioProcessingMessageAsync<T>(T message) where T : class
    {
        try
        {
            EnsureConnection();

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _settings.AudioProcessingExchange,
                routingKey: _settings.AudioProcessingRoutingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Audio processing message published successfully. MessageId: {MessageId}", properties.MessageId);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audio processing message");
            throw;
        }
    }

    public Task<bool> IsConnectedAsync()
    {
        try
        {
            return Task.FromResult(_connection?.IsOpen == true && _channel?.IsOpen == true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred while disposing RabbitMQ connection");
        }
    }
}
