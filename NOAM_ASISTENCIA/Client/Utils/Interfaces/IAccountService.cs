using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

namespace NOAM_ASISTENCIA.Client.Utils.Interfaces
{
    public interface IAccountService
    {
        Task<LoginResult> Login(LoginRequest model);
        Task Logout();
        Task<RegisterResult> Register(RegisterRequest model);
        Task<ConfirmEmailResult> ConfirmEmail(ConfirmEmailRequest model);
        Task<ResendEmailResult> ResendConfirmationEmail(ResendEmailRequest model);
    }
}
