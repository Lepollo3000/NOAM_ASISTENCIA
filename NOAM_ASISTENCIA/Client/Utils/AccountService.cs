using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
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
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AccountService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse> Register(RegisterRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

            return result!;
        }
        
        public async Task<ApiResponse> ConfirmEmail(ConfirmEmailRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/confirmemail", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

            return result!;
        }

        public async Task<ApiResponse> ResendConfirmationEmail(ResendEmailRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/resendconfirmationemail", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

            return result!;
        }

        public async Task<ApiResponse> Login(LoginRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (result!.Successful)
            {
                await _localStorage.SetItemAsync("authToken", ((LoginResult)result.Result).Token);
                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(((LoginResult)result.Result).Token);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", ((LoginResult)result.Result).Token);

                return result;
            }

            return result;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
