using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Models
{
    public class ReporteAsistenciaGeneralDTO
    {
        public string Username { get; set; } = null!;
        public string UsuarioNombre { get; set; } = null!;
        public string UsuarioApellido { get; set; } = null!;
        public string Sucursal { get; set; } = null!;
        public double HorasLaboradas { get; set; }
        public DateTime Fecha { get; set; }
    }
}
