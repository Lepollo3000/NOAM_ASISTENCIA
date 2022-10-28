using NOAM_ASISTENCIA.Shared.Models;

namespace NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels
{
    public class RegistroAsistenciaResult
    {
        public string Sucursal { get; set; } = null!;
        public string Username { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public bool EsEntrada { get; set; }
    }
}
