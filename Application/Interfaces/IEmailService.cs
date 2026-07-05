using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
     
        public interface IEmailService
        {
            /// <summary>
            /// ارسال ایمیل ساده
            /// </summary>
            Task SendEmailAsync(string to, string subject, string body);

            /// <summary>
            /// ارسال لینک بازیابی رمز عبور
            /// </summary>
            Task SendPasswordResetEmailAsync(string email, string token);

            /// <summary>
            /// ارسال اعلان انتشار ایده
            /// </summary>
            Task SendIdeaPublishedNotificationAsync(string email, string ideaTitle);

            /// <summary>
            /// ارسال دعوت‌نامه همکاری
            /// </summary>
            Task SendCollaborationInvitationAsync(string email, string ideaTitle, string inviterName, string role);

            /// <summary>
            /// ارسال اعلان تغییر وضعیت ایده
            /// </summary>
            Task SendIdeaStatusChangeNotificationAsync(string email, string ideaTitle, string oldStatus, string newStatus);
        }
    }
