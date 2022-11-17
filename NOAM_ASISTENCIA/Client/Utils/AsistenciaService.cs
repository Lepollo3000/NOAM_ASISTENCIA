using NOAM_ASISTENCIA.Client.Utils.Interfaces;
using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;
using System.Net.Http.Json;

namespace NOAM_ASISTENCIA.Client.Utils
{
    public class AsistenciaService : IAsistenciaService
    {
        private readonly HttpClient _httpClient;

        public AsistenciaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<SucursalServicioDTO>> VerificarSucursal(VerificacionSucursalRequest model)
        {
            var response = await _httpClient.GetAsync($"api/sucursalservicios/{model.IdSucursal}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SucursalServicioDTO>>();

            return result!;
        }

        public async Task<ApiResponse<RegistroAsistenciaResult>> RegistrarAsistencia(RegistroAsistenciaRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/asistencias", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegistroAsistenciaResult>>();

            return result!;
        }
    }
}
