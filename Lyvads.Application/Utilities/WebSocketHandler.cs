using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;

namespace Lyvads.Application.Utilities;

public class WebSocketHandler
{
    private readonly List<WebSocket> _sockets = new List<WebSocket>();

    public async Task Handle(HttpContext context, WebSocket webSocket)
    {
        _sockets.Add(webSocket);

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await SendMessageToAllAsync(message);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _sockets.Remove(webSocket);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    public async Task SendMessageToAllAsync(string message)
    {
        var messageBuffer = Encoding.UTF8.GetBytes(message);

        foreach (var socket in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
