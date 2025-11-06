namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;

public interface IMoiraAlertChannel
{
    Task AlertUsersAsync(JsonContent msg, CancellationToken cancellationToken = default);
}