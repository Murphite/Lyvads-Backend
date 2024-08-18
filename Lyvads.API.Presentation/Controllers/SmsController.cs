using Lyvads.API.Presentation.Dtos;
using Microsoft.AspNetCore.Mvc;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Lyvads.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private readonly ITwilioRestClient _client;

    public SmsController(ITwilioRestClient client)
    {
        _client = client;
    }


    [HttpPost("send-sms")]
    public IActionResult SendSms(SmsMessage model)
    {
        var message = MessageResource.Create(
                        to: new PhoneNumber(model.To),
                        from: new PhoneNumber(model.From),
                        body: model.Message, client: _client);

        return Ok("Success " + message.Sid);
    }
}
