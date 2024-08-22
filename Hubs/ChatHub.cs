using Microsoft.AspNetCore.SignalR;

namespace BackendChat.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMedia(string user, string mediaUrl)
        {
            await Clients.All.SendAsync("ReceiveMedia", user, mediaUrl);
        }
    }
}
