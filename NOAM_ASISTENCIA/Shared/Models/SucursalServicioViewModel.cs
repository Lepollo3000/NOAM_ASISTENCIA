using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Models
{
    public class SucursalServicioViewModel
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }
}
