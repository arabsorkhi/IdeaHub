using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using Application.Interfaces;

namespace Application.Services
{
   
        public class EmailService : IEmailService
        {
            private readonly IConfiguration _configuration;

            public EmailService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task SendEmailAsync(string to, string subject, string body)
            {
                var smtpConfig = _configuration.GetSection("Smtp");

                using var client = new SmtpClient(smtpConfig["Host"], int.Parse(smtpConfig["Port"] ?? "587"))
                {
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpConfig["FromEmail"] ?? "noreply@ideavault.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }

            public async Task SendPasswordResetEmailAsync(string email, string token)
            {
                var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={token}";

                var body = $@"
                <h2>بازیابی رمز عبور</h2>
                <p>برای بازیابی رمز عبور خود روی لینک زیر کلیک کنید:</p>
                <a href='{resetLink}'>{resetLink}</a>
                <p>این لینک تا ۲۴ ساعت اعتبار دارد.</p>
            ";

                await SendEmailAsync(email, "بازیابی رمز عبور - IdeaVault", body);
            }

            public async Task SendIdeaPublishedNotificationAsync(string email, string ideaTitle)
            {
                var body = $@"
                <h2>ایده شما منتشر شد!</h2>
                <p>ایده '{ideaTitle}' با موفقیت منتشر شد و در انتظار بررسی است.</p>
                <p>به زودی بازخورد آن را دریافت خواهید کرد.</p>
            ";

                await SendEmailAsync(email, "انتشار ایده - IdeaVault", body);
            }

        public Task SendCollaborationInvitationAsync(string email, string ideaTitle, string inviterName, string role)
        {
            throw new NotImplementedException();
        }

        public Task SendIdeaStatusChangeNotificationAsync(string email, string ideaTitle, string oldStatus, string newStatus)
        {
            throw new NotImplementedException();
        }
    }

   
}
