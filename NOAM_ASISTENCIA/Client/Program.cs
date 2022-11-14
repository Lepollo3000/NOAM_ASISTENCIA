global using Microsoft.AspNetCore.Components.Authorization;
using NOAM_ASISTENCIA.Client;
using NOAM_ASISTENCIA.Client.Utils;
using NOAM_ASISTENCIA.Client.Utils.Interfaces;
using Syncfusion.Blazor;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzU5ODg0QDMyMzAyZTMzMmUzMEFVemNwakM2WEZaTnZwV21ZSWxxNkFsUFVUZ1pEeStjc1RFUlNsN2ZFVGs9\r\n");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddBlazorise()
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

// HOST ENVIRONMENT FOR CLIENT
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddOptions();

// ACCOUNT SERVICE
builder.Services.AddScoped<IAccountService, AccountService>();
// ASISTENCIA SERVICE
builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();

// CUSTOM AUTH STATE PROVIDER
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthenticationStateProvider>());

builder.Services.AddSyncfusionBlazor();

await builder.Build().RunAsync();
