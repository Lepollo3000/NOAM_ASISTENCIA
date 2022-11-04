using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AsistenciaModels;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenciasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AsistenciasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _dbcontext = context;
            _userManager = userManager;
        }

        // GET: api/Asistencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetAsistencia()
        {
            return await _dbcontext.Asistencia.ToListAsync();
        }

        // GET: api/Asistencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asistencia>> GetAsistencium(Guid id)
        {
            var asistencium = await _dbcontext.Asistencia.FindAsync(id);

            if (asistencium == null)
            {
                return NotFound();
            }

            return asistencium;
        }

        // PUT: api/Asistencias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsistencium(Guid id, Asistencia asistencium)
        {
            if (id != asistencium.IdUsuario)
            {
                return BadRequest();
            }

            _dbcontext.Entry(asistencium).State = EntityState.Modified;

            try
            {
                await _dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                /*if (!AsistenciumExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }*/
            }

            return NoContent();
        }

        // POST: api/Asistencias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostAsistencium(RegistroAsistenciaRequest model)
        {
            var response = new ApiResponse<RegistroAsistenciaResult>();

            var user = await _userManager.FindByNameAsync(model.Username);
            var sucursal = await _dbcontext.SucursalServicios.FindAsync(model.IdSucursal);

            if (user != null && sucursal != null)
            {
                var asistencia = new Asistencia()
                {
                    IdUsuario = user.Id,
                    IdSucursal = sucursal.Id,
                    FechaEntrada = DateTime.UtcNow
                };

                _dbcontext.Asistencia.Add(asistencia);

                try
                {
                    await _dbcontext.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (AsistenciaExists(asistencia.IdUsuario, asistencia.IdSucursal, asistencia.FechaEntrada))
                    {
                        var errors = new List<string>() { "Lo sentimos. Error interno del servidor, inténtelo de nuevo más tarde." };

                        response.Successful = false;
                        response.Result = null;
                        response.ErrorMessages = errors;

                        return Conflict(response);
                    }
                    else
                    {
                        var errors = new List<string>() { "Lo sentimos. Error interno del servidor, inténtelo de nuevo más tarde." };

                        response.Successful = false;
                        response.Result = null;
                        response.ErrorMessages = errors;

                        return StatusCode(500, response);
                    }
                }

                response.Successful = true;
                response.Result = new RegistroAsistenciaResult()
                {
                    Sucursal = sucursal.Descripcion,
                    Username = user.UserName,
                    Fecha = asistencia.FechaEntrada,
                    EsEntrada = true
                };

                return CreatedAtAction("GetAsistencia", new { id = asistencia.IdUsuario }, response);
            }
            else
            {
                var errors = new List<string>() { "Información inválida. Verifique que la sucursal exista o tenga la sesión iniciada." };

                response.Successful = false;
                response.Result = null;
                response.ErrorMessages = errors;

                return BadRequest(response);
            }
        }

        // DELETE: api/Asistencias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsistencium(Guid id)
        {
            var asistencium = await _dbcontext.Asistencia.FindAsync(id);
            if (asistencium == null)
            {
                return NotFound();
            }

            _dbcontext.Asistencia.Remove(asistencium);
            await _dbcontext.SaveChangesAsync();

            return NoContent();
        }

        private bool AsistenciaExists(Guid idUsuario, int idSucursal, DateTime fechaEntrada)
        {
            return _dbcontext.Asistencia.Any(e =>
                e.IdUsuario == idUsuario &&
                e.IdSucursal == idSucursal &&
                e.FechaEntrada == fechaEntrada
            );
        }
    }
}
