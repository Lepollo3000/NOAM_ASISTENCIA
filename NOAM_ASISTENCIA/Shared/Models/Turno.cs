using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Models
{
    public class Turno
    {
        private const string requiredErrorMessage = "El campo '{0}' es requerido.";

        [Required(ErrorMessage = requiredErrorMessage)]
        public int Id { get; set; }
        [Required(ErrorMessage = requiredErrorMessage)]
        [StringLength(100)]
        public string Descripcion { get; set; } = null!;
    }
}
