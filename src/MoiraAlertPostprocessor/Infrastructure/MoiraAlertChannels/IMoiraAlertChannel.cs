namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;

public interface IMoiraAlertChannel
{
    Task AlertUsersAsync(JsonContent msg, CancellationToken cancellationToken = default);
    Task SendReplyToPostAsync(int channelPostMessageId, string text, CancellationToken cancellationToken = default);
}