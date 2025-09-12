namespace Application.Interface;

public interface IQueueService
{
    Task PublishAudioProcessingMessageAsync<T>(T message) where T : class;
    Task<bool> IsConnectedAsync();
}
