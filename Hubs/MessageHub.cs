using Microsoft.AspNetCore.SignalR;

namespace MessageService.Hubs
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(string text, DateTime timestamp, int sequenceNumber)
        {
            await Clients.All.SendAsync("ReceiveMessage", text, timestamp, sequenceNumber);
        }
    }
}
