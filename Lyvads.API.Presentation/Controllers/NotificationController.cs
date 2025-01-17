using FirebaseAdmin.Messaging;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(
        INotificationService notificationService,
         UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }


    [HttpPost]
    public async Task<IActionResult> SendMessageAsync([FromBody] MessageRequest request)
    {
        var user = await _userManager.GetUserAsync(User);

        var message = new FirebaseAdmin.Messaging.Message() 
        {
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = request.Title,
                Body = request.Body,
            },
            Data = new Dictionary<string, string>()
            {
                ["FirstName"] = user.FirstName,
                ["LastName"] = user.LastName
            },
            Token = request.DeviceToken
        };

        var messaging = FirebaseMessaging.DefaultInstance;
        var result = await messaging.SendAsync(message);

        if (!string.IsNullOrEmpty(result))
        {
            // Message was sent successfully
            return Ok("Message sent successfully!");
        }
        else
        {
            // There was an error sending the message
            throw new Exception("Error sending the message.");
        }
    }


}

public class MessageRequest
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DeviceToken { get; set; }
    
}

//[HttpPost]
//public async Task<IActionResult> NotifyUser(string userId, string message)
//{
//    await _notificationService.NotifyAsync(userId, message);
//    return Ok("Notification sent.");
//}
