using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NOAM_ASISTENCIA.Client.Utils.Interfaces;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;
using NOAM_ASISTENCIA.Shared.Utils;

namespace NOAM_ASISTENCIA.Client.Utils
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AccountService(HttpClient httpClient, CustomAuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<RegisterResult>> Register(RegisterRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResult>>();

            return result!;
        }

        public async Task<ApiResponse<ConfirmEmailResult>> ConfirmEmail(ConfirmEmailRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/confirmemail", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ConfirmEmailResult>>();

            return result!;
        }

        public async Task<ApiResponse<ResendEmailResult>> ResendConfirmationEmail(ResendEmailRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/resendconfirmationemail", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ResendEmailResult>>();

            return result!;
        }

        public async Task<ApiResponse<LoginResult>> Login(LoginRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResult>>();

            if (result!.Successful)
            {
                LoginResult modelResult = result.Result!;

                if (modelResult != null)
                {
                    await _localStorage.SetItemAsync("authToken", modelResult.Token);
                    _authenticationStateProvider.MarkUserAsAuthenticated(modelResult.Token);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", modelResult.Token);
                }

                return result;
            }

            return result;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _authenticationStateProvider.MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
