using NOAM_ASISTENCIA.Shared.Utils;
using System.Net.Http.Json;
using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Client.Utils.Interfaces;

namespace NOAM_ASISTENCIA.Client.Utils
{
    public class TurnosService : ITurnosService
    {
        private readonly HttpClient _httpClient;

        public TurnosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TurnoDTO>> GetTurnos()
        {
            var response = await _httpClient.GetAsync("api/turnos");
            var result = await response.Content.ReadFromJsonAsync<IEnumerable<TurnoDTO>>();

            return result!;
        }
    }
}
