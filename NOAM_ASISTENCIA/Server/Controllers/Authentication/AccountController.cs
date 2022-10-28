using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using NOAM_ASISTENCIA.Server.Models.Utils.MailService.Interfaces;
using NOAM_ASISTENCIA.Shared.Utils;

namespace NOAM_ASISTENCIA.Server.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration, IMailService mailService, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _mailService = mailService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            try
            {
                var newUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    Nombre = model.Nombres,
                    Apellido = model.Apellidos,
                    IdTurno = model.IdTurno
                };
                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    var badRequestResponseModel = new ApiResponse<RegisterResult>()
                    {
                        Successful = false,
                        Result = null,
                        ErrorMessages = errors
                    };

                    return BadRequest(badRequestResponseModel);
                }

                /*if (!await SendConfirmationEmailAsync(model, null, newUser))
                {
                    var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                    var modelResultFail = new ConfirmEmailResult() { Successful = false, Errors = errors };

                    return StatusCode(500, modelResultFail);
                }*/

                var okResponseModel = new ApiResponse<RegisterResult>()
                {
                    Successful = true,
                    Result = new RegisterResult()
                };

                return Ok(okResponseModel);
            }
            catch (Exception e)
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var internalServerErrorResponseModel = new ApiResponse<RegisterResult>()
                {
                    Successful = false,
                    Result = null,
                    ErrorMessages = errors
                };

                return StatusCode(500, internalServerErrorResponseModel);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                var errors = new List<string>() { "Usuario no encontrado o inexistente." };
                var badRequestResponseModel = new ApiResponse<ResendEmailResult>()
                {
                    Successful = false,
                    Result = null,
                    ErrorMessages = errors
                };

                return BadRequest(badRequestResponseModel);
            }

            // SE ENVIA EL CORREO
            /*if (!await SendConfirmationEmailAsync(null, model, user, false))
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var modelResultFail = new ResendEmailResult() { Successful = false, Errors = errors };

                return StatusCode(500, modelResultFail);
            }*/

            var okResponseModel = new ApiResponse<ResendEmailResult>()
            {
                Successful = true,
                Result = new ResendEmailResult()
                {
                    UserEmail = user.Email,
                    Username = user.UserName
                }
            };

            return Ok(okResponseModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);

                if (user == null)
                {
                    var errors = new List<string>() { "Usuario no encontrado o inexistente." };
                    var badRequestResponseModel = new ApiResponse<ConfirmEmailResult>()
                    {
                        Successful = false,
                        Result = null,
                        ErrorMessages = errors
                    };

                    return BadRequest(badRequestResponseModel);
                }

                // SE DECODIFICA EL TOKEN A LA FORMA ORIGINAL PARA QUE LO ACEPTE IDENTITY :D
                model.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

                // SE INTENTA CONFIRMAR LA CUENTA CON EL TOKEN DADO
                var result = await _userManager.ConfirmEmailAsync(user, model.Token);

                if (!result.Succeeded)
                {
                    // VERIFICA SI ALGUNO DE LOS ERRORES ES POR TOKEN INVALIDO
                    var errors = result.Errors.Select(x => x.Description);
                    var isTokenError = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken));
                    var badRequestResponseModel = new ApiResponse<ConfirmEmailResult>()
                    {
                        Successful = false,
                        Result = new ConfirmEmailResult()
                        {
                            UserEmail = user.Email,
                            Username = user.UserName
                        },
                        ErrorMessages = errors
                    };

                    if (isTokenError)
                    {
                        badRequestResponseModel.Result.IsTokenError = true;
                    }

                    return BadRequest(badRequestResponseModel);
                }

                var okResponseModel = new ApiResponse<ConfirmEmailResult>()
                {
                    Successful = true,
                    Result = new ConfirmEmailResult()
                    {
                        UserEmail = user.Email,
                        Username = user.UserName
                    }
                };

                return Ok(okResponseModel);
            }
            catch (Exception e)
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var internalServerErrorResponseModel = new ApiResponse<ConfirmEmailResult>()
                {
                    Successful = false,
                    Result = null,
                    ErrorMessages = errors
                };

                return StatusCode(500, internalServerErrorResponseModel);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                // INTENTA OBTENER EL USUARIO CON EL NOMBRE DE USUARIO DADO
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user != null)
                {
                    // SI EL USUARIO ESTA BLOQUEADO POR UN ADMINISTRADOR
                    if (user.Lockout)
                    {
                        var errors = new List<string>() { "No se pudo iniciar la sesión. Contacte a un administrador." };

                        var forbiddenResponseModel = new ApiResponse<LoginResult>()
                        {
                            Successful = false,
                            Result = null,
                            ErrorMessages = errors
                        };

                        // FORBIDDEN
                        return StatusCode(403, forbiddenResponseModel);
                    }

                    // INTENTA INICIAR SESION CON LAS CREDENCIALES DADAS
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                    if (!result.Succeeded)
                    {
                        var errors = new List<string>();

                        if (result.IsLockedOut)
                            errors.Add("Ya no tiene más intentos por ahora. Espere un momento e intente de nuevo.");
                        else if (result.IsNotAllowed)
                            errors.Add("Su cuenta aún no ha sido confirmada. Póngase en contacto con un administrador para más información.");
                        else
                            errors.Add("Credenciales inválidas. Verifique que se hayan ingresado correctamente.");

                        var badRequestResponseModel = new ApiResponse<LoginResult>()
                        {
                            Successful = false,
                            Result = null,
                            ErrorMessages = errors
                        };

                        return BadRequest(badRequestResponseModel);
                    }
                }
                else
                {
                    var errors = new List<string>() { "Credenciales inválidas. Verifique que se hayan ingresado correctamente." };

                    var badRequestResponseModel = new ApiResponse<LoginResult>()
                    {
                        Successful = false,
                        Result = null,
                        ErrorMessages = errors
                    };

                    return BadRequest(badRequestResponseModel);
                }


                // OBTIENE EL USUARIO Y ROLES PARA GENERAR LOS CLAIMS
                var roles = await _userManager.GetRolesAsync(user!);
                var claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.Name, model.UserName));

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // SE GENERA EL TOKEN DE SESION USANDO LAS CREDENCIALES Y CLAIMS PARA DESPUES ENVIARLO AL CLIENTE
                var config = _configuration.GetSection("JwtBearer");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["IssuerSigningKey"]));
                var credencials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.Now.AddDays(Convert.ToInt32(config["JwtExpiryInDays"]));
                var token = new JwtSecurityToken(
                    config["ValidIssuer"],
                    config["ValidAudience"],
                    claims,
                    expires: expiry,
                    signingCredentials: credencials
                );

                var okResponseModel = new ApiResponse<LoginResult>()
                {
                    Successful = true,
                    Result = new LoginResult()
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(token)
                    }
                };

                return Ok(okResponseModel);
            }
            catch (Exception e)
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var internalServerErrorResponseModel = new ApiResponse<LoginResult>()
                {
                    Successful = false,
                    Result = null,
                    ErrorMessages = errors
                };

                return StatusCode(500, internalServerErrorResponseModel);
            }
        }

        private async Task<bool> SendConfirmationEmailAsync(RegisterRequest? registerModel, ResendEmailRequest? resendModel, ApplicationUser newUser, bool noEsNuevo = true)
        {
            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                // SE CODIFICA EN BASE 64 PARA QUE PUEDA PASAR POR EL URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // URL A UNA PANTALLA DEL LADO DEL CLIENTE
                var callbackUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/account/register/confirmemail/{newUser.Id}/{code}";
                callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

                if (registerModel != null)
                    await _mailService.SendRegisterEmailAsync(registerModel, callbackUrl);
                else if (resendModel != null)
                    await _mailService.ResendConfirmationEmailAsync(newUser, callbackUrl);

                return true;
            }
            catch (Exception e)
            {
                if (!noEsNuevo)
                    await _userManager.DeleteAsync(newUser);

                return false;
            }
        }
    }
}
