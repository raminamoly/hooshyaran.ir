using Hooshyaran.Web.Models;

namespace Hooshyaran.Web.Services;

public interface IDemoRequestNotificationService
{
    Task NotifyAsync(DemoRequest demoRequest, CancellationToken cancellationToken = default);
}
