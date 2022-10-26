using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

namespace NOAM_ASISTENCIA.Client.Utils.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<LoginResult>> Login(LoginRequest model);
        Task Logout();
        Task<ApiResponse<RegisterResult>> Register(RegisterRequest model);
        Task<ApiResponse<ConfirmEmailResult>> ConfirmEmail(ConfirmEmailRequest model);
        Task<ApiResponse<ResendEmailResult>> ResendConfirmationEmail(ResendEmailRequest model);
    }
}
