using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NOAM_ASISTENCIA.Server;
using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Server.Models.Utils.Mail;
using NOAM_ASISTENCIA.Server.Models.Utils.MailService;
using NOAM_ASISTENCIA.Server.Models.Utils.MailService.Interfaces;
using NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels;
using System.Text;

// Scaffold-DbContext "Server=localhost,1433;Database=NOAM_ASISTENCIA;User Id=sa;Password=Pa55w.rd" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -DataAnnotations -Force
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<ErroresTraducidos>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

// REGLAS PARA TOKEN DE AUTENTICACION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(op =>
    {
        var configuration = builder.Configuration.GetSection("JwtBearer");

        op.IncludeErrorDetails = true;
        op.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["IssuerSigningKey"])),
            ValidAudience = configuration["ValidAudience"],
            ValidIssuer = configuration["ValidIssuer"],
            RequireExpirationTime = Convert.ToBoolean(configuration["RequireExpirationTime"]),
            RequireAudience = Convert.ToBoolean(configuration["RequireAudience"]),
            ValidateIssuer = Convert.ToBoolean(configuration["ValidateIssuer"]),
            ValidateAudience = Convert.ToBoolean(configuration["ValidateAudience"]),
            ValidateLifetime = Convert.ToBoolean(configuration["ValidateLifetime"]),
            ValidateIssuerSigningKey = Convert.ToBoolean(configuration["ValidateIssuerSigningKey"])
        };
    });

// CONFIGURACIONES DE ENVIO CORREOS DESDE USER SECRETS
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<MailSettings>>().Value);

builder.Services.AddTransient<IMailService, MailService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// INITIALIZE DATABASE CONTEXT
using (var scope = app.Services.CreateScope())
{
    var dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    InitializeDb(dbcontext, userManager, roleManager);
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

static void InitializeDb(ApplicationDbContext dbcontext, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
{
    InitDB.TryToMigrate(dbcontext);
    InitDB.TrySeedDefaultData(dbcontext);
    InitDB.TryCreateDefaultUsersAndRoles(userManager, roleManager);
}