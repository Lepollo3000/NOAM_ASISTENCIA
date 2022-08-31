using Microsoft.AspNetCore.Identity;
//using NOAM_ASISTENCIA.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NOAM_ASISTENCIA.Server.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            Asistencias = new HashSet<Asistencium>();
        }

        [Required]
        public string Nombre { get; set; } = null!;
        [Required]
        public string Apellido { get; set; } = null!;
        [Required]
        public int? IdTurno { get; set; }
        [Required]
        public bool Lockout { get; set; }
        [Required]
        public bool ForgotPassword { get; set; }

        [ForeignKey("IdTurno")]
        [InverseProperty("ApplicationUsers")]
        public virtual Turno IdTurnoNavigation { get; set; } = null!;
        [InverseProperty("IdUsuarioNavigation")]
        public virtual ICollection<Asistencium> Asistencias { get; set; }
    }
}