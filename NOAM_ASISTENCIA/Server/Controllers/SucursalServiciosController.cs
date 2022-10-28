using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NOAM_ASISTENCIA.Server.Data;
using NOAM_ASISTENCIA.Server.Models;
using NOAM_ASISTENCIA.Shared.Models;
using NOAM_ASISTENCIA.Shared.Utils;
using NOAM_ASISTENCIA.Shared.Utils.AuthModels;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SucursalServiciosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SucursalServiciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SucursalServicios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SucursalServicio>>> GetSucursalServicios()
        {
            return await _context.SucursalServicios.ToListAsync();
        }

        // GET: api/SucursalServicios/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSucursalServicio(int id)
        {
            var sucursalServicio = await _context.SucursalServicios.FindAsync(id);
            var response = new ApiResponse<SucursalServicioViewModel>();

            if (sucursalServicio == null)
            {
                var errors = new List<string>() { "Lo sentimos. La sucursal solicitada no se encontró o no existe." };

                response.Successful = false;
                response.ErrorMessages = errors;

                return NotFound(response);
            }
            else
            {
                response.Successful = true;
                response.Result = new SucursalServicioViewModel()
                {
                    Id = sucursalServicio.Id,
                    Descripcion = sucursalServicio.Descripcion
                };
            }

            return Ok(response);
        }

        // PUT: api/SucursalServicios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSucursalServicio(int id, SucursalServicio sucursalServicio)
        {
            if (id != sucursalServicio.Id)
            {
                return BadRequest();
            }

            _context.Entry(sucursalServicio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SucursalServicioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SucursalServicios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SucursalServicio>> PostSucursalServicio(SucursalServicio sucursalServicio)
        {
            _context.SucursalServicios.Add(sucursalServicio);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSucursalServicio", new { id = sucursalServicio.Id }, sucursalServicio);
        }

        // DELETE: api/SucursalServicios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSucursalServicio(int id)
        {
            var sucursalServicio = await _context.SucursalServicios.FindAsync(id);
            if (sucursalServicio == null)
            {
                return NotFound();
            }

            _context.SucursalServicios.Remove(sucursalServicio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SucursalServicioExists(int id)
        {
            return _context.SucursalServicios.Any(e => e.Id == id);
        }
    }
}
