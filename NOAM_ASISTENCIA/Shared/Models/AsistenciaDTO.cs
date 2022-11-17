using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NOAM_ASISTENCIA.Shared.Models
{
    public class AsistenciaDTO
    {
        public string Username { get; set; } = null!;
        public string NombreUsuario { get; set; } = null!;
        public string ApellidoUsuario { get; set; } = null!;
        public string NombreSucursal { get; set; } = null!;
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
    }
}
