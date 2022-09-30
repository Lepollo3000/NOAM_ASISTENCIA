using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
}
