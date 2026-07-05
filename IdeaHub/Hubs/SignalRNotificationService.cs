using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using IdeaHub.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IdeaHub.Hubs
{ 

    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHubContext> _hub;

        public SignalRNotificationService(IHubContext<NotificationHubContext> hub)
        {
            _hub = hub;
        }
        //ایمیل نوتیفیکیشن با SendGrid (رایگان تا ۱۰۰ ایمیل/روز)
        public async Task SendToUserAsync(Guid userId,
            string title,
            string message)
        {
            await _hub.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", title, message);
        }

        public async Task BroadcastAsync(string title,
            string message)
        {
            await _hub.Clients.All
                .SendAsync("ReceiveNotification", title, message);
        }

        public async Task SendToGroupAsync(string groupName, string title, string message)
        {
            await _hub.Clients
                .Group(groupName)
                .SendAsync("ReceiveNotification", title, message);

        }
    }
}
