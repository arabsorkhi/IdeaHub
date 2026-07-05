using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace IdeaHub.Controllers
{




    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// ارسال نوتیفیکیشن به یک کاربر خاص
        /// </summary>
        [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] NotificationDto notification)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.SendToUserAsync(
                notification.UserId,
                notification.Title,
                notification.Message);

            _logger.LogInformation("Notification sent to user {UserId}", notification.UserId);
            return Ok(new { success = true, message = "Notification sent successfully" });
        }

        /// <summary>
        /// ارسال نوتیفیکیشن به همه کاربران
        /// </summary>
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastNotificationDto broadcast)
        {
            if (string.IsNullOrEmpty(broadcast.Title) || string.IsNullOrEmpty(broadcast.Message))
                return BadRequest("Title and message are required");

            await _notificationService.BroadcastAsync(broadcast.Title, broadcast.Message);

            _logger.LogInformation("Broadcast sent: {Title}", broadcast.Title);
            return Ok(new { success = true, message = "Broadcast sent successfully" });
        }

        /// <summary>
        /// ارسال نوتیفیکیشن به یک گروه
        /// </summary>
        [HttpPost("send-to-group")]
        public async Task<IActionResult> SendToGroup([FromBody] GroupNotificationDto groupNotification)
        {
            if (string.IsNullOrEmpty(groupNotification.GroupName))
                return BadRequest("Group name is required");

            await _notificationService.SendToGroupAsync(
                groupNotification.GroupName,
                groupNotification.Title,
                groupNotification.Message);

            _logger.LogInformation("Notification sent to group {GroupName}", groupNotification.GroupName);
            return Ok(new { success = true, message = "Notification sent to group successfully" });
        }
    }

    // DTOهای مربوط به Controller
    public class BroadcastNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class GroupNotificationDto
    {
        public string GroupName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}