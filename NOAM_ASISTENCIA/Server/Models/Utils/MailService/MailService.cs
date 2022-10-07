using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NOAM_ASISTENCIA.Server.Models.Utils.Mail;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Server.Models.Utils.MailService.Interfaces;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

namespace NOAM_ASISTENCIA.Server.Models.Utils.MailService
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string username, string userEmail, string redirectUrl)
        {
            string FilePath = Directory.GetCurrentDirectory() + "\\Data\\Mail\\Templates\\RegisterTemplate.html";
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();

            str.Close();
            MailText = MailText
                .Replace("[Username]", username)
                .Replace("UrlRedirect", redirectUrl);

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = $"Confirmación de cuenta para {username}";

            var builder = new BodyBuilder();
            builder.HtmlBody = MailText;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.Auto);
            //smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async Task SendRegisterEmailAsync(RegisterRequest request, string redirectUrl)
        {
            await SendEmailAsync(request.Username, request.Email, redirectUrl);
        }

        public async Task ResendConfirmationEmailAsync(ApplicationUser request, string redirectUrl)
        {
            await SendEmailAsync(request.UserName, request.Email, redirectUrl);
        }
    }
}
