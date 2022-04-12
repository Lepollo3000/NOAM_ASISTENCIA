using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class InitDBController : ControllerBase
    {
        //private readonly FSW_POSDevContext _dbcontext;
        private readonly ApplicationDbContext _identitycontext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public InitDBController(/*FSW_POSDevContext dbcontext,*/ ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //_dbcontext = dbcontext;
            _identitycontext = applicationDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Get()
        {
            //INTENTA MIGRAR LA INFORMACION A LA DB
            if (!InitDB.TryToMigrate(/*_dbcontext,*/ _identitycontext))
            {
                var error = "Error interno del servidor al migrar la BD. Contacte a su administrador.";

                return StatusCode(500, error);
            }

            //INTENTA CREAR LOS USUARIOS ADMIN Y CAJERO
            if (!InitDB.TryCreateDefaultUsersAndRoles(_userManager, _roleManager))
            {
                var error = "Error interno del servidor al crear usuarios predefinidos. Contacte a su administrador.";

                return StatusCode(500, error);
            }
            /*
            //INTENTA CREAR LOS USUARIOS ADMIN Y CAJERO
            if (!InitDB.TrySeedDefaultData(_dbcontext))
            {
                var error = "Error interno del servidor al crear datos predefinidos. Contacte a su administrador.";

                return StatusCode(500, error);
            }
            */
            string mensaje = "Base de datos actualizada correctamente.";

            return StatusCode(200, mensaje);
        }
    }
}
