using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Utils.AuthModels
{
    public class RegisterRequest
    {
        private const string requiredErrorMessage = "El campo '{0}' es requerido.";

        [Required(ErrorMessage = requiredErrorMessage)]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = requiredErrorMessage)]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = requiredErrorMessage)]
        [Display(Name = "Nombre(s)")]
        public string Nombres { get; set; } = null!;
        [Required(ErrorMessage = requiredErrorMessage)]
        [Display(Name = "Apellido(s)")]
        public string Apellidos { get; set; } = null!;
        [Display(Name = "Turno")]
        public int IdTurno { get; set; }
        [Required(ErrorMessage = requiredErrorMessage)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = requiredErrorMessage)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas deben coincidir.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
