using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas deben coincidir.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
