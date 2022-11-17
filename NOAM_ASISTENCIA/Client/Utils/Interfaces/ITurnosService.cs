using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;

namespace NOAM_ASISTENCIA.Client.Utils.Interfaces
{
    public interface ITurnosService
    {
        Task<IEnumerable<TurnoDTO>> GetTurnos();
    }
}
