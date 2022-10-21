using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class LoginRequest
    {
        private const string requiredErrorMessage = "El campo {0} es requerido.";

        [Required(ErrorMessage = requiredErrorMessage)]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = requiredErrorMessage)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;
        [Display(Name = "Recuérdame")]
        public bool RememberMe { get; set; }
    }
}
