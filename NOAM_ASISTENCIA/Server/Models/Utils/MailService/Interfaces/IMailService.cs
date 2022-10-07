using NOAM_ASISTENCIA.Server.Models.Utils.Mail;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

namespace NOAM_ASISTENCIA.Server.Models.Utils.MailService.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(string username, string userEmail, string redirectUrl);
        Task SendRegisterEmailAsync(RegisterRequest request, string redirectUrl);
        Task ResendConfirmationEmailAsync(ApplicationUser request, string redirectUrl);
    }
}
