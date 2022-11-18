using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Models
{
    public class UserDTO
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string TurnoNombre { get; set; } = null!;
        public bool Lockout { get; set; }
    }
}
