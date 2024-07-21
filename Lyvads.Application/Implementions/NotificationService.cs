using Lyvads.Application.Interfaces;
using Lyvads.Application.Utilities;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Repositories;

namespace Lyvads.Application.Implementions;

public class NotificationService : INotificationService
{
    private readonly IRepository _Repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WebSocketHandler _webSocketHandler;

    public NotificationService(IRepository Repository, IUnitOfWork unitOfWork, WebSocketHandler webSocketHandler)
    {
        _Repository = Repository;
        _unitOfWork = unitOfWork;
        _webSocketHandler = webSocketHandler;
    }

    public async Task NotifyAsync(string userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _Repository.Add(notification);
        await _unitOfWork.SaveChangesAsync();

        await _webSocketHandler.SendMessageToAllAsync(message);
    }
}