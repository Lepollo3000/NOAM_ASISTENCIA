using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NOAM_ASISTENCIA.Client.Utils.Interfaces;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

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

        public async Task<RegisterResult> Register(RegisterRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", model);
            var result = await response.Content.ReadFromJsonAsync<RegisterResult>();

            return result!;
        }
        
        public async Task<ConfirmEmailResult> ConfirmEmail(ConfirmEmailRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/confirmemail", model);
            var result = await response.Content.ReadFromJsonAsync<ConfirmEmailResult>();

            return result!;
        }

        public async Task<ResendEmailResult> ResendConfirmationEmail(ResendEmailRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/resendconfirmationemail", model);
            var result = await response.Content.ReadFromJsonAsync<ResendEmailResult>();

            return result!;
        }

        public async Task<LoginResult> Login(LoginRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();

            if (result!.Successful)
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Token);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);

                return result;
            }

            return result;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
