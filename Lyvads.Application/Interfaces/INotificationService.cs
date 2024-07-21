
namespace Lyvads.Application.Interfaces;

public interface INotificationService
{
   Task NotifyAsync(string creatorId, string message);
}
