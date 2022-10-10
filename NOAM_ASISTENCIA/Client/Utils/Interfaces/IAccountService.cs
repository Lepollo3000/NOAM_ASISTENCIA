using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

namespace NOAM_ASISTENCIA.Client.Utils.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse> Login(LoginRequest model);
        Task Logout();
        Task<ApiResponse> Register(RegisterRequest model);
        Task<ApiResponse> ConfirmEmail(ConfirmEmailRequest model);
        Task<ApiResponse> ResendConfirmationEmail(ResendEmailRequest model);
    }
}
