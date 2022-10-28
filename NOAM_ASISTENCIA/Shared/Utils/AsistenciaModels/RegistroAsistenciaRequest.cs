using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels
{
    public class RegistroAsistenciaRequest
    {
        public string Username { get; set; } = null!;
        public int IdSucursal { get; set; }
    }
}
