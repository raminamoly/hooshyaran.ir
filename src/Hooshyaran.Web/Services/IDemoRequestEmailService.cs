using Hooshyaran.Web.Models;

namespace Hooshyaran.Web.Services;

public interface IDemoRequestEmailService
{
    Task NotifyAsync(DemoRequest demoRequest, CancellationToken cancellationToken = default);
}
