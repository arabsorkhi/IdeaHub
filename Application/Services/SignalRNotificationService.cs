using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Application.Services
{ 

    public class SignalRNotificationService : INotificationService
    {
        // private readonly IHubContext<INotificationHubContext> _hub;  // ارسال از طریق Interface (نه مستقیم SignalR)

        private readonly INotificationHubContext _hubContext;
        private ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(  INotificationHubContext hubContext, ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            // _hub = hub;
        }
        //ایمیل نوتیفیکیشن با SendGrid (رایگان تا ۱۰۰ ایمیل/روز)
        public async Task SendToUserAsync(Guid userId,
            string title,
            string message)
        {
            //await _hubContext. SendMessageToUserAsync
            //    (userId.ToString(),  message);
            // اعتبارسنجی
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                throw new ArgumentException("Title and message are required");

            // لاگ قبل از ارسال
            _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, title);

            // ارسال از طریق Interface (نه مستقیم SignalR)
            await _hubContext.SendMessageToUserAsync(userId.ToString(), title, message);

            // لاگ بعد از ارسال
            _logger.LogInformation("Notification sent to user {UserId} successfully", userId);
        }

        public async Task BroadcastAsync(string title,
            string message)
        {
            //await _hubContext.Clients.All
            //    .SendAsync("ReceiveNotification", title, message);
            // اعتبارسنجی
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                throw new ArgumentException("Title and message are required");

            // لاگ
            _logger.LogInformation("Broadcasting notification: {Title}", title);

            // ارسال
            await _hubContext.SendMessageToAllAsync(title, message);

            _logger.LogInformation("Broadcast sent successfully");
        }

        public async Task SendToGroupAsync(string groupName, string title, string message)
        {
            // اعتبارسنجی
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("Group name is required");

            // منطق خاص گروه
            if (groupName.StartsWith("Admin") && !IsUserAdmin())
                throw new UnauthorizedAccessException("Only admins can send to admin groups");

            _logger.LogInformation("Sending notification to group {GroupName}: {Title}", groupName, title);

            await _hubContext.SendToGroupAsync(groupName, title, message);
            // .Group(groupName)
            //  .SendAsync("ReceiveNotification", title, message);

        }
        private bool IsUserAdmin() => true; // منطق احراز هویت
    }
}
