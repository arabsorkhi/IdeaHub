using Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace IdeaHub.Hubs
{
    //A Hub is a central point in the SignalR API that handles connections, groups, and messaging.

    //چون Application نباید از SignalR خبر داشته باشد.
    //در Application فقط یک Interface تعریف کن.
    // در Infrastructure پیاده‌سازی
    public class NotificationHubContext : Hub
    {
        private readonly ILogger<NotificationHubContext> _logger;

        public NotificationHubContext(ILogger<NotificationHubContext> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// ثبت ConnectionId، احراز هویت، ارسال پیام خوش‌آمدگویی
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // ✅ اینجا کدهای هنگام اتصال کاربر

            // مثال: ذخیره ConnectionId در دیتابیس یا Cache
            // await _userConnectionService.AddConnectionAsync(Context.UserIdentifier, Context.ConnectionId);


            var userId = Context.UserIdentifier;

            // IP کاربر
            var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
            var connectionId = Context.ConnectionId;
           
            
            _logger.LogInformation("User {UserId} connected with ConnectionId: {ConnectionId}",
                userId, connectionId);
            
            // هدرهای درخواست
            var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString();
            
            // اتصال به گروه پیش‌فرض (مثلاً گروه کاربر)
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(connectionId, $"user-{userId}");
            }
            // ارسال پیام خوش‌آمدگویی به کاربر تازه متصل شده
            await Clients.Caller.SendAsync("ReceiveNotification",
                "System",
                "Welcome to Notification Hub!",
                NotificationType.Info);

            // اطلاع به دیگران که کاربر جدیدی متصل شده
            await Clients.Others.SendAsync("UserConnected", Context.UserIdentifier);

            await base.OnConnectedAsync();
        }
        /// <summary>
        /// پاک‌سازی منابع، لاگ کردن، اطلاع به دیگر کاربران
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // ✅ اینجا کدهای هنگام قطع اتصال کاربر

            // مثال: حذف ConnectionId از دیتابیس یا Cache
            // await _userConnectionService.RemoveConnectionAsync(Context.ConnectionId);
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            // لاگ کردن خطا (در صورت وجود)
            if (exception != null)
            {
                // Log exception
                Console.WriteLine($"User {Context.UserIdentifier} disconnected with error: {exception.Message}");
                _logger.LogError(exception, "User {UserId} disconnected with error", Context.UserIdentifier);

            }
            else
            {
                _logger.LogInformation("User {UserId} disconnected normally", Context.UserIdentifier);
            }

            // حذف از گروه‌ها (به صورت خودکار انجام می‌شود)
            // اما اگر نیاز به عملیات اضافی دارید:
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, $"user-{userId}");
            }

            // اطلاع به دیگران که کاربری قطع شده
            await Clients.Others.SendAsync("UserDisconnected", Context.UserIdentifier);

            await base.OnDisconnectedAsync(exception);
        }

        // ============= متدهای کلاینت =============

        /// <summary>
        /// پیوستن به گروه
        /// </summary>
        public async Task JoinGroupAsync(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("Group name cannot be empty");
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveNotification",
                "System",
                $"You joined group: {groupName}",
                NotificationType.Success);

            _logger.LogInformation("User {UserId} joined group {GroupName}",
                Context.UserIdentifier, groupName);
        }
        /// <summary>
        /// خروج از گروه
        /// </summary>
        public async Task LeaveGroupAsync(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("Group name cannot be empty");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("ReceiveNotification",
                "System",
                $"You left group: {groupName}",
                NotificationType.Info);

            _logger.LogInformation("User {UserId} left group {GroupName}",
                Context.UserIdentifier, groupName);
        }
        /// <summary>
        /// ارسال پیام به همه (متد نمونه)
        /// </summary>
        public async Task SendMessageToAllAsync(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage",
                Context.UserIdentifier,
                message,
                DateTime.UtcNow);
        }
        /// <summary>
        /// ارسال پیام به یک کاربر خاص (متد نمونه)
        /// </summary>
        public async Task SendMessageToUserAsync(string targetUserId, string message,string title)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveMessage",
                Context.UserIdentifier,
                message,title,
                DateTime.UtcNow);
        }
        public async Task SendNotificationAsync(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

    }
}
