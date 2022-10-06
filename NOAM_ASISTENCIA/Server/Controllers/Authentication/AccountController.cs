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

namespace NOAM_ASISTENCIA.Server.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            try
            {
                var newUser = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    var modelResult = new RegisterResult { Successful = false, Errors = errors };

                    return BadRequest(modelResult);
                }

                if (!await SendConfirmationEmailAsync(model, null, newUser))
                {
                    var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                    var modelResultFail = new ConfirmEmailResult() { Successful = false, Errors = errors };

                    return StatusCode(500, modelResultFail);
                }

                return Ok(new RegisterResult { Successful = true });
            }
            catch (Exception e)
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var modelResultFail = new ConfirmEmailResult() { Successful = false, Errors = errors };

                return StatusCode(500, modelResultFail);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendEmailRequest model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                var errors = new List<string>() { "Usuario no encontrado o inexistente." };
                var modelResultFail = new ResendEmailResult() { Errors = errors };

                return BadRequest(modelResultFail);
            }

            // SE ENVIA EL CORREO
            if (!await SendConfirmationEmailAsync(null, model, user, false))
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var modelResultFail = new ResendEmailResult() { Successful = false, Errors = errors };

                return StatusCode(500, modelResultFail);
            }

            var modelResult = new ResendEmailResult() { Successful = true, UserEmail = user.Email, Username = user.UserName };

            return Ok(modelResult);
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

                /*if (registerModel != null)
                    await _mailService.SendRegisterEmailAsync(registerModel, callbackUrl);
                else if (resendModel != null)
                    await _mailService.ResendConfirmationEmailAsync(newUser, callbackUrl);*/

                return true;
            }
            catch (Exception e)
            {
                if (!noEsNuevo)
                    await _userManager.DeleteAsync(newUser);

                return false;
            }
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
                    var modelResultFail = new ConfirmEmailResult() { Errors = errors };

                    return BadRequest(modelResultFail);
                }

                // SE DECODIFICA EL TOKEN A LA FORMA ORIGINAL PARA QUE LO ACEPTE IDENTITY :D
                model.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

                // SE INTENTA CONFIRMAR LA CUENTA CON EL TOKEN DADO
                var result = await _userManager.ConfirmEmailAsync(user, model.Token);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    // VERIFICA SI ALGUNO DE LOS ERRORES ES POR TOKEN INVALIDO
                    var isTokenError = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken));
                    var modelResultFail = new ConfirmEmailResult() { UserEmail = user.Email, Username = user.UserName, Errors = errors };

                    if (isTokenError)
                    {
                        modelResultFail.IsTokenError = true;
                    }

                    return BadRequest(modelResultFail);
                }

                var modelResult = new ConfirmEmailResult() { Successful = true, Username = user.UserName, UserEmail = user.Email };

                return Ok(modelResult);
            }
            catch (Exception e)
            {
                var errors = new List<string>() { "Error interno del servidor, inténtelo de nuevo más tarde." };
                var modelResultFail = new ConfirmEmailResult() { Successful = false, Errors = errors };

                return StatusCode(500, modelResultFail);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

                if (!result.Succeeded)
                {
                    var error = "Credenciales inválidas. Verifique que se hayan ingresado correctamente.";
                    var modelResultFail = new LoginResult { Successful = false, Error = error };

                    return BadRequest(modelResultFail);
                }

                // OBTIENE USUARIO, ROLES Y DEMAS PARA GENERAR LOS CLAIMS
                var user = await _userManager.FindByNameAsync(model.UserName);
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.Name, model.UserName));

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // SE GENERA EL TOKEN DE SESION USANDO LAS CREDENCIALES Y CLAIMS PARA DESPUES ENVIARLO AL CLIENTE
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));
                var token = new JwtSecurityToken(
                    _configuration["JwtIssuer"],
                    _configuration["JwtAudience"],
                    claims,
                    expires: expiry,
                    signingCredentials: creds
                );
                var modelResult = new LoginResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) };

                return Ok(modelResult);
            }
            catch (Exception e)
            {
                var error = "Error interno del servidor, inténtelo de nuevo más tarde.";
                var modelResultFail = new LoginResult { Successful = false, Error = error };

                return StatusCode(500, modelResultFail);
            }
        }
    }

}
