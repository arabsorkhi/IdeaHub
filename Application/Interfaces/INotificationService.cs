using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{//در Infrastructure پیاده‌سازی
    public interface INotificationService
    { 
        /// <summary>
        /// ارسال نوتیفیکیشن به یک کاربر خاص
        /// </summary>
        Task SendToUserAsync(Guid userId, string title, string message);
        /// <summary>
        /// ارسال نوتیفیکیشن به همه کاربران
        /// </summary>
        Task BroadcastAsync(string title, string message);
        /// <summary>
        /// ارسال نوتیفیکیشن به یک گروه
        /// </summary>
        Task SendToGroupAsync(string groupName, string title, string message);

    }
    public interface INotificationHubContext
    {
        Task SendMessage(string message);
        Task JoinGroupAsync(string groupName);
        Task LeaveGroupAsync(string groupName);
        Task SendMessageToAllAsync(string title,string message);

        Task SendMessageToUserAsync(string targetUserId,string title, string message);
        Task SendNotificationAsync(string message);
        Task SendToGroupAsync(string groupName, string title, string message);
    }
}
