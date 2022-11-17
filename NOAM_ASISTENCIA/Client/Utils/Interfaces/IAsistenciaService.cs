using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels;

namespace NOAM_ASISTENCIA.Client.Utils.Interfaces
{
    public interface IAsistenciaService
    {
        Task<ApiResponse<SucursalServicioDTO>> VerificarSucursal(VerificacionSucursalRequest model);
        Task<ApiResponse<RegistroAsistenciaResult>> RegistrarAsistencia(RegistroAsistenciaRequest model);
    }
}
