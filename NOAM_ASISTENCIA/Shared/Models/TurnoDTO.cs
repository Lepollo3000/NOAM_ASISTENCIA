using System.ComponentModel.DataAnnotations;

namespace NOAM_ASISTENCIA.Shared.Models
{
    public class TurnoDTO
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
        public string DescripcionCorta { get; set; } = null!;
    }
}
