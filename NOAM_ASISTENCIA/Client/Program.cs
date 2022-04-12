using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using NOAM_ASISTENCIA.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddBlazorise()
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDg4MTk3QDMxMzkyZTMyMmUzMEJhZlJDUXQ0c3Y1VFRpS3NnUVRMQzR5SXlaWmZiNHdGMFUzSnhQeFNiV2M9");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("WebAPI.NoAuthenticationClient",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddHttpClient("NOAM_ASISTENCIA.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("WebAPI.NoAuthenticationClient"));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("NOAM_ASISTENCIA.ServerAPI"));

builder.Services.AddApiAuthorization();

await builder.Build().RunAsync();
